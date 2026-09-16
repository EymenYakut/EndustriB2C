import { Component, HostListener, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CategoryDto, ProductDetailDto, ProductListDto } from '../../core/models/models';

@Component({
  selector: 'app-product-list',
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  products: ProductListDto[] = [];
  categories: CategoryDto[] = [];
  category = '';
  q = '';
  searchText = '';
  page = 1;
  pageSize = 12;
  total = 0;
  totalPages = 1;
  quantities: Record<number, number> = {};
  cardMessage: Record<number, string> = {};
  cardError: Record<number, string> = {};
  adding: Record<number, boolean> = {};

  detail?: ProductDetailDto;
  modalLoading = false;
  modalQty = 1;
  modalMessage = '';
  modalError = '';
  modalAdding = false;
  zoomOpen = false;
  zoomIndex = 0;
  filtersOpen = false;
  expanded: Record<string, boolean> = {};
  private openedSlug = '';
  private loadToken = 0;
  private catalogReady = false;

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit(): void {
    this.api.categories().subscribe(c => this.categories = c);
    this.route.queryParamMap.subscribe(p => {
      const category = p.get('category') || '';
      const q = p.get('q') || '';
      const page = Math.max(1, parseInt(p.get('page') || '1', 10) || 1);
      if (!this.catalogReady || category !== this.category || q !== this.q || page !== this.page) {
        this.category = category;
        this.q = q;
        this.searchText = q;
        this.page = page;
        this.catalogReady = true;
        this.load();
      }
      this.filtersOpen = false;
      const slug = p.get('urun') || '';
      if (slug) {
        this.openBySlug(slug);
      } else {
        this.dismissModal(false);
      }
    });
  }

  ngOnDestroy(): void {
    document.body.style.overflow = '';
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.zoomOpen) {
      this.closeZoom();
      return;
    }
    if (this.filtersOpen) {
      this.filtersOpen = false;
      return;
    }
    if (this.detail || this.modalLoading) {
      this.close();
    }
  }

  load(): void {
    this.api.products(this.category || undefined, this.q || undefined, this.page, this.pageSize).subscribe(r => {
      this.products = r.items || [];
      this.total = r.total || 0;
      this.page = r.page || this.page;
      this.pageSize = r.pageSize || this.pageSize;
      this.totalPages = Math.max(1, r.totalPages || 1);
      for (const p of this.products) {
        if (this.quantities[p.id] == null) {
          this.quantities[p.id] = 1;
        }
      }
    });
  }

  filterParams(slug?: string, page = 1): Record<string, string> {
    const params: Record<string, string> = {};
    if (slug) {
      params['category'] = slug;
    }
    if (this.searchText.trim()) {
      params['q'] = this.searchText.trim();
    }
    if (page > 1) {
      params['page'] = String(page);
    }
    return params;
  }

  applySearch(): void {
    this.router.navigate(['/urunler'], { queryParams: this.filterParams(this.category || undefined, 1) });
  }

  resetFilters(): void {
    this.searchText = '';
    this.expanded = {};
    this.router.navigate(['/urunler']);
  }

  goToPage(page: number): void {
    const next = Math.min(this.totalPages, Math.max(1, page));
    this.router.navigate(['/urunler'], { queryParams: this.filterParams(this.category || undefined, next) });
  }

  get pageStart(): number {
    return this.total === 0 ? 0 : (this.page - 1) * this.pageSize + 1;
  }

  get pageEnd(): number {
    return Math.min(this.total, this.page * this.pageSize);
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;
    const current = this.page;
    const size = 5;
    let start = Math.max(1, current - 2);
    let end = Math.min(total, start + size - 1);
    start = Math.max(1, end - size + 1);
    const pages: number[] = [];
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }

  get selectedCategoryName(): string {
    for (const c of this.categories) {
      if (c.slug === this.category) {
        return c.name;
      }
      const child = (c.children || []).find(s => s.slug === this.category);
      if (child) {
        return `${c.name} / ${child.name}`;
      }
    }
    return 'Tüm ürünler';
  }

  get totalCategoryCount(): number {
    return this.categories.reduce((sum, c) => sum + (c.productCount || 0), 0);
  }

  isGroupActive(c: CategoryDto): boolean {
    return this.category === c.slug || (c.children || []).some(s => s.slug === this.category);
  }

  isOpen(c: CategoryDto): boolean {
    if (Object.prototype.hasOwnProperty.call(this.expanded, c.slug)) {
      return this.expanded[c.slug];
    }
    return this.isGroupActive(c);
  }

  toggleGroup(c: CategoryDto): void {
    this.expanded[c.slug] = !this.isOpen(c);
  }

  readable(name: string): string {
    return (name || '')
      .toLocaleLowerCase('tr-TR')
      .replace(/(^|[\s/–—\-()]+)([\p{L}])/gu, (_, sep: string, ch: string) => sep + ch.toLocaleUpperCase('tr-TR'));
  }

  qtyOf(p: ProductListDto): number {
    return this.quantities[p.id] ?? 1;
  }

  setQty(p: ProductListDto, qty: number): void {
    this.quantities[p.id] = this.clamp(qty, p.stock);
  }

  adjust(p: ProductListDto, delta: number): void {
    this.setQty(p, this.qtyOf(p) + delta);
  }

  add(p: ProductListDto): void {
    if (p.stock < 1 || this.adding[p.id]) {
      return;
    }
    this.adding[p.id] = true;
    this.cardMessage[p.id] = '';
    this.cardError[p.id] = '';
    this.api.addToCart(p.id, this.qtyOf(p)).subscribe({
      next: () => {
        this.adding[p.id] = false;
        this.cardMessage[p.id] = 'Sepete eklendi';
      },
      error: e => {
        this.adding[p.id] = false;
        this.cardError[p.id] = e.error?.message || 'Sepete eklenemedi.';
      }
    });
  }

  open(p: ProductListDto): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { urun: p.slug },
      queryParamsHandling: 'merge'
    });
  }

  close(): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { urun: null },
      queryParamsHandling: 'merge'
    });
  }

  adjustModal(delta: number): void {
    if (!this.detail) {
      return;
    }
    this.modalQty = this.clamp(this.modalQty + delta, this.detail.stock);
  }

  setModalQty(qty: number): void {
    if (!this.detail) {
      return;
    }
    this.modalQty = this.clamp(qty, this.detail.stock);
  }

  addFromModal(): void {
    if (!this.detail || this.detail.stock < 1 || this.modalAdding) {
      return;
    }
    this.modalAdding = true;
    this.modalMessage = '';
    this.modalError = '';
    this.api.addToCart(this.detail.id, this.modalQty).subscribe({
      next: () => {
        this.modalAdding = false;
        this.modalMessage = 'Ürün sepete eklendi.';
      },
      error: e => {
        this.modalAdding = false;
        this.modalError = e.error?.message || 'Sepete eklenemedi.';
      }
    });
  }

  get zoomImages(): string[] {
    if (!this.detail) {
      return [];
    }
    const fromGallery = (this.detail.images || [])
      .slice()
      .sort((a, b) => (a.isPrimary === b.isPrimary ? a.sortOrder - b.sortOrder : (a.isPrimary ? -1 : 1)))
      .map(i => i.filePath)
      .filter(Boolean);
    if (fromGallery.length) {
      return fromGallery;
    }
    return this.detail.imageUrl ? [this.detail.imageUrl] : [];
  }

  get zoomSrc(): string {
    return this.zoomImages[this.zoomIndex] || '';
  }

  openZoom(event?: Event): void {
    event?.stopPropagation();
    if (!this.zoomImages.length) {
      return;
    }
    this.zoomIndex = 0;
    this.zoomOpen = true;
  }

  closeZoom(event?: Event): void {
    event?.stopPropagation();
    this.zoomOpen = false;
  }

  zoomStep(delta: number, event?: Event): void {
    event?.stopPropagation();
    const total = this.zoomImages.length;
    if (total < 2) {
      return;
    }
    this.zoomIndex = (this.zoomIndex + delta + total) % total;
  }

  private openBySlug(slug: string): void {
    if (this.openedSlug === slug && this.detail) {
      document.body.style.overflow = 'hidden';
      return;
    }
    this.openedSlug = slug;
    const token = ++this.loadToken;
    this.modalLoading = true;
    this.modalMessage = '';
    this.modalError = '';
    this.modalQty = 1;
    document.body.style.overflow = 'hidden';
    this.api.productBySlug(slug).subscribe({
      next: d => {
        if (token !== this.loadToken) {
          return;
        }
        this.detail = d;
        this.modalQty = 1;
        this.modalLoading = false;
      },
      error: () => {
        if (token !== this.loadToken) {
          return;
        }
        this.modalLoading = false;
        this.close();
      }
    });
  }

  private dismissModal(clearSlug = true): void {
    this.loadToken++;
    this.detail = undefined;
    this.modalLoading = false;
    this.modalMessage = '';
    this.modalError = '';
    this.modalAdding = false;
    this.zoomOpen = false;
    if (clearSlug) {
      this.openedSlug = '';
    }
    document.body.style.overflow = '';
  }

  private clamp(qty: number, stock: number): number {
    const max = Math.max(1, stock || 1);
    return Math.max(1, Math.min(max, Math.floor(qty) || 1));
  }
}
