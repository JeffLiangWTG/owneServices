using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgContactsFilterBusinessObject))]
	sealed class OrgContactsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLogsFilter()
		{
			var contact1 = GetNewContact();
			var contact2 = GetNewContact();
			var contact3 = GetNewContact();
			contact1.OC_ContactName = "AAA";
			contact2.OC_ContactName = "BBB";
			contact3.OC_ContactName = "CCC";

			var log1 = contact1.Logs.AddNew();
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_SE_NKEvent = Events.BatchProcessorLogCode;
				log1.SL_EventTime = new ZDateTime(2015, 1, 1);
			}

			var log2 = contact2.Logs.AddNew();
			using (log2.LockForUpdatingKeyFieldsForTesting())
			{
				log2.SL_SE_NKEvent = Events.BookedCode;
				log2.SL_EventTime = new ZDateTime(2015, 1, 1);
			}

			var log3 = contact3.Logs.AddNew();
			using (log3.LockForUpdatingKeyFieldsForTesting())
			{
				log3.SL_SE_NKEvent = Events.DangerousGoodsChangedCode;
				log3.SL_EventTime = new ZDateTime(2015, 1, 1);
			}

			var log4 = contact3.Logs.AddNew();
			using (log4.LockForUpdatingKeyFieldsForTesting())
			{
				log4.SL_SE_NKEvent = Events.BatchProcessorLogCode;
				log4.SL_EventTime = new ZDateTime(2015, 1, 1);
			}

			Factory.Save();

			var filterBizObj = GetFilterBizObj();

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();

			AssertEquals(3, contactsCollection.Count);

			var relatedLogsFilter = (ModuleGuidForeignCollectionFilter)filterBizObj["Contact Logs"];
			relatedLogsFilter.IsActive = true;

			var isBEventFilter = relatedLogsFilter.SelectedFilters.AddTextFilterStrip("Event Code", "B");
			isBEventFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			relatedLogsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			contactsCollection.Load(filterBizObj.Filter);
			AssertEquals(3, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact2, contactsCollection);
			AssertCollectionContains(contact3, contactsCollection);

			var isBPLEventFilter = relatedLogsFilter.SelectedFilters.AddTextFilterStrip("Event Code", Events.BatchProcessorLogCode);
			isBPLEventFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			contactsCollection.Load(filterBizObj.Filter);

			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact3, contactsCollection);

			relatedLogsFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			isBPLEventFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			contactsCollection.Load(filterBizObj.Filter);
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);
		}

		public void TestDashboardActivityFilter()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity1 = organisation.SalesOpportunities.AddNew();
			opportunity1.P8_Status = "CRT";

			var opportunity2 = organisation.SalesOpportunities.AddNew();
			opportunity2.P8_Status = "CCC";

			Factory.Save();

			var contact1 = GetNewContact();
			var contact2 = GetNewContact();
			var contact3 = GetNewContact();
			contact1.OC_ContactName = "AAA";
			contact2.OC_ContactName = "BBB";
			contact3.OC_ContactName = "CCC";

			opportunity1.P8_OC = contact1.PK;
			opportunity2.P8_OC = contact2.PK;

			Factory.Save();

			var filterBizObj = GetFilterBizObj();

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();

			AssertEquals(3, contactsCollection.Count);

			var dashboardFilter = (ModuleGuidForeignCollectionFilter)filterBizObj["Sales Activity"];
			dashboardFilter.IsActive = true;

			var oppFilter = dashboardFilter.SelectedFilters.AddTextFilterStrip("Opportunity Status", "C");
			oppFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;

			dashboardFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			contactsCollection.Load(filterBizObj.Filter);
			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact2, contactsCollection);

			oppFilter.Property = "CRT";
			oppFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			contactsCollection.Load(filterBizObj.Filter);

			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);

			dashboardFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			contactsCollection.Load(filterBizObj.Filter);
			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);
			AssertCollectionContains(contact3, contactsCollection);
		}

		public void TestNotificationRoleFilter()
		{
			var contact1 = GetNewContact();
			contact1.OC_Email = "c1@contact.com";

			var contact2 = GetNewContact();
			contact2.OC_Email = "c2@contact.com";

			var contact3 = GetNewContact();
			contact3.OC_Email = "c3@contact.com";

			var document1 = contact1.Documents.AddNew();
			document1.OD_DocumentGroup = ContactType.Receivables.ToString();
			document1.OD_DefaultContact = true;

			var document2 = contact2.Documents.AddNew();
			document2.OD_DocumentGroup = ContactType.Miscellaneous.ToString();
			document2.OD_DefaultContact = true;

			var document3 = contact2.Documents.AddNew();
			document3.OD_DocumentGroup = ContactType.Receivables.ToString();
			document3.OD_DefaultContact = true;

			Factory.Save();

			var filterBizObj = GetFilterBizObj();

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(3, contactsCollection.Count);

			var filter = (ModuleTextFilter)filterBizObj["Notification Role"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.IsActive = true;

			filter.Property = ContactType.Miscellaneous.ToString();
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);

			filter.Property = ContactType.Receivables.ToString();
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact2, contactsCollection);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact3, contactsCollection);
		}

		public void TestEmail()
		{
			var contact1 = GetNewContact();
			contact1.OC_Email = "andrew@cargowise.com";
			var contact1SecondaryEmail = Factory.New<OrgContactItem>();
			contact1SecondaryEmail.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			contact1SecondaryEmail.OI_OC = contact1.PK;
			contact1SecondaryEmail.OI_Address = "andrew@home.com";
			contact1SecondaryEmail.OI_IsPrimary = false;

			var contact2 = GetNewContact();
			contact2.OC_Email = "sam@wisetechglobal.com";

			var contact3 = GetNewContact();
			contact3.OC_Email = "richard@wisetechglobal.com";
			var contact3SecondaryEmail = Factory.New<OrgContactItem>();
			contact3SecondaryEmail.OI_ContactItemType = OrgContactItemTypes.Codes.Email;
			contact3SecondaryEmail.OI_OC = contact3.PK;
			contact3SecondaryEmail.OI_Address = "richard@cargowise.com";
			contact3SecondaryEmail.OI_IsPrimary = false;

			var contact4 = GetNewContact();

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleTextFilter)filterBizObj["Email"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.IsActive = true;

			var contactsCollection1 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection1.Load();
			AssertCollectionContains(contact1, contactsCollection1);
			AssertCollectionContains(contact2, contactsCollection1);
			AssertCollectionContains(contact3, contactsCollection1);
			AssertCollectionContains(contact4, contactsCollection1);
			AssertEquals(4, contactsCollection1.Count);

			filter.Property = "andrew";
			var contactsCollection2 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection2.Load();
			AssertCollectionContains(contact1, contactsCollection2);
			AssertEquals(1, contactsCollection2.Count);

			filter.Property = "com";
			var contactsCollection3 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection3.Load();
			AssertCollectionContains(contact1, contactsCollection3);
			AssertCollectionContains(contact2, contactsCollection3);
			AssertCollectionContains(contact3, contactsCollection3);
			AssertEquals(3, contactsCollection3.Count);

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotContain;
			filter.Property = "cargowise.com";
			var contactsCollection4 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection4.Load();
			AssertCollectionContains(contact2, contactsCollection4);
			AssertCollectionContains(contact4, contactsCollection4);
			AssertEquals(2, contactsCollection4.Count);
		}

		public void TestJobCategory()
		{
			var contact1 = GetNewContact();
			contact1.OC_Email = "edward@cargowise.com";
			contact1.OC_JobCategory = "Accounts Manager";
			var contact2 = GetNewContact();
			contact2.OC_Email = "richard@gmail.com";

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleTextFilter)filterBizObj["JobCategory"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Contains;
			filter.IsActive = true;

			var contactsCollection1 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection1.Load();
			AssertCollectionContains(contact1, contactsCollection1);
			AssertCollectionContains(contact2, contactsCollection1);
			AssertEquals(2, contactsCollection1.Count);

			filter.Property = "Accounts Manager";
			var contactsCollection2 = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection2.Load();
			AssertCollectionContains(contact1, contactsCollection2);
			AssertEquals(1, contactsCollection2.Count);
		}

		public void TestInactiveOrgContact()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = false;
			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_OH = orgHeader1.PK;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_OH = orgHeader2.PK;

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleTextFilter)filterBizObj["Name"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Test";
			filter.IsActive = true;

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertCollectionContains(contact2, contactsCollection);
			AssertEquals(1, contactsCollection.Count);
		}

		public void TestWebAccessEnabledOnlyOrgContact()
		{
			var contact1 = GetNewContact();
			contact1.OC_Email = "edward@cargowise.com";
			contact1.OC_WebAccessEnabled = true;

			var contact2 = GetNewContact();
			contact2.OC_Email = "richard@gmail.com";

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleFlagsFilter)filterBizObj["Web Access"];
			filter.Property0 = true;
			filter.IsActive = true;

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertCollectionContains(contact1, contactsCollection);
			AssertEquals(1, contactsCollection.Count);

			filter.Property0 = false;
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
		}

		public void TestNumContactsNameFilter()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;
			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_OH = orgHeader1.PK;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_OH = orgHeader2.PK;

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleTextFilter)filterBizObj["Name"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.Property = "Test";
			filter.IsActive = true;

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertCollectionContains(contact2, contactsCollection);
			AssertCollectionContains(contact1, contactsCollection);
			AssertEquals(2, contactsCollection.Count);
		}

		public void TestPrimaryWorkplaceContactsOnly()
		{
			var filterBizObj = GetFilterBizObj();
			var primaryContactFilter = (ModuleTextFilter)filterBizObj["Is Primary Workplace Contact"];
			AssertNotNull(primaryContactFilter);
			var nameFilter = (ModuleTextFilter)filterBizObj["Name"];
			AssertNotNull(nameFilter);
			nameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nameFilter.Property = "Test";
			nameFilter.IsActive = true;

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_FullName = "Test User Only - Fred Nerk";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Test User Only - Jason Smith";

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;
			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_OH = orgHeader1.PK;
			contact1.OC_IsActive = true;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_OH = orgHeader2.PK;
			contact2.OC_IsActive = true;

			contact1.OC_PER = person1.PK;
			contact1.OC_Title = "boss";
			contact1.OC_IsActive = false;

			contact2.OC_PER = person2.PK;
			contact2.OC_Title = "developer";

			var primaryRelationshipPivot = Factory.NewWithValidTestData<GlbPersonPrimaryRelationship>();
			primaryRelationshipPivot.PPR_PER = person1.PK;
			primaryRelationshipPivot.Primary = contact1;

			Factory.Save();

			var expectedContact1 = Factory.Load<OrgContact>(contact1.PK);
			var expectedContact2 = Factory.Load<OrgContact>(contact2.PK);

			primaryContactFilter.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.PrimaryWorkplaceContactsOnly;
			primaryContactFilter.IsActive = true;
			var contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(1, contactsSubset.Count);
			AssertCollectionContains(contact1, contactsSubset);

			primaryContactFilter.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.NonPrimaryWorkplaceContacts;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(1, contactsSubset.Count);
			AssertCollectionContains(expectedContact2, contactsSubset);

			primaryContactFilter.Property = OrgConstants.FilterControl.ContactIsPrimaryWorkplace.Code.AllContacts;
			primaryContactFilter.IsActive = true;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(2, contactsSubset.Count);
			AssertCollectionContains(expectedContact1, contactsSubset);
			AssertCollectionContains(expectedContact2, contactsSubset);
		}

		public void TestVerifiedDateFilter()
		{
			var filterBizObj = GetFilterBizObj();
			var nameFilter = (ModuleTextFilter)filterBizObj["Name"];
			nameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nameFilter.Property = "Test";
			nameFilter.IsActive = true;
			var verifiedDateFilter = (ModuleDateFilter)filterBizObj["Verified"];
			verifiedDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			verifiedDateFilter.Property1 = new ZDate(2019, 1, 1);
			verifiedDateFilter.Property2 = new ZDate(2019, 1, 1);
			verifiedDateFilter.IsActive = true;

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;
			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_OH = orgHeader1.PK;
			contact1.OC_DetailsVerified = new ZDate(2018, 1, 1);

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_OH = orgHeader2.PK;
			contact2.OC_DetailsVerified = new ZDate(2019, 1, 1);

			Factory.Save();

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertCollectionContains(contact2, contactsCollection);
			AssertEquals(1, contactsCollection.Count);

			verifiedDateFilter.Property1 = new ZDate(2018, 1, 1);
			verifiedDateFilter.Property2 = new ZDate(2018, 11, 1);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);

			verifiedDateFilter.Property1 = new ZDate(2019, 10, 1);
			verifiedDateFilter.Property2 = new ZDate(2020, 1, 1);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(0, contactsCollection.Count);

			var contact3 = GetNewContact();
			contact3.OC_ContactName = "Test3";
			contact3.OC_OH = orgHeader2.PK;
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(0, contactsCollection.Count);
		}

		public void TestPersonSubModuleFilter()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;

			var person1 = Factory.NewWithValidTestData<GlbPerson>();
			person1.PER_IsActive = true;
			person1.PER_FullName = "Test1";
			var person2 = Factory.NewWithValidTestData<GlbPerson>();
			person2.PER_FullName = "Test2";
			person2.PER_IsActive = true;

			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_PER = person1.PK;
			contact1.OC_OH = orgHeader1.PK;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_PER = person2.PK;
			contact2.OC_OH = orgHeader1.PK;

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var filterBizObj = GetFilterBizObj();
			var nameFilter = (ModuleTextFilter)filterBizObj["Name"];
			nameFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nameFilter.Property = "Test";
			nameFilter.IsActive = true;
			var glbPersonFilter = (ModuleGuidForeignCollectionFilter)filterBizObj["Person"];
			var glbPersonNameFilter = glbPersonFilter.SelectedFilters.AddTextFilterStrip("Full Name", person1.PER_FullName);
			glbPersonFilter.IsActive = true;

			Factory.Save();
			AssertEquals(contact1.IsInDatabase, true);
			AssertEquals(contact2.IsInDatabase, true);
			AssertEquals(person1.IsInDatabase, true);
			AssertEquals(person2.IsInDatabase, true);

			var collection1 = new OrgContactCollection(Factory, filterBizObj.Filter);
			collection1.Load();
			AssertEquals(collection1.Count, 1);
			Assert("Collection should contain Person1", collection1.Contains(contact1));
			Assert("Collection should not contain Person2", !collection1.Contains(contact2));

			glbPersonNameFilter.Property = person2.PER_FullName;
			var collection2 = new OrgContactCollection(Factory, filterBizObj.Filter);
			collection2.Load();
			Assert("Collection should not contain Person1", !collection2.Contains(contact1));
			Assert("Collection should contain Person2", collection2.Contains(contact2));

			SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var filterBizObj1 = GetFilterBizObj();
			var nameFilter1 = (ModuleTextFilter)filterBizObj1["Name"];
			nameFilter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			nameFilter1.Property = "Test";
			nameFilter1.IsActive = true;
			var glbPersonFilterNotEnabled = (ModuleGuidForeignCollectionFilter)filterBizObj1["Person"];
			Assert(glbPersonFilterNotEnabled == null);
		}

		public void TestDeliveryStatusFilter()
		{
			var emailAddressNDR = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddressNDR.GI_EmailAddress = "harrypotter@hogwarts.com";
			emailAddressNDR.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.NonDeliveryReport;

			var emailAddressUNV = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddressUNV.GI_EmailAddress = "snape@hogwarts.com";
			emailAddressUNV.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.Unverified;

			var emailAddressVER = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddressVER.GI_EmailAddress = "ron@hogwarts.com";
			emailAddressVER.GI_DeliveryStatus = EmailDeliveryReportStatus.Codes.ValidReport;

			var contact1 = GetNewContact();
			contact1.OC_Email = "harrypotter@hogwarts.com";

			var contact2 = GetNewContact();
			contact2.OC_Email = "snape@hogwarts.com";

			var contact3 = GetNewContact();
			contact3.OC_Email = "ron@hogwarts.com";

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var deliveryStatusFilter = (ModuleTextFilter)filterBizObj["Delivery Status"];
			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			deliveryStatusFilter.IsActive = true;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			var contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(1, contactsSubset.Count);
			AssertCollectionContains(contact1, contactsSubset);

			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.Unverified;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(1, contactsSubset.Count);
			AssertCollectionContains(contact2, contactsSubset);

			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.ValidReport;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(1, contactsSubset.Count);
			AssertCollectionContains(contact3, contactsSubset);

			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.ValidReport;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(2, contactsSubset.Count);
			AssertCollectionContains(contact1, contactsSubset);
			AssertCollectionContains(contact2, contactsSubset);

			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.NonDeliveryReport;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(2, contactsSubset.Count);
			AssertCollectionContains(contact2, contactsSubset);
			AssertCollectionContains(contact3, contactsSubset);

			deliveryStatusFilter.Property = EmailDeliveryReportStatus.Codes.Unverified;
			deliveryStatusFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NotEqual;
			contactsSubset = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsSubset.Load();
			AssertEquals(2, contactsSubset.Count);
			AssertCollectionContains(contact1, contactsSubset);
			AssertCollectionContains(contact3, contactsSubset);
		}

		public void TestStatusReportTimeFilter()
		{
			var emailAddress1 = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddress1.GI_EmailAddress = "harrypotter@hogwarts.com";
			emailAddress1.GI_DeliveryStatus = "NDR";
			emailAddress1.GI_DeliveryReportTimeUtc = new ZDate(2018, 1, 1);

			var emailAddress2 = Factory.NewWithValidTestData<GlbEmailAddress>();
			emailAddress2.GI_EmailAddress = "snape@hogwarts.com";
			emailAddress2.GI_DeliveryStatus = "UNV";
			emailAddress2.GI_DeliveryReportTimeUtc = new ZDate(2019, 1, 1);

			var contact1 = GetNewContact();
			contact1.OC_Email = "harrypotter@hogwarts.com";

			var contact2 = GetNewContact();
			contact2.OC_Email = "snape@hogwarts.com";

			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var verifiedByDateFilter = (ModuleDateFilter)filterBizObj["Status Report Date"];
			verifiedByDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			verifiedByDateFilter.Property1 = new ZDate(2017, 1, 1);
			verifiedByDateFilter.Property2 = new ZDate(2018, 5, 1);
			verifiedByDateFilter.IsActive = true;

			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);

			verifiedByDateFilter.Property1 = new ZDate(2017, 1, 1);
			verifiedByDateFilter.Property2 = new ZDate(2019, 1, 1);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact2, contactsCollection);

			verifiedByDateFilter.Property1 = new ZDate(2018, 6, 1);
			verifiedByDateFilter.Property2 = new ZDate(2019, 1, 1);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);

			verifiedByDateFilter.Property1 = new ZDate(2020, 6, 1);
			verifiedByDateFilter.Property2 = new ZDate(2020, 1, 1);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(0, contactsCollection.Count);
		}

		public void TestOrganisationName()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test organization INC";
			orgHeader.OH_Code = "TOINC";

			var mainOrgAddress = Factory.New<OrgAddress>();
			mainOrgAddress.OA_CompanyNameOverride = "Main test organization INC";
			mainOrgAddress.Address1 = "105 Test street";
			mainOrgAddress.OA_OH = orgHeader.PK;

			var mainOrgAddressCapability = Factory.New<OrgAddressCapability>();
			mainOrgAddressCapability.PZ_IsMainAddress = true;
			mainOrgAddressCapability.PZ_AddressType = OrgAddressType.Office.Code;
			mainOrgAddressCapability.PZ_OA = mainOrgAddress.PK;

			var contactOrgAddress = Factory.New<OrgAddress>();
			contactOrgAddress.OA_CompanyNameOverride = "Contact test organization INC";
			contactOrgAddress.Address1 = "6 Test drive";
			contactOrgAddress.OA_OH = orgHeader.PK;

			var contact = GetNewContact();
			contact.OC_OH = orgHeader.PK;
			contact.OC_OA_OrgAddress = contactOrgAddress.PK;

			Factory.Save();
			var filterBizObj = GetFilterBizObj();

			var filter = (ModuleTextFilter)filterBizObj["Organization Name"];
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
			filter.IsActive = true;

			filter.Property = "main";
			var contactsCollection1 = GetFilteredCollection(filterBizObj);
			AssertCollectionNotContains(contact, contactsCollection1);
			AssertEquals(0, contactsCollection1.Count);

			filter.Property = "test";
			var contactsCollection2 = GetFilteredCollection(filterBizObj);
			AssertCollectionNotContains(contact, contactsCollection2);
			AssertEquals(0, contactsCollection2.Count);

			filter.Property = "cont";
			var contactsCollection3 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection3);
			AssertEquals(1, contactsCollection3.Count);

			contact.OC_OA_OrgAddress = ZGuid.Empty;
			Factory.Save();
			filter.Property = "main";
			var contactsCollection4 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection4);
			AssertEquals(1, contactsCollection4.Count);

			mainOrgAddressCapability.PZ_IsMainAddress = ZBool.False;
			Factory.Save();
			filter.Property = "test";
			var contactsCollection5 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection5);
			AssertEquals(1, contactsCollection5.Count);
		}

		public void TestWorkingLocation()
		{
			var orgHeaderLocation = Factory.New<RefUNLOCO>();
			orgHeaderLocation.RL_Code = "TSLCA";

			var mainAddressLocation = Factory.New<RefUNLOCO>();
			mainAddressLocation.RL_Code = "TSLCB";

			var contactAddressLocation = Factory.New<RefUNLOCO>();
			contactAddressLocation.RL_Code = "TSLCC";

			// Org Header
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "Test organization INC";
			orgHeader.OH_Code = "TOINC";
			orgHeader.OH_RL_NKClosestPort = orgHeaderLocation.RL_Code;

			var mainOrgAddress = Factory.New<OrgAddress>();
			mainOrgAddress.OA_CompanyNameOverride = "Main test organization INC";
			mainOrgAddress.Address1 = "105 Test street";
			mainOrgAddress.OA_OH = orgHeader.PK;
			mainOrgAddress.OA_RL_NKRelatedPortCode = mainAddressLocation.RL_Code;

			var mainOrgAddressCapability = Factory.New<OrgAddressCapability>();
			mainOrgAddressCapability.PZ_IsMainAddress = true;
			mainOrgAddressCapability.PZ_AddressType = OrgAddressType.Office.Code;
			mainOrgAddressCapability.PZ_OA = mainOrgAddress.PK;

			var contactOrgAddress = Factory.New<OrgAddress>();
			contactOrgAddress.OA_CompanyNameOverride = "Contact test organization INC";
			contactOrgAddress.Address1 = "6 Test drive";
			contactOrgAddress.OA_OH = orgHeader.PK;
			contactOrgAddress.OA_RL_NKRelatedPortCode = contactAddressLocation.RL_Code;

			var contact = GetNewContact();
			contact.OC_OH = orgHeader.PK;
			contact.OC_OA_OrgAddress = contactOrgAddress.PK;
			Factory.Save();

			var filterBizObj = GetFilterBizObj();
			var filter = (ModuleGuidFilter)filterBizObj["Working Location"];
			filter.IsActive = true;

			filter.Property = orgHeaderLocation.PK;
			var contactsCollection1 = GetFilteredCollection(filterBizObj);
			AssertCollectionNotContains(contact, contactsCollection1);
			AssertEquals(0, contactsCollection1.Count);

			filter.Property = mainAddressLocation.PK;
			var contactsCollection2 = GetFilteredCollection(filterBizObj);
			AssertCollectionNotContains(contact, contactsCollection2);
			AssertEquals(0, contactsCollection2.Count);

			filter.Property = contactAddressLocation.PK;
			var contactsCollection3 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection3);
			AssertEquals(1, contactsCollection3.Count);

			contact.OC_OA_OrgAddress = ZGuid.Empty;
			Factory.Save();
			filter.Property = mainAddressLocation.PK;
			var contactsCollection4 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection4);
			AssertEquals(1, contactsCollection4.Count);

			mainOrgAddressCapability.PZ_IsMainAddress = ZBool.False;
			Factory.Save();
			filter.Property = orgHeaderLocation.PK;
			var contactsCollection5 = GetFilteredCollection(filterBizObj);
			AssertCollectionContains(contact, contactsCollection5);
			AssertEquals(1, contactsCollection5.Count);
		}

