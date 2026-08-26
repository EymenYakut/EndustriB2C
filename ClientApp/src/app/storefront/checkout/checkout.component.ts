import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { CartDto, UserAddressDto } from '../../core/models/models';

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html'
})
export class CheckoutComponent implements OnInit {
  cart?: CartDto;
  addresses: UserAddressDto[] = [];
  error = '';
  model: any = {
    customerName: '',
    customerEmail: '',
    customerPhone: '',
    shippingFullName: '',
    shippingPhone: '',
    shippingCity: '',
    shippingDistrict: '',
    shippingAddressLine: '',
    shippingPostalCode: '',
    couponCode: '',
    notes: ''
  };

  constructor(private api: ApiService, public auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.api.getCart().subscribe(c => this.cart = c);
    if (this.auth.user) {
      this.model.customerName = this.auth.user.fullName;
      this.model.customerEmail = this.auth.user.email;
      this.model.customerPhone = this.auth.user.phone;
      this.api.addresses().subscribe(a => {
        this.addresses = a;
        const d = a.find(x => x.isDefault) || a[0];
        if (d) this.applyAddress(d);
      });
    }
  }

  applyAddress(a: UserAddressDto): void {
    this.model.shippingFullName = a.fullName;
    this.model.shippingPhone = a.phone;
    this.model.shippingCity = a.city;
    this.model.shippingDistrict = a.district;
    this.model.shippingAddressLine = a.addressLine;
    this.model.shippingPostalCode = a.postalCode;
  }

  submit(): void {
    this.error = '';
    this.api.checkout(this.model).subscribe({
      next: order => this.router.navigate([this.auth.isLoggedIn ? '/hesabim' : '/'], { queryParams: { siparis: order.orderNumber } }),
      error: e => this.error = e.error?.message || 'Sipariş oluşturulamadı.'
    });
  }
}
