import { Component, OnInit } from '@angular/core';
import { Message } from '../_models/message';
import { Pagination } from '../_models/pagination';
import { MessageService } from '../_services/message.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
  messages: Message[] = [];
  pagination!: Pagination;
  container = 'Unread';
  pageNumber = 1;
  pageSize = 5;
  loadingFlag = false;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadingFlag = true;
    this.loadMessages();
    this.loadingFlag = false;
  }
  loadMessages() {
    this.messageService.getMessage(this.pageNumber, this.pageSize, this.container)
      .subscribe(response => {
        this.messages = response.result;
        this.pagination = response.pagination;
      });
  }
  pageChanged(event: any) {
    this.pageNumber = event.page;
    this.loadMessages();
  }
  deleteMessage(id: number) {
    this.messageService.deleteMessage(id).subscribe(() => {
      this.messages.splice(this.messages.findIndex(m => m.id === id), 1);
    });
  }
}
