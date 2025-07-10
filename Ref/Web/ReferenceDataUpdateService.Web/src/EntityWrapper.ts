import { ServiceType } from "./EntityManager";
import { IEntity, EntityState } from "./models/IEntity";

export interface IEntityWrapper {
	service : ServiceType | null,
	entity : IEntity,
	state : EntityState,
	name : string,
}

export class EntityWrapper implements IEntityWrapper {
    constructor(entity : IEntity, state : EntityState, name : string, service : ServiceType | null) {
        this.entity = entity;
        this.state =  state;
        this.name = name;
        this.service = service;
    }

    service : ServiceType | null;
	entity : IEntity;
	state : EntityState;
	name : string;
}