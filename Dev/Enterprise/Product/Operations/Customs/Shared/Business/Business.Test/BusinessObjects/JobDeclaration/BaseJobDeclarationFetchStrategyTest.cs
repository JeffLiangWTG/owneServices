using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	sealed class BaseJobDeclarationFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForViewForJobColums()
		{
			var jobStatus = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_Status);
			var baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			baseJobDeclaration.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, jobStatus) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var holdReason = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_HoldReason);
			baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			baseJobDeclaration.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, holdReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));

			Factory.DropHints();
			var profitLossReason = nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_ProfitLossReasonCode);
			baseJobDeclaration = Factory.New<BaseJobDeclaration>();
			baseJobDeclaration.FetchStrategy.FetchForView(new [] { new TableColumn(string.Empty, profitLossReason) });
			AssertEquals("Factory should have fetch hint for JobHeader table", 1, Factory.ActiveFetchHintsForTable(JobHeader.Schema.TableName));
		}

		public void TestFetchForView_HoldReason()
		{
			AssertFetchForView(nameof(BaseJobDeclaration.Job) + "+" + JobHeader.Schema.JH_HoldReason, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_JobStatus()
		{
			AssertFetchForView(nameof(BaseJobDeclaration.Job) + "+" + JobHeader.Schema.JH_Status, new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_ProfitLossReason()
		{
			AssertFetchForView(nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_ProfitLossReasonCode), new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		public void TestFetchForView_TotalProfitRevenueMargin()
		{
			AssertFetchForView(nameof(BaseJobDeclaration.Job) + "+" + nameof(BaseJobDeclaration.Job.JH_TotalProfitRevenueMargin), new Dictionary<string, int>
			{
				{ JobHeader.Schema.TableName, 1 }
			});
		}

		void AssertFetchForView(string propertyName, Dictionary<string, int> expectedDbHits)
		{
			for (var i = 0; i <= 10; i++)
			{
				Factory.NewWithValidTestData<BaseJobDeclaration>();
			}
			Factory.Save();

			var headers = Factory.Load<BaseJobDeclaration>(new ZQuery());
			Factory.ResetDatabaseLoadCount();

			foreach (var header in headers)
			{
				header.FetchStrategy.FetchForView(new[]
				{
					new TableColumn(string.Empty, propertyName)
				});
			}

			foreach (var header in headers)
			{
				_ = header.ZPropertyInfoHash.GetPropertySafe(propertyName).Value;
			}

			AssertDbHits(expectedDbHits, Factory);
			Factory.ResetDatabaseLoadCount();
		}

		public void TestClusterKeyFetchHints()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dec = newFactory.Load<BaseJobDeclaration>(declaration.PK);

			using (AssertDbHitsForAllFactories("LoadChildEditableObjects",
				new Dictionary<string, int>
				{
					{ CusContainerSchema.Constants.TableName, 1 }
				},
				ignoreUnspecified: true))
			{
				dec.FetchStrategy.FetchForLoadChildEditableObjects();
				_ = dec.CusContainers.Any();
			}
		}

		public void TestFetchForView_Import()
		{
			TestFetchForView(idx => CreateFullyPopulatedObject(idx, "IMP", false));
		}

		public void TestFetchForView_Import_WithShipment()
		{
			TestFetchForView(idx => CreateFullyPopulatedObject(idx, "IMP", true));
		}

		public void TestFetchForView_Export()
		{
			TestFetchForView(idx => CreateFullyPopulatedObject(idx, "EXP", false));
		}

		public void TestFetchForView_Export_WithShipment()
		{
			TestFetchForView(idx => CreateFullyPopulatedObject(idx, "EXP", true));
		}

		void TestFetchForView(Func<int, BaseJobDeclaration> declarationConstructor)
		{
			var dec1 = declarationConstructor(1);
			var dec2 = declarationConstructor(2);
			Factory.Save();

			TestFetchForView(dec1, dec2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			return new BaseJobDeclarationCollection(factory);
		}

		BaseJobDeclaration CreateFullyPopulatedObject(int uniqueIndex, ZString messageType, bool withShipment)
		{
			var numberOfInvoices = 3;
			var numberOfInvoiceLines = 10;
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_MessageType = messageType;
			declaration.JE_IsCancelled = false;

			if (withShipment)
			{
				declaration.JE_JS = Factory.New<ForwardingShipment>().PK;

				var transportCompany = OrgHeader.New(Factory);
				transportCompany.OH_IsLocalTransport = true;
				transportCompany.MainAddress.OA_Address1 = "Add1";
				transportCompany.OH_Code = $"ORG{uniqueIndex}";

				declaration.DeliveryOrPickupCartageCoPK = transportCompany.PK;
				declaration.JE_FCLDeliveryOrPickupEquipmentNeeded = "WUP";
			}

			var declarationSupplier = OrgHeader.New(Factory);
			declarationSupplier.OH_Code = $"SUP{uniqueIndex}";
			declarationSupplier.OH_IsConsignor = true;
			declarationSupplier.MainAddress.OA_Address1 = "Add1";
			declaration.JE_OH_Supplier = declarationSupplier.PK;

			var declarationImporter = OrgHeader.New(Factory);
			declarationImporter.OH_Code = $"IMP{uniqueIndex}";
			declarationImporter.OH_IsConsignee = true;
			declarationImporter.MainAddress.OA_Address1 = "Add1";
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "AUSYD";

			declaration.JE_OA_SellerAddress = declarationSupplier.MainAddress.PK;
			declaration.JE_OA_SoldToPartyAddress = declarationSupplier.MainAddress.PK;
			declaration.JE_OA_ShipToPartyAddress = declarationSupplier.MainAddress.PK;
			declaration.JE_OA_ManufacturerAddress = declarationSupplier.MainAddress.PK;
			declaration.JE_OA_ConsigneeAddress = declarationSupplier.MainAddress.PK;

			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			for (int invoiceLoop = 1; invoiceLoop <= numberOfInvoices; invoiceLoop++)
			{
				var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				var supplier = OrgHeader.New(Factory);
				supplier.OH_IsConsignor = true;
				supplier.MainAddress.OA_Address1 = "Add1";
				supplier.OH_Code = $"SUP{uniqueIndex}.{invoiceLoop}";
				invoice.JZ_OH_Supplier = supplier.PK;
				var parentPK = ZGuid.Empty;
				for (int i = 0; i < numberOfInvoiceLines; i++)
				{
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();

					if ((i % 3) == 1)
					{
						invoiceLine.JI_ParentID = ZGuid.Empty;
						var part = OrgSupplierPart.New(Factory);
						part.OP_PartNum = "PART" + i.ToString();
						var relation = part.RelatedOrganisations.AddNew();
						relation.OU_OH = supplier.PK;
						relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;

						var partUnit = part.PartUnits.AddNew();
						partUnit.OF_QuantityInParent = 24m;
						partUnit.OF_ParentPackType = "CTN";

						invoiceLine.JI_PartNo = part.OP_PartNum;

						parentPK = invoiceLine.PK;
					}
					else
					{
						invoiceLine.JI_ParentID = parentPK;
					}
				}
			}

			return declaration;
		}

		public void TestEntryLineUseCusAddInfo()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var fetchDecider1 = new BaseJobDeclarationFetchStrategy.ChildTypeSupported(declaration1);
			AssertEquals("EntryLineUseCusAddInfo = false in base", false, fetchDecider1.EntryLineUseCusAddInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var declaration2 = Factory.New<BaseJobDeclaration>();
				var fetchDecider2 = new BaseJobDeclarationFetchStrategy.ChildTypeSupported(declaration2);
				AssertEquals("EntryLineUseCusAddInfo = true in CA", true, fetchDecider2.EntryLineUseCusAddInfo);
			}
		}

		public void TestEntryLineUseCusCodeData()
		{
			var declaration1 = Factory.New<BaseJobDeclaration>();
			var fetchDecider1 = new BaseJobDeclarationFetchStrategy.ChildTypeSupported(declaration1);
			AssertEquals("EntryLineUseCusCodeData = false in base", false, fetchDecider1.EntryLineUseCusCodeData);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration2 = Factory.New<BaseJobDeclaration>();
				var fetchDecider2 = new BaseJobDeclarationFetchStrategy.ChildTypeSupported(declaration2);
				AssertEquals("EntryLineUseCusCodeData = true in GB", true, fetchDecider2.EntryLineUseCusCodeData);
			}
		}

		public void TestFetchForLoad()
		{
			CombineAssertions(() =>
			{
				var jobDec = Factory.New<JobDeclarationForTest>();
				var header = jobDec.CustomsEntryHeaders.AddNew();
				var shipment = Factory.New<ForwardingShipment>();
				jobDec.JE_JS = shipment.PK;
				Factory.Save();

				var strategy = new BaseJobDeclarationFetchStrategy(jobDec);
				strategy.FetchForLoad();
				AssertEquals("Active Hint", @"GenAddOnColumn
GenCustomAddOnRuleAck
GenCustomAddOnValue
OrgCompanyData
ProcessTaskNotification
ProcessTaskTemplate", new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends());

				strategy.FetchForLoadChildEditableObjects();
				var stmALogCount = Factory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName);
				AssertEquals("When we load a declaration, we do not need to load all StmALogs.", 0, stmALogCount);

				AssertEquals("table name with fetch hints after FetchForLoadChildEditableObjects was called", @"CusContainer
CusContainerInvoiceLinePivot
CusDecHouseBill
CusDecHouseContainerPack
CusEntryHeader
CusEntryInstruction
CusEntryNum
CusEquipment
CusHouseContPackInvoiceHeaderPivot
CusHouseContPackInvoiceLinePivot
CusUnderbondDec
EDIMessage
GenAddOnColumn
GenCustomAddOnRuleAck
GenCustomAddOnValue
GlbBranch
JobComInvLineComponentInventory
JobComInvLineRefs
JobComInvoiceHeader
JobComInvoiceHeaderRefs
JobComInvoiceLine
JobComInvoiceLineTax
JobDecRefs
JobDocAddress
JobDocsAndCartage
JobOrderHeader
OrgCompanyData
ProcessHeader
ProcessTaskNotification
ProcessTasks
ProcessTaskTemplate
StmNote", new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends());

				var cusEntryNumCount = Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName);
				TableColumn tc1 = new TableColumn("JobDeclaration", "WorkflowItems+Milestones+LastMilestone+P9_SE_NKMilestoneEvent");
				TableColumn tc2 = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.OrderNumbers);
				TableColumn tc3 = new TableColumn("JobDeclaration", "DocsAndCartage+JP_CustomDate1");
				TableColumn tc4 = new TableColumn("JobDeclaration", "Branch");
				TableColumn tc5 = new TableColumn("JobDeclaration", "WorkflowItems+Milestones+NextMilestone+P9_Description");
				TableColumn tc6 = new TableColumn("JobDeclaration", "DocsAndCartage+JP_CustomDate2");
				TableColumn tc7 = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.DeclarationNumber);
				TableColumn tc8 = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.EarliestCustomsEntryIssueDate);
				TableColumn tc9 = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.AuditDate);
				TableColumn tc10 = new TableColumn("JobDeclaration", nameof(BaseJobDeclaration.Job) + "+" + JobHeader.Schema.JH_Status);
				TableColumn tc11 = new TableColumn("JobDeclaration", nameof(BaseJobDeclaration.Job) + "+" + JobHeader.Schema.JH_HoldReason);
				strategy.FetchForView(new TableColumn[] { tc1, tc2, tc3, tc4, tc5, tc6, tc7, tc9, tc10, tc11 });
				AssertEquals("table name with fetch hints after FetchForView was called", @"AccTransactionHeader
CusContainer
CusContainerInvoiceLinePivot
CusDecHouseBill
CusDecHouseContainerPack
CusEntryHeader
CusEntryInstruction
CusEntryNum
CusEquipment
CusHouseContPackInvoiceHeaderPivot
CusHouseContPackInvoiceLinePivot
CusUnderbondDec
DtbBookingConsolidation
EDIMessage
GenAddOnColumn
GenCustomAddOnRuleAck
GenCustomAddOnValue
GlbBranch
JobCartage
JobComInvLineComponentInventory
JobComInvLineRefs
JobComInvoiceHeader
JobComInvoiceHeaderRefs
JobComInvoiceLine
JobComInvoiceLineTax
JobConsolTransport
JobDecRefs
JobDocAddress
JobDocsAndCartage
JobHeader
JobOrderHeader
OrgCompanyData
ProcessHeader
ProcessTaskNotification
ProcessTasks
ProcessTaskTemplate
StmNote", new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends());
				AssertEquals(cusEntryNumCount, Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));

				cusEntryNumCount = Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName);
				strategy.FetchForView(new TableColumn[] { tc8 });
				AssertEquals("No new fetch hint needed as DeclarationNumber should already added the fetch hint required", @"AccTransactionHeader
CusContainer
CusContainerInvoiceLinePivot
CusDecHouseBill
CusDecHouseContainerPack
CusEntryHeader
CusEntryInstruction
CusEntryNum
CusEquipment
CusHouseContPackInvoiceHeaderPivot
CusHouseContPackInvoiceLinePivot
CusUnderbondDec
DtbBookingConsolidation
EDIMessage
GenAddOnColumn
GenCustomAddOnRuleAck
GenCustomAddOnValue
GlbBranch
JobCartage
JobComInvLineComponentInventory
JobComInvLineRefs
JobComInvoiceHeader
JobComInvoiceHeaderRefs
JobComInvoiceLine
JobComInvoiceLineTax
JobConsolTransport
JobDecRefs
JobDocAddress
JobDocsAndCartage
JobHeader
JobOrderHeader
OrgCompanyData
ProcessHeader
ProcessTaskNotification
ProcessTasks
ProcessTaskTemplate
StmNote", new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends());
				AssertEquals(cusEntryNumCount, Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));

				shipment = Factory.New<ForwardingShipment>();
				jobDec.JE_JS = shipment.PK;
				cusEntryNumCount = Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName);
				strategy = new BaseJobDeclarationFetchStrategy(jobDec);
				strategy.FetchForView(new TableColumn[] { tc8 });
				AssertEquals("Active Hint", @"AccTransactionHeader
CusContainer
CusContainerInvoiceLinePivot
CusDecHouseBill
CusDecHouseContainerPack
CusEntryHeader
CusEntryInstruction
CusEntryNum
CusEquipment
CusHouseContPackInvoiceHeaderPivot
CusHouseContPackInvoiceLinePivot
CusUnderbondDec
DtbBookingConsolidation
EDIMessage
GenAddOnColumn
GenCustomAddOnRuleAck
GenCustomAddOnValue
GlbBranch
JobCartage
JobComInvLineComponentInventory
JobComInvLineRefs
JobComInvoiceHeader
JobComInvoiceHeaderRefs
JobComInvoiceLine
JobComInvoiceLineTax
JobConsolTransport
JobDecRefs
JobDocAddress
JobHeader
JobOrderHeader
OrgCompanyData
ProcessHeader
ProcessTaskNotification
ProcessTasks
ProcessTaskTemplate
StmNote", new ZStringBuilder(Factory.GetAllFetchHintedTableNames().OrderBy(x => x)).ToStringWithNewLineBetweenAppends());
				AssertEquals(cusEntryNumCount, Factory.ActiveFetchHintsForTable(CusEntryNumSchema.Constants.TableName));
			});
		}

		public void TestFetchForViewDoesNotAddFetchHintForAccTransactionLinesAndAccTransactionHeader()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			var job = new JobHeader.Loader(shipment).TryCreate();
			Factory.Save();

			var fetchHintsForAccTransactionLinesBefore = Factory.ActiveFetchHintsForTable(AutoAccTransactionLines.Schema.TableName);
			var fetchHintsForAccTransactionHeaderBefore = Factory.ActiveFetchHintsForTable(AutoAccTransactionHeader.Schema.TableName);

			var totalBilledAmountColumn = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.TotalBilledAmount);
			var totalInvoicedAmountColumn = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.TotalInvoicedAmount);
			var totalOutstandingAmountColumn = new TableColumn("JobDeclaration", BaseJobDeclaration.Schema.TotalOutstandingAmount);
			var strategy = new BaseJobDeclarationFetchStrategy(declaration);
			strategy.FetchForView(new TableColumn[] { totalBilledAmountColumn, totalInvoicedAmountColumn, totalOutstandingAmountColumn });

			var fetchHintsForAccTransactionLinesAfter = Factory.ActiveFetchHintsForTable(AutoAccTransactionLines.Schema.TableName);
			var fetchHintsForAccTransactionHeaderAfter = Factory.ActiveFetchHintsForTable(AutoAccTransactionHeader.Schema.TableName);

			AssertEquals(fetchHintsForAccTransactionLinesBefore, fetchHintsForAccTransactionLinesAfter);
			AssertEquals(fetchHintsForAccTransactionHeaderBefore, fetchHintsForAccTransactionHeaderAfter);
		}

		public void TestFetchHintForRegisterEditableChildObject()
		{
			var helper = new FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>();
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory).ToArray();
			var declaration = (BaseJobDeclaration)data.First(o => o.Object.GetType() == typeof(BaseJobDeclaration)).Object;
			var strategy = new BaseJobDeclarationFetchStrategy(declaration);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("BaseJobDeclaration fetch hints count", count + 13, Factory.ActiveTableFetchHints);

			var factory = new BusinessObjectFactory();
			var declarationLoaded = factory.Load<BaseJobDeclaration>(declaration.PK);
			declarationLoaded.LoadChildEditableObjects();
			var dataSet = ((INeedDataSet)declarationLoaded).Data;
			foreach (var o in data.Where(o => o.Object.PK != declarationLoaded.PK))
			{
				AssertEquals($"row factory should have table:name={o.TableName}", 1, dataSet.Tables.Cast<DataTable>().Count(t => t.TableName == o.TableName));
				AssertEquals($"{o.TableName} should have row:{o.PKColumnName}={o.Object.PK}", 1, dataSet.Tables[o.TableName].Rows.Cast<DataRow>().Count(r => new ZGuid(r[o.PKColumnName]) == o.Object.PK));
			}
		}

		public void TestFetchHintForRegisterEditableChildObject_DbHits()
		{
			var helper = new FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>();
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory).ToArray();

			Factory.ResetDatabaseLoadCount();
			using (RowFactory.SetCachedTables())
			{
				BusinessObjectFactory factory;
				var ignoreStackTraceBeforeThis = !TestingState.IsRunningOnDAT ? System.Environment.StackTrace.SplitByLine().Last() : null;
				var declaration = (BaseJobDeclaration)data.First(o => o.Object.GetType() == typeof(BaseJobDeclaration)).Object;
				var tablesToCollectQueriesFor = !TestingState.IsRunningOnDAT ? GetTablesToCollectQueriesForRegisterEditableChildObject().Union(RegisterEditableChildObjectExpectedHitCount.Keys).Distinct().ToArray() : null;
				using (AssertDbHitsForAllFactories("BaseJobDeclarationDBHitsTest", RegisterEditableChildObjectExpectedHitCount, true, false, 1, stackTraceToIgnore: ignoreStackTraceBeforeThis, tablesToCollectQueriesFor: tablesToCollectQueriesFor, logNonPersistentTableHit: true))
				{
					factory = new BusinessObjectFactory
					{
						NameForDebugging = "BaseJobDeclarationDBHitsTestFactory"
					};
					var declarationLoaded = factory.Load<BaseJobDeclaration>(declaration.PK);
					factory.ResetDatabaseLoadCount();
					declarationLoaded.LoadChildEditableObjects();
				}
				FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>.RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
			}
		}

		Dictionary<string, int> RegisterEditableChildObjectExpectedHitCount => new Dictionary<string, int>
		{
			{ CusDecHouseBillSchema.Constants.TableName, 1 },
			{ CusDecHouseContainerPackSchema.Constants.TableName, 1 },
			{ CusEntryLineSchema.Constants.TableName, 2 },
			{ JobComInvoiceLineSchema.Constants.TableName, 1 },
			{ JobComInvoiceHeaderSchema.Constants.TableName, 1 },
			{ JobConsolTransportSchema.Constants.TableName, 2 },
			{ ProcessTasksSchema.Constants.TableName, 2 },
		};

		public IEnumerable<string> GetTablesToCollectQueriesForRegisterEditableChildObject()
		{
			var result = FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader,
					BaseJobComInvoiceLine>.GetCommonTablesToCollectQueriesForRegisterEditableChildObject();

			var jobDeclaration = Factory.New<BaseJobDeclaration>();
			var strategy = new BaseJobDeclarationFetchStrategy(jobDeclaration);
			if (jobDeclaration.SupportDeclarationRefs)
			{
				result.Add(JobDecRefsSchema.Constants.TableName);
			}

			if (strategy.SupportCusAddInfo<ICusAddInfoTypeSupporter>(jobDeclaration.Invoices))
			{
				result.Add(CusAddInfoSchema.Constants.TableName);
			}

			if (strategy.SupportCusAddInfo<ICusCodeDataTypeSupporter>(jobDeclaration.Invoices))
			{
				result.Add(CusCodeDataSchema.Constants.TableName);
			}

			if (!jobDeclaration.CustomsEntryInstructionProvider.IsNoEntryInstruction)
			{
				result.Add(CusEntryInstructionSchema.Constants.TableName);
			}

			if (jobDeclaration.SupportMultipleWarehouseEntry)
			{
				result.Add(CusEntryInstructionSchema.Constants.TableName);
			}

			if (jobDeclaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				result.Add(CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName);
			}

			if (jobDeclaration.SupportInvoiceLineRefs)
			{
				result.Add(JobComInvLineRefsSchema.Constants.TableName);
			}

			if (jobDeclaration.SupportsJobComInvoiceLineTax)
			{
				result.Add(JobComInvoiceLineTaxSchema.Constants.TableName);
			}

			if (jobDeclaration.SupportsChcPivotBetweenInvoiceLineAndPacking)
			{
				result.Add(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName);
			}

			result.Add(CusContainerInvoiceLinePivotSchema.Constants.TableName);

			return result;
		}

		class JobDeclarationForTest : BaseJobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override bool SupportsChzPivotBetweenInvoiceHeaderAndPackingCore => true;
			protected internal override bool SupportDeclarationRefs => true;
			protected override bool SupportsChcPivotBetweenInvoiceLineAndPackingCore => true;
			protected internal override bool SupportMultipleWarehouseEntryCore => true;
			protected override bool SupportsJobComInvoiceLineTaxCore => true;
			public override bool SupportInvoiceLineRefs => true;
		}
	}
}
