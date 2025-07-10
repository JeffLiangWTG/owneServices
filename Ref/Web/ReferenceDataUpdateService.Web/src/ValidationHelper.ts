import {
	IValidationService,
	ValidationServiceWrapper,
} from "./ValidationService";
import { IEntity } from "./models/IEntity";
import { IEntityManager, ServiceType } from "./EntityManager";
import { IValidationResults } from "./ValidationResults";
import { Filter, FilterOps, IFilter } from "./Filter";
import { EntityHelper } from "./EntityHelper";
import { RefCusCodeListAttributeUserView } from "./models/RefCusCodeListAttributeUserView";
import IRefCusCodeListAttributeName from "./models/IRefCusCodeListAttributeName";
import { RefCusCodeListUserView } from "./models/RefCusCodeListUserView";
import {
	RefCusCodeListFRFallbackInvokeForm,
	IApplicationList,
} from "./RefCusCodeListFRFallbackInvokeForm";
import { IRefApplicationAttribute } from "./models/IRefApplicationAttribute";
import RefApplicationAttributeTypeEnum from "./models/RefApplicationAttributeTypeEnum";
import axios from "axios";

declare var __SafeAPI__: string;

export class ValidationHelper {
	static isRequired(value: any): string {
		if (!value) {
			return "This field is required.";
		}
		return "";
	}

	static isNotEmptyString(value: any): string {
		if (!value || value.length == 0) {
			return "Value cannot be empty";
		}
		return "";
	}

	static async listValidation(
		value: string,
		listEntityName: string,
		listCodePropertyName: string,
		entityManager: IEntityManager,
		operation: FilterOps = FilterOps.Equals,
		serviceType: ServiceType = ServiceType.Safe,
		type: string = "string"
	): Promise<string> {
		if (value) {
			let filter: IFilter = new Filter(
				listCodePropertyName,
				operation,
				value as any,
				type
			);
			return await this.filterListValidation(
				value,
				listEntityName,
				listCodePropertyName,
				[filter],
				entityManager,
				serviceType
			);
		}
		return "";
	}

	static async filterListValidation(
		value: string,
		listEntityName: string,
		listCodePropertyName: string,
		filters: IFilter[],
		entityManager: IEntityManager,
		serviceType: ServiceType = ServiceType.Safe
	): Promise<string> {
		if (value) {
			if (!filters.find((x) => x.propertyName == listCodePropertyName)) {
				let filterValue = value as any;
				filters.push({
					propertyName: listCodePropertyName,
					operation: FilterOps.Equals,
					type: "string",
					value: filterValue,
				});
			}
			let values = await entityManager.getAsync<IEntity>(
				listEntityName,
				[serviceType],
				filters,
				false
			);
			if (!values || values.length == 0) {
				return "This value is not in the list";
			}
		}
		return "";
	}

	static async dateOverlapValidation(
		entityManager: IEntityManager,
		entityName: string,
		entity: IEntity,
		uniqueCols: string[]
	): Promise<string> {
		let startDateColumn = EntityHelper.getPropertyEndsWith(
			entity,
			"_StartDate"
		);
		let endDateColumn = EntityHelper.getPropertyEndsWith(entity, "_EndDate");
		let isPublishColumn = EntityHelper.getPropertyEndsWith(
			entity,
			"_IsPublished"
		);
		let startDateValue = Date.parse(entity[startDateColumn]);
		let endDateValue = Date.parse(entity[endDateColumn]);
		let isPublished = entity[isPublishColumn] as boolean;
		if (!isNaN(startDateValue) && !isNaN(endDateValue) && isPublished) {
			let filters = uniqueCols.map(
				(c) => new Filter(c, FilterOps.Equals, entity[c], "")
			);
			let primaryKeyColumn = EntityHelper.getPropertyEndsWith(entity, "_PK");
			let matchedRecords = await entityManager.getAsync(
				entityName,
				[ServiceType.Safe],
				filters,
				false
			);
			matchedRecords = matchedRecords.filter(
				(x) => (x as IEntity)[isPublishColumn] == true
			);
			let result = false;
			matchedRecords.forEach((x) => {
				let unsavedRecord = x as IEntity;
				if (
					unsavedRecord[primaryKeyColumn].toString() !=
						entity[primaryKeyColumn].toString() &&
					((Date.parse(unsavedRecord[startDateColumn]) >= startDateValue &&
						Date.parse(unsavedRecord[startDateColumn]) <= endDateValue) ||
						(Date.parse(unsavedRecord[endDateColumn]) >= startDateValue &&
							Date.parse(unsavedRecord[endDateColumn]) <= endDateValue))
				) {
					result = true;
				}
			});
			return result
				? entityName +
						" (" +
						uniqueCols.map((c) => entity[c]).join() +
						") date(s) overlaps with existing record."
				: "";
		}
		return "";
	}

