using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.CusReconBase;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(ConsolidatedDeclaration))]
	class ConsolidatedDeclarationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCodeAndDescriptionProperty()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CodeProperty", AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber, CodePropertyAttribute.CodePropertyNameFromType(typeof(ConsolidatedDeclaration)));
				AssertEquals("DescriptionProperty", AutoCusReconDeclaration.Schema.CRD_JobReferenceNumber, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(ConsolidatedDeclaration)));
			});
		}

		public void TestSingleBusinessObjectAroundARow()
		{
			AssertEquals(1, typeof(ConsolidatedDeclaration).GetCustomAttributes(typeof(SingleObjectAroundARow), false).Length);
		}

		public void TestTypeDecider()
		{
			AssertType<ConsolidatedDeclarationTypeDecider>(ConsolidatedDeclaration.TypeDecider);
		}

		public void TestIsAutoLogged()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Logs", 0, consolidatedDeclaration.Logs.GetAllLogs().Count);
				Factory.Save();
				AssertEquals("Save Log", 1, consolidatedDeclaration.Logs.GetAllLogs().Count);
			});
		}

		public void TestSetDefaultValues()
		{
			AssertEquals("CRD_GB_Branch", GlbBranch.CurrentBranch.PK, consolidatedDeclaration.CRD_GB_Branch);
		}

		public void TestBranchCode()
		{
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, consolidatedDeclaration.BranchCode);
		}

		public void TestBranchCode_Caption()
		{
			var propertyInfo = consolidatedDeclaration.GetType().GetProperty(ConsolidatedDeclaration.Schema.BranchCode);
			AssertEquals("Branch", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestBranchName()
		{
			AssertEquals(GlbBranch.CurrentBranch.GB_BranchName, consolidatedDeclaration.BranchName);
		}

		public void TestBranchName_Caption()
		{
			var propertyInfo = consolidatedDeclaration.GetType().GetProperty(ConsolidatedDeclaration.Schema.BranchName);
			AssertEquals("Branch Name", DataBoundResourceStrings.GetDataForProperty(propertyInfo).Caption);
		}

		public void TestCRD_JobReferenceNumber_Caption()
		{
			AssertEquals("Job Number", DataBoundResourceStrings.GetDataForProperty(consolidatedDeclaration.CRD_JobReferenceNumberInfo).Caption);
		}

		public void TestHumanReadableName()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.CRD_JobReferenceNumber = ZString.Empty;
				AssertEquals("CRD_JobReferenceNumber empty", "Consolidated Declaration", consolidatedDeclaration.HumanReadableName);

				consolidatedDeclaration.CRD_JobReferenceNumber = "MON000014";
				AssertEquals("CRD_JobReferenceNumber = 'MON000014'", "Consolidated Declaration - MON000014", consolidatedDeclaration.HumanReadableName);
			});
		}

		public void TestCountryCode()
		{
			CombineAssertions(() =>
			{
				using (consolidatedDeclaration.Branch.Company.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
				{
					AssertEquals("CountryCode from Branch", Core.Constants.CountryCodes.Latvia, consolidatedDeclaration.CountryCode);
				}

				consolidatedDeclaration.CRD_GB_Branch = ZGuid.Empty;
				AssertEquals("CountryCode from CurrentCompany", Core.Constants.CountryCodes.Eritrea, consolidatedDeclaration.CountryCode);
			});
		}

		public void TestCRD_MessageStatus_ReadOnly()
		{
			AssertEquals(true, consolidatedDeclaration.CRD_MessageStatusInfo.ReadOnly);
		}

		public void TestCRD_ApplicationCode_ReadOnly()
		{
			AssertEquals(true, consolidatedDeclaration.CRD_ApplicationCodeInfo.ReadOnly);
		}

		public void TestCRD_JobReferenceNumber_ReadOnly()
		{
			AssertEquals(true, consolidatedDeclaration.CRD_JobReferenceNumberInfo.ReadOnly);
		}

		public void TestCRD_CustomsStatus_ReadOnly()
		{
			AssertEquals(true, consolidatedDeclaration.CRD_CustomsStatusInfo.ReadOnly);
		}

		public void TestMessages()
		{
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = consolidatedDeclaration;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = consolidatedDeclaration;
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Elements", new[] { message1, message2 }, consolidatedDeclaration.Messages.Cast<EDIMessage>());
				AssertEquals("IsManagedForDataRefresh", true, consolidatedDeclaration.Messages.IsManagedForDataRefresh);
			});
		}

		public void TestLeadDeclaration()
		{
			CombineAssertions(() =>
			{
				consolidatedDeclaration.JobDeclarations.RemoveAll();
				AssertNull("Default to null", consolidatedDeclaration.LeadDeclaration);
				var declaration1 = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<DummyJobDeclaration>(Factory);
				consolidatedDeclaration.JobDeclarations.Add(declaration1);
				AssertEquals("First added declaration will be lead", declaration1, consolidatedDeclaration.LeadDeclaration);
				var declaration2 = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<DummyJobDeclaration>(Factory);
				consolidatedDeclaration.JobDeclarations.Add(declaration2);
				AssertEquals("Later added declaration will not be lead", declaration1, consolidatedDeclaration.LeadDeclaration);
				consolidatedDeclaration.JobDeclarations.RemoveAt(0);
				AssertEquals("Later added declaration will be lead", declaration2, consolidatedDeclaration.LeadDeclaration);
			});
		}

		public void TestPopulateConsolidatedDeclarationEntryNo_OnSaving()
		{
			Factory.Save();
			AssertEquals("Consolidated Declaration Entry Number(" + consolidatedDeclaration.CRD_JobReferenceNumber + ") should match with regex 'CE[0-9]{8}$'", true, System.Text.RegularExpressions.Regex.IsMatch(consolidatedDeclaration.CRD_JobReferenceNumber, @"^CE[0-9]{8}$"));
		}

		public void TestJobDeclarations()
		{
			CombineAssertions(() =>
			{
				AssertEquals(true, consolidatedDeclaration.JobDeclarations is IConsolidatedJobDeclarationCollection<BaseJobDeclaration>);
				AssertEquals(consolidatedDeclaration, consolidatedDeclaration.JobDeclarations.Parent);
			});
		}

		public void TestIsConsolidated()
		{
			CombineAssertions(() =>
			{
				var dbHitsBaseLine = Factory.GetTableHitCount(CusReconEntry.Schema.TableName);
				AssertEquals("IsConsolidated", true, ConsolidatedDeclaration.IsConsolidated(consolidatedDeclaration.LeadDeclaration));
				AssertEquals("GetConsolidatedDeclaration", consolidatedDeclaration, ConsolidatedDeclaration.GetConsolidatedDeclaration(consolidatedDeclaration.LeadDeclaration));
				var dbHitsNew = Factory.GetTableHitCount(CusReconEntry.Schema.TableName);
				AssertEquals("Should have read from cache", dbHitsBaseLine, dbHitsNew);
				var declaration = ConsolidatedDeclarationTestHelper.CreateJobDeclarationReadyForConsolidation<DummyJobDeclaration>(Factory);
				AssertEquals("IsConsolidated for non-consolidated declaration", false, ConsolidatedDeclaration.IsConsolidated(declaration));
				consolidatedDeclaration.JobDeclarations.Add(declaration);
				AssertEquals("IsConsolidated for newly consolidated declaration", true, ConsolidatedDeclaration.IsConsolidated(declaration));
				consolidatedDeclaration.JobDeclarations.RemoveFromRelationship(declaration);
				AssertEquals("IsConsolidated for removed consolidated declaration", false, ConsolidatedDeclaration.IsConsolidated(declaration));
				Factory.Save();
				var newFactory = new BusinessObjectFactory();
				AssertEquals("IsConsolidated for consolidated declaration not loaded", true, ConsolidatedDeclaration.IsConsolidated(newFactory.Load<BaseJobDeclaration>(consolidatedDeclaration.LeadDeclaration.PK)));
				AssertEquals("GetConsolidatedDeclaration for consolidated declaration not loaded", consolidatedDeclaration.PK, ConsolidatedDeclaration.GetConsolidatedDeclaration(newFactory.Load<BaseJobDeclaration>(consolidatedDeclaration.LeadDeclaration.PK)).PK);

				var declaration2 = Factory.New<BaseJobDeclaration>();
				declaration2.JE_EntryStatus = "";
				AssertEquals("IsConsolidated when entry status is not equal to ATC", false, ConsolidatedDeclaration.IsConsolidated(declaration2));
				declaration2.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
				AssertEquals("IsConsolidated when entry status is equal to ATC", true, ConsolidatedDeclaration.IsConsolidated(declaration2));
			});
		}

		public void TestHasConsolidatedEntryChanges()
		{
			AssertEquals("No consolidatedEntry logs", false, consolidatedDeclaration.HasConsolidatedEntryChanges);
			var conLog = consolidatedDeclaration.Logs.AddNew(AutoEvents.ConsolidatedEntryChanged, ZDateTimeOffset.Now, null);
			AssertEquals("Has consolidatedEntry logs", true, consolidatedDeclaration.HasConsolidatedEntryChanges);
			conLog.Cancel();
			AssertEquals("No consolidatedEntry logs", false, consolidatedDeclaration.HasConsolidatedEntryChanges);
		}

		public void TestPopulateEntryStatusAndMessageStatus()
		{
			CombineAssertions(() =>
			{
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
				var message = consolidatedDeclaration.Factory.New<EDIMessage>();
				consolidatedDeclaration.Messages.Add(message);
				AssertExceptionThrown<DeveloperNotificationException>("Syncing from a front-end user is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(message));
				using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without a message is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(null));
					message.HasChanges = false;
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without accompanying message changes is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(message));
					AssertExceptionThrown<DeveloperNotificationException>("Syncing without linked message is prohibited", () => consolidatedDeclaration.SyncStatusAfterMessageProcessing(consolidatedDeclaration.Factory.New<EDIMessage>()));

					consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
					consolidatedDeclaration.Factory.Save();
					message = consolidatedDeclaration.Factory.New<EDIMessage>();
					consolidatedDeclaration.Messages.Add(message);
					consolidatedDeclaration.CRD_CustomsStatus = "XXX";
					consolidatedDeclaration.CRD_MessageStatus = "ABC";
					consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
					AssertEquals($"Post message processing: Entry Status should sync from consolidated declaration to job declarations", consolidatedDeclaration.CRD_CustomsStatus, consolidatedDeclaration.LeadDeclaration.JE_EntryStatus);
					AssertEquals($"Post message processing: Message Status should sync from consolidated declaration to job declarations", consolidatedDeclaration.CRD_MessageStatus, consolidatedDeclaration.LeadDeclaration.JE_MessageStatus);

					consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(new BusinessObjectFactory(), 2);
					consolidatedDeclaration.Factory.Save();
					message = consolidatedDeclaration.Factory.New<EDIMessage>();
					consolidatedDeclaration.Messages.Add(message);
					consolidatedDeclaration.LeadDeclaration.JE_EntryStatus = "XXX";
					consolidatedDeclaration.LeadDeclaration.JE_MessageStatus = "ABC";
					consolidatedDeclaration.SyncStatusAfterMessageProcessing(message);
					AssertEquals($"Post message processing: Entry Status should sync from job declaration to consolidated declarations", consolidatedDeclaration.LeadDeclaration.JE_EntryStatus, consolidatedDeclaration.CRD_CustomsStatus);
					AssertEquals($"Post message processing: Message Status should sync from job declaration to consolidated declarations", consolidatedDeclaration.LeadDeclaration.JE_MessageStatus, consolidatedDeclaration.CRD_MessageStatus);
				}
			});
		}

		public void TestBuildAggregateDeclaration()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.LeadDeclaration.DeclarationNumber = "EntryNumber123";
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[0]);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[1]);

			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "TESTIMP";
			var deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(ZArchitecture.Business.AddressType.PIC));
			deliveryAddress.OA_Address1 = "Addr 1";
			deliveryAddress.OA_City = "CTY";
			var leadDeclaration = consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_OH_Importer = importer.PK;
			leadDeclaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			Factory.Save();
			consolidatedDeclaration.Messages.AddNew().EM_MessageNum = "MSG567";
			consolidatedDeclaration.CRD_JobReferenceNumber = "CRD";

			var baselineChangeNumber = Factory.LastChangeNumber;
			var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
			CombineAssertions(() =>
			{
				AssertEquals("Build does not make factory dirty", baselineChangeNumber, Factory.LastChangeNumber);

				AssertType<ReadOnlyBusinessObjectFactory>(aggregateDeclaration.Factory);
				AssertEquals("Refresh Bus notifications are disabled", false, aggregateDeclaration.Factory.RefreshEnabled);
				AssertEquals("JE_DeclarationReference", "CRD", aggregateDeclaration.JE_DeclarationReference);
				AssertEquals("DeclarationNumber", "EntryNumber123", aggregateDeclaration.DeclarationNumber);
				AssertEquals("Weight", new ZArchitecture.ZWeight(200m, "KG"), aggregateDeclaration.GrossWeight);
				AssertEquals("Volume", new ZArchitecture.ZVolume(400m, ""), aggregateDeclaration.Volume);
				AssertEquals("TotalPayable", 70m, aggregateDeclaration.ActiveEntryHeaders[0].CH_TotalPaid);
				AssertEquals("Invoices", 2, aggregateDeclaration.Invoices.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice Lines", 2, aggregateDeclaration.InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice 1 Lines", 1, aggregateDeclaration.Invoices[0].InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("Invoice 2 Lines", 1, aggregateDeclaration.Invoices[1].InvoiceLines.Select(_ => _.PK).Distinct().Count());
				AssertEquals("DocAddresses", 2, aggregateDeclaration.DocAddresses.Count);
				AssertEquals("Delivery Address", "ADDR 1 CTY", aggregateDeclaration.ImporterDeliveryAddress.AddressAsASingleLine);
				AssertEquals("MergedLines", 2, aggregateDeclaration.ActiveEntryHeaders[0].MergedLines.Count);
				AssertEquals("AllEntryLines", 2, aggregateDeclaration.ActiveEntryHeaders[0].AllEntryLines.Count);
				AssertContainsExactElementsInAnyOrder("Bills", new[] { "MB000", "HB111", "MB000", "HB111" }, aggregateDeclaration.Bills.Cast<Bill>().Select(_ => _.CU_BillNum));
				AssertEquals("Message", "MSG567", (aggregateDeclaration.ActiveEntryHeaders[0].Messages.Single() as EDIMessage).EM_MessageNum);
				AssertEquals("Packages", 2, aggregateDeclaration.Packages.Count);
				AssertEquals("Containers", 2, aggregateDeclaration.CusContainers.Count);
			});
		}

		public void TestImportAggregateDeclaration()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[0]);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[1]);
			Factory.Save();
			CombineAssertions(() =>
			{
				var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
				var factoryCapturingChanges = new BusinessObjectFactory();
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges);
				AssertEquals("Importing without action does not make factory dirty", 0, factoryCapturingChanges.LastChangeNumber);

				aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
				aggregateDeclaration.JE_MessageStatus = "123";
				var message = aggregateDeclaration.Factory.New<EDIMessage>();
				aggregateDeclaration.ActiveEntryHeaders[0].Messages.Add(message);
				var attach = aggregateDeclaration.Factory.New<EDIMessageAttach>();
				message.MessageAttachments.Add(attach);
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges);
				var changeSet = factoryCapturingChanges.GetChanges();
				AssertContainsExactElementsInAnyOrder("Changed tables by importing is JobDeclaration", new[] { "CRD", "JE" }, changeSet.GetChangedObjects().Select(bizo => bizo.SessionInstance.PKSchemaColumn.ColumnPrefix).Distinct());
				AssertContainsExactElementsInAnyOrder("Added tables by importing are EDIMessage, EDIMessageAttach", new[] { "EM", "EG" }, changeSet.GetAddedObjects().Select(bizo => bizo.PKSchemaColumn.ColumnPrefix).Distinct());
			});
		}

		public void TestImportAggregateDeclaration_ImportMessagesFalse()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[0]);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[1]);
			Factory.Save();
			CombineAssertions(() =>
			{
				var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
				aggregateDeclaration.JE_MessageStatus = "123";
				var message = aggregateDeclaration.Factory.New<EDIMessage>();
				aggregateDeclaration.ActiveEntryHeaders[0].Messages.Add(message);
				var attach = aggregateDeclaration.Factory.New<EDIMessageAttach>();
				message.MessageAttachments.Add(attach);
				var factoryCapturingChanges = new BusinessObjectFactory();
				consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration, factoryCapturingChanges, false);
				var changeSet = factoryCapturingChanges.GetChanges();
				AssertContainsExactElementsInAnyOrder("Changed tables by importing is JobDeclaration", new[] { "CRD", "JE" }, changeSet.GetChangedObjects().Select(bizo => bizo.SessionInstance.PKSchemaColumn.ColumnPrefix).Distinct());
				AssertEquals("Messages and their attachments not added", 0, changeSet.GetAddedObjects().Length);
			});
		}

		public void TestImportMessages()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[0]);
			PrepareDeclarationForAggregation(consolidatedDeclaration.JobDeclarations[1]);
			Factory.Save();
			CombineAssertions(() =>
			{
				var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
				aggregateDeclaration.JE_MessageStatus = "123";
				var message1 = aggregateDeclaration.ActiveEntryHeaders[0].Messages.AddNew();
				var attach1 = aggregateDeclaration.Factory.New<EDIMessageAttach>();
				message1.MessageAttachments.Add(attach1);
				var message2 = aggregateDeclaration.ActiveEntryHeaders[0].Messages.AddNew();
				var attach2 = aggregateDeclaration.Factory.New<EDIMessageAttach>();
				message2.MessageAttachments.Add(attach2);
				var factoryCapturingChanges = consolidatedDeclaration.Factory;
				consolidatedDeclaration.ImportMessages(aggregateDeclaration, factoryCapturingChanges);
				var changeSet = factoryCapturingChanges.GetChanges();
				AssertContainsExactElementsInAnyOrder("Added tables by importing are EDIMessage, EDIMessageAttach", new[] { "EM", "EG" }, changeSet.GetAddedObjects().Select(bizo => bizo.PKSchemaColumn.ColumnPrefix).Distinct());
			});
		}

		public void TestImportAggregateDeclaration_Concurrency()
		{
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory, 2);
			Factory.Save();
			CombineAssertions(() =>
			{
				var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
				var consolidateDeclarationInNewFactory = new BusinessObjectFactory().Load<DummyConsolidatedDeclaration>(consolidatedDeclaration.PK);
				consolidateDeclarationInNewFactory.JobDeclarations.RemoveFromRelationship(consolidateDeclarationInNewFactory.JobDeclarations[1]);
				consolidateDeclarationInNewFactory.Factory.Save();
				AssertExceptionThrown<ZCannotSaveException>(() => { consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration); });
			});
		}

		public void TestImportAggregateDeclaration_LeadDeclarationHighestLineNumber()
		{
			Factory.Save();
			var aggregateDeclaration = consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregateDeclaration.ActiveEntryHeaders[0].CH_HighestLineNumber = 1;
			consolidatedDeclaration.ImportAggregateDeclaration(aggregateDeclaration);
			AssertEquals("Lead Declaration highest line number is unchanged", (ZShort)0, consolidatedDeclaration.LeadDeclaration.ActiveEntryHeaders[0].CH_HighestLineNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<DummyConsolidatedDeclaration>(Factory);
		}
		ConsolidatedDeclaration consolidatedDeclaration;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => consolidatedDeclaration;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consolidatedDeclaration;

		void PrepareDeclarationForAggregation(BaseJobDeclaration declaration)
		{
			declaration.GrossWeight = new ZArchitecture.ZWeight(100m, "KG");
			declaration.Volume = new ZArchitecture.ZVolume(200m, "");
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.DocAddresses.AddNew();
			declaration.ActiveEntryHeaders[0].AllEntryLines.AddNew().InvoiceLines.AddRange(declaration.InvoiceLines);
			declaration.ActiveEntryHeaders[0].MergedLines.Add(declaration.ActiveEntryHeaders[0].AllEntryLines[0]);
			declaration.CusContainers.AddNew();
			declaration.JE_MasterBill = "MB000";
			declaration.JE_HouseBill = "HB111";
			declaration.Packages[0].CW_PackQty = 1;
			declaration.ActiveEntryHeaders[0].Packages.AddRange(declaration.Packages);
			declaration.ActiveEntryHeaders[0].CH_TotalPaid = 35m;
			declaration.InvoiceLines[0].Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 385m, "NZD");
		}
	}

	class DummyConsolidatedDeclaration : ConsolidatedDeclaration
	{
		public DummyConsolidatedDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_ApplicationCode = ConsolidatedDeclaration.ApplicationCodes.TSW;
		}

		protected override IConsolidatedJobDeclarationCollection<BaseJobDeclaration> CreateNewJobDeclarationCollection() => new ConsolidatedJobDeclarationCollection<DummyJobDeclaration>(this);
	}

	class DummyJobDeclaration : BaseJobDeclaration
	{
		public DummyJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ZString DeclarationNumber
		{
			get => declarationNumber;
			set
			{
				declarationNumber = value;
				var entryNumber = CreateNewCusEntryNumber();
				entryNumber.CE_EntryNum = value;
				entryNumber.CE_EntryType = "@@@";
			}
		}
		ZString declarationNumber;
	}
}
