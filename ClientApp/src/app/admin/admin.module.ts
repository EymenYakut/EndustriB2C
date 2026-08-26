import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminGuard } from '../core/guards/auth.guard';
import { AdminLayoutComponent } from './layout/admin-layout.component';
import { AdminLoginComponent } from './login/admin-login.component';
import { AdminDashboardComponent } from './dashboard/admin-dashboard.component';
import { AdminProductsComponent } from './products/admin-products.component';
import { AdminOrdersComponent } from './orders/admin-orders.component';
import { AdminCustomersComponent } from './customers/admin-customers.component';
import { AdminCampaignsComponent } from './campaigns/admin-campaigns.component';
import { AdminFeaturesComponent } from './features/admin-features.component';
import { AdminUsersComponent } from './users/admin-users.component';
import { AdminCategoriesComponent } from './categories/admin-categories.component';

const routes: Routes = [
  { path: 'login', component: AdminLoginComponent },
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [AdminGuard],
    children: [
      { path: '', component: AdminDashboardComponent },
      { path: 'urunler', component: AdminProductsComponent },
      { path: 'kategoriler', component: AdminCategoriesComponent },
      { path: 'siparisler', component: AdminOrdersComponent },
      { path: 'musteriler', component: AdminCustomersComponent },
      { path: 'kampanyalar', component: AdminCampaignsComponent },
      { path: 'ozellikler', component: AdminFeaturesComponent },
      { path: 'kullanicilar', component: AdminUsersComponent }
    ]
  }
];

@NgModule({
  declarations: [
    AdminLayoutComponent,
    AdminLoginComponent,
    AdminDashboardComponent,
    AdminProductsComponent,
    AdminOrdersComponent,
    AdminCustomersComponent,
    AdminCampaignsComponent,
    AdminFeaturesComponent,
    AdminUsersComponent,
    AdminCategoriesComponent
  ],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class AdminModule {}
