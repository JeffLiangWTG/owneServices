import { IEntityWrapper } from "./EntityWrapper";
import { IEntity } from "./models/IEntity";

export interface IEntityBroadcastService {
	broadcastUpdate(pk: string, entityWrapper: IEntityWrapper): void;
	broadcastDelete(entity: IEntity): void;
	subscribe(
		onUpdate: (pk: string, entityWrapper: IEntityWrapper) => void,
		onDelete: (entity: IEntity) => void
	): void;
}

interface EntityBroadcastMessage {
	eventName: string;
	entity?: IEntity;
	pk?: string;
	entityWrapper?: IEntityWrapper;
}

export class EntityBroadcastService implements IEntityBroadcastService {
	private broadcastChannel: BroadcastChannel;
	private updateEventName = "updateEntity";
	private deleteEventName = "deleteEntity";

	constructor(channelName = "entitywrapper-repository-channel") {
		this.broadcastChannel = new BroadcastChannel(channelName);
	}

	broadcastUpdate(pk: string, entityWrapper: IEntityWrapper): void {
		this.broadcastChannel.postMessage({
			eventName: this.updateEventName,
			pk,
			entityWrapper
		} as EntityBroadcastMessage);
	}

	broadcastDelete(entity: IEntity): void {
		this.broadcastChannel.postMessage({
			eventName: this.deleteEventName,
			entity
		} as EntityBroadcastMessage);
	}

	subscribe(
		onUpdate: (pk: string, entityWrapper: IEntityWrapper) => void,
		onDelete: (entity: IEntity) => void
	): void {
		this.broadcastChannel.onmessage = (event: MessageEvent<EntityBroadcastMessage>) => {
			if (event.data.eventName === this.updateEventName && event.data.pk && event.data.entityWrapper) {
				onUpdate(event.data.pk, event.data.entityWrapper);
			} else if (event.data.eventName === this.deleteEventName && event.data.entity) {
				onDelete(event.data.entity);
			}
		};
	}
}