#if !WINZOR
		[TestDate(2023, 05, 18)]
		public void TestPasswordInstructionLastSentTimeFilter()
		{
			var filterBizObj = GetFilterBizObj();
			var lastSentTimeDateFilter = (ModuleDateFilter)filterBizObj["Send Password Instructions - Last Sent Time"];
			AssertNotNull("Precondition: Send Password Instructions - Last Sent Time should not be null", lastSentTimeDateFilter);
			lastSentTimeDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			lastSentTimeDateFilter.IsActive = true;

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_IsActive = true;
			var contact1 = GetNewContact();
			contact1.OC_ContactName = "Test1";
			contact1.OC_Email = "contact1@test.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.OC_OH = orgHeader1.PK;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_IsActive = true;
			var contact2 = GetNewContact();
			contact2.OC_ContactName = "Test2";
			contact2.OC_Email = "contact2@test.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.OC_OH = orgHeader2.PK;

			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(contact1.CompanyPKForLogin, Guid.Empty, Guid.Empty, "http://localhost/webtracker");
			var contactSendEmailSetResetPassword = new ContactSendEmailSetResetPassword();
			contactSendEmailSetResetPassword.SendPasswordInstructions(contact1, displayMsg: false);
			var supportName = Env.CurrentUser.FullName;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test Staff";
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				TestDateAttribute.Date = new DateTime(2023, 05, 20);
				contactSendEmailSetResetPassword.SendPasswordInstructions(contact2, displayMsg: false);
				TestDateAttribute.Date = new DateTime(2023, 05, 22);
				contactSendEmailSetResetPassword.SendPasswordInstructions(contact2, displayMsg: false);
			}

			AssertEquals("outgoing emails count should be 3", 3, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Precondition: contact1 Password Instruction Last Sent Time should be 2023-05-18", new ZDateTime(2023, 05, 18).ToLocalBranchTime(), contact1.PasswordInstructionLastSentTime);
			AssertEquals("Precondition: contact1 Password Instruction Sent By should be CargoWise One Support", supportName, contact1.PasswordInstructionSentBy);
			AssertEquals("Precondition: contact2 Password Instruction Last Sent Time should be 2023-05-22", new ZDateTime(2023, 05, 22).ToLocalBranchTime(), contact2.PasswordInstructionLastSentTime);
			AssertEquals("Precondition: contact2 Password Instruction Sent By should be Test Staff", staff.GS_FullName, contact2.PasswordInstructionSentBy);

			lastSentTimeDateFilter.Property1 = new ZDate(2023, 5, 18);
			lastSentTimeDateFilter.Property2 = new ZDate(2023, 5, 19);
			var contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);

			lastSentTimeDateFilter.Property1 = new ZDate(2023, 5, 20);
			lastSentTimeDateFilter.Property2 = new ZDate(2023, 5, 21);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(0, contactsCollection.Count);

			lastSentTimeDateFilter.Property1 = new ZDate(2023, 5, 22);
			lastSentTimeDateFilter.Property2 = new ZDate(2023, 5, 23);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);

			var contact3 = GetNewContact();
			contact3.OC_ContactName = "Test3";
			contact3.OC_Email = "contact3@test.com";
			contact3.OC_WebAccessEnabled = true;
			contact3.OC_OH = orgHeader2.PK;
			Factory.Save();

			AssertEquals("Precondition: contact3 Password Instruction Last Sent Time should be empty", ZDateTime.Empty, contact3.PasswordInstructionLastSentTime);
			AssertEquals("Precondition: contact3 Password Instruction Sent By should be empty", "", contact3.PasswordInstructionSentBy);
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact2, contactsCollection);

			lastSentTimeDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(2, contactsCollection.Count);
			AssertCollectionContains(contact1, contactsCollection);
			AssertCollectionContains(contact2, contactsCollection);

			lastSentTimeDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			contactsCollection = new OrgContactCollection(Factory, filterBizObj.Filter);
			contactsCollection.Load();
			AssertEquals(1, contactsCollection.Count);
			AssertCollectionContains(contact3, contactsCollection);
		}
