import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  error = '';

  constructor(private auth: AuthService, private api: ApiService, private router: Router) {}

  submit(): void {
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: res => {
        this.api.loadCart();
        this.router.navigate([res.user.userType === 0 ? '/admin' : '/']);
      },
      error: e => this.error = e.error?.message || 'Giriş başarısız.'
    });
  }
}
