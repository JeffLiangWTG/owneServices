using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(CusEntryInstructionLookups))]
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestBondedWarehouseTypeList()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var instruction = testDeclaration.CusEntryInstruction;
			var list = instruction.Lookups.BondedWarehouseTypeList;
			NUnit.Framework.Assert.That(list, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<BondedWarehouseTypeList>()));
			NUnit.Framework.Assert.That(list.CodesAsString, NUnit.Framework.Is.EqualTo("CPW, CCP"));
		}

		[ExpectNoExceptions]
		public void TestOrganisationsFindBoxCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			var organisationsFindBoxCollection = instruction.Lookups.OrganisationsFindBoxCollection;
			var filter = organisationsFindBoxCollection.FilterBusinessObjectDefaults["Registration Country/Type:Property1"];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(filter.Value, NUnit.Framework.Is.EqualTo(Core.Constants.CountryCodes.Taiwan).Using(CustomComparers.TypeComparison), "Registration Country/Type:Property1:1");
				filter = organisationsFindBoxCollection.FilterBusinessObjectDefaults["Registration Country/Type:Property2"];
				NUnit.Framework.Assert.That(filter.Value, NUnit.Framework.Is.EqualTo(OrgCusCode.CodeTypes.WarehouseControlledPremisesID).Using(CustomComparers.TypeComparison), "Registration Country/Type:Property2:1");
			});
		}

		[ExpectNoExceptions]
		public void TestStyleList()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			CombineAssertions("Export Style List", () =>
			{
				declaration.JE_MessageType = "EXP";
				var exportStyleList = instruction.Lookups.StyleList;
				NUnit.Framework.Assert.That(exportStyleList.CodesAsString, NUnit.Framework.Is.EqualTo("B1, B2, B8, B9, D1, D5, F4, F5, G3, G5"));
			}

			);
			CombineAssertions("Import Style List", () =>
			{
				declaration.JE_MessageType = "IMP";
				var importStyleList = instruction.Lookups.StyleList;
				NUnit.Framework.Assert.That(importStyleList.CodesAsString, NUnit.Framework.Is.EqualTo("B6, D2, D7, D8, F1, F2, F3, G1, G2, G7, L1"));
			}

			);
			CombineAssertions("MessageType Empty Style List", () =>
			{
				declaration.JE_MessageType = "";
				var messageTypeEmptyStyleList = instruction.Lookups.StyleList;
				NUnit.Framework.Assert.That(messageTypeEmptyStyleList.CodesAsString, NUnit.Framework.Is.Null.Or.Empty);
			}

			);
		}

		[ExpectNoExceptions]
		public void TestExaminationZoneList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "CommodityInspection");
			var placeAA02 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA02", "Place AA 02", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA02.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			var placeAB01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AB01", "Place AB 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAB01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AB");
			var placeAA01 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityInspection, "AA01", "Place AA 01", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(placeAA01.PK, RefCusCodeListAttributeTypes.Codes.CustomsOffice, "AA");
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var instruction = declaration.CusEntryInstruction;
			instruction.CEI_CustomsOffice = "AA";
			var locListAA = instruction.Lookups.ExaminationZoneList;
			NUnit.Framework.Assert.That(locListAA.CodesAsString, NUnit.Framework.Is.EqualTo(@"AA01, AA02"));
			NUnit.Framework.Assert.That(instruction.Lookups.ExaminationZoneList, NUnit.Framework.Is.SameAs(locListAA));
			instruction.CEI_CustomsOffice = "AB";
			var locListAB = instruction.Lookups.ExaminationZoneList;
			NUnit.Framework.Assert.That(locListAB.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(locListAB.ContainsCode("AB01"), NUnit.Framework.Is.True, "AB01");
			NUnit.Framework.Assert.That(instruction.Lookups.ExaminationZoneList, NUnit.Framework.Is.SameAs(locListAB));
			instruction.CEI_CustomsOffice = "ZZ";
			var locListAC = instruction.Lookups.ExaminationZoneList;
			NUnit.Framework.Assert.That(locListAC.Count, NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeList()
		{
			new TestTWCreator(Factory).CreateCustomsOffice();
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var officeCode = cusEntryInstruction.Lookups.CustomsOfficeList;
			NUnit.Framework.Assert.That(officeCode, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.ZArchitecture.Core.CodeDescriptionPairList)));
			NUnit.Framework.Assert.That(officeCode.GetDescriptionFromCode("BA"), NUnit.Framework.Is.EqualTo("Taipei office"));
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoodsCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var codeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var officeCodeList = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BA", "Taipei office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var facilityCodeType = helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "Facilities");
			var facility = helper.CreateCusCodeList(Core.Constants.CountryCodes.Taiwan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "ANP0060D", "XXXXXX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, Core.Constants.CountryCodes.Taiwan);
			facility.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.CustomsOffice, "BA");
			Factory.Save();
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			cusEntryInstruction.CEI_CustomsOffice = "BA";
			cusEntryInstruction.CEI_GoodsLocation = "ANP0060D";
			var collection = (BusinessObjectCollection)cusEntryInstruction.Lookups.GoodsLocationCollection;
			collection.Reload(true);
			NUnit.Framework.Assert.That(cusEntryInstruction.Lookups.GoodsLocationCollection.Count > 0, NUnit.Framework.Is.EqualTo(true));
			var filters = collection.FilterBusinessObjectDefaults;
			var listTypeFilter = filters["List Type:Property"];
			var attributeNameFilter = filters["Attribute Name:Property"];
			var attributeValueFilter = filters["Attribute Value:Property"];
			NUnit.Framework.Assert.That(listTypeFilter.Value, NUnit.Framework.Is.EqualTo("FAC").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(attributeNameFilter.Value, NUnit.Framework.Is.EqualTo("CUSTOMSOFFICE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(attributeValueFilter.Value, NUnit.Framework.Is.EqualTo("BA").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestReasonforDutyList()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var list = cusEntryInstruction.Lookups.ReasonforDutyList;
			var list1 = Factory.GetCachedValue<ReasonforDutyList>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(list1.Count));
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.EqualTo(list1).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(14));
			});
		}

		[ExpectNoExceptions]
		public void TestExamModeList()
		{
			var cusEntryInstruction = Factory.New<CusEntryInstruction>();
			var list = cusEntryInstruction.Lookups.ExamModeList;
			var list2 = Factory.GetCachedValue<ExamModeList>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(list2.Count));
				NUnit.Framework.Assert.That(list, NUnit.Framework.Is.EqualTo(list2).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(list.Count, NUnit.Framework.Is.EqualTo(8));
			});
		}

		[ExpectNoExceptions]
		public void TestBoxNumberList()
		{
			new TestTWCreator(Factory).CreateRegistryItemCusBrokerageBoxNumber();
			var entryInstruction = Factory.New<CusEntryInstruction>();
			var lookups = entryInstruction.Lookups;
			NUnit.Framework.Assert.That(lookups.BoxNumberList.Count, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestRORPaymentMethodList()
		{
			var lookups = Factory.New<CusEntryInstruction>().Lookups;
			NUnit.Framework.Assert.That(lookups.RORPaymentMethodList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<RORPaymentMethodList>()));
		}
	}
}
