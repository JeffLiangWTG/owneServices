using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class GuaranteeCountrySpecificInstructionTest : SharedCusPermitCountrySpecificInstructionTest<GuaranteeCountrySpecificInstruction>
	{
		public void TestGetByCountryCode_EuropeanUnion()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Latvia);
			AssertEquals("Enterprise.Customs.EU.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_FR()
		{
			foreach (var country in Core.Constants.CountryCodes.FranceAndOverseasDepartmentsUnderItsCustomsJurisdiction)
			{
				var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, country);
				AssertEquals($"GuaranteeCountrySpecificInstruction for {country}", "Enterprise.Customs.FR.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
			}
		}

		public void TestGetByCountryCode_DE()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Germany);
			AssertEquals("Enterprise.Customs.DE.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_GB()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("Enterprise.Customs.GB.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_PL()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Poland);
			AssertEquals("Enterprise.Customs.PL.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_NL()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Netherlands);
			AssertEquals("Enterprise.Customs.NL.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_ES()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Spain);
			AssertEquals("Enterprise.Customs.ES.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_IT()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Italy);
			AssertEquals("Enterprise.Customs.IT.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_BE()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.Belgium);
			AssertEquals("Enterprise.Customs.BE.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCode_NotAppointed()
		{
			var instruction = GuaranteeCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.China);
			AssertEquals("Enterprise.Customs.Business.GuaranteeCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public new void TestGetTypeList()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList();
			AssertType<CodeDescriptionPairList>(typeList);
		}

		public new void TestGetSubTypeList()
		{
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			var subTypeList = guaranteeHeader.CountrySpecificInstruction.GetSubTypeList("TRA");
			AssertEquals("No SubType list has been defined for base guarantees", 0, subTypeList.Count);
		}

		public void TestGetTypeList_CountryCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "Guarantee Types");
			helper.CreateNewOrGetExistingCusCodeList("ER", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GuaranteeType, "ZZZ", "Description for ZZZ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var guaranteeHeader = Factory.New<BaseCusGuaranteeHeader>();
			var typeList = guaranteeHeader.CountrySpecificInstruction.GetTypeList("ER");
			CombineAssertions(() =>
			{
				AssertEquals("Count", 1, typeList.Count);
				AssertEquals("ZZZ", typeList[0].Code);
				AssertEquals("Description for ZZZ", typeList[0].Description);
			});
		}

		public new void TestGetTransactionTypeList()
		{
			base.TestGetTransactionTypeList();

			var transactionTypeList = countrySpecificInstruction.GetTransactionTypeList("", "");
			AssertEquals("CodesAsString", "ADJ, CUS, OBL, TRA, OBA", transactionTypeList.CodesAsString);
		}

		protected override GuaranteeCountrySpecificInstruction GetNewCusPermitCountrySpecificInstruction(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();
			return guaranteeHeader.CountrySpecificInstruction;
		}
	}
}
