using System.Collections.Generic;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class GlbCompanyCampaignDocumentSupporterTest : TestCaseWithFactory
	{
		#region GlbCompanyCampaignDocumentSupporter

		public void TestGlbCompanyCampaignDocumentSupporter()
		{
			GlbCompanyCampaign campaign = Factory.New<GlbCompanyCampaign>();
			AssertEquals("Customisation security checkpoint", Env.Security.CampaignManagementCustomiseDocuments, campaign.DocumentSupporter.CustomisationSecurityCheckpoint);
			AssertEquals("Core.Constants.DataContext.CompanyCampaign is Supported", true, campaign.DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CompanyCampaign)));
			AssertEquals("Business Context", BusinessContext.CompanyCampaign, campaign.DocumentSupporter.BusinessContext);
		}

		public void TestDocumentEventSource_DocumentPrintRequested()
		{
			var item = Factory.New<StmMenuItem>();
			GlbCompanyCampaign campaign = new GlbCompanyCampaignTestHelper(Factory).GetCampaignWithoutErrors();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "noboby@nowhere.no";
			contact.OC_OH = org.PK;

			Factory.Save();

			GlbCampaignContactCollection campaignContactCollection = Testing.GlbCompanyCampaignSenderTest.ContactCollection(new List<BusinessObject>() { contact }, campaign);

			campaign.LoadFilteredContacts(campaignContactCollection);

			AssertEquals("Pre-condition: no campaign item", 0, campaign.CampaignsItemsSent.Count);

			DocumentCancelEventArgs eventArgs = new DocumentCancelEventArgs(item);
			((GlbCompanyCampaign.GlbCompanyCampaignDocumentSupporter)campaign.DocumentSupporter).DocumentEventSource_DocumentPrintRequested(null, eventArgs);
			Assert("Cancel should be true", eventArgs.Cancel);
			AssertEquals("Campaign has been sent", 1, campaign.CampaignsItemsSent.Count);
		}

		#endregion
	}
}
