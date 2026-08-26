import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { FeatureHeaderDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-features',
  templateUrl: './admin-features.component.html'
})
export class AdminFeaturesComponent implements OnInit {
  list: FeatureHeaderDto[] = [];
  model: any = { name: '', unit: '', sortOrder: 0 };

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.admin.features().subscribe(f => this.list = f);
  }

  save(): void {
    this.admin.saveFeature(this.model, this.model.id).subscribe(() => {
      this.model = { name: '', unit: '', sortOrder: 0 };
      this.reload();
    });
  }

  edit(f: FeatureHeaderDto): void {
    this.model = { ...f };
  }

  remove(id: number): void {
    this.admin.deleteFeature(id).subscribe(() => this.reload());
  }
}
