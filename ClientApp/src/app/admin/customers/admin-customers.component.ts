import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { ORDER_STATUS, UserDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-customers',
  templateUrl: './admin-customers.component.html'
})
export class AdminCustomersComponent implements OnInit {
  customers: UserDto[] = [];
  detail: any;
  statuses = ORDER_STATUS;

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.admin.customers().subscribe(c => this.customers = c);
  }

  open(id: number): void {
    this.admin.customer(id).subscribe(d => this.detail = d);
  }

  toggle(user: UserDto): void {
    this.admin.updateCustomer(user.id, {
      firstName: user.firstName,
      lastName: user.lastName,
      phone: user.phone,
      userType: user.userType,
      isActive: !user.isActive
    } as any).subscribe(() => this.admin.customers().subscribe(c => this.customers = c));
  }
}
