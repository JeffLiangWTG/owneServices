using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method)]
	public sealed class SelfManagedTariffCountriesAttribute : TestSetupAttribute
	{
		public SelfManagedTariffCountriesAttribute(params string[] countryCodes)
		{
			CountryCodes = countryCodes;
		}

		public override void SetUp(TestCase testCase)
		{
			const string SMTAR = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SelfManagedCountry;
			const string ZZ = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(SMTAR, "'Countries that self manage Tariffs");
			foreach (var countryCode in CountryCodes)
			{
				helper.CreateNewOrGetExistingCusCodeList(ZZ, SMTAR, countryCode, RefCountry.LoadFromCountryCode(factory, countryCode).RN_Desc, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			factory.Save();
		}

		public override void TearDown(TestCase testCase)
		{
			// Tests run in a transaction, so the records above will be removed when the transaction ends.
		}

		public string[] CountryCodes { get; }
	}
}
