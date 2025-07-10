export enum FilterOps {
	Equals,
	Contains,
	NotEquals,
	In,
	PlaceholderForSelection,
	DateToday,
	DateYesterday,
	DateSevenDaysAgo,
	DateForteenDaysAgo,
	DateLastMonth,
	DateRange,
}

export interface IFilter {
	[key: string]: string | any;
	propertyName: string;
	operation: FilterOps;
	value: object;
	type: string;
}

export class Filter implements IFilter {
	constructor(
		propertyName: string,
		operation: FilterOps,
		value: object,
		type: string
	) {
		this.propertyName = propertyName;
		this.operation = operation;
		this.value = value;
		this.type = type;
	}
	[key: string]: string | any;

	propertyName: string;
	operation: FilterOps;
	value: object;
	type: string;
}
