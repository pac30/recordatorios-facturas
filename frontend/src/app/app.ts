import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { CurrencyPipe, DatePipe, DecimalPipe } from '@angular/common';
import { FacturaResumen, FacturasService, ResultadoCliente } from './facturas.service';

interface GrupoCliente {
  clienteId: string;
  cliente: string;
  email: string;
  iniciales: string;
  facturas: FacturaResumen[];
  total: number;
  porEnviar: number;
}

type Aviso = { tipo: 'ok' | 'error'; texto: string };

type Kpis = Record<'montoTotal' | 'primer' | 'segundo' | 'desactivadas' | 'montoPendiente', number>;
const KPIS_EN_CERO: Kpis = { montoTotal: 0, primer: 0, segundo: 0, desactivadas: 0, montoPendiente: 0 };

const ESTADOS_RECORDATORIO = ['primerrecordatorio', 'segundorecordatorio'];

@Component({
  selector: 'app-root',
  imports: [CurrencyPipe, DatePipe, DecimalPipe],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly servicio = inject(FacturasService);

  readonly estados = ['primerrecordatorio', 'segundorecordatorio', 'desactivado'];
  readonly facturas = signal<FacturaResumen[]>([]);
  readonly filtro = signal<string>('todos');
  readonly busqueda = signal('');
  readonly cargando = signal(false);
  readonly mensaje = signal<Aviso | null>(null);
  readonly plegados = signal<ReadonlySet<string>>(new Set());
  readonly procesando = signal<string | null>(null);
  /** Facturas cuyo estado cambió en la última carga: la tabla las resalta unos segundos. */
  readonly cambiadas = signal<ReadonlySet<string>>(new Set());
  /** Valores de las tarjetas, animados hacia el resumen real. */
  readonly kpis = signal<Kpis>(KPIS_EN_CERO);
  private animacion = 0;

  readonly visibles = computed(() => {
    const q = this.busqueda().trim().toLowerCase();
    return this.facturas().filter(f =>
      (this.filtro() === 'todos' || f.estado === this.filtro()) &&
      (!q || f.cliente.toLowerCase().includes(q) || f.numero.toLowerCase().includes(q)));
  });

  readonly grupos = computed<GrupoCliente[]>(() => {
    const mapa = new Map<string, GrupoCliente>();
    for (const f of this.visibles()) {
      const g = mapa.get(f.clienteId) ?? {
        clienteId: f.clienteId, cliente: f.cliente, email: f.email, facturas: [], total: 0, porEnviar: 0,
        iniciales: f.cliente.split(' ').slice(0, 2).map(p => p[0]).join('').toUpperCase()
      };
      g.facturas.push(f);
      g.total += f.monto;
      if (ESTADOS_RECORDATORIO.includes(f.estado)) g.porEnviar++;
      mapa.set(f.clienteId, g);
    }
    return [...mapa.values()];
  });

  readonly conteo = computed(() => {
    const c: Record<string, number> = {};
    for (const f of this.facturas()) c[f.estado] = (c[f.estado] ?? 0) + 1;
    return c;
  });

  readonly resumen = computed(() => {
    const todas = this.facturas();
    const enRecordatorio = todas.filter(f => ESTADOS_RECORDATORIO.includes(f.estado));
    return {
      facturas: todas.length,
      clientes: new Set(todas.map(f => f.clienteId)).size,
      montoTotal: todas.reduce((s, f) => s + f.monto, 0),
      enRecordatorio: enRecordatorio.length,
      primer: todas.filter(f => f.estado === 'primerrecordatorio').length,
      segundo: todas.filter(f => f.estado === 'segundorecordatorio').length,
      montoEnRecordatorio: enRecordatorio.reduce((s, f) => s + f.monto, 0),
      desactivadas: todas.filter(f => f.estado === 'desactivado').length
    };
  });

  constructor() {
    this.cargar();
    effect(() => {
      const r = this.resumen();
      untracked(() => this.animarKpis({
        montoTotal: r.montoTotal, primer: r.primer, segundo: r.segundo,
        desactivadas: r.desactivadas, montoPendiente: r.montoEnRecordatorio
      }));
    });
  }

  cargar(): void {
    this.cargando.set(true);
    this.servicio.obtenerResumen().subscribe({
      next: datos => {
        const previos = new Map(this.facturas().map(f => [f.id, f.estado]));
        const cambiadas = new Set(datos.filter(f => previos.has(f.id) && previos.get(f.id) !== f.estado).map(f => f.id));
        this.facturas.set(datos);
        this.cargando.set(false);
        if (cambiadas.size) {
          this.cambiadas.set(cambiadas);
          setTimeout(() => this.cambiadas.set(new Set()), 3200);
        }
      },
      error: () => this.fallo('No se pudo conectar con la API. ¿Está corriendo el backend?')
    });
  }

  procesarTodos(): void {
    this.procesando.set('todos');
    this.servicio.procesarTodos().subscribe({
      next: res => this.tras(res),
      error: () => this.fallo('Falló el procesamiento.')
    });
  }

  procesarCliente(clienteId: string): void {
    this.procesando.set(clienteId);
    this.servicio.procesarCliente(clienteId).subscribe({
      next: res => this.tras([res]),
      error: () => this.fallo('Falló el procesamiento del cliente.')
    });
  }

  reiniciarDemo(): void {
    this.cargando.set(true);
    this.servicio.reiniciarDemo().subscribe({
      next: r => {
        this.avisar({ tipo: 'ok', texto: `Datos reiniciados: ${r.clientes} clientes y ${r.facturas} facturas.` });
        this.cargar();
      },
      error: () => this.fallo('No se pudieron reiniciar los datos.')
    });
  }

  alternar(clienteId: string): void {
    const s = new Set(this.plegados());
    if (!s.delete(clienteId)) s.add(clienteId);
    this.plegados.set(s);
  }

  /** Días transcurridos desde el vencimiento (negativo si aún no vence). */
  diasVencida(f: FacturaResumen): number {
    return Math.floor((Date.now() - new Date(f.fechaVencimiento).getTime()) / 86_400_000);
  }

  /** Paso del ciclo: 1 = primer recordatorio, 2 = segundo, 3 = desactivada, 0 = fuera del ciclo. */
  paso(f: FacturaResumen): number {
    return this.estados.indexOf(f.estado) + 1;
  }

  cerrarAviso(): void {
    this.mensaje.set(null);
  }

  private tras(res: ResultadoCliente[]): void {
    const todas = res.flatMap(r => r.facturas);
    const ok = todas.filter(f => f.exito).length;
    const fallidas = todas.length - ok;
    if (todas.length === 0) {
      this.avisar({ tipo: 'ok', texto: 'No había facturas en recordatorio para procesar.' });
    } else {
      this.avisar({
        tipo: fallidas ? 'error' : 'ok',
        texto: `${ok} correo(s) enviado(s) y estado actualizado` +
          (fallidas ? `; ${fallidas} fallaron (${todas.find(f => !f.exito)?.error})` : '')
      });
    }
    this.procesando.set(null);
    this.cargar();
  }

  /** Hace "contar" las tarjetas desde el valor actual hasta el nuevo (ease-out). */
  private animarKpis(hasta: Kpis): void {
    const desde = this.kpis();
    const id = ++this.animacion;
    const inicio = performance.now();
    const duracion = 800;
    const claves = Object.keys(hasta) as (keyof Kpis)[];
    const paso = (ahora: number) => {
      if (id !== this.animacion) return;
      const p = Math.min(1, (ahora - inicio) / duracion);
      const e = 1 - Math.pow(1 - p, 3);
      this.kpis.set(Object.fromEntries(claves.map(k => [k, desde[k] + (hasta[k] - desde[k]) * e])) as Kpis);
      if (p < 1) requestAnimationFrame(paso);
    };
    requestAnimationFrame(paso);
  }

  private avisar(aviso: Aviso): void {
    this.mensaje.set(aviso);
    setTimeout(() => { if (this.mensaje() === aviso) this.mensaje.set(null); }, 6000);
  }

  private fallo(texto: string): void {
    this.cargando.set(false);
    this.procesando.set(null);
    this.avisar({ tipo: 'error', texto });
  }
}