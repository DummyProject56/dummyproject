import { NgModule } from '@angular/core';
import { provideRouter, RouterModule, Routes } from '@angular/router';
import { ListComponent } from './components/employees/list/list.component';
import { FormComponent } from './components/employees/form/form.component';
import { ImportComponent } from './components/employees/import/import.component';
import { DetailComponent } from './components/employees/detail/detail.component';

const routes: Routes = [
  { path: '', redirectTo: 'employees', pathMatch: 'full' },
  { path: 'employees', component: ListComponent },
  { path: 'employees/create', component: FormComponent },
  { path: 'employees/import', component: ImportComponent },
  { path: 'employees/:id/edit', component: FormComponent },
  { path: 'employees/:id', component: DetailComponent },
  { path: '**', redirectTo: 'employees' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
