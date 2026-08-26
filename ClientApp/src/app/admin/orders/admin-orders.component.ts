import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { OrderDetailDto, OrderListDto, ORDER_STATUS } from '../../core/models/models';

@Component({
  selector: 'app-admin-orders',
  templateUrl: './admin-orders.component.html'
})
export class AdminOrdersComponent implements OnInit {
  orders: OrderListDto[] = [];
  selected?: OrderDetailDto;
  statuses = ORDER_STATUS;
  statusKeys = [0, 1, 2, 3, 4, 5];

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.reload();
  }

  reload(): void {
    this.admin.orders().subscribe(o => this.orders = o);
  }

  open(id: number): void {
    this.admin.order(id).subscribe(o => this.selected = o);
  }

  setStatus(status: number): void {
    if (!this.selected) return;
    this.admin.updateOrderStatus(this.selected.id, status).subscribe(o => {
      this.selected = o;
      this.reload();
    });
  }
}
