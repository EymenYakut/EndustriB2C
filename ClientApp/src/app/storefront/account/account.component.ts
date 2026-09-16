import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { OrderDetailDto, OrderListDto, ORDER_STATUS, UserAddressDto } from '../../core/models/models';

@Component({
  selector: 'app-account',
  templateUrl: './account.component.html',
  styleUrls: ['./account.component.css']
})
export class AccountComponent implements OnInit, OnDestroy {
  orders: OrderListDto[] = [];
  addresses: UserAddressDto[] = [];
  statuses = ORDER_STATUS;
  addr: Partial<UserAddressDto> = this.emptyAddr();
  detail?: OrderDetailDto;
  detailLoading = false;
  error = '';
  private pendingOrderNo = '';

  constructor(
    public auth: AuthService,
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.pendingOrderNo = this.route.snapshot.queryParamMap.get('siparis') || '';
    this.api.myOrders().subscribe(o => {
      this.orders = o;
      if (this.pendingOrderNo) {
        const match = o.find(x => x.orderNumber === this.pendingOrderNo);
        if (match) {
          this.openOrder(match);
        }
      }
    });
    this.reloadAddresses();
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.detail || this.detailLoading) {
      this.closeOrder();
    }
  }

  statusClass(status: number): string {
    return `st-${status}`;
  }

  openOrder(order: OrderListDto): void {
    this.detailLoading = true;
    this.error = '';
    document.body.style.overflow = 'hidden';
    this.api.myOrder(order.id).subscribe({
      next: d => {
        this.detail = d;
        this.detailLoading = false;
      },
      error: () => {
        this.detailLoading = false;
        document.body.style.overflow = '';
        this.error = 'Sipariş detayı yüklenemedi.';
      }
    });
  }

  closeOrder(): void {
    this.detail = undefined;
    this.detailLoading = false;
    document.body.style.overflow = '';
    if (this.pendingOrderNo) {
      this.pendingOrderNo = '';
      this.router.navigate([], { relativeTo: this.route, queryParams: { siparis: null }, queryParamsHandling: 'merge' });
    }
  }

  reloadAddresses(): void {
    this.api.addresses().subscribe(a => this.addresses = a);
  }

  saveAddr(): void {
    this.api.saveAddress(this.addr).subscribe(() => {
      this.reloadAddresses();
      this.addr = this.emptyAddr();
    });
  }

  removeAddr(id: number): void {
    this.api.deleteAddress(id).subscribe(() => this.reloadAddresses());
  }

  private emptyAddr(): Partial<UserAddressDto> {
    return { title: 'Ev', fullName: '', phone: '', city: '', district: '', addressLine: '', postalCode: '', isDefault: false };
  }
}
