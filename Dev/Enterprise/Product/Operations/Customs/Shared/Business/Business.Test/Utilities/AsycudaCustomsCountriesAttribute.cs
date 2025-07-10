using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[System.AttributeUsage(System.AttributeTargets.Assembly | System.AttributeTargets.Class | System.AttributeTargets.Method)]
	public sealed class AsycudaCustomsCountriesAttribute : TestSetupAttribute
	{
		public AsycudaCustomsCountriesAttribute(params string[] countryCodes)
		{
			CountryCodes = countryCodes;
		}

		public override void SetUp(TestCase testCase)
		{
			const string ASY = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda;
			const string ZZ = Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping;
			var factory = new BusinessObjectFactory();
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(ASY, "Asycuda country");
			foreach (var countryCode in CountryCodes)
			{
				helper.CreateNewOrGetExistingCusCodeList(ZZ, ASY, countryCode, RefCountry.LoadFromCountryCode(factory, countryCode).RN_Desc, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			factory.Save();
		}

		public override void TearDown(TestCase testCase)
		{
			// Tests run in a transaction, so the records above will be removed when the transaction ends.
		}

		#region Implementation

		public string[] CountryCodes { get; }

		#endregion
	}
}
