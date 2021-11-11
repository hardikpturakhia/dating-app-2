import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { observable, Observable, ObservableInput, UnsubscriptionError } from 'rxjs';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmService {
  bsModalRef!: BsModalRef;
  constructor(private modalService: BsModalService) { }

  confirm(title = 'Confirmation', message = 'You are sure?', btnOkText = 'Ok', btnCancelText = 'Cancel'):Observable<boolean> {
    const config = {
      initialState: {
        title,
        message,
        btnOkText,
        btnCancelText
      }
    };
    this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);
    return new Observable<boolean>(this.getResult());
  }
  private getResult() {
    return (observer: { next: (arg0: any) => void; complete: () => void; }) => {
      const subscription  = this.bsModalRef.onHidden.subscribe(()=> {
        observer.next(this.bsModalRef.content.result);
        observer.complete();
      });
      return{    unsubscribe() {
        subscription.unsubscribe();
      }
      }
    }
  }
} 