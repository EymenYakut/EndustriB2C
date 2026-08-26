export interface UserDto {
  id: number;
  email: string;
  phone: string;
  firstName: string;
  lastName: string;
  fullName: string;
  userType: number;
  isActive: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

export interface AuthResponse {
  accessToken: string;
  expiresIn: number;
  user: UserDto;
}

export interface UserAddressDto {
  id: number;
  title: string;
  fullName: string;
  phone: string;
  city: string;
  district: string;
  addressLine: string;
  postalCode?: string;
  isDefault: boolean;
}

export interface ProductListDto {
  id: number;
  sku: string;
  category: string;
  manufacturer: string;
  categories: CategoryDto[];
  categoryIds: number[];
  stock: number;
  isActive: boolean;
  name: string;
  shortDescription?: string;
  slug: string;
  price: number;
  discountedPrice?: number;
  currency: string;
  imageUrl?: string;
  campaignName?: string;
}

export interface ProductDetailDto extends ProductListDto {
  description?: string;
  images: ProductImageDto[];
  features: ProductFeatureValueDto[];
  prices: ProductPriceDto[];
  translations: ProductTranslationDto[];
}

export interface ProductImageDto {
  id: number;
  filePath: string;
  altText?: string;
  sortOrder: number;
  isPrimary: boolean;
}

export interface ProductFeatureValueDto {
  id: number;
  headerId: number;
  headerName: string;
  unit?: string;
  value: string;
}

export interface ProductPriceDto {
  id: number;
  amount: number;
  currency: string;
  campaignId?: number;
  campaignName?: string;
  validFrom?: string;
  validTo?: string;
  isCurrent: boolean;
}

export interface ProductTranslationDto {
  id?: number;
  languageCode: string;
  name: string;
  shortDescription?: string;
  description?: string;
  slug: string;
}

export interface FeatureHeaderDto {
  id: number;
  name: string;
  unit?: string;
  sortOrder: number;
}

export interface CategoryDto {
  id: number;
  name: string;
  slug: string;
  description?: string;
  imageUrl?: string;
  sortOrder: number;
  isActive: boolean;
  productCount: number;
}

export interface CartDto {
  id: number;
  isGuest: boolean;
  itemCount: number;
  subTotal: number;
  discountAmount: number;
  total: number;
  items: CartItemDto[];
}

export interface CartItemDto {
  id: number;
  productId: number;
  name: string;
  sku: string;
  imageUrl?: string;
  quantity: number;
  stock: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderListDto {
  id: number;
  orderNumber: string;
  customerName: string;
  customerEmail: string;
  total: number;
  status: number;
  createdAt: string;
  itemCount: number;
}

export interface OrderDetailDto extends OrderListDto {
  customerPhone: string;
  shippingFullName: string;
  shippingPhone: string;
  shippingCity: string;
  shippingDistrict: string;
  shippingAddressLine: string;
  shippingPostalCode?: string;
  subTotal: number;
  discountAmount: number;
  campaignName?: string;
  notes?: string;
  lines: { productId: number; productName: string; productSku: string; quantity: number; unitPrice: number; lineTotal: number; }[];
}

export interface CampaignDto {
  id: number;
  name: string;
  description?: string;
  discountType: number;
  discountValue: number;
  couponCode?: string;
  minOrderAmount?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  imageUrl?: string;
  isRunning: boolean;
}

export interface DashboardDto {
  customers: number;
  products: number;
  orders: number;
  pendingOrders: number;
  revenue: number;
  activeCampaigns: number;
}

export const ORDER_STATUS: Record<number, string> = {
  0: 'Beklemede',
  1: 'Onaylandı',
  2: 'Hazırlanıyor',
  3: 'Kargoda',
  4: 'Teslim edildi',
  5: 'İptal'
};

