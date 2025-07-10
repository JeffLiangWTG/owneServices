//@ts-ignore
import buildQuery from "odata-query";
import { IFilter, FilterOps, Filter } from "./Filter";
import { Batch } from "ts-odatajs";
import { IEntity, EntityState } from "./models/IEntity";
import { IOData, OData } from "./OData";
import { IEntityWrapper, EntityWrapper } from "./EntityWrapper";
import HttpStatus from "http-status-codes";
import _ from "underscore";
import { EntityHelper } from "./EntityHelper";
import axios from "axios";
import moment from "moment";
import IODataQueryOptions from "./models/IODataQueryOptions";
import { IEntityWrapperRepository, EntityWrapperRepository } from "./EntityWrapperRepository";
import { IEntityBroadcastService, EntityBroadcastService } from "./EntityBroadcastService";

declare var __StagingAPI__: string;
declare var __SafeAPI__: string;

const MaxResults = 1000;

export enum ServiceType {
	Safe,
	Staging,
}

export interface IEntityManager {
	getAsync<T extends IEntity>(
		entityName: string,
		serviceTypes: ServiceType[],
		filters: IFilter[],
		reload: boolean,
		systemVersion?: string,
		queryOptions?: IODataQueryOptions
	): Promise<T[]>;
	saveChanges(
		serviceType: ServiceType
	): Promise<{ success: boolean; message: string }>;
	add(entity: IEntity, entityName: string): void;
	update(entity: IEntity): void;
	remove(entity: IEntity): void;
	isInDatabase(entityOrPK: IEntity | string): boolean;
	clear(): void;
	filterEntities<T extends IEntity>(entities: T[], filters: IFilter[]): T[];
	getTypeFromMetaData(
		entityName: string,
		propertyName: string,
		metaData: IMetaData[]
	): string;
	getEntityMetaData(): Promise<IMetaData[]>;
	initialise(): Promise<void>;
	reload(entityName: string, serviceTypes: ServiceType[], filters: IFilter[]): Promise<void>;
}

export interface IMetaData {
	entityName: string;
	columnName: string;
	columnType: string;
}

export class EntityManager implements IEntityManager {
	oData: IOData;
	entityMetaData: Promise<IMetaData[]>;
	entityWrapperRepository: IEntityWrapperRepository;
	entityBroadcastService: IEntityBroadcastService;

	constructor() {
		this.getRequestMethod = this.getRequestMethod.bind(this);
		this.oData = new OData();
		this.entityWrapperRepository = new EntityWrapperRepository();
		this.entityBroadcastService = new EntityBroadcastService();
		this.entityBroadcastService.subscribe(
			(pk, entityWrapper) => this.entityWrapperRepository.update(pk, entityWrapper),
			(entity) => this.entityWrapperRepository.delete(entity)
		)
	}

	async initialise(): Promise<void> {
		this.entityMetaData = this.getMetaData();
	}

	getEntityMetaData(): Promise<IMetaData[]> {
		return this.entityMetaData;
	}

	async getMetaData(): Promise<IMetaData[]> {
		let requestUrl = __SafeAPI__ + "$metadata";
		var config = {
			headers: { Accept: "text/xml" },
		};
		let response = await axios.get(requestUrl, config);
		return await this.populateEntityMetaData(response.data);
	}

	populateEntityMetaData(response: string): IMetaData[] {
		let result = new Array<IMetaData>();
		var parser = new DOMParser();
		var xmlDoc = parser.parseFromString(response, "text/xml");
		if (xmlDoc != undefined) {
			var schemas = xmlDoc.getElementsByTagName("Schema");
			if (schemas.length != 0) {
				var entities = schemas[0].getElementsByTagName("EntityType");
				for (let index = 0; index < entities.length; index++) {
					let entityName = entities[index].getAttribute("Name");
					for (
						let childIndex = 0;
						childIndex <
						entities[index].getElementsByTagName("Property").length;
						childIndex++
					) {
						let columnName = entities[index]
							.getElementsByTagName("Property")
							[childIndex].getAttribute("Name");
						let columnType = this.convertMetaDataTypeToODataType(
							entities[index]
								.getElementsByTagName("Property")
								[childIndex].getAttribute("Type") || ""
						);
						if (
							entityName != undefined &&
							columnName != undefined &&
							columnType != undefined
						) {
							let value: IMetaData = {
								entityName: entityName,
								columnName: columnName,
								columnType: columnType,
							};
							result.push(value);
						}
					}
				}
			}
		}
		return result;
	}

