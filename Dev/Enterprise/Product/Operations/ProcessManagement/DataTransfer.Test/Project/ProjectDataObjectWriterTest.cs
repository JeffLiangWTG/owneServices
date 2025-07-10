using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class ProjectDataObjectWriterTest : ActivityDataObjectWriterTestCase<Project>
	{
		public void TestPopulateClient_WithEmptyClientAddressAndContact()
		{
			var project = BizoFactory.New<Project>();
			project.WKP_Summary = "This is not a test";
			BizoFactory.Save();

			AssertEquals(ZGuid.Empty, project.WKP_OA_ClientAddress);
			AssertEquals(ZGuid.Empty, project.WKP_OC_Contact);

			var activity = ActivityTestHelper.WriteActivity(project);

			AssertNull(activity.OrganizationAddressCollection);
		}

		public void TestPopulateClient_WithEmptyClientAddress_ButValidContact()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory);

			var project = BizoFactory.New<Project>();
			project.WKP_Summary = "This is not a test";
			project.WKP_OC_Contact = contact.PK;
			BizoFactory.Save();

			AssertEquals(ZGuid.Empty, project.WKP_OA_ClientAddress);

			var activity = ActivityTestHelper.WriteActivity(project);

			AssertNotNull(activity.OrganizationAddressCollection);
			AssertEquals(1, activity.OrganizationAddressCollection.Count);

			var activityAddress = activity.OrganizationAddressCollection.Single();
			AssertEquals("The Organisation", activityAddress.CompanyName);
			AssertEquals(contact.Header.MainAddress.Address1, activityAddress.Address1);
			AssertEquals("Jan Michael Vincent", activityAddress.Contact);
		}

		public void TestPopulateClient_WithEmptyClientContact_ButValidAddress()
		{
			var contact = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory);

			var project = BizoFactory.New<Project>();
			project.WKP_Summary = "This is not a test";
			project.WKP_OA_ClientAddress = contact.Header.MainAddress.PK;
			BizoFactory.Save();

			AssertEquals(ZGuid.Empty, project.WKP_OC_Contact);

			var activity = ActivityTestHelper.WriteActivity(project);

			var activityAddress = activity.OrganizationAddressCollection.Single();
			AssertEquals("The Organisation", activityAddress.CompanyName);
			AssertEquals(contact.Header.MainAddress.Address1, activityAddress.Address1);
			AssertNull(activityAddress.Contact);
		}

		protected override Project GetBusinessObject()
		{
			var project = BizoFactory.NewWithValidTestData<Project>();
			project.WKP_Summary = "I hope our prices aren't too low!";
			project.WKP_Details = ZBlob.FromUTF8("Get your shit together.");
			project.WKP_Status = "CAN";
			project.WKP_Type = "AAA";
			project.WKP_SubType = "BBB";
			project.WKP_Module = "CCC";
			project.WKP_Priority = "DDD";

			var contact1 = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory, "Vandelay Industries");
			var address = contact1.Header.MainAddress;
			address.Address1 = "123 Canada Street";
			address.Address2 = "Unit 123";
			address.City = "Canada City";
			address.State = "Nova Scotia";
			address.Postcode = "B0V 1A0";
			address.OA_RN_NKCountryCode = "CA";
			address.OA_Email = "some@email.com";
			address.OA_RL_NKRelatedPortCode = "AUSYD";

			var otherAddress = contact1.Header.Addresses.AddNew();
			otherAddress.Address1 = "15 Yemen Road";
			otherAddress.OA_RN_NKCountryCode = "YE";

			var contact2 = ProcessMgmtTestHelper.CreateOrganizationAndContact(BizoFactory, "Simple Rick's", "Mrs Sullivan", "mrs@simplericks.com");
			var address2 = contact2.Header.MainAddress;
			address2.Address1 = "3a/72 O'Riordan Street";
			address2.City = "Alexandria";
			address2.State = "NSW";
			address2.Postcode = "2015";
			address2.OA_RN_NKCountryCode = "AU";
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			BizoFactory.Save();

			project.WKP_OC_Contact = contact1.PK;
			project.WKP_OA_ClientAddress = otherAddress.PK;
			project.WKP_OC_TechnicalContact = contact2.PK;

			var manager = GlbStaff.New(BizoFactory);
			manager.GS_Code = "JAN";
			manager.GS_FullName = "Janet";
			project.WKP_GS_NKProjectManager = manager.GS_Code;

			return project;
		}

		protected override IEnumerable<IWorkTaskRelatedItem> GetRelatedItems()
		{
			var project = ProcessMgmtTestHelper.CreateProject(BizoFactory, "Yeah, I'd like to order one large phone with extra phones please.");
			project.WKP_ProjectNumber = "PROJECTNUM";
			yield return project;

			var workitem = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory, "Cellphone. No, no, no, no. Rotary. And pay phone on half.");
			workitem.WKI_WorkItemNumber = "WKINUM";
			yield return workitem;
		}

		protected override IWorkTaskRelatedItem GetSecondLevelRelatedItem()
		{
			return ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
		}

		protected override string ExpectedSummary => "I hope our prices aren't too low!";
		protected override string ExpectedDescription => "Get your shit together.";
		protected override string ExpectedStatus => "CAN";
		protected override string ExpectedSelectionCriterion1 => "AAA";
		protected override string ExpectedSelectionCriterion2 => "BBB";
		protected override string ExpectedSelectionCriterion3 => "CCC";
		protected override string ExpectedSelectionCriterion4 => "DDD";
		protected override string ExpectedSelectionCriterion5 => null;
		protected override string ExpectedBranchCode => null;
		protected override string ExpectedDepartmentCode => null;
		protected override string ExpectedLocation => null;
		protected override string ExpectedCompanyCode => null;
		protected override string ExpectedProjectManagerCode => "JAN";
		protected override string ExpectedClient1Name => "Jan Michael Vincent";
		protected override string ExpectedClient1Address1 => "15 Yemen Road";
		protected override string ExpectedClient2Name => "Mrs Sullivan";
		protected override string ExpectedClient2Address1 => "3a/72 O'Riordan Street";
		protected override IEnumerable<string> ExpectedRelatedItems => new[]
		{
			"Project: PROJECTNUM - Yeah, I'd like to order one large phone with extra phones please.",
			"WorkItem: WKINUM - Cellphone. No, no, no, no. Rotary. And pay phone on half.",
		};

		protected override IEnumerable<string> GetAdditionalExpectedNoteText()
		{
			return new[]
			{
				@"18-Jul-18 00:00 E - Canceled
----------------------------------------------------------------------------------------------------------------------
18-Jul-18 00:00 E - Project Created
----------------------------------------------------------------------------------------------------------------------
",
			};
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.Project;
	}
}
