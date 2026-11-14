import { ImportError } from './import-error';

export interface ImportResult {
  totalRecords: number;
  successfulRecords: number;
  failedRecords: number;
  errors: ImportError[];
}
