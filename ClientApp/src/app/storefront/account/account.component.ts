import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderListDto, ORDER_STATUS, UserAddressDto } from '../../core/models/models';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html'
})
export class AccountComponent implements OnInit {
  orders: OrderListDto[] = [];
  addresses: UserAddressDto[] = [];
  statuses = ORDER_STATUS;
  addr: any = { title: 'Ev', fullName: '', phone: '', city: '', district: '', addressLine: '', postalCode: '', isDefault: true };

  constructor(public auth: AuthService, private api: ApiService) {}

  ngOnInit(): void {
    this.api.myOrders().subscribe(o => this.orders = o);
    this.reloadAddresses();
  }

  reloadAddresses(): void {
    this.api.addresses().subscribe(a => this.addresses = a);
  }

  saveAddr(): void {
    this.api.saveAddress(this.addr).subscribe(() => {
      this.reloadAddresses();
      this.addr = { title: 'Ev', fullName: '', phone: '', city: '', district: '', addressLine: '', postalCode: '', isDefault: false };
    });
  }

  removeAddr(id: number): void {
    this.api.deleteAddress(id).subscribe(() => this.reloadAddresses());
  }
}
