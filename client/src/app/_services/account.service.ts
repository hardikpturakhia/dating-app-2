import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ReplaySubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';
@Injectable({
  providedIn: 'root'
})
export class AccountService {
  private currentUserSource = new ReplaySubject<User>(1);
  private baseUrl: string = environment.apiUrl;
  currentUser$ = this.currentUserSource.asObservable();
  constructor(private http: HttpClient) { }

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model).pipe(
      map((response: User) => {
        const user: User = response;
        if (user) {
          console.log(user);
          this.setCurrentUser(user);
        }
      }) 
    );
  }
  setCurrentUser(user: User) {
    if(user.token){
    localStorage.setItem('user', JSON.stringify(user));  
    this.currentUserSource.next(user);
    }
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUserSource = new ReplaySubject<User>(1);
    this.currentUser$ = this.currentUserSource.asObservable();
  }



  registerUser(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model).pipe(
      map((user: User) => {
        if (user) {
          console.log(user);
          this.setCurrentUser(user);
        }
      })
    );
  }
}
