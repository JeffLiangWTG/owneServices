import { EntityBroadcastService } from "../EntityBroadcastService";
import { IEntityWrapper } from "../EntityWrapper";
import { IEntity } from "../models/IEntity";

const mockEntity: IEntity = { id: "1", name: "Test" } as any;
const mockEntityWrapper: IEntityWrapper = {
	entity: mockEntity,
	state: 0,
	name: "TestEntity",
	service: null
} as any;

describe("EntityBroadcastService", () => {
	let broadcastService: EntityBroadcastService;
	let mockPostMessage: jest.Mock;
	let mockChannel: any;

	beforeEach(() => {
		mockPostMessage = jest.fn();
		mockChannel = {
			postMessage: mockPostMessage,
			onmessage: null
		};
		(global as any).BroadcastChannel = jest.fn(() => mockChannel);
		broadcastService = new EntityBroadcastService("test-channel");
	});

	afterEach(() => {
		jest.clearAllMocks();
	});

	it("should broadcast update", () => {
		broadcastService.broadcastUpdate("1", mockEntityWrapper);
		expect(mockPostMessage).toHaveBeenCalledWith({
			eventName: "updateEntity",
			pk: "1",
			entityWrapper: mockEntityWrapper
		});
	});

	it("should broadcast delete", () => {
		broadcastService.broadcastDelete(mockEntity);
		expect(mockPostMessage).toHaveBeenCalledWith({
			eventName: "deleteEntity",
			entity: mockEntity
		});
	});

	it("should call onUpdate when receiving updateEntity event", () => {
		const onUpdate = jest.fn();
		const onDelete = jest.fn();
		broadcastService.subscribe(onUpdate, onDelete);

		const event = {
			data: {
				eventName: "updateEntity",
				pk: "1",
				entityWrapper: mockEntityWrapper
			}
		};
		// Simulate receiving a message
		mockChannel.onmessage(event);

		expect(onUpdate).toHaveBeenCalledWith("1", mockEntityWrapper);
		expect(onDelete).not.toHaveBeenCalled();
	});

	it("should call onDelete when receiving deleteEntity event", () => {
		const onUpdate = jest.fn();
		const onDelete = jest.fn();
		broadcastService.subscribe(onUpdate, onDelete);

		const event = {
			data: {
				eventName: "deleteEntity",
				entity: mockEntity
			}
		};
		mockChannel.onmessage(event);

		expect(onDelete).toHaveBeenCalledWith(mockEntity);
		expect(onUpdate).not.toHaveBeenCalled();
	});
});
