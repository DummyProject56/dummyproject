import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Employee } from '../../interfaces/employees/employee';
import { EmployeeResponse } from '../../interfaces/employees/employee-response';
import { environment } from '../../../environments/environment';
import { EmployeeFilter } from '../../interfaces/employees/employee-filter';
import { ImportResult } from '../../interfaces/import/import-result';

export type EmployeePayload = Omit<Employee, 'id'>;

@Injectable({
  providedIn: 'root',
})
export class EmployeeService {
  path = '/employee';

  constructor(private http: HttpClient) {}

  getAllEmployees(filters: EmployeeFilter): Observable<EmployeeResponse> {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filters)) {
      if (value !== null && value !== undefined && value !== '') {
        params = params.set(key, value.toString());
      }
    }
    return this.http.get<EmployeeResponse>(environment.url + this.path, {
      params,
    });
  }

  getEmployee(id: number): Observable<Employee> {
    return this.http.get<Employee>(environment.url + this.path + `/${id}`);
  }

  createEmployee(payload: EmployeePayload): Observable<Employee> {
    return this.http.post<Employee>(environment.url + this.path, payload);
  }

  updateEmployee(id: number, payload: EmployeePayload): Observable<Employee> {
    return this.http.put<Employee>(
      environment.url + this.path + `/${id}`,
      payload,
    );
  }

  importFile(file: File): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post<ImportResult>(
      environment.url + this.path + '/import',
      formData,
    );
  }

  uploadDocuments(employeeId: number, files: File[]) {
    const formData = new FormData();
    files.forEach((file) => {
      formData.append('files', file);
    });

    return this.http.post(
      environment.url + this.path + `/${employeeId}/documents`,
      formData,
    );
  }

  downloadDocument(employeeId: number, documentId: number): Observable<Blob> {
    return this.http.get(
      environment.url + this.path + `/${employeeId}/documents/${documentId}`,
      { responseType: 'blob' },
    );
  }

  downloadZip(employeeId: number, documentIds: number[]): Observable<Blob> {
    return this.http.post(
      environment.url + this.path + `/${employeeId}/documents/download-zip`,
      { documentIds },
      { responseType: 'blob' },
    );
  }
}
