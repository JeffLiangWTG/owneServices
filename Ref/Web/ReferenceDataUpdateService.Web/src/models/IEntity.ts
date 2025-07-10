export enum EntityState {
	Added,
	Unchanged,
	Modified,
	Removed
}

export interface IEntity {
	[propertyName: string]: any,
}
