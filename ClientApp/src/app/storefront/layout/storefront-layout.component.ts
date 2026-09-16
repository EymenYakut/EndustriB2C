import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CategoryDto } from '../../core/models/models';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-storefront-layout',
  templateUrl: './storefront-layout.component.html'
})
export class StorefrontLayoutComponent implements OnInit {
  menuOpen = false;
  footerCategories: CategoryDto[] = [];

  constructor(public api: ApiService, private router: Router) {
    this.router.events.pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(() => this.menuOpen = false);
  }

  ngOnInit(): void {
    this.api.loadCart();
    this.api.categories().subscribe(c => this.footerCategories = c);
  }
}
