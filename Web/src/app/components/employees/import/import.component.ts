import { EmployeeService } from '../../../services/employees/employee.service';
import { ImportResult } from '../../../interfaces/import/import-result';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-import',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './import.component.html',
  styleUrls: ['./import.component.scss'],
})
export class ImportComponent {
  selectedFile: File | null = null;
  result: ImportResult | null = null;
  errorMessage = '';
  loading = false;

  constructor(
    private employeeService: EmployeeService,
    private router: Router,
  ) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const selected = input.files?.[0] ?? null;

    this.errorMessage = '';
    this.result = null;
    this.selectedFile = null;

    if (!selected) return;

    const extension = selected.name.split('.').pop()?.toLowerCase();

    if (!['csv', 'xlsx'].includes(extension ?? '')) {
      this.errorMessage =
        'Invalid file type. Please select a CSV or XLSX file.';
      input.value = '';
      return;
    }

    if (selected.size === 0 || selected.size > 10 * 1024 * 1024) {
      this.errorMessage = 'File size must be between 1 byte and 10 MB.';
      input.value = '';
      return;
    }

    this.selectedFile = selected;
  }

  upload(): void {
    if (!this.selectedFile || this.loading) return;
    this.errorMessage = '';
    this.loading = true;
    this.employeeService.importFile(this.selectedFile).subscribe({
      next: (result) => {
        this.result = result;
        this.loading = false;

        if (result.failedRecords === 0) {
          this.goToEmployees();
        }
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Import failed';
      },
    });
  }

  goToEmployees(): void {
    this.router.navigate(['/employees'], {
      state: { importSummary: this.result },
    });
  }
}
