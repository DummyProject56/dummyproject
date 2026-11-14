import { EmployeeService } from '../../../services/employees/employee.service';
import { EmployeeFilter } from '../../../interfaces/employees/employee-filter';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Employee } from '../../../interfaces/employees/employee';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import * as dayjs from 'dayjs';

@Component({
  selector: 'app-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
})
export class ListComponent {
  employees: Employee[] = [];
  loading = false;
  errorMessage = '';
  total = 0;
  page = 1;
  pageSize = 10;
  filterForm!: FormGroup;

  constructor(
    private employeeService: EmployeeService,
    private fb: FormBuilder,
    private router: Router,
  ) {
    this.filterForm = this.fb.group({
      search: [''],
      department: [''],
      status: [''],
      fromDate: [''],
      toDate: [''],
    });
  }

  ngOnInit(): void {
    this.loadEmployees();
  }

  get totalPages(): number {
    return Math.ceil(this.total / this.pageSize);
  }

  get pageStart(): number {
    return this.total === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get pageEnd(): number {
    return Math.min(this.page * this.pageSize, this.total);
  }

  loadEmployees(): void {
    this.loading = true;
    this.errorMessage = '';
    const formValue = this.filterForm.getRawValue();
    const filters: EmployeeFilter = {
      search: formValue.search?.trim() || '',
      department: formValue.department?.trim() || '',
      status: formValue.status || '',
      fromDate: formValue.fromDate || '',
      toDate: formValue.toDate || '',
      page: this.page,
      pageSize: this.pageSize,
    };
    this.employeeService.getAllEmployees(filters).subscribe({
      next: (response) => {
        this.employees = response.employees.map((employee) => {
          employee.joiningDate = dayjs(employee.joiningDate).format(
            'DD-MM-YYYY',
          );
          return employee;
        });
        this.total = response.total;
        this.loading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load employees.';
        this.loading = false;
      },
    });
  }

  applyFilters(): void {
    this.page = 1; // Reset to first page when filters change
    this.loadEmployees();
  }

  clearFilters(): void {
    this.filterForm.reset({
      search: '',
      department: '',
      status: '',
      fromDate: '',
      toDate: '',
    });
    this.page = 1;
    this.loadEmployees();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page = page;
    this.loadEmployees();
  }

  view(id: number): void {
    this.router.navigate(['/employees', id]);
  }

  edit(id: number): void {
    this.router.navigate(['/employees', id, 'edit']);
  }
}
