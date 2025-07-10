import { EntityManager, IMetaData, ServiceType } from "../EntityManager";
import IRefCusCodeType from "../models/IRefCusCodeType";
import { Filter, FilterOps } from "../Filter";
import { Mock, It, Times } from "typemoq";
import { IOData, OData } from "../OData";
import { EntityState } from "../models/IEntity";
import { HttpOData, Batch, oData } from "ts-odatajs";
import HttpStatus from "http-status-codes";
import ISourceDataUserView from "../models/ISourceDataUserView";
import { EntityWrapper } from "../EntityWrapper";

describe("EntityManager", () => {
	it("clear", async () => {
		let oData = Mock.ofType<IOData>();
		var type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_Description).returns(() => "aaaaa");
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		oData
			.setup((x) =>
				x.read(
					"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_Description eq 'aaaaa'&$top=1001"
				)
			)
			.returns(() => Promise.resolve([type.object]));
		let entityManager = new EntityManager();
		entityManager.oData = oData.object;
		await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false
		);
		expect(entityManager.entityWrapperRepository.get("1")).not.toBeUndefined();

		entityManager.clear();
		oData.verify((x) => x.clearCache(), Times.once());
		expect(entityManager.entityWrapperRepository.get("1")).toBeUndefined();
	});

	it("caching requests", async () => {
		let oData = Mock.ofType<IOData>();
		var type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_Description).returns(() => "aaaaa");
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		oData
			.setup((x) =>
				x.read(
					"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_Description eq 'aaaaa'&$top=1001"
				)
			)
			.returns(() => Promise.resolve([type.object]));
		let entityManager = new EntityManager();
		entityManager.oData = oData.object;
		await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false
		);
		expect(entityManager.entityWrapperRepository.get("1")).not.toBeUndefined();

		await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false
		);
		type.setup((x) => x.ZZK_PK).returns(() => "2");
		expect(entityManager.entityWrapperRepository.get("1")).not.toBeUndefined();
		expect(entityManager.entityWrapperRepository.get("2")).toBeUndefined();
		entityManager.clear();
		await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false
		);
		expect(entityManager.entityWrapperRepository.get("1")).toBeUndefined();
		expect(entityManager.entityWrapperRepository.get("2")).not.toBeUndefined();
	});

	it("getAsync", async () => {
		let OData = Mock.ofType<IOData>();
		var type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_Description).returns(() => "aaaaa");
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		OData.setup((x) =>
			x.read(
				"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_CodeType eq 'AA'&$top=1001"
			)
		).returns(() => Promise.resolve([type.object]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		let result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[new Filter("ZZK_CodeType", FilterOps.Equals, "AA" as any, "string")],
			false
		);
		expect(result.length).toEqual(0);
		OData.setup((x) =>
			x.read(
				"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_Description eq 'aaaaa'&$top=1001"
			)
		).returns(() => Promise.resolve([type.object]));
		entityManager = new EntityManager();
		entityManager.oData = OData.object;
		result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false
		);
		expect(result[0].ZZK_Description).toEqual("aaaaa");
		expect(entityManager.entityWrapperRepository.get("1")!.service).toEqual(ServiceType.Safe);
		expect(entityManager.entityWrapperRepository.get("1")!.state).toEqual(EntityState.Unchanged);
		expect(entityManager.entityWrapperRepository.get("1")!.entity).toEqual(type.object);
		expect(entityManager.entityWrapperRepository.get("1")!.name).toEqual("RefCusCodeType");
		OData.setup((x) =>
			x.read(
				"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_Description eq 'aaaaa'&$top=1001&SystemVersionUTC=2019-09-24T08:02:32.4541583Z"
			)
		).returns(() => Promise.resolve([type.object]));
		entityManager = new EntityManager();
		entityManager.oData = OData.object;
		result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Equals,
					"aaaaa" as any,
					"string"
				),
			],
			false,
			"2019-09-24T08:02:32.4541583Z"
		);
		expect(result.length).toEqual(1);
	});

	it("getAsync_IgnoreCase", async () => {
		let OData = Mock.ofType<IOData>();
		var type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_CodeType).returns(() => "AAA");
		type.setup((x) => x.ZZK_Description).returns(() => "Description");
		type.setup((x) => x.ZZK_PK).returns(() => "123");
		OData.setup((x) =>
			x.read(
				"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_CodeType eq 'Aaa'&$top=1001"
			)
		).returns(() => Promise.resolve([type.object]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		let result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[new Filter("ZZK_CodeType", FilterOps.Equals, "Aaa" as any, "string")],
			false
		);
		expect(result.length).toEqual(1);
		expect(result[0].ZZK_PK).toEqual("123");

		OData.setup((x) =>
			x.read(
				"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=contains(ZZK_Description,'des')&$top=1001"
			)
		).returns(() => Promise.resolve([type.object]));
		entityManager = new EntityManager();
		entityManager.oData = OData.object;
		result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[
				new Filter(
					"ZZK_Description",
					FilterOps.Contains,
					"des" as any,
					"string"
				),
			],
			false
		);
		expect(result.length).toEqual(1);
		expect(result[0].ZZK_Description).toEqual("Description");
	});

	it("getAsyncByDates", async () => {
		let OData = Mock.ofType<IOData>();
		var sda1 = Mock.ofType<ISourceDataUserView>();
		sda1.setup((x) => x.SDA_Filename).returns(() => "aaaaa");
		sda1
			.setup((x) => x.SDA_CreatedTime)
			.returns(() => new Date("2020-01-03T00:00:00.000Z"));
		sda1.setup((x) => x.SDA_PK).returns(() => "1");
		OData.setup((x) =>
			x.read(
				"http://localhost:17488/odata/SourceDataUserViewUpdate?$filter=((SDA_CreatedTime gt 2020-01-01T00:00:00.000Z) and (SDA_CreatedTime lt 2021-01-01T10:00:00.000Z))&$top=1001"
			)
		).returns(() => Promise.resolve([sda1.object]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		let result = await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			[
				new Filter(
					"SDA_CreatedTime",
					FilterOps.DateRange,
					"2020-01-01T00:00:00.000Z to 2021-01-01T10:00:00.000Z" as any,
					"datetime"
				),
			],
			false
		);
		expect(result.length).toEqual(1);
		expect(result[0].SDA_Filename).toEqual("aaaaa");
		expect(result[0].SDA_CreatedTime).toEqual(sda1.object.SDA_CreatedTime);
		expect(entityManager.entityWrapperRepository.get("1")!.entity).toEqual(sda1.object);
		expect(entityManager.entityWrapperRepository.get("1")!.name).toEqual("SourceDataUserView");
	});

	it("getAsync_byDates_Today", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			[
				new Filter(
					"SDA_CreatedTime",
					FilterOps.DateToday,
					"2020-01-01T00:00:00.000Z" as any,
					"datetime"
				),
			],
			false
		);
		OData.verify(
			(x) =>
				x.read(
					"http://localhost:17488/odata/SourceDataUserViewUpdate?$filter=SDA_CreatedTime gt 2019-12-31T13:00:00.000Z&$top=1001"
				),
			Times.once()
		);
	});

	it("getAsync_queryOptions_orderBy_single", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			[],
			false,
			undefined,
			{ orderBy: ["SDA_CreatedTime desc"] }
		);
		OData.verify(
			(x) =>
				x.read(
					"http://localhost:17488/odata/SourceDataUserViewUpdate?$top=1001&$orderby=SDA_CreatedTime desc"
				),
			Times.once()
		);
	});

	it("getAsync_queryOptions_orderBy_multiples", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			[],
			false,
			undefined,
			{ orderBy: ["SDA_CreatedTime asc", "SDA_SourceTime desc"] }
		);

		OData.verify(
			(x) =>
				x.read(
					"http://localhost:17488/odata/SourceDataUserViewUpdate?$top=1001&$orderby=SDA_CreatedTime asc,SDA_SourceTime desc"
				),
			Times.once()
		);
	});

	it("getAsync_exposeException", async () => {
		let OData = Mock.ofType<IOData>();
		let requestUri = "http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_CodeType eq 'AA'&$top=1001";
		let errorResponse = { message: "403: Error Reading data", statusCode: 403, requestUri: requestUri , statusText: "Forbidden" };
		OData.setup((x) =>
			x.read(
				requestUri
			)
		).returns(() => Promise.reject(errorResponse));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;

		const consoleSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

		await expect(entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[new Filter("ZZK_CodeType", FilterOps.Equals, "AA" as any, "string")],
			false
		)).rejects.toEqual(errorResponse);
		
		expect(consoleSpy).toHaveBeenCalledWith(errorResponse);

		consoleSpy.mockRestore();
	});

	it("getAsync_swallowException", async () => {
		let OData = Mock.ofType<IOData>();
		let requestUri = "http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=ZZK_CodeType eq 'AA'&$top=1001";
		let errorResponse = { message: "301: Error Reading data", statusCode: 301, requestUri: requestUri , statusText: "redirect" };
		OData.setup((x) =>
			x.read(
				requestUri
			)
		).returns(() => Promise.reject(errorResponse));
		let entityManager = new EntityManager();
		entityManager.oData = OData.object;

		const consoleSpy = jest.spyOn(console, 'log').mockImplementation(() => {});

		let result = await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[new Filter("ZZK_CodeType", FilterOps.Equals, "AA" as any, "string")],
			false
		);

		expect(result.length).toEqual(0);
		expect(consoleSpy).toHaveBeenCalledWith(errorResponse);

		consoleSpy.mockRestore();
	});

	it("FilterOps.In", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		var entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await entityManager.getAsync<IRefCusCodeType>(
			"RefCusCodeType",
			[ServiceType.Safe],
			[new Filter("ZZK_CodeType", FilterOps.In, ["AA", "BB"] as any, "string")],
			false
		);
		OData.verify(
			(x) =>
				x.read(
					"http://localhost:37016/odata/RefCusCodeTypeUpdate?$filter=((ZZK_CodeType eq 'AA') or (ZZK_CodeType eq 'BB'))&$top=1001"
				),
			Times.once()
		);
	});

	it("FilterOps.Date", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		var entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await entityManager.getAsync<ISourceDataUserView>(
			"SourceDataUserView",
			[ServiceType.Staging],
			[
				new Filter(
					"SDA_CreatedTime",
					FilterOps.DateRange,
					"2020-01-01T00:00:00.000Z to 2021-01-01T00:00:00.000Z" as any,
					"datetime"
				),
			],
			false
		);
		OData.verify(
			(x) =>
				x.read(
					"http://localhost:17488/odata/SourceDataUserViewUpdate?$filter=((SDA_CreatedTime gt 2020-01-01T00:00:00.000Z) and (SDA_CreatedTime lt 2021-01-01T00:00:00.000Z))&$top=1001"
				),
			Times.once()
		);
	});

	it("Invalid filter", async () => {
		let OData = Mock.ofType<IOData>();
		OData.setup((x) => x.read(It.isAny())).returns(() => Promise.resolve([]));
		var entityManager = new EntityManager();
		entityManager.oData = OData.object;
		await expect(
			entityManager.getAsync<IRefCusCodeType>(
				"RefCusCodeType",
				[ServiceType.Safe],
				[
					new Filter(
						"ZZK_CodeType",
						{} as FilterOps,
						["AA", "BB"] as any,
						"string"
					),
				],
				false
			)
		).rejects.toThrowError("Filter is not yet configured.");
	});

	it("add", () => {
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		var entityManager = new EntityManager();
		entityManager.oData = Mock.ofType<OData>().object;
		entityManager.add(type.object, "RefCusCodeType");
		var entityWrapper = entityManager.entityWrapperRepository.get("1")!;
		expect(entityWrapper.name).toEqual("RefCusCodeType");
		expect(entityWrapper.entity).toEqual(type.object);
		expect(entityWrapper.service).toBeNull();
		expect(entityWrapper.state).toEqual(EntityState.Added);
	});

	it("update", () => {
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		var entityManager = new EntityManager();
		entityManager.oData = Mock.ofType<OData>().object;
		entityManager.add(type.object, "RefCusCodeType");
		entityManager.update(type.object);
		expect(entityManager.entityWrapperRepository.get("1")!.state).toEqual(EntityState.Added);
		var entityWrapper = entityManager.entityWrapperRepository.get("1")!;
		entityWrapper.state = EntityState.Unchanged;
		entityManager.entityWrapperRepository.update("1", entityWrapper);
		entityManager.update(type.object);
		expect(entityManager.entityWrapperRepository.get("1")!.state).toEqual(EntityState.Modified);
	});

	it("remove", () => {
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		var entityManager = new EntityManager();
		entityManager.oData = Mock.ofType<OData>().object;
		entityManager.add(type.object, "RefCusCodeType");
		var entityWrapper = entityManager.entityWrapperRepository.get("1")!;
		entityWrapper.state = EntityState.Unchanged;
		entityManager.entityWrapperRepository.update("1", entityWrapper);
		entityManager.remove(type.object);
		expect(entityManager.entityWrapperRepository.get("1")!.state).toEqual(EntityState.Removed);
		var entityWrapper = entityManager.entityWrapperRepository.get("1")!;
		entityWrapper.state = EntityState.Added;
		entityManager.entityWrapperRepository.update("1", entityWrapper);
		entityManager.remove(type.object);
		expect(entityManager.entityWrapperRepository.get("1")).toBeUndefined();
	});

	it("isInDatabase", () => {
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		var entityManager = new EntityManager();
		entityManager.oData = Mock.ofType<OData>().object;
		entityManager.add(type.object, "RefCusCodeType");
		expect(entityManager.isInDatabase(type.object)).toBeFalsy();
		var entityWrapper = entityManager.entityWrapperRepository.get("1")!;
		entityWrapper.service = ServiceType.Safe;
		entityManager.entityWrapperRepository.update("1", entityWrapper);
		expect(entityManager.isInDatabase(type.object)).toBeTruthy();
	});

	it("saveChanges", async () => {
		let response = Mock.ofType<HttpOData.Response>();
		response.setup((x: any) => x.then).returns(() => undefined);
		let oData = Mock.ofType<OData>();
		oData
			.setup((x) => x.batch(It.isAnyString(), It.isAny()))
			.returns(() => new Promise((resolve) => resolve(response.object)));
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		let entityManager = new EntityManager();
		entityManager.oData = oData.object;
		entityManager.entityWrapperRepository.update("1", {
			state: EntityState.Added,
			service: ServiceType.Safe,
			entity: type.object,
			name: "RefCusCodeType",
		});
		await entityManager.saveChanges(ServiceType.Safe);
		oData.verify(
			(x) =>
				x.batch(
					It.isAnyString(),
					It.is<Batch.BatchRequest>((r) => {
						let request = r.__batchRequests[0]
							.__changeRequests[0] as HttpOData.Request;
						return (
							request.headers!["Content-ID"] == "0" &&
							request.data == type.object &&
							request.method == "POST"
						);
					})
				),
			Times.once()
		);
	});

	it("saveChanges_RemoveDeletedEntities", async () => {
		let changeResponse = Mock.ofType<Batch.ChangeResponse>();
		changeResponse.setup((x) => x.message).returns(() => undefined);
		let batchResponseSet: Batch.ChangeResponseSet[] = [
			{ __changeResponses: [changeResponse.object] },
		];

		let batchResponse = Mock.ofType<Batch.BatchResponse>();
		batchResponse
			.setup((x) => x.__batchResponses)
			.returns(() => batchResponseSet);

		let response = Mock.ofType<HttpOData.Response>();
		response.setup((x: any) => x.then).returns(() => undefined);
		response.setup((x) => x.statusCode).returns(() => HttpStatus.OK.toString());
		response.setup((x) => x.data).returns(() => batchResponse.object);

		let oData = Mock.ofType<OData>();
		oData
			.setup((x) => x.batch(It.isAnyString(), It.isAny()))
			.returns(() => new Promise((resolve) => resolve(response.object)));
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		let entityManager = new EntityManager();
		entityManager.oData = oData.object;
		entityManager.entityWrapperRepository.update("1", {
			state: EntityState.Removed,
			service: ServiceType.Safe,
			entity: type.object,
			name: "RefCusCodeType",
		});
		await entityManager.saveChanges(ServiceType.Safe);
		oData.verify((x) => x.clearCache(), Times.once());
		expect(entityManager.entityWrapperRepository.get("1")).toBeUndefined();
	});

	it("saveChanges clears odata cache", async () => {
		let changeResponse = Mock.ofType<Batch.ChangeResponse>();
		changeResponse.setup((x) => x.message).returns(() => undefined);
		let batchResponseSet: Batch.ChangeResponseSet[] = [
			{ __changeResponses: [changeResponse.object] },
		];

		let batchResponse = Mock.ofType<Batch.BatchResponse>();
		batchResponse
			.setup((x) => x.__batchResponses)
			.returns(() => batchResponseSet);

		let response = Mock.ofType<HttpOData.Response>();
		response.setup((x: any) => x.then).returns(() => undefined);
		response.setup((x) => x.statusCode).returns(() => HttpStatus.OK.toString());
		response.setup((x) => x.data).returns(() => batchResponse.object);

		let oData = Mock.ofType<OData>();
		oData
			.setup((x) => x.batch(It.isAnyString(), It.isAny()))
			.returns(() => new Promise((resolve) => resolve(response.object)));
		let type = Mock.ofType<IRefCusCodeType>();
		type.setup((x) => x.ZZK_PK).returns(() => "1");
		let entityManager = new EntityManager();
		entityManager.oData = oData.object;
		entityManager.entityWrapperRepository.update("1", {
			state: EntityState.Added,
			service: ServiceType.Safe,
			entity: type.object,
			name: "RefCusCodeType",
		});
		await entityManager.saveChanges(ServiceType.Safe);
		oData.verify((x) => x.clearCache(), Times.once());
		expect(entityManager.entityWrapperRepository.get("1")).not.toBeUndefined();
	});

	it("convertMetaDataTypeToODataType", () => {
		let entityManager = new EntityManager();
		expect(entityManager.convertMetaDataTypeToODataType("Edm.Guid")).toEqual(
			"guid"
		);
		expect(entityManager.convertMetaDataTypeToODataType("Edm.String")).toEqual(
			"string"
		);
		expect(entityManager.convertMetaDataTypeToODataType("UnknownType")).toEqual(
			""
		);
	});

	it("getMetaData", () => {
		let entityManager = new EntityManager();
		let response =
			'<edmx:Edmx xmlns:edmx="http://docs.oasis-open.org/odata/ns/edmx" Version="4.0">' +
			"<edmx:DataServices>" +
			'<Schema xmlns="http://docs.oasis-open.org/odata/ns/edm" Namespace="CargoWise.RefDbRepo.Service.Schema_0_9">' +
			'<EntityType Name="RefCusTariff">' +
			"<Key>" +
			'<PropertyRef Name="ZZ1_PK"/>' +
			"</Key>" +
			'<Property Name="ZZ1_PK" Type="Edm.Guid" Nullable="false"/>' +
			'<Property Name="ZZ1_ZZI_TariffType" Type="Edm.Guid" Nullable="false"/>' +
			'<Property Name="ZZ1_TariffCode" Type="Edm.String"/>' +
			'<Property Name="ZZ1_IAMUnique" Type="Edm.Int16" Nullable="false"/>' +
			'<Property Name="ZZ1_Description" Type="Edm.String"/>' +
			'<Property Name="ZZ1_StartDate" Type="Edm.DateTimeOffset" Nullable="false"/>' +
			'<Property Name="ZZ1_EndDate" Type="Edm.DateTimeOffset" Nullable="false"/>' +
			'<Property Name="ZZ1_ZZF_NKTaxOrFeeCode" Type="Edm.String"/>' +
			'<Property Name="ZZ1_ZZZ_NKDataGrouping" Type="Edm.String"/>' +
			'<Property Name="ZZ1_CompositeKeyOnZZ5" Type="Edm.String"/>' +
			'<Property Name="ZZ1_PublishedDate" Type="Edm.DateTimeOffset"/>' +
			'<Property Name="ZZ1_SysStartTime" Type="Edm.DateTimeOffset" Nullable="false"/>' +
			'<Property Name="ZZ1_SysEndTime" Type="Edm.DateTimeOffset" Nullable="false"/>' +
			'<NavigationProperty Name="RefCusConditions" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusCondition)"/>' +
			'<NavigationProperty Name="RefCusRates" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusRate)"/>' +
			'<NavigationProperty Name="RefCusTariffType" Type="CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffType"/>' +
			'<NavigationProperty Name="RefCusTariffAttributes" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffAttribute)"/>' +
			'<NavigationProperty Name="RefCusTariffLanguages" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffLanguage)"/>' +
			'<NavigationProperty Name="RefCusTariffNationalCodes" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffNationalCode)"/>' +
			'<NavigationProperty Name="RefCusTariffRelationships" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffRelationship)"/>' +
			'<NavigationProperty Name="RefCusTariffUOMs" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffUOM)"/>' +
			'<NavigationProperty Name="RefCusVATApplicabilities" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusVATApplicability)"/>' +
			'<NavigationProperty Name="RefCusTariffAdditionalCodes" Type="Collection(CargoWise.RefDbRepo.Service.Schema_0_9.RefCusTariffAdditionalCode)"/>' +
			"</EntityType>" +
			"</Schema>" +
			"</edmx:DataServices>" +
			"</edmx:Edmx>";
		entityManager.populateEntityMetaData(response);
		entityManager.getMetaData().then((metadata) => {
			expect(metadata.length).toEqual(13);
			expect(metadata[0].columnName).toEqual("ZZ1_PK");
			expect(metadata[0].columnType).toEqual("guid");
			expect(metadata[0].entityName).toEqual("RefCusTariff");
			expect(metadata[2].columnName).toEqual("ZZ1_TariffCode");
			expect(metadata[2].columnType).toEqual("string");
			expect(metadata[2].entityName).toEqual("RefCusTariff");
			expect(metadata[3].columnName).toEqual("ZZ1_IAMUnique");
			expect(metadata[3].columnType).toEqual("int");
			expect(metadata[3].entityName).toEqual("RefCusTariff");
			expect(metadata[5].columnName).toEqual("ZZ1_StartDate");
			expect(metadata[5].columnType).toEqual("datetime");
			expect(metadata[5].entityName).toEqual("RefCusTariff");
		});
	});

	it("getTypeFromMetaData", () => {
		let entityManager = new EntityManager();
		let entityMetaDataResult = new Array<IMetaData>();
		entityMetaDataResult.push({
			entityName: "RefCusTariff",
			columnName: "ZZ1_PK",
			columnType: "guid",
		});
		expect(
			entityManager.getTypeFromMetaData(
				"RefCusTariff",
				"ZZ1_PK",
				entityMetaDataResult
			)
		).toEqual("guid");
		expect(() =>
			entityManager.getTypeFromMetaData(
				"RefCusTariff",
				"ZZ1",
				entityMetaDataResult
			)
		).not.toThrow();
	});

	it("reload updates repository with wrappers", async () => {
		const entityManager = new EntityManager();
		const mockWrapper = {
			entity: { test_PK: "1", test_name: "Test 1" },
			state: EntityState.Unchanged,
			name: "TestEntity",
			service: ServiceType.Safe
		};
		entityManager.getEntityWrappers = jest.fn().mockResolvedValue([mockWrapper]);
		entityManager.entityWrapperRepository.add({ test_PK: "1", test_name: "Test 1" }, "TestEntity");
		const wrapper = entityManager.entityWrapperRepository.get("1")!;
		entityManager.entityWrapperRepository.update("1", new EntityWrapper(
			wrapper.entity,
			EntityState.Modified,
			wrapper.name,
			wrapper.service
		));

		await entityManager.reload("TestEntity", [ServiceType.Safe], []);
		expect(entityManager.entityWrapperRepository.get("1")!.state).toBe(EntityState.Unchanged);
		expect(entityManager.entityWrapperRepository.get("1")!.entity).toEqual({ test_PK: "1", test_name: "Test 1" });
	});

	it("saveChanges broadcasts updates and deletes", async () => {
		const entityManager = new EntityManager();
		entityManager.oData = {
			batch: jest.fn().mockResolvedValue({
				statusCode: "200",
				data: {
					__batchResponses: [
						{ __changeResponses: [{ message: undefined }] }
					]
				}
			}),
			clearCache: jest.fn()
		} as any;

		const broadcastUpdate = jest.fn();
		const broadcastDelete = jest.fn();
		entityManager.entityBroadcastService = {
			broadcastUpdate,
			broadcastDelete,
			subscribe: jest.fn()
		};

		const type = { test_PK: "1", name: "Test" };
		entityManager.entityWrapperRepository.update("1", {
			state: EntityState.Added,
			service: ServiceType.Safe,
			entity: type,
			name: "TestEntity"
		});

		await entityManager.saveChanges(ServiceType.Safe);

		expect(broadcastUpdate).toHaveBeenCalledWith(
			"1",
			expect.objectContaining({
				entity: type,
				state: EntityState.Unchanged,
				name: "TestEntity",
				service: ServiceType.Safe
			})
		);

		entityManager.entityWrapperRepository.update("1", {
			state: EntityState.Removed,
			service: ServiceType.Safe,
			entity: type,
			name: "TestEntity"
		});
		await entityManager.saveChanges(ServiceType.Safe);
		expect(broadcastDelete).toHaveBeenCalledWith(type);
	});
});
