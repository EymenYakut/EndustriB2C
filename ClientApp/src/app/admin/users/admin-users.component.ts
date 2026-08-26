import { Component, OnInit } from '@angular/core';
import { AdminApiService } from '../../core/services/admin-api.service';
import { UserDto } from '../../core/models/models';

@Component({
  selector: 'app-admin-users',
  templateUrl: './admin-users.component.html'
})
export class AdminUsersComponent implements OnInit {
  users: UserDto[] = [];

  constructor(private admin: AdminApiService) {}

  ngOnInit(): void {
    this.admin.users().subscribe(u => this.users = u);
  }
}
