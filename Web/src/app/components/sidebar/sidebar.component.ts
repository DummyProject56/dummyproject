import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Component } from '@angular/core';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent {
  menuItems = [
    { label: 'Employees', path: '/employees', icon: '👥' },
    { label: 'Add Employee', path: '/employees/create', icon: '➕' },
    { label: 'Bulk Import', path: '/employees/import', icon: '📤' },
  ];
}
