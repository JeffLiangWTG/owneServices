import { IEntity } from "./models/IEntity";

export class EntityHelper {
    static isEditable(entity: IEntity) : boolean {
        let editableProperty = EntityHelper.getPropertyEndsWith(entity, "IsEditable");
        return !editableProperty || entity[editableProperty];
    }

    static isSystem(entity : IEntity) : boolean {
        return entity[EntityHelper.getPropertyEndsWith(entity, "IsSystem")];
	}

	static getPK(entity : IEntity) : string {
		return entity[EntityHelper.getPropertyEndsWith(entity, "_PK")];
    }
  
    static getPropertyEndsWith(entity : IEntity, propertyName : string) : string {
        return Object.keys(entity).find(x => x.endsWith(propertyName)) as string;
    }

    static getProperty(entity: IEntity, propertyName: string) : string {
        return Object.keys(entity).find(x => x == (propertyName)) as string;
    }
}