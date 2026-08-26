import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppComponent } from './app.component';
import { AuthInterceptor } from './core/interceptors/auth.interceptor';
import { AuthGuard } from './core/guards/auth.guard';
import { AuthService } from './core/services/auth.service';
import { StorefrontLayoutComponent } from './storefront/layout/storefront-layout.component';
import { HomeComponent } from './storefront/home/home.component';
import { ProductListComponent } from './storefront/products/product-list.component';
import { ProductDetailComponent } from './storefront/product-detail/product-detail.component';
import { CartComponent } from './storefront/cart/cart.component';
import { CheckoutComponent } from './storefront/checkout/checkout.component';
import { LoginComponent } from './storefront/auth/login.component';
import { RegisterComponent } from './storefront/auth/register.component';
import { AccountComponent } from './storefront/account/account.component';
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
    CheckoutComponent,
    LoginComponent,
    RegisterComponent,
    AccountComponent,
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
          { path: 'odeme', component: CheckoutComponent },
          { path: 'giris', component: LoginComponent },
          { path: 'kayit', component: RegisterComponent },
          { path: 'hesabim', component: AccountComponent, canActivate: [AuthGuard] },
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
