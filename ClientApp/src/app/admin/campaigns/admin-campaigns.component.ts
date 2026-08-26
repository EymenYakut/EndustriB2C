import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { CampaignDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-campaigns',
  templateUrl: './admin-campaigns.component.html'
})
export class AdminCampaignsComponent implements OnInit {
  list: CampaignDto[] = [];
  editing: any = null;

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void { this.reload(); }

  reload(): void {
    this.admin.campaigns().subscribe(c => this.list = c);
  }

  startNew(): void {
    const now = new Date();
    const later = new Date();
    later.setMonth(later.getMonth() + 1);
    this.editing = {
      name: '',
      description: '',
      discountType: 0,
      discountValue: 10,
      couponCode: '',
      minOrderAmount: 0,
      startDate: now.toISOString().slice(0, 16),
      endDate: later.toISOString().slice(0, 16),
      isActive: true
    };
  }

  edit(c: CampaignDto): void {
    this.editing = {
      ...c,
      startDate: c.startDate?.slice(0, 16),
      endDate: c.endDate?.slice(0, 16)
    };
  }

  save(): void {
    const payload = {
      ...this.editing,
      startDate: new Date(this.editing.startDate).toISOString(),
      endDate: new Date(this.editing.endDate).toISOString()
    };
    this.admin.saveCampaign(payload, this.editing.id).subscribe(() => {
      this.editing = null;
      this.reload();
    });
  }

  remove(id: number): void {
    this.admin.deleteCampaign(id).subscribe(() => this.reload());
  }
}
