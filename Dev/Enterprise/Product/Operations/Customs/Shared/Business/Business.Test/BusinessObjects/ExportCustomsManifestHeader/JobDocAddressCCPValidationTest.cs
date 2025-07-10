using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDocAddressCCPValidationTest : TestCaseWithFactory
	{
		public void TestCheckOrganisationPKHasErrorWhenPicked()
		{
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.PackDepotAddress.OrganisationPK = frmOrgHeader.PK;
			AssertHasError(header.PackDepotAddress.OrganisationPKInfo, "This selected Pack Depot Address does not have a linked Customs Controlled Premise code");
		}

		public void TestCheckOrganisationPKNoError()
		{
			frmOrgHeader.MainAddress.LocalControlledPremisesID = "Z123Z";
			var header = Factory.New<ExportCustomsManifestHeader>();
			header.PackDepotAddress.OrganisationPK = frmOrgHeader.PK;
			AssertEquals(false, header.PackDepotAddress.OrganisationPKInfo.Notifications.HasErrors());
		}

		OrgHeader frmOrgHeader;
		protected override void SetUp()
		{
			base.SetUp();
			frmOrgHeader = Factory.New<OrgHeader>();
			frmOrgHeader.FillWithValidTestData();
			frmOrgHeader.OH_FullName = "JPDuminy Bond Stores";
			frmOrgHeader.OH_IsWarehouseClient = true;
			_ = frmOrgHeader.Addresses.AddNew();
		}
	}
}
