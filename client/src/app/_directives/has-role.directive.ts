import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { take } from 'rxjs/operators';
import { AdminGuard } from '../_guards/admin.guard';
import { User } from '../_models/user';
import { AccountService } from '../_services/account.service';

@Directive({
  selector: '[appHasRole]' //*appHasRole ='["Admin","Member"]'
})
export class HasRoleDirective {
  @Input() appHasRole!: string[];
  user!: User;
  constructor(private viewContainerRef: ViewContainerRef, private templateRef: TemplateRef<any>,
    private accountService: AccountService) {
    this.accountService.currentUser$.pipe(take(1)).subscribe(user => {
      this.user = user;
    });
  }
  ngOnInit(): void {
    //clear view if no roles
    if (!this.user?.roles || this.user.roles == null) {
      this.viewContainerRef.clear();
      return;
    }
    else if (this.user?.roles.some(r => this.appHasRole.includes(r))) {
      this.viewContainerRef.createEmbeddedView(this.templateRef);
    }
    else {
      this.viewContainerRef.clear();
      return;
    }
  }
}
