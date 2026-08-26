import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CampaignDto, CategoryDto, ProductListDto } from '../../core/models/models';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html'
})
export class HomeComponent implements OnInit {
  categories: CategoryDto[] = [];
  products: ProductListDto[] = [];
  campaigns: CampaignDto[] = [];
  orderNumber = '';

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.orderNumber = this.route.snapshot.queryParamMap.get('siparis') || '';
    this.api.products().subscribe(p => this.products = p.slice(0, 8));
    this.api.campaigns().subscribe(c => this.campaigns = c);
    this.api.categories().subscribe(c => this.categories = c);
  }
}
