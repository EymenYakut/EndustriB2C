import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { FeatureHeaderDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-features',
  templateUrl: './admin-features.component.html'
})
export class AdminFeaturesComponent implements OnInit {
  list: FeatureHeaderDto[] = [];
  editing: any = null;

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.admin.features().subscribe(f => this.list = f);
  }

  startNew(): void {
    this.editing = { name: '', unit: '', sortOrder: this.list.length };
  }

  save(): void {
    this.admin.saveFeature(this.editing, this.editing.id).subscribe(() => {
      this.editing = null;
      this.reload();
    });
  }

  edit(f: FeatureHeaderDto): void {
    this.editing = { ...f };
  }

  remove(id: number): void {
    this.admin.deleteFeature(id).subscribe(() => this.reload());
  }
}
