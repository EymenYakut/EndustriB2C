import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CampaignDto, CategoryDto, DashboardDto, FeatureHeaderDto, OrderDetailDto, OrderListDto, ProductDetailDto, UserDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AdminApiService {
  constructor(private http: HttpClient) {}

  dashboard(): Observable<DashboardDto> {
    return this.http.get<DashboardDto>('/api/admin/users/dashboard');
  }

  users(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>('/api/admin/users');
  }

  customers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>('/api/admin/customers');
  }

  customer(id: number): Observable<any> {
    return this.http.get(`/api/admin/customers/${id}`);
  }

  updateCustomer(id: number, body: Partial<UserDto>): Observable<UserDto> {
    return this.http.put<UserDto>(`/api/admin/customers/${id}`, body);
  }

  products(): Observable<ProductDetailDto[]> {
    return this.http.get<ProductDetailDto[]>('/api/admin/products');
  }

  product(id: number): Observable<ProductDetailDto> {
    return this.http.get<ProductDetailDto>(`/api/admin/products/${id}`);
  }

  saveProduct(body: any, id?: number): Observable<ProductDetailDto> {
    return id
      ? this.http.put<ProductDetailDto>(`/api/admin/products/${id}`, body)
      : this.http.post<ProductDetailDto>('/api/admin/products', body);
  }

  deleteProduct(id: number): Observable<any> {
    return this.http.delete(`/api/admin/products/${id}`);
  }

  uploadImage(productId: number, file: File, isPrimary: boolean): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    form.append('isPrimary', String(isPrimary));
    return this.http.post(`/api/admin/products/${productId}/images`, form);
  }

  deleteImage(productId: number, imageId: number): Observable<void> {
    return this.http.delete<void>(`/api/admin/products/${productId}/images/${imageId}`);
  }

  orders(): Observable<OrderListDto[]> {
    return this.http.get<OrderListDto[]>('/api/admin/orders');
  }

  order(id: number): Observable<OrderDetailDto> {
    return this.http.get<OrderDetailDto>(`/api/admin/orders/${id}`);
  }

  updateOrderStatus(id: number, status: number): Observable<OrderDetailDto> {
    return this.http.put<OrderDetailDto>(`/api/admin/orders/${id}/status`, { status });
  }

  campaigns(): Observable<CampaignDto[]> {
    return this.http.get<CampaignDto[]>('/api/admin/campaigns');
  }

  saveCampaign(body: any, id?: number): Observable<CampaignDto> {
    return id
      ? this.http.put<CampaignDto>(`/api/admin/campaigns/${id}`, body)
      : this.http.post<CampaignDto>('/api/admin/campaigns', body);
  }

  deleteCampaign(id: number): Observable<void> {
    return this.http.delete<void>(`/api/admin/campaigns/${id}`);
  }

  features(): Observable<FeatureHeaderDto[]> {
    return this.http.get<FeatureHeaderDto[]>('/api/admin/features');
  }

  saveFeature(body: any, id?: number): Observable<FeatureHeaderDto> {
    return id
      ? this.http.put<FeatureHeaderDto>(`/api/admin/features/${id}`, body)
      : this.http.post<FeatureHeaderDto>('/api/admin/features', body);
  }

  deleteFeature(id: number): Observable<void> {
    return this.http.delete<void>(`/api/admin/features/${id}`);
  }

  categories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/admin/categories');
  }

  saveCategory(body: any, id?: number): Observable<CategoryDto> {
    return id
      ? this.http.put<CategoryDto>(`/api/admin/categories/${id}`, body)
      : this.http.post<CategoryDto>('/api/admin/categories', body);
  }

  deleteCategory(id: number): Observable<void> {
    return this.http.delete<void>(`/api/admin/categories/${id}`);
  }

  uploadCategoryImage(id: number, file: File): Observable<CategoryDto> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<CategoryDto>(`/api/admin/categories/${id}/image`, form);
  }
}
