import { IEntityWrapper, EntityWrapper } from "./EntityWrapper";
import { IEntity, EntityState } from "./models/IEntity";
import { EntityHelper } from "./EntityHelper";

export interface IEntityWrapperRepository {
	add(entity: IEntity, entityName: string): void;
	update(pk: string, entityWrapper: IEntityWrapper): void;
	delete(entity: IEntity): void;
	clear(): void;
	getAll(): IEntityStore;
	get(pk: string): IEntityWrapper | undefined;
}

interface IEntityStore {
	[pk: string]: IEntityWrapper;
}

export class EntityWrapperRepository implements IEntityWrapperRepository {
	entities: IEntityStore;

	constructor() {
		this.entities = {};
	}

	add(entity: IEntity, entityName: string): void {
		this.entities[EntityHelper.getPK(entity)] = new EntityWrapper(
			entity,
			EntityState.Added,
			entityName,
			null
		);
	}

	update(pk: string, entityWrapper: IEntityWrapper): void {
		this.entities[pk] = entityWrapper;
	}

	delete(entity: IEntity): void {
		var pk = EntityHelper.getPK(entity);
		delete this.entities[pk];
	}

	clear(): void {
		this.entities = {};
	}

	getAll(): IEntityStore {
		return this.entities;
	}

	get(pk: string): IEntityWrapper | undefined {
		return this.entities[pk] || undefined;
	}
}
