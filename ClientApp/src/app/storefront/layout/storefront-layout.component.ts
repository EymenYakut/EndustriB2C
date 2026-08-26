import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { CategoryDto } from '../../core/models/models';

@Component({
  selector: 'app-storefront-layout',
  templateUrl: './storefront-layout.component.html'
})
export class StorefrontLayoutComponent implements OnInit {
  menuOpen = false;
  footerCategories: CategoryDto[] = [];

  constructor(public auth: AuthService, public api: ApiService) {}

  ngOnInit(): void {
    this.api.loadCart();
    this.api.categories().subscribe(c => this.footerCategories = c);
  }

  logout(): void {
    this.auth.logout();
  }
}
