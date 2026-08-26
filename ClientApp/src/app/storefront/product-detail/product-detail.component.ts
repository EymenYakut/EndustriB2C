import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { ProductDetailDto } from '../../core/models/models';

@Component({
  selector: 'app-product-detail',
  templateUrl: './product-detail.component.html'
})
export class ProductDetailComponent implements OnInit {
  product?: ProductDetailDto;
  qty = 1;
  message = '';
  error = '';

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(p => {
      const slug = p.get('slug') || '';
      this.api.productBySlug(slug).subscribe({
        next: r => this.product = r,
        error: () => this.router.navigate(['/urunler'])
      });
    });
  }

  add(): void {
    if (!this.product) return;
    this.error = '';
    this.api.addToCart(this.product.id, this.qty).subscribe({
      next: () => this.message = 'Ürün sepete eklendi. Üye olmadan da sipariş verebilirsiniz.',
      error: e => this.error = e.error?.message || 'Sepete eklenemedi.'
    });
  }

  get unitPrice(): number {
    return this.product?.discountedPrice || this.product?.price || 0;
  }
}
