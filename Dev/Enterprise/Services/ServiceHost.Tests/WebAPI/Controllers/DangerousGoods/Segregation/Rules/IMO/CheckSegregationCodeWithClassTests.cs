using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.ServiceHost.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO;

using static Enterprise.MasterFiles.Business.UNDGSubstanceLookups;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.DangerousGoods.Segregation.Rules.IMO
{
	class IMOCheckSegregationCodeWithClassTests : TestCaseWithFactory
	{
		bool expectMessage => true;
		public void TestCheck_ApplicableStandardIsIMO()
		{
			var rule = new CheckSegregationCodeWithClass();
			AssertEquals(UNDGSubstanceStandardTypes.IMO, rule.ApplicableStandard);
		}
		public void TestCheck_WhenSegregationCodeSG7IsIncompatibleWithClass3()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG7", "3", "SG7 : Stow \"away from\" class 3.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG15IsIncompatibleWithClass3()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG15", "3", "SG15 : Stow \"separated from\" class 3.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG14Fail()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG14", "1.6", "SG14 : Stow \"separated from\" class 1 except for division 1.4S.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG14Pass()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG14", "1.4S", "SG14 : Stow \"separated from\" class 1 except for division 1.4S.", !expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG25()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG25", "2.1", "SG25 : Stow \"separated from\" goods of classes 2.1 and 3.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG26()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG26", "2.1", "SG26 : In addition: from goods of classes 2.1 and 3 when stowed on deck of a container ship a minimum distance of two container spaces athwart ship shall be maintained, when stowed on Ro-Ro ships a distance of 6 m athwart ship shall be maintained.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG48()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG48", "3", "SG48 : Stow \"separated from\" combustible material (particularly liquids).", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG48Warning()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG48", "5.1", "SG48 : Stow \"separated from\" combustible material (particularly liquids).", !expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG53()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG53", "1.2", "SG53 : Shall not be stowed together with combustible material in the same cargo transport unit.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG63()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG63", "1.4S", "SG63 : Stow \"separated longitudinally by an intervening complete compartment or hold\" from class 1.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG65()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG65", "1.6", "SG65 : Stow \"separated by a complete compartment or hold\" from class 1 except for division 1.4.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG67()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG67", "1.4", "SG67 : Stow \"separated from division 1.4 and \"separated longitudinally by an intervening complete compartment or hold from\" divisions 1.1, 1.2 and 1.3.", expectMessage);
		}
		public void TestCheck_WhenSegregationCodeSG78()
		{
			Check_WhenSegregationCodeIsCompatibleWithClassTakenPair("SG78", "1.2", "SG78 : Stow \"separated longitudinally by an intervening complete compartment or hold\" from division 1.1, 1.2 and 1.5.", expectMessage);
		}

		void Check_WhenSegregationCodeIsCompatibleWithClassTakenPair(ZString key, ZString value, ZString message, ZBool expectMessage)
		{
			var segregationCode = key;
			var classCode = value;
			var commonData = Factory.New<UNDGCommonData>();
			commonData.DC_Type = UNDGCommonDataLookups.TypeConstants.StowageSegmentationRequirements;
			commonData.DC_Index = "AAAAAA";
			commonData.DC_Descriptor = message;

			DGSubstanceTestHelper.Create("123", "a", "IMO");
			var substance1 = UNDGSubstanceLoader.LoadSubstances(Factory, "123", "a", "IMO").First();
			DGSubstanceTestHelper.Create("456", "a", "IMO");
			var substance2 = UNDGSubstanceLoader.LoadSubstances(Factory, "456", "a", "IMO").First();

			substance2.DG_Class = classCode;

			var attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "AAAAAA";
			attribute.DA_DG = substance1.PK;
			AssertEquals("1 attribute", segregationCode, substance1.SegregationCodesForBinding);

			attribute = Factory.New<ViewUNDGAttribute>();
			attribute.DA_Type = ViewUNDGAttributeLookups.TypeConstants.StowageSegmentation_DangerousGoods;
			attribute.DA_Index = "BBBBBB";
			attribute.DA_DG = substance2.PK;

			var rule = new CheckSegregationCodeWithClass();

			var messages = rule.Check(substance1, substance2);

			if (expectMessage)
			{
				AssertEquals(1, messages.Count());
				AssertEquals(message, messages.First().Text);
			}
		}
	}
}
