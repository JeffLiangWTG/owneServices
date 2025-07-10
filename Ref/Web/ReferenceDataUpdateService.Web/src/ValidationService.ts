import { IEntity } from "./models/IEntity";
import { ValidationHelper } from "./ValidationHelper";

export interface IValidationService {
    [propertyName: string]: ((data: IEntity, propertyName: string, isSaving: boolean) => string | Promise<string>)[];
}

export class ValidationServiceWrapper {
    constructor (validationServices: IValidationService[], ValidationCallback: (isValidating: boolean) => void){
        this.ValidationCallback = ValidationCallback; 
        this._validationService = validationServices;  
    }

    ValidationCallback: (isValidating: boolean) => void;
    private _validationAwaitingCount : number = 0;
    
    private _validationService : IValidationService[];
    public GetValidationService(arrayPositionInteger: number = 0) : IValidationService {
        return this._validationService[arrayPositionInteger];
    }

    public IncreaseValidationCount(){
        this._validationAwaitingCount++;
        if ((this._validationAwaitingCount !== 0) !== (this._validationAwaitingCount - 1 !== 0)) {
            this.ValidationCallback(this._validationAwaitingCount !== 0);            
        }
    }

    public DecreaseValidationCount(){
        this._validationAwaitingCount--;
        if ((this._validationAwaitingCount !== 0) !== (this._validationAwaitingCount + 1 !== 0)) {
        this.ValidationCallback(this._validationAwaitingCount !== 0);
        }
    }
}