	static isGreaterThanOrEqualOne(value: number): string {
		if (value < 1) {
			return "This field is required to be greater than 1";
		}
		return "";
	}

	static isGreaterThanOrEqualZero(value: number): string {
		if (value < 0) {
			return "This field is required to be greater than 0";
		}
		return "";
	}

	static isUpperCase(value: string): string {
		for (let i = 0; i < value.length; i++) {
			let char = value.charAt(i);
			if (char >= "a" && char <= "z") {
				return "This field can't contain lowercase letters";
			}
		}
		return "";
	}

	static startDateAndEndDateValidation(
		startDate: string,
		endDate: string
	): string {
		if (new Date(startDate) > new Date(endDate)) {
			return "The start date should not be later than the end date";
		}
		return "";
	}

	static dateRangeValidation(
		dateToValidate: string,
		minimumDate?: string,
		maximumDate?: string
	): string {
		let date = new Date(dateToValidate);
		if (minimumDate === undefined || new Date(minimumDate) === undefined) {
			minimumDate = "1900-01-01T00:00:00Z";
		}
		if (maximumDate === undefined || new Date(maximumDate) === undefined) {
			maximumDate = "2079-06-06T23:59:59Z";
		}
		if (date > new Date(maximumDate)) {
			let maxDate = new Date(maximumDate);
			maxDate.setMinutes(maxDate.getMinutes() + maxDate.getTimezoneOffset());
			return "Date cannot be later than " + maxDate.toLocaleDateString();
		}
		if (date < new Date(minimumDate)) {
			let minDate = new Date(minimumDate);
			minDate.setMinutes(minDate.getMinutes() + minDate.getTimezoneOffset());
			return "Date cannot be earlier than " + minDate.toLocaleDateString();
		}
		return "";
	}

	static validateLength(value: string, length: number): string {
		if (value.length != length) {
			return "This field must have " + length + " characters";
		}
		return "";
	}

	static async validateDuplication(
		entityManager: IEntityManager,
		entityName: string,
		entity: IEntity,
		uniqueCols: string[],
		serviceType: ServiceType = ServiceType.Safe
	): Promise<string> {
		let metaData = await entityManager.getEntityMetaData();
		let filters = uniqueCols.map(
			(c) =>
				new Filter(
					c,
					FilterOps.Equals,
					entity[c],
					entityManager.getTypeFromMetaData(entityName, c, metaData)
				)
		);
		let pkCol = EntityHelper.getPropertyEndsWith(entity, "_PK");
		filters.push(new Filter(pkCol, FilterOps.NotEquals, entity[pkCol], "guid"));
		let duplicated = await entityManager.getAsync(
			entityName,
			[serviceType],
			filters,
			false
		);
		return duplicated.length > 0
			? entityName +
					" (" +
					uniqueCols.map((c) => entity[c]).join() +
					") already exists."
			: "";
	}

	static async validateProperty(
		validationService: ValidationServiceWrapper,
		entity: IEntity,
		propertyName: string,
		validationServiceArrayPosition: number = 0,
		isSaving = false
	): Promise<string[]> {
		if (
			validationService.GetValidationService(validationServiceArrayPosition)[
				propertyName
			]
		) {
			validationService.IncreaseValidationCount();
			let result = null;
			result = (
				await Promise.all(
					validationService
						.GetValidationService(validationServiceArrayPosition)
						[propertyName].map((v) => v(entity, propertyName, isSaving))
				)
			).filter((r) => r != "");
			validationService.DecreaseValidationCount();
			return result;
		}
		return [];
	}

	static async validateFilterProperty(
		validationService: IValidationService,
		entity: IEntity,
		propertyName: string,
		isSaving = false
	): Promise<string[]> {
		if (validationService[propertyName]) {
			return (
				await Promise.all(
					validationService[propertyName].map((v) =>
						v(entity, propertyName, isSaving)
					)
				)
			).filter((r) => r != "");
		}
		return [];
	}

