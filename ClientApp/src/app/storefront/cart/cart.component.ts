import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { CartDto, CartItemDto } from '../../core/models/models';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css']
})
export class CartComponent implements OnInit {
  cart?: CartDto;
  busyId?: number;
  error = '';

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getCart().subscribe(c => this.cart = c);
  }

  adjust(item: CartItemDto, delta: number): void {
    this.setQty(item, item.quantity + delta);
  }

  setQty(item: CartItemDto, qty: number): void {
    const next = Math.max(1, Math.min(item.stock, Math.floor(qty) || 1));
    if (next === item.quantity || this.busyId) {
      return;
    }
    this.busyId = item.id;
    this.error = '';
    this.api.updateCartItem(item.id, next).subscribe({
      next: c => {
        this.cart = c;
        this.busyId = undefined;
      },
      error: e => {
        this.busyId = undefined;
        this.error = e.error?.message || 'Adet güncellenemedi.';
      }
    });
  }

  askInfo(): void {
    if (!this.cart?.items?.length) {
      return;
    }
    const lines = this.cart.items.map(i => `- ${i.quantity} x ${i.name} (${i.sku})`);
    const message = [
      'Merhaba, sepetimdeki ürünler hakkında bilgi almak istiyorum.',
      '',
      'Ürünler:',
      ...lines
    ].join('\n');
    window.open(`https://wa.me/905534380409?text=${encodeURIComponent(message)}`, '_blank', 'noopener');
  }

  remove(id: number): void {
    if (this.busyId) {
      return;
    }
    this.busyId = id;
    this.error = '';
    this.api.removeCartItem(id).subscribe({
      next: c => {
        this.cart = c;
        this.busyId = undefined;
      },
      error: e => {
        this.busyId = undefined;
        this.error = e.error?.message || 'Ürün kaldırılamadı.';
      }
    });
  }
}
