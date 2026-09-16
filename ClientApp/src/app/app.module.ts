import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { AuthService } from './core/services/auth.service';
import { StorefrontLayoutComponent } from './storefront/layout/storefront-layout.component';
import { HomeComponent } from './storefront/home/home.component';
import { ProductListComponent } from './storefront/products/product-list.component';
import { ProductDetailComponent } from './storefront/product-detail/product-detail.component';
import { CartComponent } from './storefront/cart/cart.component';
import { AboutComponent } from './storefront/about/about.component';
import { ContactComponent } from './storefront/contact/contact.component';

export function initAuth(auth: AuthService) {
  return () => auth.init();
}

@NgModule({
  declarations: [
    AppComponent,
    StorefrontLayoutComponent,
    HomeComponent,
    ProductListComponent,
    ProductDetailComponent,
    CartComponent,
    AboutComponent,
    ContactComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'admin', loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule) },
      {
        path: '',
        component: StorefrontLayoutComponent,
        children: [
          { path: '', component: HomeComponent, pathMatch: 'full' },
          { path: 'urunler', component: ProductListComponent },
          { path: 'urunler/:slug', component: ProductDetailComponent },
          { path: 'sepet', component: CartComponent },
          { path: 'odeme', redirectTo: 'sepet', pathMatch: 'full' },
          { path: 'giris', redirectTo: '', pathMatch: 'full' },
          { path: 'kayit', redirectTo: '', pathMatch: 'full' },
          { path: 'hesabim', redirectTo: '', pathMatch: 'full' },
          { path: 'hakkimizda', component: AboutComponent },
          { path: 'iletisim', component: ContactComponent }
        ]
      }
    ])
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    { provide: APP_INITIALIZER, useFactory: initAuth, deps: [AuthService], multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
