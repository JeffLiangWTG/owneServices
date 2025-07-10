using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListAttributeCombined))]
	sealed class ZZRefCusCodeListAttributeCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZZE_StartDate_Caption() => AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ZZRefCusCodeListAttributeCombined), nameof(ZZRefCusCodeListAttributeCombined.ZZE_StartDate), includesInherit: false, x => x.Caption.Equals("Start Date"));

		public void TestZZE_EndDate_Caption() => AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(ZZRefCusCodeListAttributeCombined), nameof(ZZRefCusCodeListAttributeCombined.ZZE_EndDate), includesInherit: false, x => x.Caption.Equals("End Date"));

		public void TestZZE_StartDate_ZZE_EndDate_ReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC FAC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, isDateRangeUsed: true);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DESC DEF", codeType.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea);
			var codeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType.ZZK_CodeType, "ADFSA", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(codeList.PK, "ABC", ZString.Empty, false);
			helper.CreateCusCodeListAttribute(codeList.PK, "DEF", ZString.Empty, false);
			Factory.Save();

			var codeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			codeListCombined.ZZD_Code = "ADFSA";
			codeListCombined.ZZD_CodeType = codeType.ZZK_CodeType;
			codeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attributeCombined1 = codeListCombined.Attributes.AddNew();
			attributeCombined1.ZZE_ZXE_NKName = "ABC";
			attributeCombined1.ZZE_Value = YesNoList.Codes.Yes;
			var attributeCombined2 = codeListCombined.Attributes.AddNew();
			attributeCombined2.ZZE_ZXE_NKName = "DEF";
			attributeCombined2.ZZE_Value = YesNoList.Codes.Yes;

			CombineAssertions(() =>
			{
				AssertEquals("attributeCombined1: useDateRange: StartDate", expected: false, attributeCombined1.ZZE_StartDateInfo.ReadOnly);
				AssertEquals("attributeCombined1: useDateRange: EndDate", expected: false, attributeCombined1.ZZE_EndDateInfo.ReadOnly);
				AssertEquals("attributeCombined2: useDateRange not used: StartDate", expected: true, attributeCombined2.ZZE_StartDateInfo.ReadOnly);
				AssertEquals("attributeCombined2: useDateRange not used: EndDate", expected: true, attributeCombined2.ZZE_EndDateInfo.ReadOnly);
			});
		}

		public void TestDescriptionOfZZE_Value()
		{
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("JP", "Japanese");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var codeType = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var codeList = factory.New<RefCusCodeList>();
			codeList.ZZD_ZZK_NKCodeType = codeType.ZZK_CodeType;
			codeList.ZZD_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			codeList.ZZD_StartDate = ZDateTime.Today.AddYears(-10);
			codeList.ZZD_EndDate = ZDateTime.Today.AddYears(10);
			codeList.ZZD_Code = "LXT";
			codeList.ZZD_Description = "LXTEST";

			var refCusCodeListAttributeName = factory.New<RefCusCodeListAttributeName>();
			refCusCodeListAttributeName.ZXE_ZZK_NKCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			refCusCodeListAttributeName.ZXE_ZZZ_NKDataGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			refCusCodeListAttributeName.ZXE_Name = "CUSTOMSOFFICE";
			refCusCodeListAttributeName.ZXE_ZZK_NKCodeTypeForValueList = codeType.ZZK_CodeType;

			var language = factory.New<RefCusCodeListLanguage>();
			language.ZXA_ZZD_CodeList = codeList.PK;
			language.ZXA_ZX6_NKLanguage = "JP";
			language.ZXA_Description = "テスト";

			factory.Save();

			var refCusCodeListAttribute = factory.New<ZZRefCusCodeListAttributeCombined>();
			refCusCodeListAttribute.ZZE_ZZD_CodeList = codeList.PK;
			refCusCodeListAttribute.ZZE_ZXE_NKName = "CUSTOMSOFFICE";
			refCusCodeListAttribute.ZZE_Value = "LXT";
			AssertEquals("LXTEST", refCusCodeListAttribute.DescriptionOfZZE_Value);

			GlbStaff.CurrentUser.GS_WorkingLanguage = "JA-JP";
			AssertEquals("テスト", refCusCodeListAttribute.DescriptionOfZZE_Value);

			refCusCodeListAttributeName.ZXE_ZZK_NKCodeTypeForValueList = string.Empty;
			AssertEquals(string.Empty, refCusCodeListAttribute.DescriptionOfZZE_Value);
		}

		public void TestReadOnly()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			ZZRefCusCodeListAttributeCombined attribute = codeList.Attributes.AddNew();
			codeList.ZZD_IsSystem = false;
			AssertEquals(false, attribute.ReadOnly);
			attribute.ReadOnly = true;
			AssertEquals(true, attribute.ReadOnly);
			codeList.ZZD_IsSystem = true;
			AssertEquals(true, attribute.ReadOnly);
			attribute.ReadOnly = false;
			AssertEquals(true, attribute.ReadOnly);
		}

		public void TestClone()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var attribute = codeList.Attributes.AddNew("BOB", "BUILDER");
			var clonedAttribute = (ZZRefCusCodeListAttributeCombined)attribute.Clone();
			AssertEquals("clonedAttribute.ZZE_ZZD_CodeList", codeList.PK, clonedAttribute.ZZE_ZZD_CodeList);
			AssertEquals("clonedAttribute.ZZE_ZXE_NKName", "BOB", clonedAttribute.ZZE_ZXE_NKName);
			AssertEquals("clonedAttribute.ZZE_Value", "BUILDER", clonedAttribute.ZZE_Value);
		}

		public void TestICanDeleteMembers()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var attribute = codeList.Attributes.AddNew();
			codeList.ZZD_IsSystem = false;
			ICanDelete attributeCanDelete = attribute;
			AssertEquals(true, attributeCanDelete.CanDelete);
			codeList.ZZD_IsSystem = true;
			AssertEquals(false, attributeCanDelete.CanDelete);
			AssertEquals(ZZRefCusCodeListCombined.CannotDeleteSystemGenerated, attributeCanDelete.ReasonForNotAbleToDelete);
		}

		public void TestTransportModes()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			var attribute = codeList.Attributes.AddNew();
			AssertEquals("ZZE_TransportModes", ZString.Empty, attribute.ZZE_TransportModes);
			AssertEquals("TransportModePairList.Count", RefTransportModesHelper.GetList(Factory).Count, attribute.TransportModePairList.Count);
			AssertEquals(0, attribute.TransportModePairList.Count(x => x.Value));
			int i = 0;
			var settedTransportModes = new List<string>();
			foreach (var transportMode in RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				settedTransportModes.Add(transportMode);
				attribute[ZZRefCusCodeListAttributeCombined.GetTransportModePropertyName(transportMode)] = true;
				AssertEquals("TransportModePairList.Count", ++i, attribute.TransportModePairList.Count(x => x.Value));
				Assert($"TransportModePairList[{transportMode}]", attribute.TransportModePairList.FirstOrDefault(x => x.Description == transportMode).Value);
				AssertEquals("ZZE_TransportModes", string.Join(",", settedTransportModes), attribute.ZZE_TransportModes);
			}

			foreach (var transportMode in RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				settedTransportModes.Remove(transportMode);
				attribute[ZZRefCusCodeListAttributeCombined.GetTransportModePropertyName(transportMode)] = false;
				AssertEquals("TransportModePairList.Count", --i, attribute.TransportModePairList.Count(x => x.Value));
				Assert($"TransportModePairList[{transportMode}]", !attribute.TransportModePairList.FirstOrDefault(x => x.Description == transportMode).Value);
				AssertEquals("ZZE_TransportModes", string.Join(",", settedTransportModes), attribute.ZZE_TransportModes);
			}
		}

		public void TestCodeListAttributeName()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC FAC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DESC ABC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType);
			var cusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "ADFSA", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			var cusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "F1325", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(cusCodeList1.PK, "ABC", ZString.Empty, false);
			helper.CreateCusCodeListAttribute(cusCodeList2.PK, "DEF", ZString.Empty, false);
			Factory.Save();
			var cusCodeListAttributeCombined1 = Factory.Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			var cusCodeListAttributeCombined2 = Factory.Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList2.PK)).FirstOrDefault();
			AssertNotNull(cusCodeListAttributeCombined1);
			AssertNotNull(cusCodeListAttributeCombined2);
			var attributeNamesFromCombined1 = cusCodeListAttributeCombined1.CodeListAttributeName;
			AssertNotNull(attributeNamesFromCombined1);
			AssertEquals(cusCodeList1.ZZD_ZZK_NKCodeType, attributeNamesFromCombined1.ZXE_ZZK_NKCodeType);
			AssertEquals(cusCodeList1.ZZD_ZZZ_NKDataGrouping, attributeNamesFromCombined1.ZXE_ZZZ_NKDataGrouping);
			AssertEquals(cusCodeListAttributeCombined1.ZZE_ZXE_NKName, attributeNamesFromCombined1.ZXE_Name);
			AssertEquals(cusCodeListAttributeCombined1.NameDescription, "DESC ABC");
			cusCodeListAttributeCombined1.ZZE_ZXE_NKName = "AAA";
			AssertNull(cusCodeListAttributeCombined1.CodeListAttributeName);
			AssertEquals(cusCodeListAttributeCombined1.NameDescription, ZString.Empty);
			var attributeNamesFromCombined2 = cusCodeListAttributeCombined2.CodeListAttributeName;
			AssertEquals(cusCodeListAttributeCombined2.ZZE_ZXE_NKName, attributeNamesFromCombined2.ZXE_Name);
		}

		public void TestCodeListAttributeNamePK()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC FAC", Core.Constants.CountryCodes.Eritrea);
			var codeType2 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DESC CUSOF", Core.Constants.CountryCodes.Eritrea);
			var codeType3 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC FAC", Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "DESC FR");
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea);
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DESC DEF", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea);
			var attributeName3 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.France);
			var attributeName4 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("GHI", "DESC DEF", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "ADFSA", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			Factory.Save();
			var cusCodeListCombined = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeListCombined.ZZD_CodeType = codeType1.ZZK_CodeType;
			cusCodeListCombined.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			cusCodeListCombined.ZZD_Code = "XXX";
			var attribute = cusCodeListCombined.Attributes.AddNew();
			AssertEquals("CodeListAttributeNamePK", ZGuid.Empty, attribute.CodeListAttributeNamePK);
			attribute.ZZE_ZXE_NKName = "ABC";
			AssertEquals("Set ZZE_ZXE_NKName", attributeName1.PK, attribute.CodeListAttributeNamePK);
			attribute.ZZE_ZXE_NKName = "DEF";
			AssertEquals("Set ZZE_ZXE_NKName", attributeName2.PK, attribute.CodeListAttributeNamePK);
			attribute.ZZE_ZXE_NKName = "GHI";
			AssertEquals("Set ZZE_ZXE_NKName", ZGuid.Empty, attribute.CodeListAttributeNamePK);
			attribute.CodeListAttributeNamePK = attributeName1.PK;
			AssertEquals("CodeListAttributeNamePK should be set", attributeName1.PK, attribute.CodeListAttributeNamePK);
			AssertEquals("ZZE_ZXE_NKName should be set", "ABC", attribute.ZZE_ZXE_NKName);
			attribute.CodeListAttributeNamePK = attributeName2.PK;
			AssertEquals("CodeListAttributeNamePK should be set", attributeName2.PK, attribute.CodeListAttributeNamePK);
			AssertEquals("ZZE_ZXE_NKName should be set", "DEF", attribute.ZZE_ZXE_NKName);
			attribute.CodeListAttributeNamePK = attributeName3.PK;
			AssertEquals("CodeListAttributeNamePK should not be set for wrong country", ZGuid.Empty, attribute.CodeListAttributeNamePK);
			AssertEquals("ZZE_ZXE_NKName should be clear", ZString.Empty, attribute.ZZE_ZXE_NKName);
			attribute.CodeListAttributeNamePK = attributeName4.PK;
			AssertEquals("CodeListAttributeNamePK should not be set for wrong Code Type", ZGuid.Empty, attribute.CodeListAttributeNamePK);
			AssertEquals("ZZE_ZXE_NKName should be clear", ZString.Empty, attribute.ZZE_ZXE_NKName);
		}

		public void TestZZE_ValueDataFieldTypeAndDecimalPlaces()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "DESC FAC", Core.Constants.CountryCodes.Eritrea);
			var codeType2 = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DESC CUSOF", Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "DESC ER");
			var cusCodeListAttributeName = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "DESC ABC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType2.ZZK_CodeType);
			var cusCodeList1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType, "ADFSA", DateTime.Today.AddYears(-1), DateTime.Today.AddYears(1));
			helper.CreateCusCodeListAttribute(cusCodeList1.PK, "ABC", ZString.Empty, false);
			Factory.Save();
			var cusCodeListAttributeCombined1 = Factory.Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			AssertEquals(nameof(FieldType.Text), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals(ZByte.Zero, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
			cusCodeListAttributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.String;
			Factory.Save();
			cusCodeListAttributeCombined1 = new BusinessObjectFactory().Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			AssertEquals(nameof(FieldType.Text), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals(ZByte.Zero, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
			cusCodeListAttributeName.ZXE_ZZK_NKCodeTypeForValueList = codeType1.ZZK_CodeType;
			Factory.Save();
			AssertEquals(nameof(FieldType.TextDropEdit), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals(ZByte.Zero, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
			cusCodeListAttributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean;
			Factory.Save();
			cusCodeListAttributeCombined1 = new BusinessObjectFactory().Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			AssertEquals(nameof(FieldType.TextDropEdit), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals(ZByte.Zero, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
			cusCodeListAttributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer;
			Factory.Save();
			cusCodeListAttributeCombined1 = new BusinessObjectFactory().Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			AssertEquals(nameof(FieldType.Integer), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals(ZByte.Zero, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
			cusCodeListAttributeName.ZXE_ValueDataType = Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal;
			cusCodeListAttributeName.ZXE_DecimalPlaces = 5;
			Factory.Save();
			cusCodeListAttributeCombined1 = new BusinessObjectFactory().Load<ZZRefCusCodeListAttributeCombined>(new ZQuery(ZZRefCusCodeListAttributeCombinedSchema.ZZE_ZZD_CodeList, cusCodeList1.PK)).FirstOrDefault();
			AssertEquals(nameof(FieldType.Decimal), cusCodeListAttributeCombined1.ZZE_ValueDataFieldType);
			AssertEquals((ZByte)5, cusCodeListAttributeCombined1.ZZE_ValueDecimalPlaces);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var cusCodeList = factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "SD";
			cusCodeList.ZZD_Code = "B0B";
			cusCodeList.ZZD_Description = "BOB THE BUILDER";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			cusCodeList.ZZD_StartDate = ZDateTime.Today;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var attribute = cusCodeList.Attributes.AddNew("BOBAttribute", "SHORT");
			return attribute;
		}
	}
}
