import { IEntity } from "./IEntity";

interface IRefCusCodeType extends IEntity {
	ZZK_PK: string,
	ZZK_CodeType: string,
	ZZK_Description: string,
	ZZK_IsReadonly: boolean,
	ZZK_MaxLength: number,
	ZZK_ZZZ_NKDataGrouping: string
}

export default IRefCusCodeType;
