import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { AuthResponse, UserDto } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenValue: string | null = null;
  private userSubject = new BehaviorSubject<UserDto | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpClient) {}

  get token(): string | null {
    return this.tokenValue;
  }

  get user(): UserDto | null {
    return this.userSubject.value;
  }

  get isLoggedIn(): boolean {
    return !!this.userSubject.value;
  }

  get isAdmin(): boolean {
    return this.userSubject.value?.userType === 0;
  }

  init(): Promise<void> {
    return this.http.post<AuthResponse>('/api/auth/refresh', {}).pipe(
      tap(res => this.setSession(res)),
      catchError(() => of(null))
    ).toPromise().then(() => undefined);
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/login', { email, password }).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(payload: { email: string; password: string; firstName: string; lastName: string; phone: string }): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/register', payload).pipe(
      tap(res => this.setSession(res))
    );
  }

  refresh(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>('/api/auth/refresh', {}).pipe(
      tap(res => this.setSession(res))
    );
  }

  logout(): void {
    this.http.post('/api/auth/logout', {}).subscribe({ complete: () => this.clear() });
    this.clear();
  }

  private setSession(res: AuthResponse): void {
    this.tokenValue = res.accessToken;
    this.userSubject.next(res.user);
  }

  private clear(): void {
    this.tokenValue = null;
    this.userSubject.next(null);
  }
}
