import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { CategoryDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-categories',
  templateUrl: './admin-categories.component.html'
})
export class AdminCategoriesComponent implements OnInit {
  list: CategoryDto[] = [];
  editing: any = null;
  error = '';

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.admin.categories().subscribe(c => this.list = c);
  }

  startNew(): void {
    this.editing = { name: '', slug: '', description: '', imageUrl: '', sortOrder: 0, isActive: true };
  }

  edit(c: CategoryDto): void {
    this.editing = { ...c };
  }

  save(): void {
    this.error = '';
    this.admin.saveCategory(this.editing, this.editing.id).subscribe({
      next: saved => {
        this.editing = saved;
        this.reload();
      },
      error: e => this.error = e.error?.message || 'Kaydedilemedi.'
    });
  }

  remove(id: number): void {
    this.admin.deleteCategory(id).subscribe({
      next: () => this.reload(),
      error: e => alert(e.error?.message || 'Silinemedi.')
    });
  }

  upload(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length || !this.editing?.id) return;
    this.admin.uploadCategoryImage(this.editing.id, input.files[0]).subscribe(c => {
      this.editing = c;
      this.reload();
    });
  }
}
