import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { CartDto, ProductDetailDto, ProductListDto, CampaignDto, OrderDetailDto, OrderListDto, UserAddressDto, UserDto, CategoryDto, PagedResult, SliderDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private cartSubject = new BehaviorSubject<CartDto | null>(null);
  cart$ = this.cartSubject.asObservable();

  constructor(private http: HttpClient) {}

  get cartCount(): number {
    return this.cartSubject.value?.itemCount ?? 0;
  }

  get cartLineCount(): number {
    return this.cartSubject.value?.items?.length ?? 0;
  }

  loadCart(): void {
    this.getCart().subscribe();
  }

  products(category?: string, q?: string, page = 1, pageSize = 12): Observable<PagedResult<ProductListDto>> {
    const params: Record<string, string> = {
      page: String(page),
      pageSize: String(pageSize)
    };
    if (category) params['category'] = category;
    if (q) params['q'] = q;
    return this.http.get<PagedResult<ProductListDto>>('/api/products', { params });
  }

  product(id: number): Observable<ProductDetailDto> {
    return this.http.get<ProductDetailDto>(`/api/products/${id}`);
  }

  productBySlug(slug: string): Observable<ProductDetailDto> {
    return this.http.get<ProductDetailDto>(`/api/products/slug/${slug}`);
  }

  campaigns(): Observable<CampaignDto[]> {
    return this.http.get<CampaignDto[]>('/api/campaigns');
  }

  sliders(): Observable<SliderDto[]> {
    return this.http.get<SliderDto[]>('/api/sliders');
  }

  categories(): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>('/api/products/categories');
  }

  getCart(): Observable<CartDto> {
    return this.http.get<CartDto>('/api/cart').pipe(tap(c => this.cartSubject.next(c)));
  }

  addToCart(productId: number, quantity = 1): Observable<CartDto> {
    return this.http.post<CartDto>('/api/cart/items', { productId, quantity }).pipe(tap(c => this.cartSubject.next(c)));
  }

  updateCartItem(id: number, quantity: number): Observable<CartDto> {
    return this.http.put<CartDto>(`/api/cart/items/${id}`, { quantity }).pipe(tap(c => this.cartSubject.next(c)));
  }

  removeCartItem(id: number): Observable<CartDto> {
    return this.http.delete<CartDto>(`/api/cart/items/${id}`).pipe(tap(c => this.cartSubject.next(c)));
  }

  checkout(payload: any): Observable<OrderDetailDto> {
    return this.http.post<OrderDetailDto>('/api/orders/checkout', payload).pipe(
      tap(() => this.getCart().subscribe())
    );
  }

  myOrders(): Observable<OrderListDto[]> {
    return this.http.get<OrderListDto[]>('/api/orders/mine');
  }

  myOrder(id: number): Observable<OrderDetailDto> {
    return this.http.get<OrderDetailDto>(`/api/orders/mine/${id}`);
  }

  account(): Observable<UserDto> {
    return this.http.get<UserDto>('/api/account');
  }

  addresses(): Observable<UserAddressDto[]> {
    return this.http.get<UserAddressDto[]>('/api/account/addresses');
  }

  saveAddress(body: Partial<UserAddressDto>, id?: number): Observable<UserAddressDto> {
    return id
      ? this.http.put<UserAddressDto>(`/api/account/addresses/${id}`, body)
      : this.http.post<UserAddressDto>('/api/account/addresses', body);
  }

  deleteAddress(id: number): Observable<void> {
    return this.http.delete<void>(`/api/account/addresses/${id}`);
  }
}
