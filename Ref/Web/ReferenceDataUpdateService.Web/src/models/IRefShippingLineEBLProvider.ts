import { IEntity } from "./IEntity";

interface IRefShippingLineEBLProvider extends IEntity {
	RSE_PK: string;
	RSE_RSL_ShippingLine: string;
	RSE_Name: string;
	RSE_IsAvailable: boolean;
	RSE_IsDefault: boolean;
}

export class RefShippingLineEBLProviderWrapper {
	constructor(
		RSE_PK: string,
		RSE_RSL_ShippingLine: string,
		RSE_Name: string,
		RSE_IsAvailable: boolean,
		RSE_IsDefault: boolean,
		isDefaultHidden?: boolean
	) {
		this.RSE_IsDefault = RSE_IsDefault;
		this.RSE_IsAvailable = RSE_IsAvailable;
		this.RSE_Name = RSE_Name;
		this.RSE_PK = RSE_PK;
		this.RSE_RSL_ShippingLine = RSE_RSL_ShippingLine;
		this.IsDefaultHidden = isDefaultHidden ?? !RSE_IsAvailable;
	}

	RSE_PK: string;
	RSE_RSL_ShippingLine: string;
	RSE_Name: string;
	RSE_IsAvailable: boolean;
	RSE_IsDefault: boolean;
	IsDefaultHidden: boolean;

	objectToInterface(): IRefShippingLineEBLProvider {
		let result: IRefShippingLineEBLProvider = {
			RSE_PK: this.RSE_PK,
			RSE_RSL_ShippingLine: this.RSE_RSL_ShippingLine,
			RSE_Name: this.RSE_Name,
			RSE_IsAvailable: this.RSE_IsAvailable,
			RSE_IsDefault: this.RSE_IsDefault,
		};
		return result;
	}
}

export class RefShippingLineEBLProviderName implements IEntity {
	constructor(RSE_Name: string) {
		this.RSE_Name = RSE_Name;
	}
	RSE_Name: string;
}

export default IRefShippingLineEBLProvider;
