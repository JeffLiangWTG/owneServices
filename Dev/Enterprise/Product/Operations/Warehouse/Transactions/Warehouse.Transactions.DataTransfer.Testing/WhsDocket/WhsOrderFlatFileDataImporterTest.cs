using System;
using System.Collections.Generic;
using CargoWise.IO;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	class WhsOrderFlatFileDataImporterTest : WhsDocketFlatFileDataImporterTest<WhsOrder>
	{
		protected override string PathToTestFile => resourceRetriever.Value.SaveResourceToFile("Enterprise.Warehouse.Transactions.DataTransfer.Testing.TestFiles.PL12-Oct-2006155602.txt", "PL12-Oct-2006155602.txt");

		protected override void SetupData()
		{
			var whs = Helper.CreateWarehouse("WAREHOUSE");
			whs.WW_WarehouseCode = "BNE";

			var client = Helper.CreateClient("DANPACWLG");
			client.OH_FullName = "Alcan Packaging Danaflex";
			client.OH_RL_NKClosestPort = "NZWLG";
			client.Addresses.MainAddress.OA_Address1 = "101 Collins Ave";

			var part = Helper.CreateProduct(client, "X8434");
			part.OP_Desc = "SK10 PLAIN MARAFLEX 380MM TUBING";

			Factory.Save();
		}

		protected override FlatFileDataImporter GetDataImporter()
		{
			return new WhsOrderFlatFileDataImporter();
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}

	sealed class WhsOrderFlatFileDataImporterWithNotificationTest : WhsOrderFlatFileDataImporterTest
	{
		public void TestImportSucceeds_EmailIsSent()
		{
			SetupData();
			SetupNotificationGroup();
			EmailNotificationForErrorsOnly = false;
			Factory.Save();
			AssertEquals("Precondition: Saved Emails.", 0, EmailsCreated.Count);

			Importer.ImportData(PathToTestFile, Notify, SourceInfo.EmptySourceInfo);
			AssertEquals("Email created.", 1, EmailsCreated.Count);
			var email = EmailsCreated[0];
			AssertEquals("Subject should contain success notification.", true, email.Subject.Contains("Succeeded"));
			AssertEquals("No attachments expected.", 0, email.Attachments.Count);
		}

		public void TestImportSucceeds_EmailIsNotSentWhenDisabled()
		{
			SetupData();
			SetupNotificationGroup();
			EmailNotificationForErrorsOnly = true;
			Factory.Save();
			AssertEquals("Precondition: Saved Emails.", 0, EmailsCreated.Count);

			Importer.ImportData(PathToTestFile, Notify, SourceInfo.EmptySourceInfo);
			AssertEquals("Email shouldn't be created.", 0, EmailsCreated.Count);
		}

		public void TestImportFails_EmailIsSent()
		{
			SetupData();
			SetupNotificationGroup();
			EmailNotificationForErrorsOnly = false;
			Factory.Save();
			AssertEquals("Precondition: Saved Emails.", 0, EmailsCreated.Count);

			// Manually force an error to trigger emails
			Notify.Notify(new ErrorNotification(ErrorType.ImportingDataError, "TEST Importing Error"));

			Importer.ImportData(PathToTestFile, Notify, SourceInfo.EmptySourceInfo);
			AssertEquals("Email created.", 1, EmailsCreated.Count);
			var email = EmailsCreated[0];
			AssertEquals("Subject should contain failure notification.", true, email.Subject.Contains("Failed"));
			AssertEquals("Error attachments expected.", 1, email.Attachments.Count);
		}

		public void TestImportFails_EmailIsSent_DifferentError()
		{
			SetupData();
			SetupNotificationGroup();
			EmailNotificationForErrorsOnly = false;
			Factory.Save();
			AssertEquals("Precondition: Saved Emails.", 0, EmailsCreated.Count);

			// Manually force an error to trigger emails
			Notify.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, "TEST Invalid File Format"));

			Importer.ImportData(PathToTestFile, Notify, SourceInfo.EmptySourceInfo);
			AssertEquals("Email created.", 1, EmailsCreated.Count);
			var email = EmailsCreated[0];
			AssertEquals("Subject should contain failure notification.", true, email.Subject.Contains("Failed"));
			AssertEquals("Error attachments expected.", 1, email.Attachments.Count);
		}

		void SetupNotificationGroup()
		{
			if (postMasterGroup == null)
			{
				postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				postMasterGroup.Staff[0].GS_EmailAddress = "postmaster@example.com";
				NotificationGroup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, postMasterGroup.PK.ToGuid());
			}
		}

		GlbGroup postMasterGroup;

		new WhsOrderFlatFileDataImporterWithNotification Importer => importer ?? (importer = new WhsOrderFlatFileDataImporterWithNotification(Notify, NotificationGroup, PathToTestFile));
		WhsOrderFlatFileDataImporterWithNotification importer;

		NotificationBuffer Notify => notify ?? (notify = new NotificationBuffer());
		NotificationBuffer notify;

		bool EmailNotificationForErrorsOnly
		{
			set => SystemDataRegistry.Instance.EmailNotificationForErrorsOnly.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		GuidRegistryItem NotificationGroup => NotificationDataRegistry.Instance.WarehouseImportNotificationGroup;

		List<EmailDef> EmailsCreated => Env.OutgoingMailManager.EmailsCreated;
	}
}