	getTypeFromMetaData(
		entityName: string,
		propertyName: string,
		metaData: IMetaData[]
	): string {
		let columnType: string = "";
		let filteredMetaData = metaData.filter(
			(x) => x.entityName == entityName && x.columnName == propertyName
		);
		if (filteredMetaData && filteredMetaData.length > 0) {
			columnType = filteredMetaData[0].columnType;
		}
		return columnType;
	}

	convertMetaDataTypeToODataType(type: string): string {
		switch (type) {
			case "Edm.Guid":
				return "guid";
				break;
			case "Edm.String":
				return "string";
				break;
			case "Edm.Int16":
				return "int";
				break;
			case "Edm.DateTimeOffset":
				return "datetime";
				break;
			case "Edm.Boolean":
				return "bit";
				break;
			case "Edm.Decimal":
				return "decimal";
				break;
			case "Edm.Byte":
				return "byte";
				break;
			case "Edm.Int32":
				return "bigint";
				break;
			default:
				return "";
				break;
		}
	}

	clear(): void {
		this.entityWrapperRepository.clear();
		this.oData.clearCache();
	}

	async reload(entityName: string, serviceTypes: ServiceType[], filters: IFilter[]): Promise<void> {
		const wrapperDict: { [pk: string]: IEntityWrapper } = {};
		const wrappers = await this.getEntityWrappers(entityName, serviceTypes, filters);
		wrappers.forEach((w) => {
			const wrapperPk = EntityHelper.getPK(w.entity);
			wrapperDict[wrapperPk] = w;
		});

		const entityWrappers = this.entityWrapperRepository.getAll();
		Object.getOwnPropertyNames(entityWrappers).forEach((pk) => {
			const entityWrapper = entityWrappers[pk];
			if (entityWrapper.name == entityName && entityWrapper.state != EntityState.Unchanged) {
				const w = wrapperDict[pk];
				if (w) {
					this.entityWrapperRepository.update(pk, w);
				} else {
					this.entityWrapperRepository.delete(entityWrapper.entity);
				}
			}
		});
	}

	async getEntityWrappers<T extends IEntity>(
		entityName: string,
		serviceTypes: ServiceType[],
		filters: IFilter[],
		systemVersion?: string,
		queryOptions?: IODataQueryOptions
	) : Promise<IEntityWrapper[]> {
		var url = entityName + "Update";
		var query: any = { top: MaxResults + 1 };
		if (queryOptions) {
			this.includeQueryOptionsIntoQuery(query, queryOptions);
		}
		if (filters) {
			query["filter"] = this.createOQueryFilter(filters);
		}
		url = url + buildQuery(query);
		if (systemVersion) {
			url = url + "&SystemVersionUTC=" + systemVersion;
		}
		const exposeStatusCodes = [HttpStatus.UNAUTHORIZED, HttpStatus.FORBIDDEN];
		let requests = serviceTypes.map((s) =>
			this.oData
				.read<T>(
					(s == ServiceType.Staging ? __StagingAPI__ : __SafeAPI__) + url
				)
				.then((data) =>
					data.map(
						(x) => new EntityWrapper(x, EntityState.Unchanged, entityName, s)
					)
				).catch((err) => {
					console.log(err);
					if (exposeStatusCodes.includes(err.statusCode)) {
						return Promise.reject(err);
					} else {
						return [];
					}
				}
			)
		);
		let wrappers = ([] as IEntityWrapper[]).concat.apply(
			[],
			await Promise.all(requests)
		);

		return wrappers;
	}

	async getAsync<T extends IEntity>(
		entityName: string,
		serviceTypes: ServiceType[],
		filters: IFilter[],
		reload: boolean,
		systemVersion?: string,
		queryOptions?: IODataQueryOptions
	): Promise<T[]> {
		let wrappers = await this.getEntityWrappers<T>(entityName, serviceTypes, filters, systemVersion, queryOptions);
		wrappers.forEach((w) => {
			let pk = EntityHelper.getPK(w.entity);
			if (reload || !this.entityWrapperRepository.get(pk)) {
				this.entityWrapperRepository.update(pk, w);
			}
		});
		var result: T[] = [];
		Object.getOwnPropertyNames(this.entityWrapperRepository.getAll())
			.map((pk) => this.entityWrapperRepository.get(pk)!)
			.forEach((x) => {
				if (x.name == entityName && x.state != EntityState.Removed) {
					result.push(x.entity as T);
				}
			});
		return this.filterEntities(result, filters);
	}

