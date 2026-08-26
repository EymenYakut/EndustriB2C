import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CategoryDto, ProductListDto } from '../../core/models/models';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html'
})
export class ProductListComponent implements OnInit {
  products: ProductListDto[] = [];
  categories: CategoryDto[] = [];
  category = '';
  q = '';

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.api.categories().subscribe(c => this.categories = c);
    this.route.queryParamMap.subscribe(p => {
      this.category = p.get('category') || '';
      this.q = p.get('q') || '';
      this.load();
    });
  }

  load(): void {
    this.api.products(this.category || undefined, this.q || undefined).subscribe(r => this.products = r);
  }
}
