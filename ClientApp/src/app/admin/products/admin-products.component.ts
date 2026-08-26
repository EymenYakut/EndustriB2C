import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { CampaignDto, CategoryDto, FeatureHeaderDto, ProductDetailDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-products',
  templateUrl: './admin-products.component.html'
})
export class AdminProductsComponent implements OnInit {
  products: ProductDetailDto[] = [];
  features: FeatureHeaderDto[] = [];
  campaigns: CampaignDto[] = [];
  categories: CategoryDto[] = [];
  editing: any = null;
  error = '';

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.reload();
    this.admin.features().subscribe(f => this.features = f);
    this.admin.campaigns().subscribe(c => this.campaigns = c);
    this.admin.categories().subscribe(c => this.categories = c.filter(x => x.isActive));
  }

  reload(): void {
    this.admin.products().subscribe(p => this.products = p);
  }

  categoryNames(p: ProductDetailDto): string {
    return (p.categories || []).map(c => c.name).join(', ') || p.category || '-';
  }

  startNew(): void {
    this.editing = {
      sku: '',
      manufacturer: '',
      categoryIds: [],
      stock: 0,
      isActive: true,
      sortOrder: 0,
      translations: [
        { languageCode: 'tr', name: '', shortDescription: '', description: '', slug: '' },
        { languageCode: 'en', name: '', shortDescription: '', description: '', slug: '' }
      ],
      prices: [{ amount: 0, currency: 'TRY', campaignId: null, isCurrent: true }],
      features: this.features.map(f => ({ headerId: f.id, value: '' }))
    };
  }

  edit(p: ProductDetailDto): void {
    this.editing = {
      id: p.id,
      sku: p.sku,
      manufacturer: p.manufacturer || '',
      categoryIds: [...(p.categoryIds || [])],
      stock: p.stock,
      isActive: p.isActive,
      sortOrder: 0,
      translations: p.translations.length ? p.translations : [{ languageCode: 'tr', name: p.name, slug: p.slug }],
      prices: p.prices.length ? p.prices : [{ amount: p.price, currency: 'TRY', isCurrent: true }],
      features: this.features.map(f => {
        const existing = p.features.find(x => x.headerId === f.id);
        return { headerId: f.id, value: existing?.value || '' };
      }),
      images: p.images
    };
  }

  isCatSelected(id: number): boolean {
    return (this.editing?.categoryIds || []).includes(id);
  }

  toggleCat(id: number, event: Event): void {
    const on = (event.target as HTMLInputElement).checked;
    const ids: number[] = this.editing.categoryIds || [];
    this.editing.categoryIds = on ? Array.from(new Set([...ids, id])) : ids.filter(x => x !== id);
  }

  save(): void {
    this.error = '';
    const id = this.editing.id;
    this.admin.saveProduct(this.editing, id).subscribe({
      next: saved => {
        this.editing.id = saved.id;
        this.reload();
      },
      error: e => this.error = e.error?.message || 'Kaydedilemedi.'
    });
  }

  remove(id: number): void {
    if (!confirm('Ürün silinsin mi?')) return;
    this.admin.deleteProduct(id).subscribe(() => this.reload());
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length || !this.editing?.id) return;
    this.admin.uploadImage(this.editing.id, input.files[0], true).subscribe(() => {
      this.admin.product(this.editing.id).subscribe(p => this.edit(p));
    });
  }

  featureName(headerId: number): string {
    return this.features.find(x => x.id === headerId)?.name || ('Özellik ' + headerId);
  }

  deleteImage(imageId: number): void {
    this.admin.deleteImage(this.editing.id, imageId).subscribe(() => {
      this.admin.product(this.editing.id).subscribe(p => this.edit(p));
    });
  }
}
