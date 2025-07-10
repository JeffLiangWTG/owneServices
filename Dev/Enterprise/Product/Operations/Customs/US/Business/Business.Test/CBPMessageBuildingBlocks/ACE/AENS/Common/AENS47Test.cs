using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.BIRD.ACE;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Testing
{
	sealed class AENS47Test : TestCaseWithFactory
	{
		public void TestUpdateInvoiceLine()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress1 = orgHeader.Addresses.AddNew();
			orgAddress1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "PHEVEAPP80SAB", GlbCompany.CurrentCompany.Country);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var notifications = new NotificationBuffer();
			var aens47 = new AENS47()
			{
				ArticlePartyTypeCode = "M",
				ArticlePartyIdentifier = "PHEVEAPP80SAB"
			};

			((IACEBIRDLineRecord)aens47).Update(invoiceLine, notifications);
			AssertEquals(orgAddress1.PK, invoiceLine.JI_OA_ManufacturerAddress);

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			aens47 = new AENS47()
			{
				ArticlePartyTypeCode = "M",
				ArticlePartyIdentifier = "  PHEVEAPP80SAB"
			};

			notifications.Clear();
			((IACEBIRDLineRecord)aens47).Update(invoiceLine, notifications);
			AssertEquals(ZGuid.Empty, invoiceLine.JI_OA_ManufacturerAddress);
			AssertContains(ZString.Format(OrganisationCreator.NoManufacturerCreatedAsMIDInvalid, "  PHEVEAPP80SAB"), notifications.AsString.Trim());

			invoiceLine.JI_OA_ManufacturerAddress = ZGuid.Empty;
			aens47 = new AENS47()
			{
				ArticlePartyTypeCode = "M",
				ArticlePartyIdentifier = "PHEVEAPP80SABCD"
			};

			notifications.Clear();
			((IACEBIRDLineRecord)aens47).Update(invoiceLine, notifications);
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_FullName, AutocreatefromMID.FullName));
			AssertEquals("New organization and address is created", org.MainAddress.PK, invoiceLine.JI_OA_ManufacturerAddress);
		}
	}
}
