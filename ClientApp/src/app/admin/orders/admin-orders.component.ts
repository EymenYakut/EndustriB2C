import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { OrderDetailDto, OrderListDto, ORDER_STATUS, ProductDetailDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.component.html'
})
export class AdminOrdersComponent implements OnInit {
  orders: OrderListDto[] = [];
  products: ProductDetailDto[] = [];
  selected?: OrderDetailDto;
  creating = false;
  saving = false;
  sameAsCustomer = true;
  error = '';
  statuses = ORDER_STATUS;
  statusKeys = [0, 1, 2, 3, 4, 5];
  form: any = null;

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.reload();
    this.admin.products().subscribe(p => this.products = (p || []).filter(x => x.isActive));
  }

  reload(): void {
    this.admin.orders().subscribe(o => this.orders = o);
  }

  open(id: number): void {
    this.creating = false;
    this.admin.order(id).subscribe(o => this.selected = o);
  }

  startNew(): void {
    this.selected = undefined;
    this.error = '';
    this.sameAsCustomer = true;
    this.form = {
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
      notes: '',
      lines: [{ productId: null, quantity: 1 }]
    };
    this.creating = true;
  }

  addLine(): void {
    this.form.lines.push({ productId: null, quantity: 1 });
  }

  removeLine(i: number): void {
    if (this.form.lines.length === 1) return;
    this.form.lines.splice(i, 1);
  }

  productLabel(p: ProductDetailDto): string {
    return `${p.sku} — ${p.name} (stok ${p.stock})`;
  }

  setStatus(status: number): void {
    if (!this.selected) return;
    this.admin.updateOrderStatus(this.selected.id, status).subscribe(o => {
      this.selected = o;
      this.reload();
    });
  }

  save(): void {
    this.error = '';
    const lines = (this.form.lines || [])
      .filter((l: any) => l.productId)
      .map((l: any) => ({ productId: +l.productId, quantity: Math.max(1, +l.quantity || 1) }));
    if (!lines.length) {
      this.error = 'En az bir ürün seçin.';
      return;
    }
    const payload = {
      ...this.form,
      shippingFullName: this.sameAsCustomer ? this.form.customerName : this.form.shippingFullName,
      shippingPhone: this.sameAsCustomer ? this.form.customerPhone : this.form.shippingPhone,
      lines
    };
    this.saving = true;
    this.admin.createOrder(payload).subscribe({
      next: order => {
        this.saving = false;
        this.creating = false;
        this.form = null;
        this.reload();
        this.selected = order;
      },
      error: e => {
        this.saving = false;
        this.error = e.error?.message || 'Sipariş oluşturulamadı.';
      }
    });
  }
}
