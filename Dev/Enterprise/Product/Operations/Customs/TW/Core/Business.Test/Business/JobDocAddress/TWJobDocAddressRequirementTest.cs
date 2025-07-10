using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWJobDocAddressRequirement))]
	sealed class TWJobDocAddressRequirementTest : TestCaseWithFactory
	{
		public void TestCheckE2_CompanyName()
		{
			docAddress.Validation.ValidateE2_CompanyName();
			AssertNoErrors(docAddress.E2_CompanyNameInfo);
		}

		public void TestCheckE2_Address1()
		{
			docAddress.Validation.ValidateE2_Address1();
			AssertNoErrors(docAddress.E2_Address1Info);
		}

		public void TestCheckE2_City()
		{
			docAddress.Validation.ValidateE2_City();
			AssertNoErrors(docAddress.E2_CityInfo);
		}

		public void TestCheckE2_Postcode()
		{
			docAddress.Validation.ValidateE2_Postcode();
			AssertNoErrors(docAddress.E2_PostcodeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			docAddress = Factory.New<TWJobDocAddress>();
			docAddress.OverrideRequirement = new TWJobDocAddressRequirement(DocAddressType.LocalClient);
			docAddress.E2_AddressOverride = true;
		}

		TWJobDocAddress docAddress;
	}
}