#endif

		#region CRM Security

		public void TestCRMSecurityFilters()
		{
			CRMSecurityProviderTest<OrgContact>.AssertFilterStrip(GetNewFilterStripBusinessObject, Env.Security.OrganisationCRMSecurity);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgContactsFilterBusinessObject();
		}

		OrgContact GetNewContact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Salutation = ContactPasswordForReducingNumberOfContactsReturned;
			return contact;
		}

		OrgContactsFilterBusinessObject GetFilterBizObj()
		{
			return new OrgContactsFilterBusinessObjectForTest();
		}

		OrgContactCollection GetFilteredCollection(OrgContactsFilterBusinessObject filterBizObj)
		{
			var collection = new OrgContactCollection(Factory, filterBizObj.Filter);
			collection.Load();
			return collection;
		}

		internal static string ContactPasswordForReducingNumberOfContactsReturned = "4E878DB3-799C-483D-91A9-9E1FF91FCB65";

		class OrgContactsFilterBusinessObjectForTest : OrgContactsFilterBusinessObject
		{
			public override ZQuery Filter
			{
				get
				{
					var result = base.Filter;
					result.AddToFilter(OrgContactSchema.OC_Salutation, OrgContactsFilterBusinessObjectTest.ContactPasswordForReducingNumberOfContactsReturned);
					return result;
				}
			}
		}

		#endregion
	}
}
