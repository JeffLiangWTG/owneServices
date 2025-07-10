import { IEntity } from "./IEntity";

export default interface ISourceDataUserView extends IEntity {
	SDA_PK: string;
	SDA_SubSource: string;
	SDA_Source: string;
	SDA_SourceTime: Date;
	SDA_Filename: string;
	SDA_CreatedTime: Date;
	SDA_Status: string;
	SDA_NotProcessedUntil: Date | null;
}
