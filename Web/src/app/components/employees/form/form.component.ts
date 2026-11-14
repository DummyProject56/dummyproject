import {
  FormGroup,
  Validators,
  FormBuilder,
  ReactiveFormsModule,
} from '@angular/forms';
import { EmployeeService } from '../../../services/employees/employee.service';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

@Component({
  selector: 'app-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './form.component.html',
  styleUrls: ['./form.component.scss'],
})
export class FormComponent {
  employeeId: number | null = null;
  employeeForm: FormGroup = this.fb.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    phone: ['1234567890', Validators.required],
    department: ['', Validators.required],
    designation: ['', Validators.required],
    joiningDate: ['', Validators.required],
    status: ['', Validators.required],
  });
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private route: ActivatedRoute,
    private router: Router,
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (id) {
      this.employeeId = Number(id);
      this.employeeService.getEmployee(this.employeeId).subscribe({
        next: (employee) => {
          this.employeeForm.patchValue({
            ...employee,
            joiningDate: employee.joiningDate.substring(0, 10),
          });
        },
        error: () => {
          this.errorMessage =
            'Unable to load employee data. Please try again later.';
        },
      });
    }
  }

  save(): void {
    if (this.employeeForm.invalid) {
      this.employeeForm.markAllAsTouched();
      return;
    }

    const request = this.employeeId
      ? this.employeeService.updateEmployee(
          this.employeeId,
          this.employeeForm.getRawValue(),
        )
      : this.employeeService.createEmployee(this.employeeForm.getRawValue());

    request.subscribe({
      next: () => {
        this.router.navigate(['/employees']);
      },
      error: (err) => {
        this.errorMessage =
          err.error?.message ||
          'Unable to save employee data. Please try again later.';
      },
    });
  }
}
