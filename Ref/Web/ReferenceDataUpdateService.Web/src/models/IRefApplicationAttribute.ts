import { IEntity } from "./IEntity";

export interface IRefApplicationAttribute extends IEntity {
	RAA_PK: string,
	RAA_ConfigFilePath: string,
	RAA_AttributeName: string,
	RAA_Value: string,
	RAA_RAT_NKType: string,
	RAA_Content: string | null,
	RAA_JobGroup: string
}

export interface IRefApplicationAttributeDefault extends IEntity {
	RAA_PK: string,
	RAA_ConfigFilePath: string,
	RAA_AttributeName: string,
	RAA_Value: string,
	RAA_RAT_NKType: string,
	RAA_Content: string | null
}

export class RefApplicationAttributeWrapper {
	constructor(RAA_PK: string, RAA_ConfigFilePath: string, RAA_AttributeName: string
		, RAA_Value: string, RAA_RAT_NKType: string, RAA_JobGroup: string
		, RAA_Content: string | null, RAA_IsDefault: boolean
		, defaultRecord: IRefApplicationAttributeDefault | null
	) {
		this.RAA_PK = RAA_PK;
		this.RAA_ConfigFilePath = RAA_ConfigFilePath;
		this.RAA_AttributeName = RAA_AttributeName;
		this.RAA_Value = RAA_Value.toString();
		this.RAA_RAT_NKType = RAA_RAT_NKType;
		this.RAA_Content = RAA_Content;
		this.RAA_IsDefault = RAA_IsDefault;
		this.RAA_JobGroup = RAA_JobGroup;
		this.defaultRecord = defaultRecord;
	}

	objectToInterface(): IRefApplicationAttribute {
		let result: IRefApplicationAttribute = {
			RAA_PK: this.RAA_PK,
			RAA_ConfigFilePath: this.RAA_ConfigFilePath,
			RAA_AttributeName: this.RAA_AttributeName,
			RAA_Value: this.RAA_Value.toString(),
			RAA_RAT_NKType: this.RAA_RAT_NKType,
			RAA_JobGroup: this.RAA_JobGroup,
			RAA_Content: this.RAA_Content
		}
		return result;
	}

	static interfaceToObject(interfaceObject: IRefApplicationAttribute, isDefault: boolean, defaultRecord: IRefApplicationAttributeDefault | null): RefApplicationAttributeWrapper {
		return new RefApplicationAttributeWrapper(
			interfaceObject.RAA_PK,
			interfaceObject.RAA_ConfigFilePath,
			interfaceObject.RAA_AttributeName,
			interfaceObject.RAA_Value.toString(),
			interfaceObject.RAA_RAT_NKType,
			interfaceObject.RAA_JobGroup,
			interfaceObject.RAA_Content,
			isDefault,
			defaultRecord
		);
	}

	RAA_PK: string;
	RAA_ConfigFilePath: string;
	RAA_AttributeName: string;
	RAA_Value: string;
	RAA_RAT_NKType: string;
	RAA_Content: string | null;
	RAA_JobGroup: string;
	RAA_IsDefault: boolean;
	Entity: any;
	defaultRecord: IRefApplicationAttributeDefault | null;
}