	static getValidationResults(
		propertyName: string,
		validationResults?: IValidationResults
	): string[] {
		if (validationResults && validationResults[propertyName]) {
			return validationResults[propertyName];
		}
		return [];
	}

	static hasErrors(validationResults: IValidationResults[]) {
		return (
			validationResults.find(
				(r) =>
					Object.getOwnPropertyNames(r).find(
						(p) => ValidationHelper.getValidationResults(p, r).length > 0
					) != undefined
			) != undefined
		);
	}

	static async validate(
		validationService: ValidationServiceWrapper,
		entity: IEntity,
		validationServiceArrayPosition: number = 0,
		isSaving = false
	): Promise<IValidationResults> {
		let results: IValidationResults = {};
		await Promise.all(
			Object.getOwnPropertyNames(
				validationService.GetValidationService(validationServiceArrayPosition)
			).map((p) =>
				ValidationHelper.validateProperty(
					validationService,
					entity,
					p,
					validationServiceArrayPosition,
					isSaving
				).then((r) => {
					results[p] = r;
				})
			)
		);
		return results;
	}

	static async refCusCodeListAttribute_validations(
		entityManager: IEntityManager,
		codeType: string,
		countryOrGrouping: string,
		codeListPk: string,
		nameToValidate: any,
		pkToValidate: any
	) {
		let filterAttrNames = [
			new Filter(
				"ZXE_ZZK_NKCodeType",
				FilterOps.Equals,
				codeType as any,
				"string"
			),
			new Filter(
				"ZXE_ZZZ_NKDataGrouping",
				FilterOps.Equals,
				countryOrGrouping as any,
				"string"
			),
		];

		let filtersAttributeUserView = [
			new Filter("ZZE_CodeType", FilterOps.Equals, codeType as any, "string"),
			new Filter(
				"ZZE_CountryOrGrouping",
				FilterOps.Equals,
				countryOrGrouping as any,
				"string"
			),
			new Filter(
				"ZZE_ZZD_CodeList",
				FilterOps.Equals,
				codeListPk as any,
				"guid"
			),
		];

		if (nameToValidate) {
			filterAttrNames.push(
				new Filter(
					"ZXE_Name",
					FilterOps.Equals,
					nameToValidate as any,
					"string"
				)
			);
		}

		let attrNames = await entityManager.getAsync<IRefCusCodeListAttributeName>(
			"RefCusCodeListAttributeName",
			[ServiceType.Safe],
			filterAttrNames,
			false
		);
		let messages: string[] = [];
		let result = "";
		if (attrNames && attrNames.length > 0) {
			let idx = 0;
			let currentItems =
				await entityManager.getAsync<RefCusCodeListAttributeUserView>(
					"RefCusCodeListAttributeUserView",
					[ServiceType.Safe],
					filtersAttributeUserView,
					false
				);

			if (nameToValidate == null && pkToValidate == null) {
				await Promise.all(
					attrNames
						.filter((x) => x.ZXE_IsMandatory)
						.map(async (attrName) => {
							if (
								currentItems.find(
									(o) =>
										o.ZZE_ZXE_NKName == attrName.ZXE_Name &&
										o.ZZE_CountryOrGrouping == attrName.ZXE_ZZZ_NKDataGrouping
								) == null
							) {
								messages[idx] =
									"Mandatory attribute " +
									attrName.ZXE_Name +
									", " +
									attrName.ZXE_Description +
									" is required";
								idx++;
							}
						})
				);
			}

			if (nameToValidate) {
				await Promise.all(
					attrNames
						.filter((x) => !x.ZXE_AllowDuplicates)
						.map(async (attrName) => {
							if (
								currentItems.filter(
									(o) =>
										o.ZZE_ZXE_NKName == attrName.ZXE_Name &&
										o.ZZE_CountryOrGrouping == attrName.ZXE_ZZZ_NKDataGrouping
								).length > 1
							) {
								messages[idx] =
									"Duplicate attribute name '" +
									attrName.ZXE_Name +
									"' is not allowed.";
								idx++;
							}
						})
				);
			}

			if (pkToValidate) {
				await Promise.all(
					attrNames
						.filter((x) => x.ZXE_IsValueMandatory)
						.map(async (attrName) => {
							if (
								currentItems.filter(
									(o) =>
										pkToValidate == o.ZZE_PK &&
										o.ZZE_ZXE_NKName == attrName.ZXE_Name &&
										o.ZZE_CountryOrGrouping ==
											attrName.ZXE_ZZZ_NKDataGrouping &&
										o.ZZE_Value == ""
								).length > 0
							) {
								messages[idx] =
									"Value is mandatory for " + attrName.ZXE_Name + ".";
								idx++;
							}
						})
				);
			}
		} else if (nameToValidate && (!attrNames || attrNames.length == 0)) {
			messages.push("This value is not in the list");
		}
		messages.map(
			(x, idx) =>
				(result +=
					x + (messages.length > 1 && idx < messages.length ? "\n" : ""))
		);
		return result;
	}

