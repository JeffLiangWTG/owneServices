import { IEntityManager, ServiceType } from "./EntityManager";
import { IValidationService } from "./ValidationService";
import { ValidationHelper } from "./ValidationHelper";
import { FilterOps, IFilter } from "./Filter";
import RefApplicationAttributeTypeEnum from "./models/RefApplicationAttributeTypeEnum";

export class ValidationServiceHelper {
	static getRefCusCodeListValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZZD_CodeType: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) => {
					if (e["ZZD_CountryOrGrouping"]) {
						let filters: IFilter[] = [
							{
								propertyName: "ZZK_ZZZ_NKDataGrouping",
								type: "string",
								value: e["ZZD_CountryOrGrouping"],
								operation: FilterOps.Equals,
							},
						];
						return ValidationHelper.filterListValidation(
							e[p],
							"RefCusCodeType",
							"ZZK_CodeType",
							filters,
							entityManager
						);
					}
					return "";
				},
			],
			ZZD_Code: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZZD_Description: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZZD_CountryOrGrouping: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) =>
					ValidationHelper.listValidation(
						e[p],
						"RefDataGrouping",
						"ZZZ_DataGrouping",
						entityManager
					),
			],
			ZZD_StartDate: [
				(e) => ValidationHelper.isRequired(e.ZZD_StartDate),
				(e) => ValidationHelper.dateRangeValidation(e.ZZD_StartDate),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZZD_StartDate,
						e.ZZD_EndDate
					),
			],
			ZZD_EndDate: [
				(e) => ValidationHelper.isRequired(e.ZZD_EndDate),
				(e) => ValidationHelper.dateRangeValidation(e.ZZD_EndDate),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZZD_StartDate,
						e.ZZD_EndDate
					),
			],
			entity: [
				(e) =>
					ValidationHelper.validateDuplication(
						entityManager,
						"RefCusCodeListUserView",
						e,
						["ZZD_CodeType", "ZZD_Code", "ZZD_CountryOrGrouping"]
					),
				(e) =>
					ValidationHelper.refCusCodeListAttribute_validations(
						entityManager,
						e["ZZD_CodeType"],
						e["ZZD_CountryOrGrouping"],
						e["ZZD_PK"],
						null,
						null
					),
			],
		};
	}

	static getRefCusCodeListAttributeValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZZE_ZXE_NKName: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) =>
					ValidationHelper.refCusCodeListAttribute_validations(
						entityManager,
						e["ZZE_CodeType"],
						e["ZZE_CountryOrGrouping"],
						e["ZZE_ZZD_CodeList"],
						e[p],
						null
					),
			],
			ZZE_Value: [
				(e) =>
					ValidationHelper.refCusCodeListAttribute_validations(
						entityManager,
						e["ZZE_CodeType"],
						e["ZZE_CountryOrGrouping"],
						e["ZZE_ZZD_CodeList"],
						null,
						e["ZZE_PK"]
					),
				(e, p) =>
					ValidationHelper.refCusCodeListAttribute_validateValueIsInTheList(
						entityManager,
						e["ZZE_CodeType"],
						e["ZZE_CountryOrGrouping"],
						e["ZZE_ZXE_NKName"],
						e[p]
					),
				(e, p) =>
					ValidationHelper.validateDuplication(
						entityManager,
						"RefCusCodeListAttributeUserView",
						e,
						["ZZE_ZXE_NKName", "ZZE_Value", "ZZE_ZZD_CodeList"]
					),
			],
			ZZE_StartDate: [
				(e, p) =>
					ValidationHelper.startDateAndEndDateValidation(
						e[p],
						e["ZZE_EndDate"]
					),
				(e, p) =>
					ValidationHelper.refCusCodeListAttribute_startAndEndDateValidations(
						entityManager,
						e[p],
						e["ZZE_EndDate"],
						e["ZZE_CodeType"],
						e["ZZE_CountryOrGrouping"],
						e["ZZE_ZXE_NKName"]
					),
			],
			ZZE_EndDate: [
				(e, p) =>
					ValidationHelper.startDateAndEndDateValidation(
						e["ZZE_StartDate"],
						e[p]
					),
				(e, p) =>
					ValidationHelper.refCusCodeListAttribute_startAndEndDateValidations(
						entityManager,
						e["ZZE_StartDate"],
						e[p],
						e["ZZE_CodeType"],
						e["ZZE_CountryOrGrouping"],
						e["ZZE_ZXE_NKName"]
					),
			],
		};
	}

	static getRefCusCodeListFilterValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (filter.propertyName == "ZZD_CodeType")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefCusCodeType",
							"ZZK_CodeType",
							entityManager,
							filter.operation
						);
					if (filter.propertyName == "ZZD_CountryOrGrouping")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefDataGrouping",
							"ZZZ_DataGrouping",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefAccTaxRateValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZAT_ReferenceRateType: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZAT_RN_NKCountry: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) =>
					ValidationHelper.listValidation(
						e[p],
						"RefCountry",
						"RN_Code",
						entityManager
					),
			],
			ZAT_RateDenominator: [
				(e, p) => ValidationHelper.isGreaterThanOrEqualOne(e[p]),
			],
			ZAT_RateNumerator: [
				(e, p) => ValidationHelper.isGreaterThanOrEqualZero(e[p]),
			],
			ZAT_StartDate: [
				(e) =>
					ValidationHelper.dateRangeValidation(
						e.ZAT_StartDate,
						"0001-01-01T00:00:00Z",
						"9999-12-31T23:59:59Z"
					),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZAT_StartDate,
						e.ZAT_EndDate
					),
				(e, _p, isSaving) =>
					isSaving
						? ValidationHelper.dateOverlapValidation(
							entityManager,
							"RefAccTaxRateUserView",
							e,
							["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]
						)
						: "",
			],
			ZAT_EndDate: [
				(e) =>
					ValidationHelper.dateRangeValidation(
						e.ZAT_EndDate,
						"0001-01-01T00:00:00Z",
						"9999-12-31T23:59:59Z"
					),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZAT_StartDate,
						e.ZAT_EndDate
					),
				(e, _p, isSaving) =>
					isSaving
						? ValidationHelper.dateOverlapValidation(
							entityManager,
							"RefAccTaxRateUserView",
							e,
							["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]
						)
						: "",
			],
		};
	}

	static getRefAccTaxRateFilterValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (filter.propertyName == "ZAT_ReferenceRateType")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefAccTaxRateUserView",
							"ZAT_ReferenceRateType",
							entityManager,
							filter.operation
						);
					if (filter.propertyName == "ZAT_RN_NKCountry")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefCountry",
							"RN_Code",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefShippingLineValidationService(
		entitymanager: IEntityManager
	): IValidationService {
		return {
			RSL_CarrierName: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e) =>
					ValidationHelper.validateDuplication(
						entitymanager,
						"RefShippingLineUserView",
						e,
						["RSL_CarrierName"]
					),
			],
			RSL_CargoWiseOneCode: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) => ValidationHelper.validateLength(e[p], 4),
				(e) => {
					if (e["RSL_IsActive"] == true) {
						return ValidationHelper.validateDuplication(
							entitymanager,
							"RefShippingLineUserView",
							e,
							["RSL_CargoWiseOneCode", "RSL_IsActive"]
						);
					}
					return "";
				},
			],
			RSL_StandardCarrierAlphaCode: [
				(e, p) => (e[p] !== "" ? ValidationHelper.validateLength(e[p], 4) : ""),
				(e, p) =>
					e[p] !== ""
						? ValidationHelper.validateDuplication(
							entitymanager,
							"RefShippingLineUserView",
							e,
							["RSL_StandardCarrierAlphaCode"]
						)
						: "",
			],
			RSL_EHubIds: [
				(e, p) => (e["RSL_IsCW1User"] ? ValidationHelper.isRequired(e[p]) : ""),
			],
		};
	}

	static getRefShippingLineFilterValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (filter.propertyName == "RSL_StandardCarrierAlphaCode")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefShippingLineUserView",
							"RSL_StandardCarrierAlphaCode",
							entityManager,
							filter.operation
						);
					if (filter.propertyName == "RSL_CargoWiseOneCode")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefShippingLineUserView",
							"RSL_CargoWiseOneCode",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefStlScriptValidationService(
		entitymanager: IEntityManager
	): IValidationService {
		return {
			STL_FeatureCode: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) => ValidationHelper.validateLength(e[p], 3),
				(e) => {
					return ValidationHelper.validateDuplication(
						entitymanager,
						"RefStlScriptUserView",
						e,
						[
							"STL_FeatureCode",
							"STL_ActiveOn",
							"STL_MinCW1Version",
							"STL_MaxCW1Version",
						]
					);
				},
			],
			STL_MinCW1Version: [
				(e, p) => ValidationHelper.validateVersionFormat(e[p]),
				(e) => {
					if (
						e["STL_MinCW1Version"] &&
						e["STL_MaxCW1Version"] &&
						ValidationHelper.compareVersion(e["STL_MinCW1Version"], e["STL_MaxCW1Version"]) >= 0
					) {
						return "Min CW Version must be less than Max CW Version";
					}
					return "";
				},
			],
			STL_MaxCW1Version: [
				(e, p) => ValidationHelper.validateVersionFormat(e[p]),
				(e) => {
					if (
						e["STL_MinCW1Version"] &&
						e["STL_MaxCW1Version"] &&
						ValidationHelper.compareVersion(e["STL_MinCW1Version"], e["STL_MaxCW1Version"]) >= 0
					) {
						return "Min CW Version must be less than Max CW Version";
					}
					return "";
				},
			],
			STL_GuidReference: [(e, p) => ValidationHelper.isRequired(e[p])],
			STL_TransactionCount: [(e, p) => ValidationHelper.isRequired(e[p])],
			STL_TransactionDateUtc: [(e, p) => ValidationHelper.isRequired(e[p])],
			STL_FromClause: [(e, p) => ValidationHelper.isRequired(e[p])],
		};
	}

	static getRefStlScriptUserViewValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (filter.propertyName == "STL_FeatureCode")
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefStlScriptUserView",
							"STL_FeatureCode",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefCusCodeFallbackInvokeValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			DateAndTime: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) => ValidationHelper.dateRangeValidation(e[p]),
			],
			Comment: [(e, p) => ValidationHelper.isRequired(e[p])],
			Applications: [
				(e, p) =>
					ValidationHelper.refCusCodeListApplicationDuplication(
						entityManager,
						e
					),
			],
		};
	}

	static getRefCusCodeFallbackRevokeValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZZD_RevokeComments: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZZD_StartDate: [
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZZD_StartDate,
						e.ZZD_EndDate
					),
			],
			ZZD_EndDate: [
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.ZZD_StartDate,
						e.ZZD_EndDate
					),
			],
		};
	}

	static getRefCusCodeFallbackRevokeAllValidationService(
		entityManager: IEntityManager,
		currentDate: string
	): IValidationService {
		return {
			revokingAllDate: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e) => ValidationHelper.dateRangeValidation(currentDate),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						currentDate,
						e.revokingAllDate
					),
				(e) =>
					ValidationHelper.startDateAndEndDateValidation(
						e.maxStartDate,
						e.revokingAllDate
					),
			],
		};
	}

	static getRefApplicationFilterValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (filter.propertyName == "CountryCode")
						return ValidationHelper.refApplicationListValidation(
							filter.value as any,
							"QRTZ_JOB_DETAILS",
							"CountryCode",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefApplicationAttributeValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			RAA_AttributeName: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e) => {
					let isDefault = e["RAA_IsDefault"] as boolean;
					if (isDefault) {
						return "";
					}
					return ValidationHelper.validateDuplication(
						entityManager,
						"RefApplicationAttribute",
						e,
						["RAA_ConfigFilePath", "RAA_AttributeName"],
						ServiceType.Staging
					);
				},
			],
			RAA_RAT_NKType: [
				(e, p) => {
					let isDefault = e["RAA_IsDefault"] as boolean;
					if (isDefault) {
						return "";
					}
					return ValidationHelper.isRequired(e[p]);
				},
				(e, p) => {
					let isDefault = e["RAA_IsDefault"] as boolean;
					if (isDefault) {
						return "";
					}
					return ValidationHelper.listValidation(
						e[p],
						"RefApplicationAttributeType",
						"RAT_Type",
						entityManager,
						FilterOps.Equals,
						ServiceType.Staging
					);
				},
			],
			RAA_Value: [
				(e, p) => {
					let isDefault = e["RAA_IsDefault"] as boolean;
					if (
						e["RAA_RAT_NKType"] == RefApplicationAttributeTypeEnum.Boolean ||
						isDefault
					) {
						return "";
					}
					return ValidationHelper.isRequired(e[p]);
				},
				(e, p) => ValidationHelper.refApplicationAttributeValidation(e, p),
			],
		};
	}

	static getRefCusProcedureFilterValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			value: [
				(e, _p) => {
					let filter = e as IFilter;
					if (
						filter.propertyName == "ZZ6_CountryOrGrouping" &&
						filter.operation == FilterOps.Equals
					)
						return ValidationHelper.listValidation(
							filter.value as any,
							"RefDataGrouping",
							"ZZZ_DataGrouping",
							entityManager,
							filter.operation
						);
					return "";
				},
			],
		};
	}

	static getRefCusProcedureValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZZ6_ProcedureCode: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) => ValidationHelper.isUpperCase(e[p]),
			],
			ZZ6_PreviousProcedureCode: [(e, p) => ValidationHelper.isUpperCase(e[p])],
			ZZ6_Concession: [(e, p) => ValidationHelper.isUpperCase(e[p])],
			ZZ6_Category: [(e, p) => ValidationHelper.isUpperCase(e[p])],
			ZZ6_Group: [(e, p) => ValidationHelper.isUpperCase(e[p])],
			ZZ6_Description: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZZ6_StartDate: [
				(e) => ValidationHelper.isRequired(e.ZZ6_StartDate),
				(e) => ValidationHelper.dateRangeValidation(e.ZZ6_StartDate),
				(e, p) =>
					ValidationHelper.startDateAndEndDateValidation(
						e[p],
						e["ZZ6_EndDate"]
					),
			],
			ZZ6_EndDate: [
				(e) => ValidationHelper.isRequired(e.ZZ6_EndDate),
				(e) => ValidationHelper.dateRangeValidation(e.ZZ6_EndDate),
				(e, p) =>
					ValidationHelper.startDateAndEndDateValidation(
						e["ZZ6_StartDate"],
						e[p]
					),
			],
			ZZ6_CountryOrGrouping: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) =>
					ValidationHelper.listValidation(
						e[p],
						"RefDataGrouping",
						"ZZZ_DataGrouping",
						entityManager
					),
			],
			ZZ6_IntoWarehouse: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_IntoOutwardProcessing"] == "Y" ||
							e["ZZ6_IntoInwardProcessing"] == "Y" ||
							e["ZZ6_IntoTemporaryImport"] == "Y" ||
							e["ZZ6_IntoTemporaryExport"] == "Y")
					) {
						return "The value of 'Into Warehouse' can't be 'Yes' when either 'Into Outward Processing' or 'Into Inward Processing' or 'Into Temporary Import' or 'Into Temporary Export' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_OutOfWarehouse: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_OutofOutwardProcessing"] == "Y" ||
							e["ZZ6_OutOfTemporaryImport"] == "Y" ||
							e["ZZ6_OutOfTemporaryExport"] == "Y" ||
							e["ZZ6_OutOfInwardProcessing"] == "Y")
					) {
						return "The value of 'Out Of Warehouse' can't be 'Yes' when either 'Out of Outward Processing' or 'Out Of Temporary Import' or 'Out Of Temporary Export' or 'Out Of Inward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_IntoInwardProcessing: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_IntoWarehouse"] == "Y" ||
							e["ZZ6_IntoTemporaryImport"] == "Y" ||
							e["ZZ6_IntoTemporaryExport"] == "Y" ||
							e["ZZ6_IntoOutwardProcessing"] == "Y")
					) {
						return "The value of 'Into Inward Processing' can't be 'Yes' when either 'Into Warehouse' or 'Into Temporary Import' or 'Into Temporary Export' or 'Into Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_OutOfInwardProcessing: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_OutOfWarehouse"] == "Y" ||
							e["ZZ6_OutOfTemporaryImport"] == "Y" ||
							e["ZZ6_OutOfTemporaryExport"] == "Y" ||
							e["ZZ6_OutofOutwardProcessing"] == "Y")
					) {
						return "The value of 'Out Of Inward Processing' can't be 'Yes' when either 'Out Of Warehouse' or 'Out Of Temporary Import' or 'Out Of Temporary Export' or 'Out of Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_IntoOutwardProcessing: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_IntoWarehouse"] == "Y" ||
							e["ZZ6_IntoInwardProcessing"] == "Y" ||
							e["ZZ6_IntoTemporaryImport"] == "Y" ||
							e["ZZ6_IntoTemporaryExport"] == "Y")
					) {
						return "The value of 'Into Outward Processing' can't be 'Yes' when either 'Into Warehouse' or 'Into Inward Processing' or 'Into Temporary Import' or 'Into Temporary Export' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_OutofOutwardProcessing: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_OutOfWarehouse"] == "Y" ||
							e["ZZ6_OutOfTemporaryImport"] == "Y" ||
							e["ZZ6_OutOfTemporaryExport"] == "Y" ||
							e["ZZ6_OutOfInwardProcessing"] == "Y")
					) {
						return "The value of 'Out of Outward Processing' can't be 'Yes' when either 'Out Of Warehouse' or 'Out Of Temporary Import' or 'Out Of Temporary Export' or 'Out Of Inward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_IntoTemporaryImport: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_IntoTemporaryExport"] == "Y" ||
							e["ZZ6_IntoWarehouse"] == "Y" ||
							e["ZZ6_IntoInwardProcessing"] == "Y" ||
							e["ZZ6_IntoOutwardProcessing"] == "Y")
					) {
						return "The value of 'Into Temporary Import' can't be 'Yes' when either 'Into Temporary Export' or 'Into Warehouse' or 'Into Inward Processing' or 'Into Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_OutOfTemporaryImport: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_OutOfTemporaryExport"] == "Y" ||
							e["ZZ6_OutOfWarehouse"] == "Y" ||
							e["ZZ6_OutOfInwardProcessing"] == "Y" ||
							e["ZZ6_OutofOutwardProcessing"] == "Y")
					) {
						return "The value of 'Out Of Temporary Import' can't be 'Yes' when either 'Out Of Temporary Export' or 'Out Of Warehouse' or 'Out Of Inward Processing' or 'Out of Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_IntoTemporaryExport: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_IntoTemporaryImport"] == "Y" ||
							e["ZZ6_IntoWarehouse"] == "Y" ||
							e["ZZ6_IntoInwardProcessing"] == "Y" ||
							e["ZZ6_IntoOutwardProcessing"] == "Y")
					) {
						return "The value of 'Into Temporary Export' can't be 'Yes' when either 'Into Temporary Import' or 'Into Warehouse' or 'Into Inward Processing' or 'Into Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			ZZ6_OutOfTemporaryExport: [
				(e, p) => {
					if (
						e[p] == "Y" &&
						(e["ZZ6_OutOfTemporaryImport"] == "Y" ||
							e["ZZ6_OutOfWarehouse"] == "Y" ||
							e["ZZ6_OutOfInwardProcessing"] == "Y" ||
							e["ZZ6_OutofOutwardProcessing"] == "Y")
					) {
						return "The value of 'Out Of Temporary Export' can't be 'Yes' when either 'Out Of Temporary Import' or 'Out Of Warehouse' or 'Out Of Inward Processing' or 'Out of Outward Processing' is 'Yes'";
					}
					return "";
				},
			],
			entity: [
				(e) =>
					ValidationHelper.validateDuplication(
						entityManager,
						"RefCusProcedureUserView",
						e,
						[
							"ZZ6_ProcedureCode",
							"ZZ6_PreviousProcedureCode",
							"ZZ6_Concession",
							"ZZ6_Category",
							"ZZ6_CountryOrGrouping",
						]
					),
			],
		};
	}

	static getRefCusProcedureAttributeValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			ZXB_Name: [(e, p) => ValidationHelper.isRequired(e[p])],
			ZXB_Value: [(e, p) => ValidationHelper.isRequired(e[p])],
		};
	}

	static getRefShippingLineEBLProviderValidationService(
		entityManager: IEntityManager
	): IValidationService {
		return {
			RSE_Name: [
				(e, p) => ValidationHelper.isRequired(e[p]),
				(e, p) =>
					e[p] !== ""
						? ValidationHelper.validateDuplication(
							entityManager,
							"RefShippingLineEBLProvider",
							e,
							["RSE_Name", "RSE_RSL_ShippingLine"]
						)
						: "",
				(e, p) =>
					e[p] !== ""
						? ValidationHelper.refShippingLineEblProviderNameListValidation(
							e[p]
						)
						: "",
			],
		};
	}
}
