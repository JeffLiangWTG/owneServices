export interface IValidationResults {
    [propertyName : string] : string[];
}

export class ValidationResults implements IValidationResults {
    [propertyName : string] : string[];
}