	static async refCusCodeListAttribute_validateValueIsInTheList(
		entityManager: IEntityManager,
		codeType: string,
		countryOrGrouping: string,
		nameToValidate: string,
		valueToValidate: string
	): Promise<string> {
		let filtersAttrNames = [
			new Filter(
				"ZXE_ZZK_NKCodeType",
				FilterOps.Equals,
				codeType as any,
				"string"
			),
			new Filter(
				"ZXE_ZZZ_NKDataGrouping",
				FilterOps.Equals,
				countryOrGrouping as any,
				"string"
			),
			new Filter("ZXE_Name", FilterOps.Equals, nameToValidate as any, "string"),
		];

		let message = "";
		let attrNames = await entityManager.getAsync<IRefCusCodeListAttributeName>(
			"RefCusCodeListAttributeName",
			[ServiceType.Safe],
			filtersAttrNames,
			false
		);
		if (attrNames) {
			await Promise.all(
				attrNames
					.filter((x) => x.ZXE_ZZK_NKCodeTypeForValueList != null)
					.map(async (attrName) => {
						let codeListFilters = [
							new Filter(
								"ZZD_CodeType",
								FilterOps.Equals,
								attrName.ZXE_ZZK_NKCodeTypeForValueList as any,
								"string"
							),
							new Filter(
								"ZZD_CountryOrGrouping",
								FilterOps.Equals,
								countryOrGrouping as any,
								"string"
							),
						];

						let codeLists =
							await entityManager.getAsync<RefCusCodeListUserView>(
								"RefCusCodeListUserView",
								[ServiceType.Safe],
								codeListFilters,
								false
							);
						if (
							codeLists.filter((x) => x.ZZD_Code == valueToValidate).length == 0
						) {
							message = "This value is not in the list";
						}
					})
			);
		}
		return message;
	}

	static async refCusCodeListAttribute_startAndEndDateValidations(
		entityManager: IEntityManager,
		startDate: Date | null,
		endDate: Date | null,
		codeType: string,
		countryOrGrouping: string,
		attributeName: string
	): Promise<string> {
		let filtersAttrNames = [
			new Filter(
				"ZXE_ZZK_NKCodeType",
				FilterOps.Equals,
				codeType as any,
				"string"
			),
			new Filter(
				"ZXE_ZZZ_NKDataGrouping",
				FilterOps.Equals,
				countryOrGrouping as any,
				"string"
			),
			new Filter("ZXE_Name", FilterOps.Equals, attributeName as any, "string"),
		];
		let attrNames = await entityManager.getAsync<IRefCusCodeListAttributeName>(
			"RefCusCodeListAttributeName",
			[ServiceType.Safe],
			filtersAttrNames,
			false
		);

		if (attrNames) {
			if (attrNames.filter((x) => x.ZXE_IsDateRangeUsed).length > 0) {
				return this.isRequired(startDate) && this.isRequired(endDate);
			}
		}
		return "";
	}

