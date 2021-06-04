import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any= {};
  
  constructor(public accountService:AccountService, private router: Router) { }

  ngOnInit(): void {
  
  }

  login() {
   return this.accountService.login(this.model).subscribe(() => {
     this.router.navigateByUrl('/members');
      });
      
  }
  logout() {
    this.accountService.logout();
    this.router.navigateByUrl('/');
  }
}