	filterEntities<T extends IEntity>(entities: T[], filters: IFilter[]): T[] {
		let results: T[] = [];
		entities.forEach((x) => {
			let matches = filters.length;
			filters.forEach((y) => {
				switch (y.operation) {
					case FilterOps.Equals:
						if (
							x[y.propertyName]?.toString().toLowerCase() ==
							y.value.toString().toLowerCase()
						) {
							matches -= 1;
						}
						break;
					case FilterOps.NotEquals:
						if (x[y.propertyName] != y.value) {
							matches -= 1;
						}
						break;
					case FilterOps.Contains:
						if (
							x[y.propertyName]
								?.toString()
								.toLowerCase()
								.includes(y.value.toString().toLowerCase())
						) {
							matches -= 1;
						}
						break;
					case FilterOps.In:
						if ((x[y.propertyName] as string[]).includes(y.value.toString())) {
							matches -= 1;
						}
						break;
					case FilterOps.DateToday:
						let dateAsStartOfDay = moment(new Date(y.value.toString()))
							.startOf("day")
							.toDate();
						if (dateAsStartOfDay <= new Date(x[y.propertyName])) {
							matches -= 1;
						}
						break;
					case FilterOps.DateRange:
					case FilterOps.DateYesterday:
					case FilterOps.DateSevenDaysAgo:
					case FilterOps.DateForteenDaysAgo:
					case FilterOps.DateLastMonth:
						let valueAsString = y.value.toString();
						const dateFrom = valueAsString
							.substring(0, valueAsString.indexOf("to"))
							.trim();
						const dateTo = valueAsString
							.substring(valueAsString.lastIndexOf("to") + "to".length)
							.trim();

						let dateFromAsStartOfDay = moment(new Date(dateFrom))
							.startOf("day")
							.toDate();
						let dateToAsEndOfDay = moment(new Date(dateTo))
							.endOf("day")
							.toDate();
						let dates = [dateFromAsStartOfDay, dateToAsEndOfDay].map((x) => x);
						if (
							new Date(x[y.propertyName]) >= dates[0] &&
							new Date(x[y.propertyName]) <= dates[1]
						) {
							matches -= 1;
						}
						break;
				}
			});
			if (matches == 0) {
				results.push(x as T);
			}
		});
		return results;
	}

	add(entity: IEntity, entityName: string): void {
		this.entityWrapperRepository.add(entity, entityName);
	}

	update(entity: IEntity) {
		let pk = EntityHelper.getPK(entity);
		let wrapper = this.entityWrapperRepository.get(pk)!;
		this.entityWrapperRepository.update(pk, new EntityWrapper(
			entity,
			wrapper.state == EntityState.Added
				? EntityState.Added
				: EntityState.Modified,
			wrapper.name,
			wrapper.service
		));
	}

	remove(entity: IEntity) {
		var pk = EntityHelper.getPK(entity);
		let wrapper = this.entityWrapperRepository.get(pk);
		if (!wrapper || wrapper.state == EntityState.Added) {
			this.entityWrapperRepository.delete(entity);
		} else {
			wrapper.state = EntityState.Removed;
			this.entityWrapperRepository.update(pk, wrapper);
		}
	}

	isInDatabase(entityOrPK: IEntity | string): boolean {
		let pk =
			typeof entityOrPK == "string"
				? entityOrPK
				: EntityHelper.getPK(entityOrPK);
		return this.entityWrapperRepository.get(pk) != undefined && this.entityWrapperRepository.get(pk)!.service != null;
	}

	async saveChanges(
		serviceType: ServiceType
	): Promise<{ success: boolean; message: string }> {
		let wrappers = Object.getOwnPropertyNames(this.entityWrapperRepository.getAll())
			.map((pk) => this.entityWrapperRepository.get(pk)!)
			.filter(
				(w) =>
					w.state != EntityState.Unchanged &&
					(w.service == serviceType || w.service == null)
			);
		if (wrappers.length > 0) {
			let baseUrl =
				serviceType == ServiceType.Safe ? __SafeAPI__ : __StagingAPI__;
			let requests = wrappers.map((w, i) => {
				let request: Batch.ChangeRequest = {
					headers: { "Content-ID": i.toString() },
					requestUri:
						baseUrl +
						w.name +
						"Update" +
						(w.state != EntityState.Added
							? "(" + EntityHelper.getPK(w.entity) + ")"
							: ""),
					data: w.entity,
					method: this.getRequestMethod(w.state),
				};
				return request;
			});
			let batchRequest: Batch.BatchRequest = {
				__batchRequests: [{ __changeRequests: requests }],
			};
			let response = await this.oData.batch(baseUrl, batchRequest);
			if (response.statusCode == HttpStatus.OK.toString()) {
				let batchResponse = response.data as Batch.BatchResponse;
				let changeResponses =
					batchResponse.__batchResponses[0].__changeResponses;
				if (_.all(changeResponses, (r) => r.message == undefined)) {
					wrappers.forEach((w) => {
						if (w.state != EntityState.Removed) {
							var entityPk = EntityHelper.getPK(w.entity);
							var savedEntityWrapper = new EntityWrapper(
									w.entity,
									EntityState.Unchanged,
									w.name,
									serviceType
								);
							this.entityWrapperRepository.update(
								entityPk,
								savedEntityWrapper
							);
							this.entityBroadcastService.broadcastUpdate(entityPk, savedEntityWrapper);
						} else {
							this.entityWrapperRepository.delete(w.entity);
							this.entityBroadcastService.broadcastDelete(w.entity);
						}
					});
					this.oData.clearCache();
					return { success: true, message: "Saving is successful." };
				} else {
					let result = "";
					changeResponses.forEach((x) => {
						if (x.message != undefined) {
							result =
								result +
								"\r\n" +
								(x.response as Batch.ChangeResponse).statusText;
						}
					});
					return { success: false, message: result };
				}
			} else {
				return {
					success: false,
					message: "There is an error while saving " + response.statusText,
				};
			}
		} else {
			return { success: false, message: "There is nothing to save." };
		}
	}

