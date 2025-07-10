using System;
using CargoWise.Data;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Tracking.Business.Data.Testing
{
	[TestedType(typeof(InvoiceLinkValueObjectDataAdapter))]
	[HttpContextEnabledTest]
	sealed class InvoiceLinkValueObjectDataAdapterTest : ValueObjectDataAdapterTest<InvoicingBase, Xsd.InvoiceLink>
	{
		public void TestBusinessObjectType()
		{
			var dataAdapter = new InvoiceLinkValueObjectDataAdapter();
			AssertEquals("Business Object Type should be InvoicingBase", typeof(InvoicingBase), dataAdapter.BusinessObjectType);
		}

		public void TestValueObjectType()
		{
			var dataAdapter = new InvoiceLinkValueObjectDataAdapter();
			AssertEquals("Value Object Type should be Xsd.InvoiceLink", typeof(Xsd.InvoiceLink), dataAdapter.ValueObjectType);
		}

		public void TestSchema()
		{
			var dataAdapter = new InvoiceLinkValueObjectDataAdapter();
			AssertEquals("Schema should be InvoiceLinkSchema", XmlSchemaDefinitions.Instance.SingleInvoiceLinkSchema, dataAdapter.Schema);
		}

		public void TestCollectionSchema()
		{
			var dataAdapter = new InvoiceLinkValueObjectDataAdapter();
			AssertEquals("CollectionSchema should be InvoiceLinkssSchema", XmlSchemaDefinitions.Instance.InvoiceLinksSchema, dataAdapter.CollectionSchema);
		}

		public void TestExportCollection()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/Tracking/");
			var testUser = LoginWebUser();

			var testParent = Factory.NewWithValidTestData<TrackingShipment>();

			var testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_JobNum = "1234";
			testJob.JH_ParentID = testParent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;
			testJob.JH_GB = GlbBranch.CurrentBranch.PK;
			testJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice1 = Factory.New<AccTransactionHeader>();
			invoice1.AH_TransactionNum = "00001000";
			invoice1.AH_TransactionType = "INV";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice1.AH_OH = testUser.LoggedInOrganisation.PK;
			invoice1.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice1.AH_JH = testJob.PK;
			invoice1.AH_InvoiceDate = new ZDateTime(2008, 1, 18, 10, 32, 17);
			invoice1.AH_ConsolidatedInvoiceRef = testParent.Reference;

			var invoice2 = Factory.New<AccTransactionHeader>();
			invoice2.AH_TransactionNum = "00002000";
			invoice2.AH_TransactionType = "INV";
			invoice2.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice2.AH_OH = testUser.LoggedInOrganisation.PK;
			invoice2.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice2.AH_JH = testJob.PK;
			invoice2.AH_InvoiceDate = new ZDateTime(2008, 1, 18, 10, 32, 17);
			invoice2.AH_ConsolidatedInvoiceRef = testParent.Reference;

			AssertNotNull(testParent.InvoiceLoader.Transactions);
			AssertEquals("Incorrect number of invoices loaded", 2, testParent.InvoiceLoader.Transactions.Count);

			var adapter = new InvoiceLinkValueObjectDataAdapter();
			var links = adapter.ExportToXmlValueObjectCollection(testParent.InvoiceLoader.Transactions, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Two Links", 2, links.Count);
		}

		protected override ValueObjectDataAdapter<InvoicingBase, Xsd.InvoiceLink> GetNewBizObjXmlDataAdapter() => new InvoiceLinkValueObjectDataAdapter();

		protected override string ExpectedRootCollectionElementName => "InvoiceLinks";

		protected override string ExpectedRootElementName => "InvoiceLink";

		protected override InvoicingBase NewBusinessObject() => Factory.NewWithValidTestData<ARInvoice>();

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var emptyInvoiceLinkPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyInvoiceLink.xml", "EmptyInvoiceLink.xml");
			return new BusinessObjectAndExpectedOutputFileName(EmptyBusinessObject(), emptyInvoiceLinkPath, ValidationKind.None, "Empty InvoiceLink");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var fullInvoiceLinkPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.FullInvoiceLink.xml", "FullInvoiceLink.xml");

			return new BusinessObjectAndExpectedOutputFileName(PopulatedBusinessObject(), fullInvoiceLinkPath, ValidationKind.Xsd | ValidationKind.FactorySave, "Fully Populated InvoiceLink");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			var emptyInvoiceLinkPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.Tracking.Business.Testing.TestFiles.EmptyInvoiceLink.xml", "EmptyInvoiceLink.xml");
			return new BusinessObjectAndExpectedOutputFileName(EmptyBusinessObject1(), emptyInvoiceLinkPath, ValidationKind.None, "Empty InvoiceLink 1");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects() => Array.Empty<BusinessObjectAndExpectedOutputFileName>();

		protected override void TestExportToAndImportFromAndExportToValueObject(BusinessObjectAndExpectedOutputFileName sample)
		{
			Assert("This test is not required because import from value is not supported, implement ImportFromValueObjectCore first", true);
		}

		protected override bool IsImportFromValueObjectSupported => false;

		protected override void SetUp()
		{
			base.SetUp();
			initialUserContext = EnvProxy.Instance.CurrentUserContext;
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			EnvProxy.Instance.SetUserContext(initialUserContext);
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
		IUserContext initialUserContext;

		InvoicingBase EmptyBusinessObject()
		{
			var invoice = Factory.New<ARInvoice>();
			invoice.AH_TransactionType = "INV";
			invoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_InvoiceDate = new ZDateTime(2008, 1, 18);
			invoice.AH_DueDate = new ZDateTime(2008, 1, 31);

			return invoice;
		}

		InvoicingBase EmptyBusinessObject1()
		{
			var invoice = EmptyBusinessObject();
			invoice.AH_TransactionCount += 1;
			return invoice;
		}

		InvoicingBase PopulatedBusinessObject()
		{
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://localhost/Tracking/");
			var testUser = LoginWebUser();

			var testobjCreator = new TestObjectCreator(Factory);

			var invoice = (ARInvoice)testobjCreator.CreateInvoice(typeof(ARInvoice), "1000", testobjCreator.AUD, 1.00m);

			invoice.AH_TransactionReference = "1000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			invoice.AH_OH = testUser.LoggedInOrganisation.PK;
			invoice.AH_InvoiceDate = new ZDateTime(2008, 1, 18, 10, 32, 17);
			invoice.AH_DueDate = new ZDateTime(2008, 1, 31);

			var invoiceLine = testobjCreator.CreateARInvoiceLine(invoice, null, testobjCreator.CC4, testobjCreator.AUD, 1m, "Desc", 105.50m);

			Factory.Save();

			// Need to have a specific PK for the invoice because the PK used to build the URL that is returned in the XML
			const string invoicePKForTesting = "01f6f60f-244c-4b4d-9b6c-f95bc7ae6d98";

			Db.Connection.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = {2}, {3} = GETUTCDATE(), {4} = 'TST' WHERE {5} = '{6}'", // Need to have specific PK for BO because it is used to build URL which should be the same
			AccTransactionLinesSchema.Constants.TableName,
			AccTransactionLinesSchema.Constants.AL_AH, "NULL",
			AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc,
			AccTransactionLinesSchema.Constants.AL_SystemLastEditUser,
			AccTransactionLinesSchema.Constants.PK, invoiceLine.PK));

			Db.Connection.ExecuteNonQuery(string.Format("DELETE FROM {0} WHERE {1} = '{2}'", // Need to have specific PK for BO because it is used to build URL which should be the same
			AccTransactionHeaderReferenceSchema.Constants.TableName,
			AccTransactionHeaderReferenceSchema.Constants.AH1_AH, invoice.PK));

			Db.Connection.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = '{2}', {3} = GETUTCDATE(), {4} = 'TST' WHERE {5} = '{6}'", // Need to have specific PK for BO because it is used to build URL which should be the same
			AccTransactionHeaderSchema.Constants.TableName,
			AccTransactionHeaderSchema.Constants.PK, "01f6f60f-244c-4b4d-9b6c-f95bc7ae6d98",
			AccTransactionHeaderSchema.Constants.AH_SystemLastEditTimeUtc,
			AccTransactionHeaderSchema.Constants.AH_SystemLastEditUser,
			AccTransactionHeaderSchema.Constants.PK, invoice.PK));

			Db.Connection.ExecuteNonQuery(string.Format("UPDATE {0} SET {1} = '{2}', {3} = GETUTCDATE(), {4} = 'TST' WHERE {5} = '{6}'", // Need to have specific PK for BO because it is used to build URL which should be the same
			AccTransactionLinesSchema.Constants.TableName,
			AccTransactionLinesSchema.Constants.AL_AH, "01f6f60f-244c-4b4d-9b6c-f95bc7ae6d98",
			AccTransactionLinesSchema.Constants.AL_SystemLastEditTimeUtc,
			AccTransactionLinesSchema.Constants.AL_SystemLastEditUser,
			AccTransactionLinesSchema.Constants.PK, invoiceLine.PK));

			invoice = Factory.Load<ARInvoice>(new ZGuid(invoicePKForTesting));
			invoice.AH_OutstandingAmount = invoice.AH_InvoiceAmount + invoice.AH_GSTAmount;
			invoice.AH_FullyPaidDate = ZDateTime.Empty;
			AssertEquals("Precondition: AH_OSTotalAmount", 105.50m, invoice.AH_OSTotalAmount);
			return invoice;
		}

		OrgContactWebUser LoginWebUser()
		{
			var contact = Factory.Load<OrgContact>(new ZGuid("f960e868-fef4-4cab-a1f5-3ace433c04e9"));
			AssertNotNull("Fixed Contact", contact);
			AssertEquals("Contact Name", "TONY MORAN - SALES", contact.OC_ContactName);
			contact.OC_WebAccessEnabled = true;
			var password = "test";
			contact.SetHashedPassword(password);

			Factory.Save();

			var user = (OrgContactWebUser)WebEnv.AppInstance.GetNewSiteUser();
			user.Login(contact.ParentOrg.OH_Code, contact.OC_Email, password);
			((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(user);
			AssertEquals("Logged in Contact Name", "TONY MORAN - SALES", WebEnv.CurrentUser.Name);
			AssertEquals("IsLogged In", true, user.IsLoggedIn);

			return user;
		}
	}
}
