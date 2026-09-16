import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-product-detail',
  template: '<p class="hint container-xl">Ürün detayı açılıyor…</p>'
})
export class ProductDetailComponent implements OnInit {
  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    this.router.navigate(['/urunler'], {
      queryParams: slug ? { urun: slug } : {},
      replaceUrl: true
    });
  }
}
