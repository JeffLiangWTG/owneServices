import { ValidationHelper } from "../ValidationHelper";
import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import IRefCusCodeType from "../models/IRefCusCodeType";
import { IValidationService, ValidationServiceWrapper } from "../ValidationService";
import { RefAccTaxRateUserView } from "../models/RefAccTaxRateUserView";
import IRefCusCodeListAttributeName from "../models/IRefCusCodeListAttributeName";
import { RefCusCodeListAttributeUserView } from "../models/RefCusCodeListAttributeUserView";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { IApplicationList } from "../RefCusCodeListFRFallbackInvokeForm";
import { IRefApplicationAttribute } from "../models/IRefApplicationAttribute";
import IQrtzJobDetails from "../models/IQrtzJobDetails";
import { Filter, FilterOps, IFilter } from "../Filter";
import axios from "axios";

describe("ValidationHelper", () => {
	it("IsRequired", () => {
		expect(ValidationHelper.isRequired(undefined)).toEqual("This field is required.");
		expect(ValidationHelper.isRequired(null)).toEqual("This field is required.");
		expect(ValidationHelper.isRequired("")).toEqual("This field is required.");
		expect(ValidationHelper.isRequired("AA")).toEqual("");
	});

	it("ListValidation", async () => {
		let codeType: IRefCusCodeType = {
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		};
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], [new Filter("ZZK_CodeType", FilterOps.Equals, "CD" as any, "string")], false)).returns(() => Promise.resolve([codeType]));
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], [new Filter("ZZK_CodeType", FilterOps.Equals, "CC" as any, "string")], false)).returns(() => Promise.resolve([]));
		expect(await ValidationHelper.listValidation("CC", "RefCusCodeType", "ZZK_CodeType", entityManager.object))
			.toEqual("This value is not in the list");
		expect(await ValidationHelper.listValidation("CD", "RefCusCodeType", "ZZK_CodeType", entityManager.object))
			.toEqual("");

		let attrName: IRefCusCodeListAttributeName = {
			ZXE_AllowDuplicates: true,
			ZXE_Description: "Desc",
			ZXE_IsMandatory: false,
			ZXE_Name: "name",
			ZXE_PK: "1",
			ZXE_IsValueMandatory: false,
			ZXE_ZZK_NKCodeType: "CDT",
			ZXE_ZZK_NKCodeTypeForValueList: "",
			ZXE_ZZZ_NKDataGrouping: "AU",
			ZXE_ColumnCaption: "",
			ZXE_DecimalPlaces: 0,
			ZXE_MaxLengthOrValue: 0,
			ZXE_MinLengthOrValue: 0,
			ZXE_ValueDataType: "",
			ZXE_IsDateRangeUsed: false
		};
		entityManager.setup(x => x.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], [new Filter("ZXE_Name", FilterOps.Equals, "name" as any, "string")], false)).returns(() => Promise.resolve([attrName]));
		entityManager.setup(x => x.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], [new Filter("ZXE_Name", FilterOps.Equals, "NoName" as any, "string")], false)).returns(() => Promise.resolve([]));
		expect(await ValidationHelper.listValidation("name", "RefCusCodeListAttributeName", "ZXE_Name", entityManager.object)).toEqual("");
		expect(await ValidationHelper.listValidation("NoName", "RefCusCodeListAttributeName", "ZXE_Name", entityManager.object)).toEqual("This value is not in the list");
	});

	it("filterListValidation", async() => {
		let codeTypes: IRefCusCodeType[] = [{
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}, {
			ZZK_PK: "2",
			ZZK_CodeType: "CAB",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}];
		let filtersNotInTheList : IFilter[] = [new Filter("ZZK_ZZZ_NKDataGrouping", FilterOps.Equals, "ZZ" as any, "string"), new Filter("ZZK_CodeType", FilterOps.Equals, "CC" as any, "string")];
		let filtersInTheList : IFilter[] = [new Filter("ZZK_ZZZ_NKDataGrouping", FilterOps.Equals, "ZZ" as any, "string"), new Filter("ZZK_CodeType", FilterOps.Equals, "CD" as any, "string")];
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], filtersNotInTheList, false)).returns(() => Promise.resolve([]));
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], filtersInTheList, false)).returns(() => Promise.resolve(codeTypes));
		expect(await ValidationHelper.filterListValidation("CC", "RefCusCodeType", "ZZK_CodeType", filtersNotInTheList, entityManager.object))
			.toEqual("This value is not in the list");
		expect(await ValidationHelper.filterListValidation("CD", "RefCusCodeType", "ZZK_CodeType", filtersInTheList, entityManager.object))
			.toEqual("");
	});

	it("isGreaterThanOrEqualOne", async () => {
		expect(ValidationHelper.isGreaterThanOrEqualOne(-1)).toEqual("This field is required to be greater than 1");
		expect(ValidationHelper.isGreaterThanOrEqualOne(0)).toEqual("This field is required to be greater than 1");
		expect(ValidationHelper.isGreaterThanOrEqualOne(1)).toEqual("");
	});

	it("isGreaterThanOrEqualZero", async () => {
		expect(ValidationHelper.isGreaterThanOrEqualZero(-1)).toEqual("This field is required to be greater than 0");
		expect(ValidationHelper.isGreaterThanOrEqualZero(0)).toEqual("");
	});

	it("isUpperCase", async () => {
		expect(ValidationHelper.isUpperCase("Aaa")).toEqual("This field can't contain lowercase letters");
		expect(ValidationHelper.isUpperCase("AAA")).toEqual("");
	});

	it("startDateAndEndDateValidation", async () => {
		expect(ValidationHelper.startDateAndEndDateValidation("2079-06-06T23:59:00Z", "1900-01-01T00:00:00Z")).toEqual("The start date should not be later than the end date");
		expect(ValidationHelper.startDateAndEndDateValidation("1900-01-01T00:00:00Z", "2079-06-06T23:59:00Z")).toEqual("");
	});

	it("dateRangeValidation", async () => {
		expect(ValidationHelper.dateRangeValidation("1900-01-01T00:00:00Z")).toEqual("");
		expect(ValidationHelper.dateRangeValidation("2079-06-06T23:59:00Z")).toEqual("");
		expect(ValidationHelper.dateRangeValidation("1899-01-01T00:00:00Z", "1900-01-01T00:00:00Z", "2079-06-06T23:59:00Z")).toEqual("Date cannot be earlier than 01/01/1900");
		expect(ValidationHelper.dateRangeValidation("2080-01-01T00:00:00Z", "1900-01-01T00:00:00Z", "2079-06-06T23:59:00Z")).toEqual("Date cannot be later than 06/06/2079");
	});

	it("validateLength", async () => {
		expect(ValidationHelper.validateLength("Length", 4)).toEqual("This field must have 4 characters");
		expect(ValidationHelper.validateLength("Length", 6)).toEqual("");
	});

	it("dateOverlapValidation", async () => {
		let taxRate1: RefAccTaxRateUserView = {
			ZAT_PK: "1",
			ZAT_StartDate: "2010-01-01T00:00:00Z",
			ZAT_EndDate: "2019-01-01T00:00:00Z",
			ZAT_RN_NKCountry: "XX",
			ZAT_ReferenceRateType: "XXX",
			ZAT_IsEditable: true,
			ZAT_IsSystem: true,
			ZAT_IsPublished: true,
			ZAT_RateDenominator: 1,
			ZAT_RateNumerator: 0
		}
		let taxRate2: RefAccTaxRateUserView = {
			ZAT_PK: "2",
			ZAT_StartDate: "2010-01-01T00:00:00Z",
			ZAT_EndDate: "2019-01-01T00:00:00Z",
			ZAT_RN_NKCountry: "XX",
			ZAT_ReferenceRateType: "XXX",
			ZAT_IsEditable: true,
			ZAT_IsSystem: true,
			ZAT_IsPublished: true,
			ZAT_RateDenominator: 1,
			ZAT_RateNumerator: 0
		}
		let taxRate3: RefAccTaxRateUserView = {
			ZAT_PK: "3",
			ZAT_StartDate: "2010-01-01T00:00:00Z",
			ZAT_EndDate: "2019-01-01T00:00:00Z",
			ZAT_RN_NKCountry: "XX",
			ZAT_ReferenceRateType: "XXX",
			ZAT_IsEditable: true,
			ZAT_IsSystem: true,
			ZAT_IsPublished: false,
			ZAT_RateDenominator: 1,
			ZAT_RateNumerator: 0
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<RefAccTaxRateUserView>("RefAccTaxRateUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([]));
		expect(await ValidationHelper.dateOverlapValidation(entityManager.object, "RefAccTaxRateUserView", taxRate1, ["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]))
			.toEqual("");

		entityManager.setup(x => x.getAsync<RefAccTaxRateUserView>("RefAccTaxRateUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([taxRate2]));
		expect(await ValidationHelper.dateOverlapValidation(entityManager.object, "RefAccTaxRateUserView", taxRate1, ["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]))
			.toEqual("RefAccTaxRateUserView (XX,XXX) date(s) overlaps with existing record.");

		entityManager.setup(x => x.getAsync<RefAccTaxRateUserView>("RefAccTaxRateUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([taxRate3]));
		expect(await ValidationHelper.dateOverlapValidation(entityManager.object, "RefAccTaxRateUserView", taxRate1, ["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]))
			.toEqual("");

		entityManager.setup(x => x.getAsync<RefAccTaxRateUserView>("RefAccTaxRateUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([taxRate1]));
		expect(await ValidationHelper.dateOverlapValidation(entityManager.object, "RefAccTaxRateUserView", taxRate3, ["ZAT_RN_NKCountry", "ZAT_ReferenceRateType"]))
			.toEqual("");
	});

	it("validateProperty", async () => {
		let validationService: IValidationService = {
			propertyA: [() => "Error 1", () => "Error 2"]
		};
		let validationServiceWrapper = new ValidationServiceWrapper([validationService], (isValidating: boolean) => void {});
		expect(await ValidationHelper.validateProperty(validationServiceWrapper, {}, "propertyA"))
			.toEqual(["Error 1", "Error 2"]);
	});

	it("validatePropertyOnSave", async () => {
		let validationService: IValidationService = {
			propertyA: [() => "Error 1", () => false ? "Error 2" : ""]
		};
		let validationServiceWrapper = new ValidationServiceWrapper([validationService], (isValidating: boolean) => void {});
		expect(await ValidationHelper.validateProperty(validationServiceWrapper, {}, "propertyA", 0, true))
			.toEqual(["Error 1"]);
	});

	it("validate", async () => {
		let validationService: IValidationService = {
			propertyA: [() => "Error 1", () => "Error 2"],
			propertyB: [() => "Error 3"]
		};
		let validationServiceWrapper = new ValidationServiceWrapper([validationService], (isValidating: boolean) => void {});
		expect(await ValidationHelper.validate(validationServiceWrapper, {}))
			.toEqual({ "propertyA": ["Error 1", "Error 2"], "propertyB": ["Error 3"] });
	});

	it("validateDuplication", async () => {
		let codeType1: IRefCusCodeType = {
			ZZK_PK: "1",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}
		let codeType2: IRefCusCodeType = {
			ZZK_PK: "2",
			ZZK_CodeType: "CD",
			ZZK_Description: "Hello",
			ZZK_IsReadonly: true,
			ZZK_MaxLength: 0,
			ZZK_ZZZ_NKDataGrouping: "ZZ"
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeType>("RefCusCodeType", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([codeType1]));
		expect(await ValidationHelper.validateDuplication(entityManager.object, "RefCusCodeType", codeType2, ["ZZK_CodeType"])).toEqual("RefCusCodeType (CD) already exists.");
	});

	it("validateRefCusAttribute", async () => {
		let attrName1: IRefCusCodeListAttributeName = {
			ZXE_Name: "name1",
			ZXE_ZZK_NKCodeType: "cd1",
			ZXE_AllowDuplicates: false,
			ZXE_IsMandatory: true,
			ZXE_IsValueMandatory: false,
			ZXE_ZZZ_NKDataGrouping: "AU",
			ZXE_Description: "ANY",
			ZXE_PK: "1",
			ZXE_ZZK_NKCodeTypeForValueList: "AB",
			ZXE_ColumnCaption: "",
			ZXE_DecimalPlaces: 0,
			ZXE_MaxLengthOrValue: 0,
			ZXE_MinLengthOrValue: 0,
			ZXE_ValueDataType: "",
			ZXE_IsDateRangeUsed: false
		};

		let attrName2: IRefCusCodeListAttributeName = {
			ZXE_Name: "name2",
			ZXE_ZZK_NKCodeType: "cd1",
			ZXE_AllowDuplicates: true,
			ZXE_IsMandatory: false,
			ZXE_IsValueMandatory: false,
			ZXE_ZZZ_NKDataGrouping: "AU",
			ZXE_Description: "ANY",
			ZXE_PK: "2",
			ZXE_ZZK_NKCodeTypeForValueList: "",
			ZXE_ColumnCaption: "",
			ZXE_DecimalPlaces: 0,
			ZXE_MaxLengthOrValue: 0,
			ZXE_MinLengthOrValue: 0,
			ZXE_ValueDataType: "",
			ZXE_IsDateRangeUsed: false
		};

		let attrName3: IRefCusCodeListAttributeName = {
			ZXE_Name: "name3",
			ZXE_ZZK_NKCodeType: "cd1",
			ZXE_AllowDuplicates: false,
			ZXE_IsMandatory: false,
			ZXE_IsValueMandatory: true,
			ZXE_ZZZ_NKDataGrouping: "CN",
			ZXE_Description: "ANY",
			ZXE_PK: "1",
			ZXE_ZZK_NKCodeTypeForValueList: "AB",
			ZXE_ColumnCaption: "",
			ZXE_DecimalPlaces: 0,
			ZXE_MaxLengthOrValue: 0,
			ZXE_MinLengthOrValue: 0,
			ZXE_ValueDataType: "",
			ZXE_IsDateRangeUsed: false
		};

		let cd1AttrObj = {
			ZZE_CodeType: "cd1",
			ZZE_CountryOrGrouping: "AU",
			ZZE_IsAir: false,
			ZZE_IsFix: false,
			ZZE_IsInw: false,
			ZZE_IsMai: false,
			ZZE_IsRai: false,
			ZZE_IsRoa: false,
			ZZE_IsSea: false,
			ZZE_PK: "1",
			ZZE_Value: "",
			ZZE_IsEditable: true,
			ZZE_ZXE_NKName: "name1",
			ZZE_ZZD_CodeList: "any"
		} as RefCusCodeListAttributeUserView;

		let cd1AttrObj1 = {
			ZZE_CodeType: "cd1",
			ZZE_CountryOrGrouping: "CN",
			ZZE_IsAir: false,
			ZZE_IsFix: false,
			ZZE_IsInw: false,
			ZZE_IsMai: false,
			ZZE_IsRai: false,
			ZZE_IsRoa: false,
			ZZE_IsSea: false,
			ZZE_PK: "1",
			ZZE_Value: "",
			ZZE_IsEditable: true,
			ZZE_ZXE_NKName: "name3",
			ZZE_ZZD_CodeList: "any"
		} as RefCusCodeListAttributeUserView;

		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IRefCusCodeListAttributeName>("RefCusCodeListAttributeName", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([attrName1, attrName2, attrName3]));

		entityManager.setup(x => x.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([]));
		let validationResult = await ValidationHelper.refCusCodeListAttribute_validations(entityManager.object, "cd1", "AU", "", null, null);
		expect(validationResult).toEqual("Mandatory attribute name1, ANY is required");

		entityManager.setup(x => x.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([cd1AttrObj, cd1AttrObj]));
		let duplicateResults = await ValidationHelper.refCusCodeListAttribute_validations(entityManager.object, "cd1", "AU", "", "name1", null);
		expect(duplicateResults).toEqual("Duplicate attribute name 'name1' is not allowed.");

		entityManager.setup(x => x.getAsync<RefCusCodeListAttributeUserView>("RefCusCodeListAttributeUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([cd1AttrObj1]));
		let valueMandatoryResults = await ValidationHelper.refCusCodeListAttribute_validations(entityManager.object, "cd1", "CN", "", "name3", "1");
		expect(valueMandatoryResults).toEqual("Value is mandatory for name3.");

		let code = {
			ZZD_PK: "12345",
			ZZD_Code: "AA",
			ZZD_CodeType: "AB",
			ZZD_CountryOrGrouping: "AU",
			ZZD_Description: "ab",
			ZZD_StartDate: "1900-01-01T00:00:00Z",
			ZZD_EndDate: "2079-06-06T23:59:00Z",
			ZZD_IsAir: true,
			ZZD_IsFix: false,
			ZZD_IsInw: false,
			ZZD_IsPublished: false,
			ZZD_IsRoa: false,
			ZZD_IsSea: false,
			ZZD_IsMai: false,
			ZZD_IsRai: false,
			ZZD_IsSystem: false,
			ZZD_IsExcluded: false,
			ZZD_IsEditable: false
		} as RefCusCodeListUserView;
		entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code]));
		let valueIsNotInTheList = await ValidationHelper.refCusCodeListAttribute_validateValueIsInTheList(entityManager.object, "cd1", "AU", "name1", "AC");
		expect(valueIsNotInTheList).toEqual("This value is not in the list");
	});

	it("refCusCodeListApplicationDuplication", async () => {
		let code1: RefCusCodeListUserView = {
			ZZD_PK: "12345",
			ZZD_Code: "ICS",
			ZZD_CodeType: "AB",
			ZZD_CountryOrGrouping: "AU",
			ZZD_Description: "ABC",
			ZZD_StartDate: "1900-01-01T00:00:00Z",
			ZZD_EndDate: "2079-06-06T23:59:00Z",
			ZZD_IsAir: true,
			ZZD_IsFix: false,
			ZZD_IsInw: false,
			ZZD_IsPublished: true,
			ZZD_IsRoa: false,
			ZZD_IsSea: false,
			ZZD_IsMai: false,
			ZZD_IsRai: false,
			ZZD_IsSystem: true,
			ZZD_IsEditable: true
		};
		let code2: RefCusCodeListUserView = {
			ZZD_PK: "123456",
			ZZD_Code: "1234567890,ICS",
			ZZD_CodeType: "FBK",
			ZZD_CountryOrGrouping: "FR",
			ZZD_Description: "ABC",
			ZZD_StartDate: "1900-01-01T00:00:00Z",
			ZZD_EndDate: "2079-06-06T23:59:00Z",
			ZZD_IsAir: true,
			ZZD_IsFix: false,
			ZZD_IsInw: false,
			ZZD_IsPublished: true,
			ZZD_IsRoa: false,
			ZZD_IsSea: false,
			ZZD_IsMai: false,
			ZZD_IsRai: false,
			ZZD_IsSystem: true,
			ZZD_IsEditable: true
		};
		let code3: RefCusCodeListUserView = {
			ZZD_PK: "1234567",
			ZZD_Code: "1234567890,ICS",
			ZZD_CodeType: "FBK",
			ZZD_CountryOrGrouping: "FR",
			ZZD_Description: "ABC",
			ZZD_StartDate: "1900-01-01T00:00:00Z",
			ZZD_EndDate: "2079-06-06T23:59:00Z",
			ZZD_IsAir: true,
			ZZD_IsFix: false,
			ZZD_IsInw: false,
			ZZD_IsPublished: false,
			ZZD_IsRoa: false,
			ZZD_IsSea: false,
			ZZD_IsMai: false,
			ZZD_IsRai: false,
			ZZD_IsSystem: true,
			ZZD_IsEditable: true
		};

		let application: IApplicationList = {
			IsSelectAll: false,
			IsDeltaT: false,
			IsDeltaG: false,
			IsDeltaX: false,
			IsGamma: false,
			IsIcs: true,
			IsEcs: false,
			DateAndTime: "2020-10-30T12:00:00Z",
			Comment: "Test"
		}
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code1]));
		let validationResult = await ValidationHelper.refCusCodeListApplicationDuplication(entityManager.object, application);
		expect(validationResult).toEqual("");

		entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code2]));
		validationResult = await ValidationHelper.refCusCodeListApplicationDuplication(entityManager.object, application);
		expect(validationResult).toEqual("A record for the following application(s) with overlapping dates already exists. Cannot create a new record starting before another ends. { ICS }");

		entityManager.setup(x => x.getAsync<RefCusCodeListUserView>("RefCusCodeListUserView", [ServiceType.Safe], It.isAny(), false)).returns(() => Promise.resolve([code3]));
		validationResult = await ValidationHelper.refCusCodeListApplicationDuplication(entityManager.object, application);
		expect(validationResult).toEqual("");
	});

	it("refApplicationAttributeValidation", async () => {
		let attribute1: IRefApplicationAttribute = {
			RAA_PK: "1",
			RAA_ConfigFilePath: "AA.config",
			RAA_RAT_NKType: "123",
			RAA_AttributeName: "A",
			RAA_Value: "AAA",
			RAA_JobGroup: "",
			RAA_Content: null
		};
		let attribute2: IRefApplicationAttribute = {
			RAA_PK: "2",
			RAA_ConfigFilePath: "BB.config",
			RAA_RAT_NKType: "File",
			RAA_AttributeName: "B",
			RAA_Value: "123.txt",
			RAA_JobGroup: "",
			RAA_Content: null
		};

		let attribute3: IRefApplicationAttribute = {
			RAA_PK: "3",
			RAA_ConfigFilePath: "CC.config",
			RAA_RAT_NKType: "File",
			RAA_AttributeName: "C",
			RAA_Value: "123",
			RAA_JobGroup: "",
			RAA_Content: "Ik5PVkFJUiIgTGltaXRlZCBMaWFiaWxpdHkgQ29tcGFueSAgICAg"
		}

		let attribute4: IRefApplicationAttribute = {
			RAA_PK: "4",
			RAA_ConfigFilePath: "DD.config",
			RAA_RAT_NKType: "File",
			RAA_AttributeName: "D",
			RAA_Value: "Ref\\RefAirline List.txt",
			RAA_JobGroup: "",
			RAA_Content: "Ik5PVkFJUiIgTGltaXRlZCBMaWFiaWxpdHkgQ29tcGFueSAgICAg"
		};

		expect(ValidationHelper.refApplicationAttributeValidation(attribute1, "RAA_Value")).toEqual("");
		expect(ValidationHelper.refApplicationAttributeValidation(attribute2, "RAA_Value")).toEqual("Please choose a file.");
		expect(ValidationHelper.refApplicationAttributeValidation(attribute3, "RAA_Value")).toEqual("Please input a valid relative file path.");
		expect(ValidationHelper.refApplicationAttributeValidation(attribute4, "RAA_Value")).toEqual("");
	});

	it("refApplicationListValidation", async () => {
		let qrtzJob: IQrtzJobDetails = {
			JOB_PK: "1",
			SCHED_NAME: "AA",
			JOB_NAME: "Uxml Producer",
			JOB_GROUP: "AA",
			DESCRIPTION: "",
			JOB_CLASS_NAME: "",
			CountryCode: "ZZ",
			ProgramArgs: "",
			ProgramExePath: "AA.exe"
		};
		let entityManager = Mock.ofType<IEntityManager>();
		entityManager.setup(x => x.getAsync<IQrtzJobDetails>("QRTZ_JOB_DETAILS", [ServiceType.Staging], It.isAny(), false)).returns(() => Promise.resolve([qrtzJob]));
		entityManager.setup(x => x.filterEntities([qrtzJob], It.isAny())).returns(() => [qrtzJob]);
		expect(await ValidationHelper.refApplicationListValidation("za", "QRTZ_JOB_DETAILS", "CountryCode", entityManager.object))
			.toEqual("This value is not in the list");
		expect(await ValidationHelper.refApplicationListValidation("ZZ", "QRTZ_JOB_DETAILS", "CountryCode", entityManager.object))
			.toEqual("");
	});

	it("refShippingLineEblProviderNameListValidation", async () => {
		jest.mock("axios");
		axios.get = jest.fn().mockReturnValue({data: ["name1"]});

		let result = await ValidationHelper.refShippingLineEblProviderNameListValidation("name");
		expect(result).toBe("This value is not in the list");

		result = await ValidationHelper.refShippingLineEblProviderNameListValidation("name1");
		expect(result).toBe("");
	});

	it("ValidateVersionFormat", async () => {
		var message = "Invalid version format. Please use the format: x.x.x.x, number must be between 0 and 999";
		expect(ValidationHelper.validateVersionFormat("22.33.44.ab")).toEqual(message);
		expect(ValidationHelper.validateVersionFormat("22.33.44.9999")).toEqual(message);
		expect(ValidationHelper.validateVersionFormat("12.01.2.2")).toEqual(message);
		expect(ValidationHelper.validateVersionFormat("000.000.000.000")).toEqual(message);

		expect(ValidationHelper.validateVersionFormat("999.999.999.999")).toEqual("");
		expect(ValidationHelper.validateVersionFormat("22.33.44.55")).toEqual("");
		expect(ValidationHelper.validateVersionFormat("")).toEqual("");
	});

	it("compareVersion", async () => {
		expect(ValidationHelper.compareVersion("1.2.3.4", "1.2.3.4")).toEqual(0);
		expect(ValidationHelper.compareVersion("1.2.3.4", "4.3.2.1")).toEqual(-1);
		expect(ValidationHelper.compareVersion("1.2.3.4", "10.1.1.1")).toEqual(-1);
		expect(ValidationHelper.compareVersion("1.2.3.4", "0.9.9.9")).toEqual(1);
	});
});
