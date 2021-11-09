import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
import { PresenceService } from './presence.service';
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private currentUserSource = new ReplaySubject<User>(1);
  private baseUrl: string = environment.apiUrl;
  currentUser$ = this.currentUserSource.asObservable();
  constructor(private http: HttpClient, private presence:PresenceService) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user: User = response;
        if (user) {
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
      })
    );
  }
  setCurrentUser(user: User) {
    user.roles = [];
    const roles = this.getDecodeToken(user.token).role;
    Array.isArray(roles) ? user.roles = roles : user.roles.push(roles);
    if (user.token) {
      localStorage.setItem('user', JSON.stringify(user));
      this.currentUserSource.next(user);
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource = new ReplaySubject<User>(1);
    this.currentUser$ = this.currentUserSource.asObservable();
    this.presence.stopHubConnection();
  }



  registerUser(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          console.log(user);
          this.setCurrentUser(user);
          this.presence.createHubConnection(user);
        }
      })
    );
  }

  getDecodeToken(token: string) {
    return JSON.parse(atob(token.split('.')[1]));
  }
}
