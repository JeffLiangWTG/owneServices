using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAddress.Loader))]
	sealed class OrgAddressLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new OrgAddress.Loader(Factory);
		}

		public void TestGetWarehouseAddressBasedOnCusCode()
		{
			var address = Loader.GetWarehouseAddressBasedOnCusCode(null);
			AssertNull("No address found", address);

			var header = Factory.New<OrgHeader>();
			header.Addresses.MainAddress.LocalControlledPremisesID = "12345";
			AssertEquals("Address", header.Addresses.MainAddress, Loader.GetWarehouseAddressBasedOnCusCode("12345"));

			header.CustomsCodes.Load(new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.ControlledPremisesID));
			if (header.CustomsCodes.Count > 0)
			{
				header.CustomsCodes[0].OK_OA_PremisesAddress = ZGuid.Empty;
			}

			AssertEquals("Address", header.Addresses.MainAddress, Loader.GetWarehouseAddressBasedOnCusCode("12345"));
		}

		public void TestLoadAddressFromLocalCode()
		{
			var header = Factory.New<OrgHeader>();
			header.Addresses.MainAddress.LocalControlledPremisesID = "12345";
			AssertEquals("Address", header.Addresses.MainAddress, Loader.LoadAddressFromLocalCode(OrgCusCode.CodeTypes.ControlledPremisesID, "12345"));
		}

		public void TestLoadFromCustomsRegoNumber()
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.MainAddress.CustomsCodes.AddNew("AAA", "123456");

			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();
			organisation2.MainAddress.CustomsCodes.AddNew("AAA", "123457");

			Factory.Save();

			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			OrgAddress[] result = new OrgAddress.Loader(Factory).LoadDBAddresses(countryCode, "AAA", "123456");
			AssertEquals("One result", 1, result.Length);
			AssertEquals(organisation.MainAddress, result[0]);

			result = new OrgAddress.Loader(Factory).LoadDBAddresses(countryCode, "AAA", "123457");
			AssertEquals("One result", 1, result.Length);
			AssertEquals(organisation2.MainAddress, result[0]);
		}

		const string PremiseID = "9914N";

		public void TestFromLocalPremiseID()
		{
			var header = Factory.New<OrgHeader>();
			header.MainAddress.OA_Address1 = "Main Address";
			var premiseCodeAddress = header.Addresses.AddNew();
			premiseCodeAddress.LocalControlledPremisesID = PremiseID;
			AssertEquals("Premise Code Address", premiseCodeAddress, Loader.FromLocalPremiseID(PremiseID));
		}

		#region SetUp

		protected override void SetUp()
		{
			base.SetUp();
			Loader = (OrgAddress.Loader)GetNewLoaderToTest();
		}

		OrgAddress.Loader Loader;

		#endregion

	}
}
