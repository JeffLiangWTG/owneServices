import { EntityWrapperRepository } from "../EntityWrapperRepository";
import { EntityState } from "../models/IEntity";
import { EntityWrapper } from "../EntityWrapper";

const mockEntity = { id: "1", name: "Test" };
const mockEntity2 = { id: "2", name: "Test2" };
const mockEntityName = "TestEntity";

jest.mock("../EntityHelper", () => ({
	EntityHelper: {
		getPK: (entity: any) => entity.id,
	},
}));

describe("EntityWrapperRepository", () => {
	let repo: EntityWrapperRepository;

	beforeEach(() => {
		repo = new EntityWrapperRepository();
	});

	it("should add an entity", () => {
		repo.add(mockEntity, mockEntityName);
		const all = repo.getAll();
		expect(all[mockEntity.id]).toBeInstanceOf(EntityWrapper);
		expect(all[mockEntity.id].entity).toEqual(mockEntity);
		expect(all[mockEntity.id].name).toBe(mockEntityName);
		expect(all[mockEntity.id].state).toBe(EntityState.Added);
	});

	it("should update an entity", () => {
		repo.add(mockEntity, mockEntityName);
		const updatedWrapper = new EntityWrapper(
			{ ...mockEntity, name: "Updated" },
			EntityState.Modified,
			mockEntityName,
			null
		);
		repo.update(mockEntity.id, updatedWrapper);
		expect(repo.get(mockEntity.id)?.entity.name).toBe("Updated");
		expect(repo.get(mockEntity.id)?.state).toBe(EntityState.Modified);
	});

	it("should delete an entity", () => {
		repo.add(mockEntity, mockEntityName);
		repo.delete(mockEntity);
		expect(repo.get(mockEntity.id)).toBeUndefined();
	});

	it("should clear all entities", () => {
		repo.add(mockEntity, mockEntityName);
		repo.add(mockEntity2, mockEntityName);
		repo.clear();
		expect(Object.keys(repo.getAll()).length).toBe(0);
	});

	it("should get an entity by pk", () => {
		repo.add(mockEntity, mockEntityName);
		const wrapper = repo.get(mockEntity.id);
		expect(wrapper).toBeInstanceOf(EntityWrapper);
		expect(wrapper?.entity).toEqual(mockEntity);
	});

	it("should get all entities", () => {
		repo.add(mockEntity, mockEntityName);
		repo.add(mockEntity2, mockEntityName);
		const all = repo.getAll();
		expect(Object.keys(all)).toContain(mockEntity.id);
		expect(Object.keys(all)).toContain(mockEntity2.id);
	});
});
