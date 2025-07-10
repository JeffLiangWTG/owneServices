using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class ImportedOrderChangesDocumentDeliveryTest : TestCaseWithFactory
	{
		public void TestDeliverImportedOrderChangesOnSave()
		{
			OrderChanges[0].BuyerPK = CreateOrganisationWithDeliveryInfo("Buyer1@email.address", "Buyer1 subject", "ORGA").PK;
			OrderChanges[0].SupplierPK = CreateOrganisationWithDeliveryInfo("Supplier1@email.address", "Supplier1 subject", "ORGB").PK;
			for (int i = 1; i < OrderChanges.Length; i++)
			{
				OrderChanges[i].BuyerPK = CreateOrganisationWithDeliveryInfo("Buyer2@email.address", "Buyer2 subject", "ORG" + i).PK;
			}

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in OrderChanges)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			AssertEquals("Email not sent before save", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);
			ZQuery query = new ZQuery();
			query.AddToFilter(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			StmPrintJob[] deliveredDocuments = Factory.Load<StmPrintJob>(query);

			// Extra Diagnostics for Intermittent Test Failure - ZA 09-Nov-09
			if (deliveredDocuments.Length != 3)
			{
				string failure = "Number of print jobs. Expected 3. Actual: " + deliveredDocuments.Length + System.Environment.NewLine;
				failure += "Number of emails in queue: " + Env.OutgoingMailManager.EmailsCreated.Count + System.Environment.NewLine;
				foreach (StmPrintJob job in deliveredDocuments)
				{
					failure += "  Attachment Format: " + job.SP_EmailAttachmentFormat + System.Environment.NewLine;
					failure += "  Document Name: " + job.SP_DocumentName + System.Environment.NewLine;
					failure += "  Destination: " + job.SP_Destination + System.Environment.NewLine;
					failure += "  Subject: " + job.SP_EmailSubjectLine + System.Environment.NewLine;
					failure += "  Email/Fax Dest: " + job.SP_Destination + System.Environment.NewLine;
				}

				deliveredDocuments = new BusinessObjectFactory().Load<StmPrintJob>(query);
				if (deliveredDocuments.Length == 3)
				{
					failure += "Reloading in new factory found 3 results." + System.Environment.NewLine;
				}
				else
				{
					failure += "Reloading in new factory found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
					query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-60));
					deliveredDocuments = Factory.Load<StmPrintJob>(query);
					if (deliveredDocuments.Length == 3)
					{
						failure += "Reloading in same factory with larger time (60 seconds) found 3 results." + System.Environment.NewLine;
					}
					else
					{
						failure += "Reloading in same factory with larger time found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
						deliveredDocuments = Factory.Load<StmPrintJob>(new ZQuery());
						if (deliveredDocuments.Length == 3)
						{
							failure += "Reloading in same factory with ALL StmPrintJobs found 3 results." + System.Environment.NewLine;
						}
						else
						{
							failure += "Reloading in same factory with ALL StmPrintJobs found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
						}
					}
				}

				Fail(failure);
			}
			else
			{
				AssertEquals("3 order change documents sent", 3, deliveredDocuments.Length);

				AssertEquals("Buyer1@email.address", deliveredDocuments[0].SP_Destination);
				AssertEquals("Buyer1 subject", true, deliveredDocuments[0].SP_EmailSubjectLine.EndsWith("Order Import Report"));

				AssertEquals("Supplier1@email.address", deliveredDocuments[1].SP_Destination);
				AssertEquals("Supplier1 subject", true, deliveredDocuments[1].SP_EmailSubjectLine.EndsWith("Order Import Report"));

				AssertEquals("Buyer2@email.address", deliveredDocuments[2].SP_Destination);
				AssertEquals("Buyer2 subject", true, deliveredDocuments[2].SP_EmailSubjectLine.EndsWith("Order Import Report"));
			}
		}

		public void TestDeliverImportedOrderChangesOnSave_UsesNotificationGroupIfNoAnOrderHasNoRecipients()
		{
			OrderChanges[0].BuyerPK = CreateOrganisationWithDeliveryInfo("Buyer@email.address", "Buyer1 subject", "ORG1").PK;
			OrderChanges[0].SupplierPK = CreateOrganisationWithDeliveryInfo("Supplier@email.address", "Supplier1 subject", "ORG2").PK;

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in OrderChanges)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			AssertEquals("Email not sent before save", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);
			ZQuery query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			StmPrintJob[] deliveredDocuments = Factory.Load<StmPrintJob>(query);

			AssertEquals("3 order change documents sent", 3, deliveredDocuments.Length);
			AssertEquals("Buyer@email.address", deliveredDocuments[0].SP_Destination);
			AssertEquals("Buyer subject", true, deliveredDocuments[0].SP_EmailSubjectLine.EndsWith("Order Import Report"));
			AssertEquals("Supplier@email.address", deliveredDocuments[1].SP_Destination);
			AssertEquals("Supplier subject", true, deliveredDocuments[1].SP_EmailSubjectLine.EndsWith("Order Import Report"));
			AssertEquals("NotificationGroup@email.address", deliveredDocuments[2].SP_Destination);
			AssertEquals(true, deliveredDocuments[2].SP_EmailSubjectLine.EndsWith("- Order Import Report"));
		}

		public void TestNotificationGroupHavingStaffWithNoEmail()
		{
			var staff = NotificationGroup.Staff.AddNew();
			staff.GS_Code = "NNN";
			staff.GS_LoginName = "noemail";
			Factory.Save();
			TestDeliverImportedOrderChangesOnSave_UsesNotificationGroupIfNoAnOrderHasNoRecipients();
		}

		public void TestEMailAttachmentTypeIsSetFromRegistryItem_ForXLS()
		{
			TestEMailAttachmentTypeIsSetFromRegistryItem(OrgConstants.AttachmentType.XLS);
		}

		public void TestEMailAttachmentTypeIsSetFromRegistryItem_ForPDF()
		{
			TestEMailAttachmentTypeIsSetFromRegistryItem(OrgConstants.AttachmentType.PDF);
		}

		public void TestEMailAttachmentTypeIsSetFromRegistryItem_ForTIF()
		{
			TestEMailAttachmentTypeIsSetFromRegistryItem(OrgConstants.AttachmentType.TIF);
		}

		void TestEMailAttachmentTypeIsSetFromRegistryItem(string attachmentType)
		{
			OrdersDataRegistry.Instance.OrderImportReportAttachmentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, attachmentType);

			OrderChanges[0].BuyerPK = CreateOrganisationWithDeliveryInfo("Buyer@email.address", "Buyer1 subject", "ORG1").PK;
			OrderChanges[0].SupplierPK = CreateOrganisationWithDeliveryInfo("Supplier@email.address", "Supplier1 subject", "ORG2").PK;

			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in OrderChanges)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			AssertEquals("Email not sent before save", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);

			ZQuery query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			StmPrintJob[] deliveredDocuments = Factory.Load<StmPrintJob>(query);

			// Extra diagnostics for Intermittent test failures - to be removed soon (9-Nov-09 ZA)
			if (deliveredDocuments.Length != 3)
			{
				string failure = "Number of print jobs. Expected 3. Actual: " + deliveredDocuments.Length + System.Environment.NewLine;
				failure += "Number of emails in queue: " + Env.OutgoingMailManager.EmailsCreated.Count + System.Environment.NewLine;
				foreach (StmPrintJob job in deliveredDocuments)
				{
					failure += "  Attachment Format: " + job.SP_EmailAttachmentFormat + System.Environment.NewLine;
					failure += "  Document Name: " + job.SP_DocumentName + System.Environment.NewLine;
					failure += "  Destination: " + job.SP_Destination + System.Environment.NewLine;
					failure += "  Subject: " + job.SP_EmailSubjectLine + System.Environment.NewLine;
					failure += "  Email/Fax Dest: " + job.SP_Destination + System.Environment.NewLine;
				}

				deliveredDocuments = new BusinessObjectFactory().Load<StmPrintJob>(query);
				if (deliveredDocuments.Length == 3)
				{
					failure += "Reloading in new factory found 3 results." + System.Environment.NewLine;
				}
				else
				{
					failure += "Reloading in new factory found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
					query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-60));
					deliveredDocuments = Factory.Load<StmPrintJob>(query);
					if (deliveredDocuments.Length == 3)
					{
						failure += "Reloading in same factory with larger time (60 seconds) found 3 results." + System.Environment.NewLine;
					}
					else
					{
						failure += "Reloading in same factory with larger time found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
						deliveredDocuments = Factory.Load<StmPrintJob>(new ZQuery());
						if (deliveredDocuments.Length == 3)
						{
							failure += "Reloading in same factory with ALL StmPrintJobs found 3 results." + System.Environment.NewLine;
						}
						else
						{
							failure += "Reloading in same factory with ALL StmPrintJobs found " + deliveredDocuments.Length + " results." + System.Environment.NewLine;
						}
					}
				}

				Fail(failure);
			}
			else
			{
				AssertEquals(attachmentType, deliveredDocuments[0].SP_EmailAttachmentFormat);
				AssertEquals(attachmentType, deliveredDocuments[1].SP_EmailAttachmentFormat);
				AssertEquals(attachmentType, deliveredDocuments[2].SP_EmailAttachmentFormat);
			}
		}

		public void TestDeliverAllImportedOrderChanges()
		{
			List<ImportedOrder> importedOrders = new List<ImportedOrder>();
			foreach (Order order in OrderChanges)
			{
				importedOrders.Add(new ImportedOrder(order));
			}

			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);
			ZQuery query = new ZQuery(StmPrintJobSchema.SP_RunDateTime, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddSeconds(-10));
			StmPrintJob[] deliveredDocuments = Factory.Load<StmPrintJob>(query);
			AssertEquals("No order change documents sent", 1, deliveredDocuments.Length);

			deliveredDocuments[0].Delete();
			importedOrders = new List<ImportedOrder>();
			importedOrders.Add(new ImportedOrder(OrdersWithChanges.AttachedOrder));
			OrdersDataRegistry.Instance.IncludeChangesNotApplied.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ImportedOrderChangesDocumentManager.DeliverAllImportedOrderChanges(importedOrders);
			deliveredDocuments = Factory.Load<StmPrintJob>(query);
			AssertEquals("Nothing to report", 0, deliveredDocuments.Length);
		}

		#region Implementation

		Guid originalNotificationGroup;
		String originalOrderImportReportType;

		OrgHeader CreateOrganisationWithDeliveryInfo(ZString emailAddress, ZString emailSubject, ZString orgCode)
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader result = newFactory.NewWithValidTestData<OrgHeader>();
			result.OH_Code = orgCode;
			EDICommunicationsMode mode = result.EDICommunicationsModes.AddNew();
			mode.EK_Module = WorkflowDescriptors.OrderWorkflowDescriptorCode;
			mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.OrderImportReport;
			mode.EK_Destination = emailAddress;
			mode.EK_ServerAddressSubject = emailSubject;
			newFactory.Save();
			return result;
		}

		GlbGroup NotificationGroup
		{
			get
			{
				if (notificationGroup == null)
				{
					notificationGroup = Factory.New<GlbGroup>();
					GlbStaff staff = notificationGroup.Staff.AddNew();
					staff.GS_Code = "ZAC";
					staff.GS_EmailAddress = "NotificationGroup@email.address";
				}
				return notificationGroup;
			}
		}
		GlbGroup notificationGroup;

		Order[] OrderChanges
		{
			get
			{
				return new Order[]
				{
					OrdersWithChanges.NewOrder,
					OrdersWithChanges.AmendedOrder,
					OrdersWithChanges.UnchangedOrder,
					OrdersWithChanges.CancelledOrder,
				};
			}
		}

		OrdersWithChangesForTest OrdersWithChanges
		{
			get { return ordersWithChanges ?? (ordersWithChanges = new OrdersWithChangesForTest(Factory)); }
		}
		OrdersWithChangesForTest ordersWithChanges;

		protected override void SetUp()
		{
			base.SetUp();
			NotificationGroup.Factory.Save();
			originalNotificationGroup = NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.PK.ToGuid());
			originalOrderImportReportType = OrdersDataRegistry.Instance.OrderImportReportAttachmentType.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
		}

		protected override void TearDown()
		{
			base.TearDown();
			NotificationDataRegistry.Instance.ImportedOrderChangesNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalNotificationGroup);
			OrdersDataRegistry.Instance.OrderImportReportAttachmentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalOrderImportReportType);
		}

		#endregion
	}
}
