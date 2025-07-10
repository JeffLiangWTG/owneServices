using CargoWise.Customs.US.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	public class HazmatInfoTypeProviderTest : TestCaseWithFactory
	{
		public void TestHazmatCode()
		{
			undg.DI_DG_NKSubs = "TEST";

			AssertEquals("TEST", provider.HazmatCode.Value);
		}

		public void TestHazmatClassCode()
		{
			undg.DI_IMOClass = "CLAS";

			AssertEquals("CLAS", provider.HazmatClassCode.Value);
		}

		public void TestHazmatDescription()
		{
			undg.Subs.DG_PSN = "DESC";

			AssertEquals("DESC", provider.HazmatDescription.Value);
		}

		public void TestHazmatContactName()
		{
			AssertEquals("BOB", provider.HazmatContactName.Value);
		}

		public void TestHazmatContactPhoneNumber()
		{
			AssertEquals("123", provider.HazmatContactPhoneNumber.Value);
		}

		public void TestHazmatFlashpointTemperature()
		{
			undg.DI_DGFlashPoint = 12m;

			AssertEquals("12", provider.HazmatFlashpointTemperature.Value);
		}

		public void TestTemperatureUnitOfMeasureCode()
		{
			AssertEquals("C", provider.TemperatureUnitOfMeasureCode.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "BOB";
			contact.OC_Phone = "123";

			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "EXP";
			subs.DG_Variant = "";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;

			undg = Factory.NewWithValidTestData<UNDGDataItem>();
			undg.LinkDefault(subs);
			undg.DI_OC_DGContact = contact.PK;

			provider = new HazmatInfoTypeProvider(undg);
		}
		IHazmatInfoType provider;
		UNDGDataItem undg;
	}
}
