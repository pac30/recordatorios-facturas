import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export type Estado = 'primerrecordatorio' | 'segundorecordatorio' | 'desactivado' | string;

export interface FacturaResumen {
  id: string;
  numero: string;
  clienteId: string;
  cliente: string;
  email: string;
  monto: number;
  fechaVencimiento: string;
  estado: Estado;
}

export interface FacturaProcesada {
  facturaId: string;
  numero: string;
  estadoAnterior: string;
  estadoNuevo: string | null;
  exito: boolean;
  error: string | null;
}

export interface ResultadoCliente {
  clienteId: string;
  cliente: string;
  email: string;
  facturas: FacturaProcesada[];
}

export const API_URL = 'http://localhost:5236/api';

@Injectable({ providedIn: 'root' })
export class FacturasService {
  private readonly http = inject(HttpClient);

  obtenerResumen(): Observable<FacturaResumen[]> {
    return this.http.get<FacturaResumen[]>(`${API_URL}/facturas`);
  }

  procesarTodos(): Observable<ResultadoCliente[]> {
    return this.http.post<ResultadoCliente[]>(`${API_URL}/recordatorios/procesar`, {});
  }

  procesarCliente(clienteId: string): Observable<ResultadoCliente> {
    return this.http.post<ResultadoCliente>(`${API_URL}/recordatorios/procesar/${clienteId}`, {});
  }

  reiniciarDemo(): Observable<{ clientes: number; facturas: number }> {
    return this.http.post<{ clientes: number; facturas: number }>(`${API_URL}/demo/reiniciar`, {});
  }
}