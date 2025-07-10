using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PermitCountrySpecificInstructionTest : SharedCusPermitCountrySpecificInstructionTest<PermitCountrySpecificInstruction>
	{
		public void TestIsQtyValIndicatorMandatory()
		{
			var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.SouthAfrica);
			AssertEquals(true, instruction.IsQtyValIndicatorMandatory);
		}

		public void TestGetByCountryCodeUseCustomsCountryOfJurisdiction()
		{
			var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.PuertoRico);
			AssertEquals("PR should load US", "Enterprise.Customs.US.Business.PermitCountrySpecificInstruction", instruction.GetType().FullName);
		}

		public void TestGetByCountryCodeUseCustomsCountryOfEuropeanUnion()
		{
			var euCountries = new[]
			{
				Core.Constants.CountryCodes.France,
				Core.Constants.CountryCodes.Germany,
				Core.Constants.CountryCodes.Italy,
				Core.Constants.CountryCodes.UnitedKingdom
			};

			foreach (var country in euCountries)
			{
				var instruction = PermitCountrySpecificInstruction.GetByCountryCode(Factory, country);
				AssertEquals("EU should load EU", "Enterprise.Customs.EU.Business.PermitCountrySpecificInstruction", instruction.GetType().FullName);
			}
		}

		[ExpectNoExceptions]
		public void TestGetByCountryCodeCustomsCountryOfChina()
		{
			_ = PermitCountrySpecificInstruction.GetByCountryCode(Factory, Core.Constants.CountryCodes.China);
		}

		public void TestIsRuleExceptionApplicable()
		{
			Assert("Default to Applicable", countrySpecificInstruction.IsRuleExceptionApplicable(""));
		}

		public void TestGetCustomLabelForPermitNumber()
		{
			AssertEquals(ZString.Empty, countrySpecificInstruction.GetCustomLabelForPermitNumber(""));
		}

		protected override PermitCountrySpecificInstruction GetNewCusPermitCountrySpecificInstruction(BusinessObjectFactory factory)
		{
			var permitHeader = Factory.NewWithValidTestData<BaseCusPermitHeader>();
			return permitHeader.CountrySpecificInstruction;
		}
	}
}
