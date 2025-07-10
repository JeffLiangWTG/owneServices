import { Mock, It } from "typemoq";
import { IEntityManager, ServiceType } from "../EntityManager";
import IRefCusCodeListAttributeName from "../models/IRefCusCodeListAttributeName";
import { RefCusCodeListAttributeUserView } from "../models/RefCusCodeListAttributeUserView";
import { RefCusCodeListUserView } from "../models/RefCusCodeListUserView";
import { RefStlScriptUserView } from "../models/RefStlScriptUserView";
import uuid from "uuid";
import { ValidationServiceHelper } from "../ValidationServiceHelper";
import { ValidationServiceWrapper } from "../ValidationService";
import { ValidationHelper } from "../ValidationHelper";
import { Filter, FilterOps } from "../Filter";

describe("RefCusCodeList related", () => {
	it("getRefCusCodeListAttributeValidationService_ZZE_ZXE_Name_ThisValueIsNotInTheList", async () => {
		let codeList1 = createCodeList("CD1", "CDT", "AU");
		let codeList2 = createCodeList("CD1", "CDT", "BR");

		let codeListAttr1 = createCodeListAttribute(
			"CDT",
			"AU",
			"name1",
			codeList1.ZZD_PK,
			"any",
			null,
			null
		);
		let codeListAttrName = createCodeListAttributeName(
			"name1",
			"CDT",
			"AU",
			"",
			true,
			true,
			false,
			false
		);
		let entityManager = Mock.ofType<IEntityManager>();

		entityManager
			.setup((x) =>
				x.getAsync<RefCusCodeListUserView>(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeList1, codeList2]));
		entityManager
			.setup((x) =>
				x.getAsync<RefCusCodeListAttributeUserView>(
					"RefCusCodeListAttributeUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeListAttr1]));

		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					[
						new Filter(
							"ZXE_ZZK_NKCodeType",
							FilterOps.Equals,
							codeList1.ZZD_CodeType as any,
							"string" as any
						),
						new Filter(
							"ZXE_ZZZ_NKDataGrouping",
							FilterOps.Equals,
							codeList1.ZZD_CountryOrGrouping as any,
							"string" as any
						),
						new Filter(
							"ZXE_Name",
							FilterOps.Equals,
							codeListAttrName.ZXE_Name as any,
							"string" as any
						),
					],
					false
				)
			)
			.returns(() => Promise.resolve([codeListAttrName]));

		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					[
						new Filter(
							"ZXE_ZZK_NKCodeType",
							FilterOps.Equals,
							codeList2.ZZD_CodeType as any,
							"string" as any
						),
						new Filter(
							"ZXE_ZZZ_NKDataGrouping",
							FilterOps.Equals,
							codeList2.ZZD_CountryOrGrouping as any,
							"string" as any
						),
						new Filter(
							"ZXE_Name",
							FilterOps.Equals,
							codeListAttrName.ZXE_Name as any,
							"string" as any
						),
					],
					false
				)
			)
			.returns(() => Promise.resolve([]));

		let validationServices = new ValidationServiceWrapper(
			[
				ValidationServiceHelper.getRefCusCodeListAttributeValidationService(
					entityManager.object
				),
			],
			() => false
		);

		let attrToTestSameNameDifferentGrouping = createCodeListAttribute(
			"CDT",
			"BR",
			"name1",
			codeList2.ZZD_PK,
			"any",
			null,
			null
		);
		let result = await ValidationHelper.validateProperty(
			validationServices,
			attrToTestSameNameDifferentGrouping,
			"ZZE_ZXE_NKName",
			0
		);
		expect(result).toEqual(["This value is not in the list"]);
	});

	it("refCusCodeListAttribute_startAndEndDateValidations_mandatoryIfAttributeName_ZXE_IsDateRangeUsed_equals_to_true", async () => {
		let entityManager = Mock.ofType<IEntityManager>();

		let codeList1 = createCodeList("CD1", "CDT", "AU");
		let codeListAttr1 = createCodeListAttribute(
			"CDT",
			"AU",
			"name1",
			codeList1.ZZD_PK,
			"any",
			null,
			null
		);
		let codeListAttrName = createCodeListAttributeName(
			"name1",
			"CDT",
			"AU",
			"",
			true,
			true,
			false,
			true
		);

		entityManager
			.setup((x) =>
				x.getAsync<RefCusCodeListUserView>(
					"RefCusCodeListUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeList1]));
		entityManager
			.setup((x) =>
				x.getAsync<RefCusCodeListAttributeUserView>(
					"RefCusCodeListAttributeUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([codeListAttr1]));
		entityManager
			.setup((x) =>
				x.getAsync<IRefCusCodeListAttributeName>(
					"RefCusCodeListAttributeName",
					[ServiceType.Safe],
					[
						new Filter(
							"ZXE_ZZK_NKCodeType",
							FilterOps.Equals,
							codeList1.ZZD_CodeType as any,
							"string" as any
						),
						new Filter(
							"ZXE_ZZZ_NKDataGrouping",
							FilterOps.Equals,
							codeList1.ZZD_CountryOrGrouping as any,
							"string" as any
						),
						new Filter(
							"ZXE_Name",
							FilterOps.Equals,
							codeListAttrName.ZXE_Name as any,
							"string" as any
						),
					],
					false
				)
			)
			.returns(() => Promise.resolve([codeListAttrName]));

		let validationServices = new ValidationServiceWrapper(
			[
				ValidationServiceHelper.getRefCusCodeListAttributeValidationService(
					entityManager.object
				),
			],
			() => false
		);
		//validate StartDate
		let result = await ValidationHelper.validateProperty(
			validationServices,
			codeListAttr1,
			"ZZE_StartDate",
			0
		);
		expect(result).toEqual(["This field is required."]);

		//validate EndDate
		result = await ValidationHelper.validateProperty(
			validationServices,
			codeListAttr1,
			"ZZE_EndDate",
			0
		);
		expect(result).toEqual(["This field is required."]);

		//set start and end date
		codeListAttr1.ZZE_StartDate = "2024-10-10";
		codeListAttr1.ZZE_EndDate = "2024-11-11";
		//validate StartDate
		result = await ValidationHelper.validateProperty(
			validationServices,
			codeListAttr1,
			"ZZE_StartDate",
			0
		);
		expect(result).toEqual([]);
		//validate EndDate
		result = await ValidationHelper.validateProperty(
			validationServices,
			codeListAttr1,
			"ZZE_EndDate",
			0
		);
		expect(result).toEqual([]);
	});

	it("refStlScriptUserViewValidations", async () => {
		let entityManager = Mock.ofType<IEntityManager>();

		let stlScriptView = new RefStlScriptUserView();
		stlScriptView.STL_PK = uuid.v1();
		stlScriptView.STL_FeatureCode = "";
		stlScriptView.STL_RoleName = "";
		stlScriptView.STL_ModuleName = "";
		stlScriptView.STL_FunctionName = "";
		stlScriptView.STL_FeatureName = "";
		stlScriptView.STL_DataGranularity = "TRN";
		stlScriptView.STL_CompanyCode = "";
		stlScriptView.STL_BranchCode = "";
		stlScriptView.STL_TransactionDateUtc = "";
		stlScriptView.STL_CreatingUserCode = "";
		stlScriptView.STL_GuidReference = "";
		stlScriptView.STL_BillingReference1 = "";
		stlScriptView.STL_BillingReference2 = "";
		stlScriptView.STL_BillingReference3 = "";
		stlScriptView.STL_BillingReference4 = "";
		stlScriptView.STL_AdditionalRefs = "";
		stlScriptView.STL_TransactionCount = "";
		stlScriptView.STL_PreparationScript = "";
		stlScriptView.STL_FromClause = "";
		stlScriptView.STL_WhereClause = "";
		stlScriptView.STL_WithOptionRecompile = false;
		stlScriptView.STL_UsedInBilling = true;
		stlScriptView.STL_ActiveOn = "ALL";
		stlScriptView.STL_MinCW1Version = "11.11.11.11";
		stlScriptView.STL_MaxCW1Version = "22.22.22.22";
		stlScriptView.STL_DateType = "DTE";
		stlScriptView.STL_CollectionStartDateUtc = null;
		(stlScriptView.STL_IsSystem = true), (stlScriptView.STL_IsPublished = true);

		entityManager
			.setup((x) =>
				x.getAsync<RefStlScriptUserView>(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([]));

		let validationServices = new ValidationServiceWrapper(
			[
				ValidationServiceHelper.getRefStlScriptValidationService(
					entityManager.object
				),
			],
			() => false
		);

		let result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_FeatureCode",
			0
		);
		expect(result).toEqual([
			"This field is required.",
			"This field must have 3 characters",
		]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_MinCW1Version",
			0
		);
		expect(result).toEqual([]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_MaxCW1Version",
			0
		);
		expect(result).toEqual([]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_GuidReference",
			0
		);
		expect(result).toEqual(["This field is required."]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_TransactionCount",
			0
		);
		expect(result).toEqual(["This field is required."]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_TransactionDateUtc",
			0
		);
		expect(result).toEqual(["This field is required."]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_FromClause",
			0
		);
		expect(result).toEqual(["This field is required."]);

		stlScriptView.STL_FeatureCode = "AAA";
		entityManager
			.setup((x) =>
				x.getAsync<RefStlScriptUserView>(
					"RefStlScriptUserView",
					[ServiceType.Safe],
					It.isAny(),
					false
				)
			)
			.returns(() => Promise.resolve([stlScriptView]));

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_FeatureCode",
			0
		);
		expect(result).toEqual([
			"RefStlScriptUserView (AAA,ALL,11.11.11.11,22.22.22.22) already exists.",
		]);

		stlScriptView.STL_MinCW1Version = "110.22.22.22";
		stlScriptView.STL_MaxCW1Version = "11.11.11.11";

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_MinCW1Version",
			0
		);
		expect(result).toEqual(["Min CW Version must be less than Max CW Version"]);

		result = await ValidationHelper.validateProperty(
			validationServices,
			stlScriptView,
			"STL_MaxCW1Version",
			0
		);
		expect(result).toEqual(["Min CW Version must be less than Max CW Version"]);
	});

	//helper functions
	function createCodeList(
		code: string,
		codeType: string,
		countryOrGrouping: string
	): RefCusCodeListUserView {
		return {
			ZZD_PK: "12345",
			ZZD_Code: code,
			ZZD_CodeType: codeType,
			ZZD_CountryOrGrouping: countryOrGrouping,
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
			ZZD_IsEditable: true,
		};
	}

	function createCodeListAttribute(
		codeType: string,
		countryOrGrouping: string,
		nKName: string,
		codeListPK: string,
		value: string,
		startDate: string | null,
		endDate: string | null
	): RefCusCodeListAttributeUserView {
		return {
			ZZE_CodeType: codeType,
			ZZE_CountryOrGrouping: countryOrGrouping,
			ZZE_IsAir: false,
			ZZE_IsFix: false,
			ZZE_IsInw: false,
			ZZE_IsMai: false,
			ZZE_IsRai: false,
			ZZE_IsRoa: false,
			ZZE_IsSea: false,
			ZZE_PK: uuid(),
			ZZE_Value: value,
			ZZE_IsEditable: true,
			ZZE_ZXE_NKName: nKName,
			ZZE_ZZD_CodeList: codeListPK,
			ZZE_StartDate: startDate,
			ZZE_EndDate: endDate,
		};
	}

	function createCodeListAttributeName(
		name: string,
		codeType: string,
		dataGrouping: string,
		nkCodeTypeForValueList: string,
		isValueMandatory: boolean,
		isMandatory: boolean,
		allowDuplicates: boolean,
		isDateRangeUsed: boolean
	): IRefCusCodeListAttributeName {
		return {
			ZXE_Name: name,
			ZXE_ZZK_NKCodeType: codeType,
			ZXE_AllowDuplicates: allowDuplicates,
			ZXE_IsMandatory: isMandatory,
			ZXE_IsValueMandatory: isValueMandatory,
			ZXE_ZZZ_NKDataGrouping: dataGrouping,
			ZXE_Description: "ANY",
			ZXE_PK: uuid(),
			ZXE_ZZK_NKCodeTypeForValueList: nkCodeTypeForValueList,
			ZXE_ColumnCaption: "",
			ZXE_DecimalPlaces: 0,
			ZXE_MaxLengthOrValue: 0,
			ZXE_MinLengthOrValue: 0,
			ZXE_ValueDataType: "",
			ZXE_IsDateRangeUsed: isDateRangeUsed,
		};
	}
});
