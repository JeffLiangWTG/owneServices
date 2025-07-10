import { IFilter } from "./Filter";
import { IEntity } from "./models/IEntity";

export interface IFilterModule {
	name: string;
	filter: IFilter;
	maxLength: number;
	listCodePropertyName?: string;
	listDescriptionPropertyName?: string;
	listEntityTypeName?: string | IEntity[];
}

export class TextFilterModule implements IFilterModule {
	constructor(name: string, filter: IFilter, maxLength: number, listCodePropertyName?: string, listDescriptionPropertyName?: string, listEntityTypeName?: string | IEntity[]) {
		this.name = name;
		this.filter = filter;
		this.maxLength = maxLength;
		this.listCodePropertyName = listCodePropertyName;
		this.listDescriptionPropertyName = listDescriptionPropertyName;
		this.listEntityTypeName = listEntityTypeName;
	}

	name: string;
	filter: IFilter;
	maxLength: number;
	listCodePropertyName?: string;
	listDescriptionPropertyName?: string;
	listEntityTypeName?: string | IEntity[];
}