	static async refCusCodeListApplicationDuplication(
		entityManager: IEntityManager,
		application: IEntity
	): Promise<string> {
		let result: string = "";
		let applicationsState: string[] =
			RefCusCodeListFRFallbackInvokeForm.getApplicationListName();
		let applicationList = application as IApplicationList;
		let startDate = applicationList.DateAndTime;
		let code = applicationList.IsDeltaT ? "," + applicationsState[0] : "";
		code += applicationList.IsDeltaG ? "," + applicationsState[1] : "";
		code += applicationList.IsDeltaX ? "," + applicationsState[2] : "";
		code += applicationList.IsGamma ? "," + applicationsState[3] : "";
		code += applicationList.IsIcs ? "," + applicationsState[4] : "";
		code += applicationList.IsEcs ? "," + applicationsState[5] : "";

		let filtersApplication: Filter[] = [];
		applicationsState.forEach((element) => {
			if (code.includes(element)) {
				filtersApplication.push(
					new Filter("ZZD_Code", FilterOps.Contains, element as any, "string")
				);
			}
		});

		if (filtersApplication.length == 0) {
			result = "Please choose an application.";
			return result;
		}

		let applications: string = "";
		let codeTypeFilter: Filter = new Filter(
			"ZZD_CodeType",
			FilterOps.Equals,
			"FBK" as any,
			"string"
		);
		for (let i = 0; i < filtersApplication.length; i++) {
			let duplicated = await entityManager.getAsync<RefCusCodeListUserView>(
				"RefCusCodeListUserView",
				[ServiceType.Safe],
				[codeTypeFilter, filtersApplication[i]],
				false
			);
			duplicated = duplicated.filter((x) => x.ZZD_IsPublished == true);
			if (duplicated.length > 0) {
				let applicationValue = "";
				var arrayCode = duplicated[0].ZZD_Code.split(",");
				if (arrayCode.length == 2) {
					applicationValue = arrayCode[1];
				}
				if (applicationValue != undefined && applicationValue != "") {
					duplicated.sort(
						(a, b) => Date.parse(a.ZZD_EndDate) - Date.parse(b.ZZD_EndDate)
					);
					if (
						new Date(startDate) <
						new Date(duplicated[duplicated.length - 1].ZZD_EndDate)
					) {
						applications += ", " + applicationValue;
					}
				}
			}
		}
		if (applications != "") {
			result +=
				"A record for the following application(s) with overlapping dates already exists. Cannot create a new record starting before another ends. {" +
				applications.substring(1) +
				" }";
		}
		return result;
	}

	static async refApplicationListValidation(
		value: string,
		listEntityName: string,
		listCodePropertyName: string,
		entityManager: IEntityManager,
		operation: FilterOps = FilterOps.Equals,
		type: string = "string"
	): Promise<string> {
		if (value) {
			let filter: IFilter = new Filter(
				listCodePropertyName,
				operation,
				value as any,
				type
			);
			let data = await entityManager.getAsync<IEntity>(
				listEntityName,
				[ServiceType.Staging],
				[],
				false
			);
			data = data.filter((x) => x[listCodePropertyName] != null);
			data = entityManager.filterEntities(data, [filter]);
			let values = data?.map((t) => t[listCodePropertyName] as string);
			if (!values || values.indexOf(value) < 0) {
				return "This value is not in the list";
			}
		}
		return "";
	}

	static refApplicationAttributeValidation(
		entity: IEntity,
		propertyName: string
	): string {
		let result: string = "";
		let attribute = entity as IRefApplicationAttribute;
		let fileNameReg = new RegExp("(.+?)\\.(\\w)+$");
		switch (propertyName) {
			case "RAA_Value": {
				if (entity.RAA_RAT_NKType == RefApplicationAttributeTypeEnum.File) {
					if (attribute.RAA_Content == null) {
						result = "Please choose a file.";
					} else if (!fileNameReg.test(entity.RAA_Value)) {
						result = "Please input a valid relative file path.";
					}
				}
				break;
			}
			default:
				break;
		}
		return result;
	}

	static async refShippingLineEblProviderNameListValidation(value: string) {
		let result: string = "";
		const safeApi = __SafeAPI__.replace("odata", "api");
		const axiosConfig = {
			headers: { "Content-Type": "application/json" },
		};

		let query = await axios.get(
			`${safeApi}RefShippingLineEBLProvider/GetDistinctNames`,
			axiosConfig
		);

		if (query && query.data) {
			if (!(query.data as any[]).find((x) => x == value)) {
				result = "This value is not in the list";
			}
		} else {
			result = "This value is not in the list";
		}
		return result;
	}

	static validateVersionFormat(value: string): string {
		if (!value) {
			return "";
		}

		const regex = /^((0)|([1-9]\d{0,2}))\.((0)|([1-9]\d{0,2}))\.((0)|([1-9]\d{0,2}))\.((0)|([1-9]\d{0,2}))$/;
		if (!regex.test(value)) {
			return "Invalid version format. Please use the format: x.x.x.x, number must be between 0 and 999";
		}

		return "";
	}

	static compareVersion(version1: string, version2: string): number {
		const parts1 = version1.split('.').map(Number);
		const parts2 = version2.split('.').map(Number);
		for (let i = 0; i < 4; i++) {
			if (parts1[i] > parts2[i]) return 1;
			if (parts1[i] < parts2[i]) return -1;
		}
		return 0;
	}
}
