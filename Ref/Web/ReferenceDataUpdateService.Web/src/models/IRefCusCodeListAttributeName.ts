import { IEntity } from "./IEntity";

interface IRefCusCodeListAttributeName extends IEntity {
	ZXE_PK: string,
	ZXE_Name: string,
	ZXE_Description: string,
	ZXE_ZZK_NKCodeType: string,
	ZXE_ZZZ_NKDataGrouping: string,
	ZXE_IsMandatory: boolean,
	ZXE_AllowDuplicates: boolean,
	ZXE_IsValueMandatory: boolean,
	ZXE_ZZK_NKCodeTypeForValueList: string,
	ZXE_ValueDataType: string,
	ZXE_MinLengthOrValue: number,
	ZXE_MaxLengthOrValue: number,
	ZXE_DecimalPlaces: number,
	ZXE_ColumnCaption: string,
	ZXE_IsDateRangeUsed: boolean
}

export default IRefCusCodeListAttributeName;
