import { Employee } from './employee';

export interface EmployeeResponse {
  total: number;
  page: number;
  pagesize: number;
  employees: Employee[];
}
