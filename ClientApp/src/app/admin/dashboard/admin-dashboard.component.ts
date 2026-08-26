import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { DashboardDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  data?: DashboardDto;

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.admin.dashboard().subscribe(d => this.data = d);
  }
}
