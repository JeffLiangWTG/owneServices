using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(ZZRefCusCodeListCombined))]
	class ZZRefCusCodeListCombinedTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGetUniqueCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);
			var au_typeA_code1 = Factory.New<ZZRefCusCodeListCombined>();
			au_typeA_code1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			au_typeA_code1.ZZD_CodeType = "TYPEA";
			au_typeA_code1.ZZD_Code = "CODE1";
			var fr_typeA_code2 = Factory.New<ZZRefCusCodeListCombined>();
			fr_typeA_code2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			fr_typeA_code2.ZZD_CodeType = "TYPEA";
			fr_typeA_code2.ZZD_Code = "CODE2";
			var eu_typeA_code2 = Factory.New<ZZRefCusCodeListCombined>();
			eu_typeA_code2.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			eu_typeA_code2.ZZD_CodeType = "TYPEA";
			eu_typeA_code2.ZZD_Code = "CODE2";
			var fr_typeB_code3 = Factory.New<ZZRefCusCodeListCombined>();
			fr_typeB_code3.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			fr_typeB_code3.ZZD_CodeType = "TYPEB";
			fr_typeB_code3.ZZD_Code = "CODE3";
			var eu_typeA_code4 = Factory.New<ZZRefCusCodeListCombined>();
			eu_typeA_code4.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			eu_typeA_code4.ZZD_CodeType = "TYPEA";
			eu_typeA_code4.ZZD_Code = "CODE4";
			Factory.Save();
			var uniqueCodes = ZZRefCusCodeListCombined.GetUniqueCodes(Factory, Core.Constants.CountryCodes.France, "TYPEA", ZDateTime.Today, false);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CODE2" }, uniqueCodes);
			var uniqueCodesIncludingParent = ZZRefCusCodeListCombined.GetUniqueCodes(Factory, Core.Constants.CountryCodes.France, "TYPEA", ZDateTime.Today, true);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CODE2", "CODE4" }, uniqueCodesIncludingParent);
		}

		public void TestSetDefault()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			AssertEquals("cusCodeList.ZZD_StartDate", ZDateTime.Today, cusCodeList.ZZD_StartDate);
			AssertEquals("cusCodeList.ZZD_EndDate", ZDateTime.MaxSmallDateTimeValue, cusCodeList.ZZD_EndDate);
			AssertEquals("cusCodeList.ZZD_CountryOrGrouping", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, cusCodeList.ZZD_CountryOrGrouping);
		}

		public void TestAutoLoggingIsEnabled()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "SD";
			cusCodeList.ZZD_Code = "B0B";
			cusCodeList.ZZD_Description = "BOB THE BUILDER";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			cusCodeList.ZZD_StartDate = ZDateTime.Today;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			AssertNull(cusCodeList.Logs.MostRecentLog);
			Factory.Save();
			AssertNotNull(cusCodeList.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem));
		}

		public void TestCanDelete()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_IsSystem = false;
			AssertEquals("CanDelete", true, cusCodeList.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", "", cusCodeList.ReasonForNotAbleToDelete);
			cusCodeList.ZZD_IsSystem = true;
			AssertEquals("CanDelete", false, cusCodeList.CanDelete);
			AssertEquals("ReasonForNotAbleToDelete", ZZRefCusCodeListCombined.CannotDeleteSystemGenerated, cusCodeList.ReasonForNotAbleToDelete);
		}

		public void TestDelete()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_CodeType = "SD";
			cusCodeList.ZZD_Code = "B0B";
			cusCodeList.ZZD_Description = "BOB THE BUILDER";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			cusCodeList.ZZD_StartDate = ZDateTime.Today;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var attribute1 = cusCodeList.Attributes.AddNew("BOBAttribute", "SHORT");
			var attribute2 = cusCodeList.Attributes.AddNew("BOBAttribute1", "SHORT");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var cusCodeListInDiffFactory = newFactory.Load<ZZRefCusCodeListCombined>(cusCodeList.PK);
			cusCodeListInDiffFactory.Delete();
			newFactory.Save();
			newFactory = new BusinessObjectFactory();
			AssertNull("cusCodeList.IsDeleted", newFactory.Load<ZZRefCusCodeListCombined>(cusCodeList.PK));
			AssertNull("attribute1.IsDeleted", newFactory.Load<ZZRefCusCodeListAttributeCombined>(attribute1.PK));
			AssertNull("attribute2.IsDeleted", newFactory.Load<ZZRefCusCodeListAttributeCombined>(attribute2.PK));
		}

		public void TestTemplateCopy()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_IsSystem = true;
			cusCodeList.ZZD_CodeType = "SD";
			cusCodeList.ZZD_Code = "B0B";
			cusCodeList.ZZD_Description = "BOB THE BUILDER";
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Cambodia;
			cusCodeList.ZZD_StartDate = ZDateTime.Today;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			var attribute1 = cusCodeList.Attributes.AddNew("BOBAttribute", "SHORT");
			var attribute2 = cusCodeList.Attributes.AddNew("BOBAttribute1", "SHIRT");
			var attribute3 = cusCodeList.Attributes.AddNew("BOBAttribute3", "VALUE1", ZDateTime.Today, ZDateTime.Today.AddDays(10));
			var attribute4 = cusCodeList.Attributes.AddNew("BOBAttribute3", "VALUE2", ZDateTime.Today.AddDays(11), ZDateTime.Today.AddYears(1));
			var clonedData = (ZZRefCusCodeListCombined)cusCodeList.TemplateCopy();
			AssertEquals("clonedData.ZZD_IsSystem", false, clonedData.ZZD_IsSystem);
			AssertEquals("clonedData.ZZD_CodeType", "SD", clonedData.ZZD_CodeType);
			AssertEquals("clonedData.ZZD_Code", "B0B", clonedData.ZZD_Code);
			AssertEquals("clonedData.ZZD_Description", "BOB THE BUILDER", clonedData.ZZD_Description);
			AssertEquals("clonedData.ZZD_CountryOrGrouping", Core.Constants.CountryCodes.Cambodia, clonedData.ZZD_CountryOrGrouping);
			AssertEquals("clonedData.ZZD_StartDate", ZDateTime.Today, clonedData.ZZD_StartDate);
			AssertEquals("clonedData.ZZD_EndDate", ZDateTime.Today.AddYears(1), clonedData.ZZD_EndDate);
			AssertEquals("clonedData.Attributes.Count", 4, clonedData.Attributes.Count);
			var clonedAttributes = clonedData.Attributes.Cast<ZZRefCusCodeListAttributeCombined>().OrderBy(x => x.ZZE_ZXE_NKName).ThenBy(x => x.ZZE_StartDate).ToList();
			var clonedAttribute1 = clonedAttributes[0];
			var clonedAttribute2 = clonedAttributes[1];
			var clonedAttribute3 = clonedAttributes[2];
			var clonedAttribute4 = clonedAttributes[3];

			AssertEquals("clonedAttribute1.ZZE_ZXE_NKName", "BOBAttribute", clonedAttribute1.ZZE_ZXE_NKName);
			AssertNotEquals("clonedAttribute1.PK", attribute1.PK, clonedAttribute1.PK);
			AssertEquals("clonedAttribute1.ZZE_Value", "SHORT", clonedAttribute1.ZZE_Value);
			AssertEquals("clonedAttribute2.ZZE_ZXE_NKName", "BOBAttribute1", clonedAttribute2.ZZE_ZXE_NKName);
			AssertNotEquals("clonedAttribute2.PK", attribute2.PK, clonedAttribute2.PK);
			AssertEquals("clonedAttribute2.ZZE_Value", "SHIRT", clonedAttribute2.ZZE_Value);

			AssertNotEquals("clonedAttribute3.PK", attribute2.PK, clonedAttribute2.PK);
			AssertEquals("clonedAttribute2.ZZE_Value", "SHIRT", clonedAttribute2.ZZE_Value);

			AssertNotEquals("clonedAttribute3.PK", attribute3.PK, clonedAttribute3.PK);
			AssertEquals("clonedAttribute3.ZZE_Value", "VALUE1", clonedAttribute3.ZZE_Value);
			AssertEquals("clonedAttribute3.ZZE_Value", ZDateTime.Today, clonedAttribute3.ZZE_StartDate);
			AssertEquals("clonedAttribute3.ZZE_Value", ZDateTime.Today.AddDays(10), clonedAttribute3.ZZE_EndDate);

			AssertNotEquals("clonedAttribute4.PK", attribute4.PK, clonedAttribute4.PK);
			AssertEquals("clonedAttribute4.ZZE_Value", "VALUE2", clonedAttribute4.ZZE_Value);
			AssertEquals("clonedAttribute4.ZZE_Value", ZDateTime.Today.AddDays(11), clonedAttribute4.ZZE_StartDate);
			AssertEquals("clonedAttribute4.ZZE_Value", ZDateTime.Today.AddYears(1), clonedAttribute4.ZZE_EndDate);
		}

		public void TestHasAttribute()
		{
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			var attr2 = cusCodeList2.Attributes.AddNew();
			attr2.ZZE_ZXE_NKName = "TEST";
			var cusCodeList3 = Factory.New<ZZRefCusCodeListCombined>();
			var attr3 = cusCodeList3.Attributes.AddNew();
			attr3.ZZE_ZXE_NKName = "TEST";
			attr3.ZZE_Value = "TESTV";
			Assert(!cusCodeList1.HasAttribute("TEST"));
			Assert(cusCodeList2.HasAttribute("TEST"));
			Assert(cusCodeList3.HasAttribute("TEST"));
			Assert(!cusCodeList1.HasAttribute("TEST", "TESTV"));
			Assert(!cusCodeList2.HasAttribute("TEST", "TESTV"));
			Assert(cusCodeList3.HasAttribute("TEST", "TESTV"));
		}

		public void TestHasAttributeWithDate()
		{
			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			var attr1 = cusCodeList.Attributes.AddNew();
			attr1.ZZE_ZXE_NKName = "TEST";
			attr1.ZZE_Value = "CURRENT";
			attr1.ZZE_EndDate = ZDateTime.Today.AddDays(10);
			var attr2 = cusCodeList.Attributes.AddNew();
			attr2.ZZE_ZXE_NKName = "TEST";
			attr2.ZZE_Value = "FUTURE";
			attr2.ZZE_StartDate = ZDateTime.Today.AddDays(10);

			CombineAssertions(() =>
			{
				Assert("Has attribute, no value", cusCodeList.HasAttribute("TEST"));
				Assert("Has attribute CURRENT", cusCodeList.HasAttribute("TEST", "CURRENT"));
				Assert("Has no attribute, FUTURE", !cusCodeList.HasAttribute("TEST", "FUTURE"));
				Assert("Has attribute, no value, future date", cusCodeList.HasAttribute("TEST", date: ZDateTime.Today.AddDays(11)));
				Assert("Has no attribute CURRENT, future date", !cusCodeList.HasAttribute("TEST", "CURRENT", ZDateTime.Today.AddDays(11)));
				Assert("Has attribute FUTURE, future date", cusCodeList.HasAttribute("TEST", "FUTURE", ZDateTime.Today.AddDays(11)));
			});
		}

		public void TestGetAttribute()
		{
			var tester1 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			tester1.Attributes.AddNew("ATTRC1", "ATTRV1");
			var tester2 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			tester2.Attributes.AddNew("ATTRC2", "ATTRV2");
			var tester3 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			var tester4 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			tester4.Attributes.AddNew("ATTRC1", "ATTRV1");
			tester4.Attributes.AddNew("ATTRC1", "ATTRV2");
			AssertEquals("ATTRV1", tester1.GetAttribute("ATTRC1"));
			AssertEquals("", tester2.GetAttribute("ATTRC1"));
			AssertEquals("", tester3.GetAttribute("ATTRC1"));
			AssertEquals("ATTRV1", tester4.GetAttribute("ATTRC1"));
			AssertEquals(string.Empty, tester1.GetAttribute("ATTRC2"));
			AssertEquals("ATTRV2", tester2.GetAttribute("ATTRC2"));
			AssertEquals(string.Empty, tester3.GetAttribute("ATTRC2"));
			AssertEquals(string.Empty, tester4.GetAttribute("ATTRC2"));
		}

		public void TestGetAttributeWithDate()
		{
			var dateBefore = ZDateTime.Today.AddDays(-10);
			var dateAfter = ZDateTime.Today.AddDays(10);
			var dateAfter2 = ZDateTime.Today.AddDays(20);
			var list = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			var attrValueOld = list.Attributes.AddNew("ATTR", "OLD");
			attrValueOld.ZZE_EndDate = dateBefore.AddDays(-1);
			var attrValueCurrent = list.Attributes.AddNew("ATTR", "CURRENT");
			attrValueCurrent.ZZE_StartDate = dateBefore;
			attrValueCurrent.ZZE_EndDate = dateAfter.AddDays(-1);
			var attrValueFuture = list.Attributes.AddNew("ATTR", "FUTURE");
			attrValueFuture.ZZE_StartDate = dateAfter2;

			CombineAssertions(() =>
			{
				AssertEquals("Default date range", "CURRENT", list.GetAttribute("ATTR"));
				AssertEquals("Current date range", "CURRENT", list.GetAttribute("ATTR", ZDateTime.Today));
				AssertEquals("Old date range", "OLD", list.GetAttribute("ATTR", dateBefore.AddDays(-1)));
				AssertEquals("Future date range1", string.Empty, list.GetAttribute("ATTR", dateAfter.AddDays(1)));
				AssertEquals("Future date range2", "FUTURE", list.GetAttribute("ATTR", dateAfter2));
			});
		}

		public void TestGetAttribute_CaseInsensitiveType()
		{
			// Create attributes with a type in lower case (string insteadof STRING)
			var (attributeName1, attributeName2) = SetupAttributesWithDataTypes(Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean.ToLower(), Constants.RefCusCodeListAttributeName.ValueDataTypes.String.ToLower(), true.ToString(), "STRING2");
			var stringCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "CD1", Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			// Check if attribute value is retrieved correctly (without looking at the case of the data type)
			CombineAssertions(() =>
			{
				AssertEquals("True type", true, stringCode.GetAttribute(attributeName1));
				AssertEquals("string data type", "STRING2", stringCode.GetAttribute(attributeName2));
			}

			);
		}

		public void TestGetAttributeWithType_String()
		{
			var (attributeName1, attributeName2) = SetupAttributesWithDataTypes(ZString.Empty, Constants.RefCusCodeListAttributeName.ValueDataTypes.String, "STRING1", "STRING2");
			var stringCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "CD1", Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("Empty data type", "STRING1", stringCode.GetAttribute(attributeName1));
				AssertEquals("string data type", "STRING2", stringCode.GetAttribute(attributeName2));
			}

			);
		}

		public void TestGetAttributeWithType_Boolean()
		{
			var (attributeName1, attributeName2) = SetupAttributesWithDataTypes(Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean, Constants.RefCusCodeListAttributeName.ValueDataTypes.Boolean, true.ToString(), "NAME");
			var boolCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "CD1", Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("True Attribute", true, boolCode.GetAttribute(attributeName1));
				AssertEquals("Invalid attribute value", false, boolCode.GetAttribute(attributeName2));
			}

			);
		}

		public void TestGetAttributeWithType_Integer()
		{
			var (attributeName1, attributeName2) = SetupAttributesWithDataTypes(Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer, Constants.RefCusCodeListAttributeName.ValueDataTypes.Integer, "123", "A123");
			var intCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "CD1", Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("Integer Attribute", 123, intCode.GetAttribute(attributeName1));
				AssertEquals("Invalid attribute value", ZInt.Zero, intCode.GetAttribute(attributeName2));
			}

			);
		}

		public void TestGetAttributeWithType_Decimal()
		{
			var (attributeName1, attributeName2) = SetupAttributesWithDataTypes(Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal, Constants.RefCusCodeListAttributeName.ValueDataTypes.Decimal, "12.34", "89.A");
			var decimalCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, "CD1", Core.Constants.CountryCodes.China, "TP1", ZDateTime.Today);
			CombineAssertions(() =>
			{
				AssertEquals("Decimal Attribute", 12.34m, decimalCode.GetAttribute(attributeName1));
				AssertEquals("Invalid attribute value", ZDecimal.Zero, decimalCode.GetAttribute(attributeName2));
			}

			);
		}

		public void TestSetAttribute()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateNewOrGetExistingCusCodeType("TP1", "Ref Code Type 1");
			var attName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", Core.Constants.CountryCodes.China);
			attName1.ZXE_ColumnCaption = "Attribute Name 1";
			attName1.ZXE_ValueDataType = "STRING";
			var attName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute Name 2", "TP1", Core.Constants.CountryCodes.China);
			attName2.ZXE_ColumnCaption = "Attribute Name 2";
			attName2.ZXE_ValueDataType = "INTEGER";
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD1", yesterday, tomorrow);
			codeList1.Attributes.AddNew("ATT1", "0");
			codeList1.Attributes.AddNew("ATT2", "0");
			Factory.Save();
			var tester1 = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown("Should not throw any exception even when no Attr found.", () =>
				{
					tester1.SetAttribute(attName1, "ATTRV1");
				}

				);
				var att1 = tester1.Attributes.AddNew("ATT1", "ATTRV1");
				tester1.SetAttribute(attName1, "ATTRV2");
				AssertEquals("Should have changed attr value with matched name.", "ATTRV2", att1.ZZE_Value);
				tester1.SetAttribute(attName2, 100);
				AssertEquals("Should not change attr with another name.", "ATTRV2", att1.ZZE_Value);
			}

			);
		}

		public void TestGetAttributesValues()
		{
			var zzd = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd.Attributes.AddNew("A", "1");
			zzd.Attributes.AddNew("A", "2");
			zzd.Attributes.AddNew("A", "3");
			zzd.Attributes.AddNew("B", "3");
			zzd.Attributes.AddNew("B", "4");
			CombineAssertions(() =>
			{
				AssertEquals(3, zzd.GetAttributesValues("A").Count());
				AssertEquals(2, zzd.GetAttributesValues("B").Count());
				AssertEquals(0, zzd.GetAttributesValues("C").Count());
				AssertEquals(0, zzd.GetAttributesValues("").Count());
				AssertArrayEqualsByElements(new ZString[] { "1", "2", "3" }, zzd.GetAttributesValues("A").ToArray());
				AssertArrayEqualsByElements(new ZString[] { "3", "4" }, zzd.GetAttributesValues("B").ToArray());
			});
		}

		public void TestGetAttributesValuesWithDate()
		{
			var zzd = Factory.NewWithValidTestData<ZZRefCusCodeListCombined>();
			zzd.Attributes.AddNew("A", "1");
			zzd.Attributes.AddNew("A", "2");
			zzd.Attributes.AddNew("A", "3");
			zzd.Attributes.AddNew("B", "3");
			zzd.Attributes.AddNew("B", "4");
			zzd.Attributes.AddNew("B", "5", ZDateTime.Today.AddDays(1));
			CombineAssertions(() =>
			{
				AssertEquals("A count", 3, zzd.GetAttributesValues("A").Count());
				AssertEquals("B count", 2, zzd.GetAttributesValues("B").Count());
				AssertEquals("C count", 0, zzd.GetAttributesValues("C").Count());
				AssertEquals("Empty count", 0, zzd.GetAttributesValues("").Count());
				AssertArrayEqualsByElements("A elements", new ZString[] { "1", "2", "3" }, zzd.GetAttributesValues("A").ToArray());
				AssertArrayEqualsByElements("B elements", new ZString[] { "3", "4" }, zzd.GetAttributesValues("B").ToArray());
				AssertArrayEqualsByElements("B elements with date", new ZString[] { "3", "4", "5" },
					zzd.GetAttributesValues("B", ZDateTime.Today.AddDays(1)).ToArray());
			});
		}

		public void TestLanguages()
		{
			AssertType<ZZRefCusCodeListLanguageCombinedCollection>(Factory.NewWithValidTestData<ZZRefCusCodeListCombined>().Languages);
		}

		public void TestTransportModes()
		{
			var codeList = Factory.New<ZZRefCusCodeListCombined>();
			AssertEquals("ZZD_TransportModes", ZString.Empty, codeList.ZZD_TransportModes);
			AssertEquals("TransportModePairList.Count", RefTransportModesHelper.GetList(Factory).Count, codeList.TransportModePairList.Count);
			AssertEquals(0, codeList.TransportModePairList.Count(x => x.Value));
			int i = 0;
			var settedTransportModes = new List<string>();
			foreach (var transportMode in RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				settedTransportModes.Add(transportMode);
				codeList[ZZRefCusCodeListCombined.GetTransportModePropertyName(transportMode)] = true;
				AssertEquals("TransportModePairList.Count", ++i, codeList.TransportModePairList.Count(x => x.Value));
				Assert($"TransportModePairList[{transportMode}]", codeList.TransportModePairList.FirstOrDefault(x => x.Description == transportMode).Value);
				AssertEquals("ZZD_TransportModes", string.Join(",", settedTransportModes), codeList.ZZD_TransportModes);
			}

			foreach (var transportMode in RefTransportModesHelper.GetList(Factory).Cast<CodeDescriptionPair>().Select(x => x.Code))
			{
				settedTransportModes.Remove(transportMode);
				codeList[ZZRefCusCodeListCombined.GetTransportModePropertyName(transportMode)] = false;
				AssertEquals("TransportModePairList.Count", --i, codeList.TransportModePairList.Count(x => x.Value));
				Assert($"TransportModePairList[{transportMode}]", !codeList.TransportModePairList.FirstOrDefault(x => x.Description == transportMode).Value);
				AssertEquals("ZZD_TransportModes", string.Join(",", settedTransportModes), codeList.ZZD_TransportModes);
			}
		}

		public void TestFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("CHS", "Chinese");
			var typeCode = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			helper.CreateNewOrGetExistingCusCodeType(typeCode, "CustomsOffice");
			var codeList1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, typeCode, "AAA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			var codeList2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.SouthAfrica, typeCode, "BBB", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(1));
			Factory.Save();
			var language1 = Factory.New<RefCusCodeListLanguage>();
			language1.ZXA_ZZD_CodeList = codeList1.PK;
			language1.ZXA_ZX6_NKLanguage = "ENG";
			language1.ZXA_Description = "Test Description 001";
			var language2 = Factory.New<RefCusCodeListLanguage>();
			language2.ZXA_ZZD_CodeList = codeList1.PK;
			language2.ZXA_ZX6_NKLanguage = "CHS";
			language2.ZXA_Description = "Test Description 002";
			var language3 = Factory.New<RefCusCodeListLanguage>();
			language3.ZXA_ZZD_CodeList = codeList2.PK;
			language3.ZXA_ZX6_NKLanguage = "ENG";
			language3.ZXA_Description = "Test Description 003";
			var language4 = Factory.New<RefCusCodeListLanguage>();
			language4.ZXA_ZZD_CodeList = codeList2.PK;
			language4.ZXA_ZX6_NKLanguage = "CHS";
			language4.ZXA_Description = "Test Description 004";
			Factory.Save();
			var newFactory = NewFactory();
			AssertCollectionNotContains(RefCusCodeListLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var query = new ZQuery(ZZRefCusCodeListCombinedSchema.PK, new[] { codeList1.PK, codeList2.PK });
			var codeLists = newFactory.Load<ZZRefCusCodeListCombined>(query);
			AssertEquals(2, codeLists.Length);
			AssertCollectionContains(RefCusCodeListLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
		}

		public void TestZZD_Description()
		{
			var facility = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(facility, "Facilities");
			var german = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, facility, "1", "One", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			var italian = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, facility, "2", "Two", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			var germanCombined = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, german.ZZD_Code, german.ZZD_ZZZ_NKDataGrouping, german.ZZD_ZZK_NKCodeType, ZDateTime.Today);
			var italianCombined = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, italian.ZZD_Code, italian.ZZD_ZZZ_NKDataGrouping, italian.ZZD_ZZK_NKCodeType, ZDateTime.Today);
			helper.CreateOrGetLanguage("DE", "German");
			helper.CreateNewOrGetExistingCusCodeListLanguage(german, "DE", "Eins");
			helper.CreateOrGetLanguage("IT", "Italian");
			helper.CreateNewOrGetExistingCusCodeListLanguage(italian, "IT", "Ccy");
			var currentUser = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			currentUser.GS_WorkingLanguage = "EN";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in English", "One", germanCombined.ZZD_Description);
				AssertEquals("Code in English", "Two", italianCombined.ZZD_Description);
			}

			currentUser.GS_WorkingLanguage = "DE-DE";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in German", "Eins", germanCombined.ZZD_Description);
				AssertEquals("Code in English", "Two", italianCombined.ZZD_Description);
			}

			currentUser.GS_WorkingLanguage = "IT-IT";
			Factory.Save();
			using (Env.SetTemporaryUserContext(currentUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertEquals("Code in English", "One", germanCombined.ZZD_Description);
				AssertEquals("Code in Italian", "Ccy", italianCombined.ZZD_Description);
			}
		}

		public void TestGetCodeListAttributeNames()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC DESC", Core.Constants.CountryCodes.Eritrea);
			var codeType2 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExciseProductCodes, "EPC DESC", Core.Constants.CountryCodes.Ethiopia);
			var codeType3 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC DESC", Core.Constants.CountryCodes.Singapore);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "ER DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ethiopia, "ET DESC");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Singapore, "SG DESC");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ABC", "ABC DESC", codeType1.ZZK_CodeType, Core.Constants.CountryCodes.Eritrea, codeType1.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("123", "123 DESC", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType2.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("DEF", "DEF DESC", codeType3.ZZK_CodeType, Core.Constants.CountryCodes.Singapore, codeType1.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("456", "456 DESC", codeType2.ZZK_CodeType, Core.Constants.CountryCodes.Ethiopia, codeType2.ZZK_CodeType);
			Factory.Save();
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList1.ZZD_CodeType = codeType1.ZZK_CodeType;
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			var attributeNames1 = cusCodeList1.GetCodeListAttributeNames();
			AssertContainsExactElementsInAnyOrder(new[] { "ABC" }, attributeNames1.Select(x => x.ZXE_Name));
			var attributeNamesCache = cusCodeList1.GetCodeListAttributeNames();
			Assert("AttributeNames is Cached", attributeNames1 == attributeNamesCache);
			var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList2.ZZD_CodeType = codeType2.ZZK_CodeType;
			cusCodeList2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Ethiopia;
			var attributeNames2 = cusCodeList2.GetCodeListAttributeNames();
			AssertContainsExactElementsInAnyOrder(new[] { "123", "456" }, attributeNames2.Select(x => x.ZXE_Name));
		}

		public void TestCusCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var type1 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities", Core.Constants.CountryCodes.Eritrea);
			var type2 = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			type2.ZZK_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Ethiopia;
			Factory.Save();
			var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities;
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Ethiopia;
			AssertEquals(type2.PK, cusCodeList1.CusCodeType.PK);
			cusCodeList1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Eritrea;
			AssertEquals(type1.PK, cusCodeList1.CusCodeType.PK);
			cusCodeList1.ZZD_CodeType = "XX";
			AssertNull(cusCodeList1.CusCodeType);
		}

		public void TestLoadTop1ByCountryAndCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType("FAC", "FAC DESC", Core.Constants.CountryCodes.France);
			helper.CreateNewOrGetExistingCusCodeType("FAC", "FAC DESC", Core.Constants.CountryCodes.EuropeanUnion);
			var code1 = Factory.New<ZZRefCusCodeListCombined>();
			code1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			code1.ZZD_CodeType = "FAC";
			code1.ZZD_Code = "CODE1";
			code1.ZZD_StartDate = new ZDateTime(2023, 01, 01);
			code1.ZZD_EndDate = new ZDateTime(2023, 12, 31);
			var code2 = Factory.New<ZZRefCusCodeListCombined>();
			code2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.France;
			code2.ZZD_CodeType = "FAC";
			code2.ZZD_Code = "CODE2";
			code2.ZZD_StartDate = new ZDateTime(2023, 07, 01);
			code2.ZZD_EndDate = new ZDateTime(2024, 12, 31);
			var code3 = Factory.New<ZZRefCusCodeListCombined>();
			code3.ZZD_CountryOrGrouping = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			code3.ZZD_CodeType = "FAC";
			code3.ZZD_Code = "CODE3";
			code3.ZZD_StartDate = new ZDateTime(2025, 01, 01);
			code3.ZZD_EndDate = new ZDateTime(2025, 12, 31);
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertSame("Find by date1", code1, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2023, 04, 20)));
				AssertSame("Multiple matching codes", code1, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2023, 07, 01)));
				AssertSame("Find by date2", code2, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2024, 07, 01)));
				AssertSame("Find by includeParentDataGrouping=True", code3, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2025, 02, 10)));
				AssertEquals("No match by includeParentDataGrouping=False", null, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2025, 02, 10), false));
				AssertEquals("No match by CodeType", null, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "LOC", new ZDateTime(2023, 04, 20)));
				AssertEquals("No match by Country", null, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.Australia, "FAC", new ZDateTime(2023, 04, 20)));
				AssertEquals("No match by date", null, ZZRefCusCodeListCombined.Loader.LoadTop1ByCountryAndCodeType(Factory, Core.Constants.CountryCodes.France, "FAC", new ZDateTime(2022, 01, 01)));
			});
		}

		public void TestLoadOnLastDay()
		{
			var code1 = Factory.New<ZZRefCusCodeListCombined>();
			code1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			code1.ZZD_CodeType = "TYPEA";
			code1.ZZD_Code = "CODE1";
			var codes = ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Australia, "TYPEA", ZDateTime.MaxSmallDateTime);
			AssertEquals(codes.Length, 1);
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
			return cusCodeList;
		}

		(RefCusCodeListAttributeName AttributeName1, RefCusCodeListAttributeName AttributeName2) SetupAttributesWithDataTypes(string attribute1DataType, string attribute2DataType, ZString attribute1Value, ZString attribute2Value)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("TP1", "Ref Code Type 1");
			var attributeName1 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT1", "Attribute Name 1", "TP1", Core.Constants.CountryCodes.China);
			attributeName1.ZXE_ColumnCaption = "Attribute 1";
			attributeName1.ZXE_ValueDataType = attribute1DataType;
			var attributeName2 = helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ATT2", "Attribute Name 2", "TP1", Core.Constants.CountryCodes.China);
			attributeName2.ZXE_ColumnCaption = "Attribute 2";
			attributeName2.ZXE_ValueDataType = attribute2DataType;
			var codeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "TP1", "CD1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			codeList.Attributes.AddNew("ATT1", attribute1Value);
			codeList.Attributes.AddNew("ATT2", attribute2Value);
			Factory.Save();
			return (attributeName1, attributeName2);
		}
	}
}
