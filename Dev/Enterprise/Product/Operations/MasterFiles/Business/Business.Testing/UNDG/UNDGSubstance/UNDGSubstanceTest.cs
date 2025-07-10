using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGSubstance))]
	sealed class UNDGSubstanceTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<UNDGSubstance>();
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			var obj = (UNDGSubstance)GetNewBusinessObjectForDeleteTest(Factory);
			obj.DG_UNNO = "ZZZZ";
			obj.DG_Variant = "A";

			Factory.Save();
			var objExistOnDb = UNDGSubstanceLoader.LoadSubstances(Factory, "ZZZZ", "A", "IMO").FirstOrDefault();
			AssertNotNull(objExistOnDb);
			objExistOnDb.Delete();

			var objNotOnDb = UNDGSubstanceLoader.LoadSubstances(Factory, "ZZZZ", "A", "IMO").FirstOrDefault();
			AssertNull(objNotOnDb);
		}

		public void TestValidatesWhenFilteredOut()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DetailsLanguage = Core.SharedConstants.Languages.EnglishAmerican);
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			Assert("precondition", !substance.HasErrors);

			substance.Properties.AddNew();

			//filters out attribute, so validation is not running on it normally
			substance.DetailsLanguage = Core.SharedConstants.Languages.Russian;
			substance.RunPreSaveValidation();
			Assert(substance.HasErrors);
		}

		public void TestTableNameIsViewName()
		{
			var subs = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First();
			AssertEquals(nameof(UNDGSubstance), subs.TableName);
		}

		public void TestDefaultValues()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.Gujarati;

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			AssertEquals(false, substance.DG_IsSystem);
			AssertEquals(ZString.Empty, substance.DetailsLanguage);
		}

		public void TestOnLoaded()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			DGSubstanceTestHelper.Create("123", "b", "IMO");

			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.Czech;

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			AssertEquals(ZString.Empty, substance.DetailsLanguage);

			GlbStaff.CurrentUser.GS_WorkingLanguage = ZString.Empty;
			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First();

			AssertEquals(ZString.Empty, substance.DetailsLanguage);

			substance.DetailsLanguage = ZString.Empty;
			substance.OnLoaded();
			AssertEquals(ZString.Empty, substance.DetailsLanguage);
		}

		public void TestLQSpecProvData()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			DGSubstanceTestHelper.Create("123", "b", "IMO", additionalInitialisation: subs => subs.DG_LQSpecProvIndex = "251");
			DGSubstanceTestHelper.Create("123", "c", "IMO", additionalInitialisation: subs => subs.DG_LQSpecProvIndex = "9999");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("", substance.LQSpecProvData);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First();
			AssertNotEquals("", substance.LQSpecProvData);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "c", "IMO").First();
			AssertEquals("", substance.LQSpecProvData);
		}

		public void TestDG_CodeCalculated()
		{
			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			AssertEquals("", substance.DG_Code);

			substance.DG_UNNO = "1234";
			AssertEquals("1234", substance.DG_Code);

			substance.DG_Variant = "X";
			AssertEquals("1234X", substance.DG_Code);
		}

		public void TestLowestFlashPointTemperature()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_FlashPoint = "-18 to 0 cc");
			DGSubstanceTestHelper.Create("123", "b", "IMO", additionalInitialisation: subs => subs.DG_FlashPoint = "0 cc");
			DGSubstanceTestHelper.Create("123", "c", "IMO", additionalInitialisation: subs => subs.DG_FlashPoint = "");
			DGSubstanceTestHelper.Create("123", "d", "IMO", additionalInitialisation: subs => subs.DG_FlashPoint = "below-18 cc");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("LowestFlashPointTemperature", "-18", substance.LowestFlashPointTemp);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First();
			AssertEquals("LowestFlashPointTemperature", "0", substance.LowestFlashPointTemp);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "c", "IMO").First();
			AssertEquals("LowestFlashPointTemperature", "", substance.LowestFlashPointTemp);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "d", "IMO").First();
			AssertEquals("LowestFlashPointTemperature", "-18", substance.LowestFlashPointTemp);
		}

		public void TestDG_CalcSegregateAwayFromClass()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "123";
			subs.DG_Variant = "a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			var item = Factory.NewWithValidTestData<UNDGDataItem>();
			item.LinkDefault(subs);

			string testString = "1.3; 1.6;";

			item.Substance.DG_EXVector = "10000000000000000";
			AssertEquals("Away from should be equal to the test string", testString, item.Substance.DG_AwayFrom);

			item.Substance.DG_EXVector = "20000000000000000";
			AssertEquals("Seperate from should be equal to the test string", testString, item.Substance.DG_SeperateFrom);

			item.Substance.DG_EXVector = "30000000000000000";
			AssertEquals("Seperate Compartmentally from should be equal to the test string", testString, item.Substance.DG_SeperateCompartmentFrom);

			item.Substance.DG_EXVector = "40000000000000000";
			AssertEquals("Seperate Longnitudinally from should be equal to the test string", testString, item.Substance.DG_SeperatedLongitudinallyFrom);

			item.Substance.DG_EXVector = "90000000000000000";
			AssertEquals("NotOnSameShipAs should be equal to the test string", testString, item.Substance.DG_NotOnSameShipAs);
		}

		public void TestDG_Class_List()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_Class = "1");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			AssertEquals("Class Description should be Class 1", "Class 1", substance.Lookups.DGClassList.GetDescriptionFromCode(substance.DG_Class));
		}

		public void TestDG_State_List()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_State = "S");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			AssertEquals("State Description should be Solid", UNDGSubstanceLookups.StateTypes.Description.Solid, substance.Lookups.StateList.GetDescriptionFromCode(substance.DG_State));
		}

		public void TestDG_MP_List()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_MP = "S");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			AssertEquals("Marine Pollutant Description should be Severe Marine Pollutant", UNDGSubstanceLookups.MarinePollutantTypes.SevereMarinePollutant, substance.Lookups.MarinePollutantList.GetDescriptionFromCode(substance.DG_MP));
		}

		public void TestDG_UlineEMSCheckBox()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_UlineEMS = "0");
			DGSubstanceTestHelper.Create("123", "b", "IMO", additionalInitialisation: subs => subs.DG_UlineEMS = "1");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("UlineEMSCheckBox should be false", ZBool.False, substance.DG_ULineEmsCheckBox);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First();
			AssertEquals("UlineEMSCheckBox should be true", ZBool.True, substance.DG_ULineEmsCheckBox);
		}

		public void TestDG_Calc_TechnicalNameRequirement()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_TechName = "");
			DGSubstanceTestHelper.Create("123", "b", "IMO", additionalInitialisation: subs => subs.DG_TechName = "*");
			DGSubstanceTestHelper.Create("123", "c", "IMO", additionalInitialisation: subs => subs.DG_TechName = "+");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("TechNameCheckBox should be false", TechNameTypes.Code.NotRequired, substance.DG_Calc_TechnicalNameRequirement);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "b", "IMO").First();
			AssertEquals("TechNameCheckBox should be true", TechNameTypes.Code.Required, substance.DG_Calc_TechnicalNameRequirement);

			substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "c", "IMO").First();
			AssertEquals("TechNameCheckBox should be true", TechNameTypes.Code.Other, substance.DG_Calc_TechnicalNameRequirement);

			DGSubstanceTestHelper.Create("222", "a", "IMO", additionalInitialisation: subs => subs.DG_Calc_TechnicalNameRequirement = TechNameTypes.Code.NotRequired);
			DGSubstanceTestHelper.Create("222", "b", "IMO", additionalInitialisation: subs => subs.DG_Calc_TechnicalNameRequirement = TechNameTypes.Code.Required);
			DGSubstanceTestHelper.Create("222", "c", "IMO", additionalInitialisation: subs => subs.DG_Calc_TechnicalNameRequirement = TechNameTypes.Code.Other);

			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "222", "a", "IMO").First();
			AssertEquals("TechNameCheckBox should be false", "", substance1.DG_TechName);

			substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "222", "b", "IMO").First();
			AssertEquals("TechNameCheckBox should be true", "*", substance1.DG_TechName);

			substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "222", "c", "IMO").First();
			AssertEquals("TechNameCheckBox should be true", "+", substance1.DG_TechName);
		}

		public void TestHumanReadableNameCore()
		{
			DGSubstanceTestHelper.Create("AB", "C", "IMO", additionalInitialisation: subs => subs.DG_PSN = "123");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "AB", "C", "IMO").First();
			AssertEquals("Dangerous Goods Substance - ABC - 123", substance.HumanReadableName);
		}

		public void TestReadOnly()
		{
			UNDGSubstance substance = Factory.LoadTop1<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.DG_IsSystem, ZBool.True));
			AssertEquals(true, substance.DG_IsSystem);
			AssertEquals(true, substance.DG_PSNInfo.ReadOnly);
			AssertEquals(false, substance.DetailsLanguageInfo.ReadOnly);
			AssertEquals(false, substance.DG_UsrUSDOTShippingNameInfo.ReadOnly);
			AssertEquals(false, substance.DG_IsActiveInfo.ReadOnly);
		}

		public void TestGetFieldColumnType()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");

			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();

			substance.GetFieldColumnType(UNDGSubstanceSchema.DG_UNNO, false, true);
			AssertEquals(nameof(FieldType.Text), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_UNNO, false, false));
			AssertEquals(nameof(FieldType.Text), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_UNNO, false, true));
			AssertEquals(nameof(FieldType.TextCodeFindBox), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_UNNO, true, false));
			AssertEquals(nameof(FieldType.TextDropEdit), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_UNNO, true, true));

			AssertEquals(nameof(FieldType.Guid), substance.GetFieldColumnType(UNDGSubstanceSchema.PK, false, false));
			AssertEquals(nameof(FieldType.Decimal), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_LQMaxAmt, false, false));
			AssertEquals(nameof(FieldType.Boolean), substance.GetFieldColumnType(UNDGSubstanceSchema.DG_IsActive, false, false));
		}

		public void TestCaoPermissibleQuantities()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI965);
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			var expectedPermissibleQuantities = new List<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity>()
			{
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIA, 0, Core.Constants.Weight.Kilograms, 35, Core.Constants.Weight.Kilograms),
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity(LithiumBatteryConstants.RefPackingInstructions.PI965, PackingInstructionSectionTypeList.Codes.SectionIB, 0, Core.Constants.Weight.Kilograms, 10, Core.Constants.Weight.Kilograms)
			};
			AssertContainsExactElementsInExactOrder(expectedPermissibleQuantities, substance1.CaoPermissibleQuantities);
			AssertEquals(PackingInstructionSectionTypeList.Codes.SectionIA, substance1.CaoPackInsSec1);
			AssertEquals(PackingInstructionSectionTypeList.Codes.SectionIB, substance1.CaoPackInsSec2);
			AssertEquals((ZDecimal)35, substance1.CaoMaxAmt1);
			AssertEquals((ZDecimal)10, substance1.CaoMaxAmt2);
			AssertEquals(Core.Constants.Weight.Kilograms, substance1.CaoMaxAmtUQ1);
			AssertEquals(Core.Constants.Weight.Kilograms, substance1.CaoMaxAmtUQ2);

			DGSubstanceTestHelper.Create("456", "a", "IMO", additionalInitialisation: subs => subs.DG_CargoPackIns = "111");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();
			AssertEquals(0, substance2.CaoPermissibleQuantities.Count());
			AssertEquals(ZString.Empty, substance2.CaoPackInsSec1);
			AssertEquals(ZString.Empty, substance2.CaoPackInsSec2);
			AssertEquals((ZDecimal)0, substance2.CaoMaxAmt1);
			AssertEquals((ZDecimal)0, substance2.CaoMaxAmt2);
			AssertEquals(ZString.Empty, substance2.CaoMaxAmtUQ1);
			AssertEquals(ZString.Empty, substance2.CaoMaxAmtUQ2);

			DGSubstanceTestHelper.Create("789", "a", "IMO", additionalInitialisation: subs => subs.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.Forbidden);
			var substance3 = UNDGSubstanceLoader.LoadSubstances(Factory, "789", "a", "IMO").First();
			AssertEquals(0, substance3.CaoPermissibleQuantities.Count());
			AssertEquals(ZString.Empty, substance3.CaoPackInsSec1);
			AssertEquals(ZString.Empty, substance3.CaoPackInsSec2);
			AssertEquals((ZDecimal)0, substance3.CaoMaxAmt1);
			AssertEquals((ZDecimal)0, substance3.CaoMaxAmt2);
			AssertEquals(ZString.Empty, substance3.CaoMaxAmtUQ1);
			AssertEquals(ZString.Empty, substance3.CaoMaxAmtUQ2);
		}

		public void TestPaxPermissibleQuantities()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO", additionalInitialisation: subs => subs.DG_PaxPackIns = "966");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			var expectedPermissibleQuantities = new List<UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity>()
			{
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity("966", "I", 5, "KG", 35, "KG"),
				new UNDGPermissableQuantitiesHelper.UNDGPermissibleQuantity("966", "II", 5, "KG", 5, "KG")
			};
			AssertContainsExactElementsInExactOrder(expectedPermissibleQuantities, substance1.PaxPermissibleQuantities);
			AssertEquals(PackingInstructionSectionTypeList.Codes.SectionI, substance1.PaxPackInsSec1);
			AssertEquals(PackingInstructionSectionTypeList.Codes.SectionII, substance1.PaxPackInsSec2);
			AssertEquals((ZDecimal)5, substance1.PaxMaxAmt1);
			AssertEquals((ZDecimal)5, substance1.PaxMaxAmt2);
			AssertEquals(Core.Constants.Weight.Kilograms, substance1.PaxMaxAmtUQ1);
			AssertEquals(Core.Constants.Weight.Kilograms, substance1.PaxMaxAmtUQ2);

			DGSubstanceTestHelper.Create("456", "a", "IMO", additionalInitialisation: subs => subs.DG_CargoPackIns = "111");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();
			AssertEquals(0, substance2.PaxPermissibleQuantities.Count());
			AssertEquals(ZString.Empty, substance2.PaxPackInsSec1);
			AssertEquals(ZString.Empty, substance2.PaxPackInsSec2);
			AssertEquals((ZDecimal)0, substance2.PaxMaxAmt1);
			AssertEquals((ZDecimal)0, substance2.PaxMaxAmt2);
			AssertEquals(ZString.Empty, substance2.PaxMaxAmtUQ1);
			AssertEquals(ZString.Empty, substance2.PaxMaxAmtUQ2);

			DGSubstanceTestHelper.Create("789", "a", "IMO", additionalInitialisation: subs => subs.DG_CargoPackIns = "Forbidden");
			var substance3 = UNDGSubstanceLoader.LoadSubstances(Factory, "789", "a", "IMO").First();
			AssertEquals(0, substance3.PaxPermissibleQuantities.Count());
			AssertEquals(ZString.Empty, substance3.PaxPackInsSec1);
			AssertEquals(ZString.Empty, substance3.PaxPackInsSec2);
			AssertEquals((ZDecimal)0, substance3.PaxMaxAmt1);
			AssertEquals((ZDecimal)0, substance3.PaxMaxAmt2);
			AssertEquals(ZString.Empty, substance3.PaxMaxAmtUQ1);
			AssertEquals(ZString.Empty, substance3.PaxMaxAmtUQ2);
		}

		public void TestSegregationGroups()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("No attribute", 0, substance.SegregationGroups.Length);

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG";
			attribute.DA_DG = substance.PK;
			AssertEquals("1 attribute", 1, substance.SegregationGroups.Length);
			AssertEquals("1 attribute", new ZString("SGG"), substance.SegregationGroups[0]);

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG1";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG19";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG2";
			attribute.DA_DG = substance.PK;

			AssertEquals("4 attributes", 4, substance.SegregationGroups.Length);
			AssertEquals("Not Equal", new ZString("SGG"), substance.SegregationGroups[0]);
			AssertEquals("Not Equal", new ZString("SGG1"), substance.SegregationGroups[1]);
			AssertEquals("Not Equal", new ZString("SGG2"), substance.SegregationGroups[2]);
			AssertEquals("Not Equal", new ZString("SGG19"), substance.SegregationGroups[3]);
		}

		public void TestSegregationGroupsForBinding()
		{
			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("No attribute", ZString.Empty, substance.SegregationGroupsForBinding);

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG";
			attribute.DA_DG = substance.PK;
			AssertEquals("1 attribute", "SGG", substance.SegregationGroupsForBinding);

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG1";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG19";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.SegregationGroups;
			attribute.DA_Index = "SGG2";
			attribute.DA_DG = substance.PK;

			AssertEquals("4 attributes", "SGG, SGG1, SGG2, SGG19", substance.SegregationGroupsForBinding);
		}

		public void TestSegregationCodes()
		{
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "AAAAAA";
			commonData.DC_Descriptor = "SG1 Stow \"separated from\" class 7.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "BBBBBB";
			commonData.DC_Descriptor = "SG6c For WASTE AEROSOLS: segregation as for the appropriate sub-division of class 2.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "CCCCCC";
			commonData.DC_Descriptor = "SG6b For WASTE AEROSOLS: segregation as for the appropriate sub-division of class 2.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "DDDDDD";
			commonData.DC_Descriptor = "SG19 Stow \"separated from\" class 7.";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("No attribute", 0, substance.SegregationCodes.Length);

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "AAAAAA";
			attribute.DA_DG = substance.PK;
			AssertEquals("1 attribute", 1, substance.SegregationCodes.Length);
			AssertEquals("1 attribute", new ZString("SG1"), substance.SegregationCodes[0]);

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "BBBBBB";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "CCCCCC";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "DDDDDD";
			attribute.DA_DG = substance.PK;

			AssertEquals(4, substance.SegregationCodes.Length);
			AssertEquals(new ZString("SG1"), substance.SegregationCodes[0]);
			AssertEquals(new ZString("SG6b"), substance.SegregationCodes[1]);
			AssertEquals(new ZString("SG6c"), substance.SegregationCodes[2]);
			AssertEquals(new ZString("SG19"), substance.SegregationCodes[3]);
		}

		public void TestSegregationCodesForBinding()
		{
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "AAAAAA";
			commonData.DC_Descriptor = "SG1 Stow \"separated from\" class 7.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "BBBBBB";
			commonData.DC_Descriptor = "SG6c For WASTE AEROSOLS: segregation as for the appropriate sub-division of class 2.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "CCCCCC";
			commonData.DC_Descriptor = "SG6b For WASTE AEROSOLS: segregation as for the appropriate sub-division of class 2.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "DDDDDD";
			commonData.DC_Descriptor = "SG19 Stow \"separated from\" class 7.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "EEEEEE";
			commonData.DC_Descriptor = "SG Stow \"separated from\" class 7.";

			commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "FFFFFF";
			commonData.DC_Descriptor = "SG6 Stow \"separated from\" class 7.";

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			AssertEquals("No attribute", ZString.Empty, substance.SegregationCodesForBinding);

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "AAAAAA";
			attribute.DA_DG = substance.PK;
			AssertEquals("1 attribute", "SG1", substance.SegregationCodesForBinding);

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "BBBBBB";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "CCCCCC";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "DDDDDD";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "EEEEEE";
			attribute.DA_DG = substance.PK;

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "FFFFFF";
			attribute.DA_DG = substance.PK;
			AssertEquals("6 attributes", "SG, SG1, SG6, SG6b, SG6c, SG19", substance.SegregationCodesForBinding);
		}

		public void TestStowageSegmentationDangerous_AddRemove()
		{
			var data1 = Factory.New<UNDGCommonData>();
			data1.DC_Index = "666";
			data1.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			data1.DC_Descriptor = "XXX";
			data1.DC_Language = Core.SharedConstants.Languages.Gujarati;

			var data2 = Factory.New<UNDGCommonData>();
			data2.DC_Index = "SGG1";
			data2.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			data2.DC_Descriptor = "YYY";
			data2.DC_Language = Core.SharedConstants.Languages.Gujarati;

			Factory.Save();

			var substance = Factory.New<UNDGSubstance>();
			substance.DetailsLanguage = ZString.Empty;
			CombineAssertions("Pre-condition", () =>
			{
				AssertEquals("substance.StowageSegmentationDangerous.Count", 0, substance.StowageSegmentationDangerous.Count);
				AssertEquals("substance.StowageSegmentationDangerousAttributes.Count", 0, substance.StowageSegmentationDangerousAttributes.Count);
				AssertEquals("substance.SegregationGroupAttributes.Count", 0, substance.SegregationGroupAttributes.Count);
			});

			substance.StowageSegmentationDangerous.Add(data1);
			CombineAssertions($"Add common data1", () =>
			{
				AssertEquals("substance.StowageSegmentationDangerous.Count", 1, substance.StowageSegmentationDangerous.Count);
				AssertEquals("substance.StowageSegmentationDangerousAttributes.Count", 1, substance.StowageSegmentationDangerousAttributes.Count);
				AssertEquals("substance.SegregationGroupAttributes.Count", 0, substance.SegregationGroupAttributes.Count);

				var attribute = substance.StowageSegmentationDangerousAttributes[0];
				AssertEquals("attribute.DA_Index", "666", attribute.DA_Index);
				AssertEquals("attribute.DA_Language", ZString.Empty, attribute.DA_Language);
				AssertEquals("attribute.DA_Descriptor", ZString.Empty, attribute.DA_Descriptor);
				AssertEquals("attribute.DA_Type", ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods, attribute.DA_Type);
				AssertEquals("attribute.DA_DG", substance.PK, attribute.DA_DG);
			});

			substance.StowageSegmentationDangerous.Add(data2);
			CombineAssertions($"Add common data2", () =>
			{
				AssertEquals("substance.StowageSegmentationDangerous.Count", 2, substance.StowageSegmentationDangerous.Count);
				AssertEquals("substance.StowageSegmentationDangerousAttributes.Count", 1, substance.StowageSegmentationDangerousAttributes.Count);
				AssertEquals("substance.SegregationGroupAttributes.Count", 1, substance.SegregationGroupAttributes.Count);

				var attribute = substance.SegregationGroupAttributes[0];
				AssertEquals("attribute.DA_Index", "SGG1", attribute.DA_Index);
				AssertEquals("attribute.DA_Language", ZString.Empty, attribute.DA_Language);
				AssertEquals("attribute.DA_Descriptor", ZString.Empty, attribute.DA_Descriptor);
				AssertEquals("attribute.DA_Type", ViewUNDGAttributeLookups.TypeConstants.SegregationGroups, attribute.DA_Type);
				AssertEquals("attribute.DA_DG", substance.PK, attribute.DA_DG);
			});

			substance.StowageSegmentationDangerous.Delete(data1);
			CombineAssertions($"Delete common data1", () =>
			{
				Assert(data1.IsDeleted);
				AssertEquals("substance.StowageSegmentationDangerous.Count", 1, substance.StowageSegmentationDangerous.Count);
				AssertEquals("substance.StowageSegmentationDangerousAttributes.Count", 0, substance.StowageSegmentationDangerousAttributes.Count);
				AssertEquals("substance.SegregationGroupAttributes.Count", 1, substance.SegregationGroupAttributes.Count);
			});

			substance.StowageSegmentationDangerous.Delete(data2);
			CombineAssertions($"Delete common data2", () =>
			{
				Assert(data2.IsDeleted);
				AssertEquals("substance.StowageSegmentationDangerous.Count", 0, substance.StowageSegmentationDangerous.Count);
				AssertEquals("substance.StowageSegmentationDangerousAttributes.Count", 0, substance.StowageSegmentationDangerousAttributes.Count);
				AssertEquals("substance.SegregationGroupAttributes.Count", 0, substance.SegregationGroupAttributes.Count);
			});
		}

		public void TestUsrUSDOTShippingName()
		{
			var obj = (UNDGSubstance)GetNewBusinessObjectForDeleteTest(Factory);
			obj.DG_UNNO = "ZZZZ";
			obj.DG_Variant = "A";
			var newUsnAttribute = obj.UsrUSDOTShippingNames.AddNew();
			newUsnAttribute.DA_Descriptor = "TestShippingName";
			newUsnAttribute.DA_Language = Core.Constants.Languages.English;

			Factory.Save();
			var objExistOnDb = UNDGSubstanceLoader.LoadSubstances(Factory, "ZZZZ", "A", "IMO").FirstOrDefault();
			var usnAttribute = new ViewUNDGAttribute.Loader(Factory)
						.LoadFromSubstancePK(objExistOnDb.PK)
						.FirstOrDefault(attribute => attribute.DA_Type == ViewUNDGAttributeLookups.TypeConstants.UsrUSDOTShippingNames);

			AssertNotNull(usnAttribute);
			AssertEquals("TestShippingName", usnAttribute.DA_Descriptor);
			AssertEquals(Core.Constants.Languages.English, usnAttribute.DA_Language);
		}
	}
}
