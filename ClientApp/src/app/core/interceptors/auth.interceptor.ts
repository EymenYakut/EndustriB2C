import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private refreshing = false;

  constructor(private auth: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const authReq = this.withAuth(req);
    return next.handle(authReq).pipe(
      catchError(err => {
        if (err.status === 401 && !req.url.includes('/api/auth/') && !this.refreshing) {
          this.refreshing = true;
          return this.auth.refresh().pipe(
            switchMap(() => {
              this.refreshing = false;
              return next.handle(this.withAuth(req));
            }),
            catchError(refreshErr => {
              this.refreshing = false;
              this.auth.logout();
              return throwError(() => refreshErr);
            })
          );
        }
        return throwError(() => err);
      })
    );
  }

  private withAuth(req: HttpRequest<any>): HttpRequest<any> {
    const headers: Record<string, string> = {};
    if (this.auth.token) {
      headers['Authorization'] = `Bearer ${this.auth.token}`;
    }
    return req.clone({
      withCredentials: true,
      setHeaders: headers
    });
  }
}
