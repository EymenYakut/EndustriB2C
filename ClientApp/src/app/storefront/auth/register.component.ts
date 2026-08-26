import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html'
})
export class RegisterComponent {
  model = { firstName: '', lastName: '', email: '', phone: '', password: '' };
  error = '';

  constructor(private auth: AuthService, private api: ApiService, private router: Router) {}

  submit(): void {
    this.error = '';
    this.auth.register(this.model).subscribe({
      next: () => {
        this.api.loadCart();
        this.router.navigate(['/']);
      },
      error: e => this.error = e.error?.message || e.error?.title || 'Kayıt başarısız.'
    });
  }
}
