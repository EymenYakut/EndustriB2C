import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { CartDto } from '../../core/models/models';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html'
})
export class CartComponent implements OnInit {
  cart?: CartDto;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getCart().subscribe(c => this.cart = c);
  }

  change(id: number, qty: number): void {
    this.api.updateCartItem(id, qty).subscribe(c => this.cart = c);
  }

  remove(id: number): void {
    this.api.removeCartItem(id).subscribe(c => this.cart = c);
  }
}