	private getRequestMethod(entityState: EntityState): string {
		switch (entityState) {
			case EntityState.Added:
				return "POST";
			case EntityState.Modified:
				return "PUT";
			default:
				return "DELETE";
		}
	}

	private createOQueryFilter(filters: IFilter[]): any {
		var result: any = {};
		filters.forEach((f) => {
			if (f.value !== undefined) {
				switch (f.operation) {
					case FilterOps.Equals:
						if (f.type == "datetime") {
							const simpleDate = new Date(f.value as any);
							result[f.propertyName] = simpleDate;
						} else {
							result[f.propertyName] = this.getOdataValue(
								f.operation,
								f.type,
								f.value
							);
						}
						break;
					case FilterOps.Contains:
						result[f.propertyName] = { contains: f.value };
						break;
					case FilterOps.NotEquals:
						result[f.propertyName] = this.getOdataValue(
							f.operation,
							f.type,
							f.value
						);
						break;
					case FilterOps.In:
						let values = f.value as any[];
						result["or"] = values.map((v) => {
							let r: any = {};
							r[f.propertyName] = this.getOdataValue(
								FilterOps.Equals,
								f.type,
								v
							);
							return r;
						});
						break;
					case FilterOps.DateToday:
						let dr: any = {};
						dr["gt"] = moment(new Date(f.value.toString()))
							.startOf("day")
							.toDate();
						result[f.propertyName] = dr;
						break;
					case FilterOps.DateRange:
					case FilterOps.DateForteenDaysAgo:
					case FilterOps.DateLastMonth:
					case FilterOps.DateSevenDaysAgo:
					case FilterOps.DateYesterday:
						if (f.type != "datetime") {
							throw new Error(
								"Cannot use Date filter in a field that is not a Date."
							);
						} else {
							let valueAsString = f.value.toString();
							const dateFrom = valueAsString
								.substring(0, valueAsString.indexOf("to"))
								.trim();
							const dateTo = valueAsString
								.substring(valueAsString.lastIndexOf("to") + "to".length)
								.trim();

							let dateFromAsStartOfDay = moment(new Date(dateFrom))
								.startOf("day")
								.toDate();
							let dateToAsEndOfDay = moment(new Date(dateTo))
								.endOf("day")
								.toDate();

							if (f.operation == FilterOps.DateRange) {
								dateFromAsStartOfDay = new Date(dateFrom);
								dateToAsEndOfDay = new Date(dateTo);
							}

							let values = [dateFromAsStartOfDay, dateToAsEndOfDay];
							result["and"] = values.map((v, i) => {
								let r: any = {};
								let dr: any = {};

								if (i == 0) {
									dr["gt"] = new Date(v);
								} else {
									dr["lt"] = new Date(v);
								}
								r[f.propertyName] = dr;
								return r;
							});
						}
						break;
					default:
						throw new Error("Filter is not yet configured.");
				}
			}
		});
		return result;
	}

	private getOdataValue(op: FilterOps, type: string, value: any): any {
		let result: any = {};
		let oValue = type === "guid" ? { type: type, value: value } : value;
		result[op == FilterOps.Equals ? "eq" : "ne"] = oValue;
		return result;
	}

	private includeQueryOptionsIntoQuery(
		query: any,
		oDataQueryOption: IODataQueryOptions
	) {
		if (oDataQueryOption.orderBy) {
			query["orderBy"] = oDataQueryOption.orderBy;
		}
	}
}
