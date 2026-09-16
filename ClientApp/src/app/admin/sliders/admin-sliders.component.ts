import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { SliderDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-sliders',
  templateUrl: './admin-sliders.component.html'
})
export class AdminSlidersComponent implements OnInit {
  list: SliderDto[] = [];
  editing: any = null;
  error = '';

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.admin.sliders().subscribe(s => this.list = s);
  }

  startNew(): void {
    this.editing = {
      title: '',
      subtitle: '',
      buttonText: 'Ürünleri incele',
      buttonUrl: '/urunler',
      imageUrl: '',
      sortOrder: this.list.length + 1,
      isActive: true
    };
  }

  edit(s: SliderDto): void {
    this.editing = { ...s };
  }

  save(): void {
    this.error = '';
    this.admin.saveSlider(this.editing, this.editing.id).subscribe({
      next: saved => {
        this.editing = saved;
        this.reload();
      },
      error: e => this.error = e.error?.message || 'Kaydedilemedi.'
    });
  }

  remove(id: number): void {
    if (!confirm('Bu slayt silinsin mi?')) return;
    this.admin.deleteSlider(id).subscribe(() => this.reload());
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length || !this.editing?.id) return;
    this.admin.uploadSliderImage(this.editing.id, input.files[0]).subscribe({
      next: s => {
        this.editing = s;
        this.reload();
      },
      error: e => this.error = e.error?.message || 'Görsel yüklenemedi.'
    });
  }
}
