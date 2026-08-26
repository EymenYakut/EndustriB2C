import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-login',
  templateUrl: './admin-login.component.html'
})
export class AdminLoginComponent {
  email = 'admin@endustri.com';
  password = '';
  error = '';

  constructor(private auth: AuthService, private router: Router) {}

  submit(): void {
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: res => {
        if (res.user.userType !== 0) {
          this.error = 'Bu hesap yönetici değil.';
          this.auth.logout();
          return;
        }
        this.router.navigate(['/admin']);
      },
      error: e => this.error = e.error?.message || 'Giriş başarısız.'
    });
  }
}
