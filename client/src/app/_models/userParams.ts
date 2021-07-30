import { User } from "./user";

export class UserParams {
    gender: string;
    minAge=18;
    maxAge=99;
    pageNumber: number = 1;
    pageSize:number=2;
    orderBy = 'lastActive';
    
    constructor(user:User) {
        this.gender = user.gender==='female' ? 'male' : 'female';
    }
}