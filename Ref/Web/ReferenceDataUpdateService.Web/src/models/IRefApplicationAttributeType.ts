import { IEntity } from "./IEntity";

interface IRefApplicationAttributeType extends IEntity {
	RAT_PK: string,
	RAT_Type: string,
	RAT_Description: string
}

export default IRefApplicationAttributeType;
