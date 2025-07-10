using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobDocumentRecipientConfiguration))]
	public class JobDocumentRecipientConfigurationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestJobDocumentRecipients()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";

			var anotherDocument = Factory.NewWithValidTestData<StmMenuItem>();

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			((DummyDocumentSupportable)dummyDocumentSupportable).ConsigneePK = org.PK;

			var orgDocument1 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument1.OD_SU_MenuItem = document.PK;

			var orgDocument2 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument2.OD_SU_MenuItem = document.PK;

			var orgDocument3 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument3.OD_SU_MenuItem = anotherDocument.PK;

			var orgDocument4 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument3.OD_SU_MenuItem = anotherDocument.PK;

			var exclusionForOrgDocument1 = Factory.New<JobDocumentExclusion>();
			exclusionForOrgDocument1.JDE_ParentID = dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK;
			exclusionForOrgDocument1.JDE_OD_Document = orgDocument1.PK;

			var exclusionForOrgDocument3 = Factory.New<JobDocumentExclusion>();
			exclusionForOrgDocument3.JDE_ParentID = dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK;
			exclusionForOrgDocument3.JDE_OD_Document = orgDocument3.PK;

			var exclusionForOrgDocument4 = Factory.New<JobDocumentExclusion>();
			exclusionForOrgDocument4.JDE_ParentID = ZGuid.NewZGuid();
			exclusionForOrgDocument4.JDE_OD_Document = orgDocument3.PK;

			var jobDocumentDeliveryWithMatchingParent1 = Factory.New<JobDocumentDelivery>();
			jobDocumentDeliveryWithMatchingParent1.JDC_ParentID = dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK;
			jobDocumentDeliveryWithMatchingParent1.JDC_SU_MenuItem = document.PK;

			var jobDocumentDeliveryWithMatchingParent2 = Factory.New<JobDocumentDelivery>();
			jobDocumentDeliveryWithMatchingParent2.JDC_ParentID = dummyDocumentSupportable.DocumentSupporter.BusinessObject.PK;

			var jobDocumentDeliveryWithAnotherParent1 = Factory.New<JobDocumentDelivery>();
			jobDocumentDeliveryWithAnotherParent1.JDC_ParentID = ZGuid.NewZGuid();

			var jobDocumentDeliveryWithAnotherParent2 = Factory.New<JobDocumentDelivery>();
			jobDocumentDeliveryWithAnotherParent2.JDC_ParentID = ZGuid.NewZGuid();

			var recipients = configuration.JobDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals(4, recipients.Count);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == jobDocumentDeliveryWithMatchingParent1.PK);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == jobDocumentDeliveryWithMatchingParent2.PK);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument1.PK);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument3.PK);
		}

		public void TestJobDocumentRecipients_AllowNewAndRemove()
		{
			Assert(configuration.JobDocumentRecipients.AllowNew);
			Assert(configuration.JobDocumentRecipients.AllowRemove);
		}

		public void TestOrgDocumentRecipients_AllowNewAndRemove()
		{
			Assert(!configuration.OrgDocumentRecipients.AllowNew);
			Assert(!configuration.OrgDocumentRecipients.AllowRemove);
		}

		public void TestOrgDocumentRecipients_DocumentPKFilterOnly()
		{
			var document1 = Factory.New<StmMenuItem>();
			document1.SU_MenuName = "Name";
			document1.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document1.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document1.SU_ContactType = ContactType.Consignee.Code;
			document1.SU_MenuPath = "Menu/Path";

			var document2 = Factory.NewWithValidTestData<StmMenuItem>();

			var document3 = Factory.New<StmMenuItem>();
			document3.SU_MenuName = "AnotherName";
			document3.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document3.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document3.SU_ContactType = ContactType.Consignee.Code;
			document3.SU_MenuPath = "Menu/Path";

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1Org1 = org1.Contacts.AddNew();
			contact1Org1.FillWithValidTestData();

			var contact2Org1 = org1.Contacts.AddNew();
			contact2Org1.FillWithValidTestData();

			var contactOrg2 = org2.Contacts.AddNew();
			contactOrg2.FillWithValidTestData();

			var contactNoOrg1 = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument1 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument1.OD_OC = contactNoOrg1.PK;
			orgDocument1.OD_SU_MenuItem = document1.PK;

			var orgDocument2 = contact1Org1.Documents.AddNew();
			orgDocument2.OD_SU_MenuItem = document1.PK;

			var orgDocument3 = contact2Org1.Documents.AddNew();
			orgDocument3.OD_SU_MenuItem = document1.PK;

			var contactNoOrg2 = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument4 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument4.OD_OC = contactNoOrg2.PK;
			orgDocument4.OD_SU_MenuItem = document2.PK;

			var orgDocument5 = contactOrg2.Documents.AddNew();
			orgDocument5.OD_SU_MenuItem = document1.PK;

			((DummyDocumentSupportable)dummyDocumentSupportable).ConsigneePK = org1.PK;

			Factory.Save();

			var recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("No documents selected be default, the view should be empty", 0, recipients.Count);

			configuration.DocumentPKFilter = document1.PK;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("2 recipients should match the selected document", 2, recipients.Count);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument2.PK);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument3.PK);

			configuration.DocumentPKFilter = document3.PK;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("No recipients should be match the selected document", 0, recipients.Count);
		}

		public void TestOrgDocumentRecipients_OtherFilters()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";

			var anotherDocument = Factory.NewWithValidTestData<StmMenuItem>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1Org1 = org1.Contacts.AddNew();
			contact1Org1.FillWithValidTestData();

			var contact2Org1 = org1.Contacts.AddNew();
			contact2Org1.FillWithValidTestData();

			var contactOrg2 = org2.Contacts.AddNew();
			contactOrg2.FillWithValidTestData();

			var orgContactNoOrg1 = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument1 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument1.OD_OC = orgContactNoOrg1.PK;
			orgDocument1.OD_SU_MenuItem = document.PK;

			var orgDocument2 = contact1Org1.Documents.AddNew();
			orgDocument2.OD_DocumentGroup = ContactType.Consignee.Code;

			var orgDocument3 = contact2Org1.Documents.AddNew();
			orgDocument3.OD_SU_MenuItem = document.PK;

			var orgContactNoOrg2 = Factory.NewWithValidTestData<OrgContact>();
			var orgDocument4 = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument4.OD_OC = orgContactNoOrg2.PK;
			orgDocument4.OD_SU_MenuItem = anotherDocument.PK;

			var orgDocument5 = contactOrg2.Documents.AddNew();
			orgDocument5.OD_SU_MenuItem = document.PK;

			((DummyDocumentSupportable)dummyDocumentSupportable).ConsigneePK = org1.PK;

			Factory.Save();

			var recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("No documents selected be default, the view should be empty", 0, recipients.Count);

			configuration.OrganisationPKFilter = org1.PK;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("2 recipients should match the selected organisation", 2, recipients.Count);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument2.PK);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument3.PK);

			configuration.DocumentGroupFilter = ContactType.Consignee.Code;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("1 recipient should match the selected organisation and document group", 1, recipients.Count);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument2.PK);

			configuration.DocumentGroupFilter = ContactType.Consignor.Code;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("No recipients should match the selected organisation and document group", 0, recipients.Count);

			configuration.DocumentGroupFilter = ZString.Empty;
			configuration.DocumentPKFilter = document.PK;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("1 recipient should match the selected organisation and document", 1, recipients.Count);
			AssertCollectionContains(recipients, r => r.WrappedBizoPK == orgDocument3.PK);

			configuration.DocumentPKFilter = ZGuid.Empty;
			configuration.OrganisationPKFilter = ZGuid.Empty;
			configuration.FilterOrgDocumentRecipients();
			recipients = configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>().ToList();
			AssertEquals("No filters selected so no recipients should be returned", 0, recipients.Count);
		}

		public void TestOrgDocumentRecipients_DoesNotPerformFindWithInvalidFilters()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = dummyDocumentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";

			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			var contact1 = organisation1.Contacts.AddNew();
			contact1.FillWithValidTestData();

			var contact2 = organisation2.Contacts.AddNew();
			contact2.FillWithValidTestData();

			var orgDocument1 = contact1.Documents.AddNew();
			orgDocument1.OD_SU_MenuItem = document.PK;

			var orgDocument2 = contact2.Documents.AddNew();
			orgDocument2.OD_DocumentGroup = ContactType.Receivables.Code;

			Factory.Save();

			configuration.DocumentPKFilter = ZGuid.Invalid;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(0, configuration.OrgDocumentRecipients.Count);

			configuration.DocumentPKFilter = ZGuid.Empty;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(0, configuration.OrgDocumentRecipients.Count);

			configuration.OrganisationPKFilter = organisation1.PK;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(1, configuration.OrgDocumentRecipients.Count);
			AssertEquals(orgDocument1.PK, configuration.OrgDocumentRecipients[0].WrappedBizoPK);

			configuration.OrgDocumentRecipients.RemoveAll();

			configuration.DocumentPKFilter = ZGuid.Empty;
			configuration.OrganisationPKFilter = ZGuid.Invalid;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(0, configuration.OrgDocumentRecipients.Count);

			configuration.OrganisationPKFilter = organisation1.PK;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(1, configuration.OrgDocumentRecipients.Count);
			AssertEquals(orgDocument1.PK, configuration.OrgDocumentRecipients[0].WrappedBizoPK);

			configuration.OrgDocumentRecipients.RemoveAll();

			configuration.OrganisationPKFilter = organisation2.PK;
			configuration.DocumentGroupFilter = ContactType.Payables.Code;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(0, configuration.OrgDocumentRecipients.Count);

			configuration.DocumentGroupFilter = "XXX";
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(0, configuration.OrgDocumentRecipients.Count);

			configuration.DocumentGroupFilter = ContactType.Receivables.Code;
			configuration.FilterOrgDocumentRecipients();
			AssertEquals(1, configuration.OrgDocumentRecipients.Count);
			AssertEquals(orgDocument2.PK, configuration.OrgDocumentRecipients[0].WrappedBizoPK);
		}

		public void TestClearRecipientsFilter()
		{
			configuration.DocumentPKFilter = ZGuid.NewZGuid();
			AssertNotEquals("Pre-condition", ZGuid.Empty, configuration.DocumentPKFilter);

			configuration.ClearOrgDocumentRecipientsFilters();
			AssertEquals("Filters should have been cleared", ZGuid.Empty, configuration.DocumentPKFilter);
		}

		public void TestDocuments()
		{
			AssertGreaterThan("Pre-condition", Factory.GetDatabaseCount(typeof(StmMenuItem)), 0);
			AssertEquals("Pre-condition", 0, Factory.GetDatabaseCount(typeof(StmMenuItem), new ZQuery(StmMenuItemSchema.SU_BusinessContext, nameof(BusinessContext.Test))));
			AssertEquals("Pre-condition", 0, configuration.Documents.Count);

			((DummyDocumentSupportable)dummyDocumentSupportable).Z0_Number = 123;

			var testMenuItem1 = Factory.New<StmMenuItem>();
			testMenuItem1.SU_BusinessContext = nameof(BusinessContext.Test);
			testMenuItem1.SU_ContactType = ContactType.All.Code;
			testMenuItem1.SU_MenuName = "ABC";

			var testMenuItem2 = Factory.New<StmMenuItem>();
			testMenuItem2.SU_BusinessContext = nameof(BusinessContext.Test);
			testMenuItem2.SU_ContactType = ContactType.Consignee.Code;
			testMenuItem2.SU_FilterList = "<Z0_Number> == 123";
			testMenuItem2.SU_MenuName = "DEF";

			var testMenuItem3 = Factory.New<StmMenuItem>();
			testMenuItem3.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testMenuItem3.SU_ContactType = ContactType.Receivables.Code;
			testMenuItem3.SU_FilterList = "<Z0_Number> == 456";
			testMenuItem3.SU_MenuName = "GHI";

			var testMenuItem4 = Factory.New<StmMenuItem>();
			testMenuItem4.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testMenuItem4.SU_ContactType = ContactType.Receivables.Code;
			testMenuItem4.SU_MenuName = "JKL";

			Factory.Save();

			configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
			var documents = configuration.Documents.Cast<StmMenuItem>().ToList();
			AssertEquals(2, documents.Count);
			AssertNoExceptionThrown(() =>
			{
				documents.Single(menu => menu.PK == testMenuItem1.PK);
				documents.Single(menu => menu.PK == testMenuItem2.PK);
			});
		}

		public void TestSuggestedOrganisations()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "EGI";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "JNC";

			var suggestions = new List<(MultilingualString organisationType, IOrgHeader orgHeader)>();
			suggestions.Add(((NoResString)"Consignee", org1));
			suggestions.Add(((NoResString)"Consignor", org2));

			var dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportableWithSuggestions>();
			dummyDocumentSupportable.SuggestedOrganisationsForDebug = suggestions;

			var configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
			Assert(configuration.SupportsSuggestions);

			var suggestedOrganisations = configuration.SuggestedOrganisations.Cast<SuggestedOrganisation>().ToList();
			AssertEquals(2, suggestedOrganisations.Count);
			AssertCollectionContains(suggestedOrganisations, s => s.OrganisationType == "Consignee" && s.OrganisationCode == "EGI");
			AssertCollectionContains(suggestedOrganisations, s => s.OrganisationType == "Consignor" && s.OrganisationCode == "JNC");
		}

		public void TestSuggestedOrganisations_EmptySuggestions()
		{
			var dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportableWithSuggestions>();
			var configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
			Assert(!configuration.SupportsSuggestions);

			var suggestedOrganisations = configuration.SuggestedOrganisations.Cast<SuggestedOrganisation>().ToList();
			AssertEquals(0, suggestedOrganisations.Count);
		}

		public void TestSuggestedOrganisations_DoesNotSupportSuggestions()
		{
			var dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			var configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
			Assert(!configuration.SupportsSuggestions);

			var suggestedOrganisations = configuration.SuggestedOrganisations.Cast<SuggestedOrganisation>().ToList();
			AssertEquals(0, suggestedOrganisations.Count);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject() => configuration;

		protected override void SetUp()
		{
			base.SetUp();

			dummyDocumentSupportable = Factory.NewWithValidTestData<DummyDocumentSupportable>();
			configuration = new JobDocumentRecipientConfiguration(Factory, dummyDocumentSupportable);
		}

		IDocumentSupportable dummyDocumentSupportable;
		JobDocumentRecipientConfiguration configuration;

		class DummyDocumentSupportableWithSuggestions : DummyDocumentSupportable, ISupportJobDocumentRecipient
		{
			public DummyDocumentSupportableWithSuggestions(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> ISupportJobDocumentRecipient.SuggestedOrganisations
			{
				get
				{
					if (SuggestedOrganisationsForDebug != null)
					{
						foreach (var suggestion in SuggestedOrganisationsForDebug)
						{
							yield return suggestion;
						}
					}
				}
			}

			internal IEnumerable<(MultilingualString organisationType, IOrgHeader orgHeader)> SuggestedOrganisationsForDebug
			{
				get;
				set;
			}
		}

		#endregion
	}
}
