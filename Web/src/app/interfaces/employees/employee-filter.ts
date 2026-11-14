export interface EmployeeFilter {
  search?: string;
  department?: string;
  status?: string;
  fromDate?: string;
  toDate?: string;
  page: number;
  pageSize: number;
}
