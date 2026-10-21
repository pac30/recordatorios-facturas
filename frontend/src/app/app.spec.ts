import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { App } from './app';
import { API_URL, FacturaResumen } from './facturas.service';

const factura = (id: string, clienteId: string, estado: string, monto: number): FacturaResumen => ({
  id, numero: `F-${id}`, clienteId, cliente: `Cliente ${clienteId}`, email: `${clienteId}@test.com`,
  monto, fechaVencimiento: '2026-11-01T00:00:00Z', estado
});

describe('App', () => {
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    http = TestBed.inject(HttpTestingController);
  });

  function crear() {
    const fixture = TestBed.createComponent(App);
    http.expectOne(`${API_URL}/facturas`).flush([
      factura('1', 'a', 'primerrecordatorio', 100),
      factura('2', 'a', 'desactivado', 50),
      factura('3', 'b', 'segundorecordatorio', 200),
    ]);
    fixture.detectChanges();
    return fixture;
  }

  it('agrupa las facturas por cliente y suma el total', () => {
    const app = crear().componentInstance;
    expect(app.grupos().length).toBe(2);
    expect(app.grupos()[0].total).toBe(150);
  });

  it('filtra por estado', () => {
    const app = crear().componentInstance;
    app.filtro.set('segundorecordatorio');
    expect(app.grupos().length).toBe(1);
    expect(app.visibles()[0].id).toBe('3');
  });

  it('cuenta las facturas por estado', () => {
    const app = crear().componentInstance;
    expect(app.conteo()['primerrecordatorio']).toBe(1);
  });

  it('muestra un error si la API no responde', () => {
    const fixture = TestBed.createComponent(App);
    http.expectOne(`${API_URL}/facturas`).error(new ProgressEvent('error'));
    fixture.detectChanges();
    expect(fixture.componentInstance.mensaje()?.tipo).toBe('error');
  });

  it('busca por cliente o número de factura', () => {
    const app = crear().componentInstance;
    app.busqueda.set('cliente b');
    expect(app.visibles().map(f => f.id)).toEqual(['3']);
    app.busqueda.set('f-2');
    expect(app.visibles().map(f => f.id)).toEqual(['2']);
  });

  it('calcula el resumen: monto total y facturas en recordatorio', () => {
    const r = crear().componentInstance.resumen();
    expect(r.montoTotal).toBe(350);
    expect(r.enRecordatorio).toBe(2);
    expect(r.montoEnRecordatorio).toBe(300);
    expect(r.desactivadas).toBe(1);
  });

  it('cuenta cuántas facturas por enviar tiene cada cliente', () => {
    const grupos = crear().componentInstance.grupos();
    expect(grupos.find(g => g.clienteId === 'a')?.porEnviar).toBe(1);
  });
});