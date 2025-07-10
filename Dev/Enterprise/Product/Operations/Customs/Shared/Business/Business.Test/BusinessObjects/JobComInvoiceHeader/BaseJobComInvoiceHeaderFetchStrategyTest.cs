using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	class BaseJobComInvoiceHeaderFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForValidate_NotUseUniversalConditionCheck()
		{
			AssertCacheConditionForFetchForValidation(false, "HSN");
		}

		public void TestFetchForValidate_UseUniversalConditionCheck()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				AssertCacheConditionForFetchForValidation(true, "IMP");
			}
		}

		public void TestFetchForLoad()
		{
			var invoiceHeader = Factory.New<BaseJobComInvoiceHeader>();
			invoiceHeader.JZ_JE = ZGuid.NewZGuid();
			var strategy = new BaseJobComInvoiceHeaderFetchStrategy(invoiceHeader);
			int count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoad();
			AssertEquals("hint has been added", count + 1, Factory.ActiveTableFetchHints);
		}

		public void TestFetchForLoadChildEditableObjects()
		{
			var dec = Factory.New<BaseJobDeclaration>();
			AddInvoice(dec);
			AddInvoice(dec);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			dec = newFactory.Load<BaseJobDeclaration>(dec.PK);
			dec.Transports.Load(); // causes transport count from dec to be ignored
			dec.WorkflowItems.Load(); // causes WorkflowItems count from dec to be ignored
			newFactory.ResetDatabaseLoadCount();
			dec.LoadChildEditableObjects();
			AssertInvoiceData(dec.Invoices[0]);
			AssertInvoiceData(dec.Invoices[1]);
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for JobConsolTransport", 1, newFactory.GetTableHitCount(JobConsolTransportSchema.Constants.TableName));
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for JobComInvoiceHeaderRefs", 1, newFactory.GetTableHitCount(JobComInvoiceHeaderRefsSchema.Constants.TableName));
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for ProcessTask", 1, newFactory.GetTableHitCount(ProcessTasksSchema.Constants.TableName));
		}

		public void TestFetchForView()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.Charges.AddNew();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.Charges.AddNew();

			var declaration2 = BaseJobDeclaration.New(Factory);
			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.Charges.AddNew();
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.Charges.AddNew();
			Factory.Save();

			TestFetchForView(invoice1, invoice2);
		}

		public void TestFetchHintForRegisterEditableChildObject()
		{
			var helper = new FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>();
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory, false).ToArray();
			var invoiceHeader = (BaseJobComInvoiceHeader)data.First(o => o.Object.GetType() == typeof(BaseJobComInvoiceHeader)).Object;
			var strategy = new BaseJobComInvoiceHeaderFetchStrategy(invoiceHeader);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("BaseJobDeclaration fetch hints count", count, Factory.ActiveTableFetchHints);

			var factory = new BusinessObjectFactory();
			var declarationLoaded = factory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
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
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory);
			var invoiceHeader = (BaseJobComInvoiceHeader)data.First(o => o.Object.GetType() == typeof(BaseJobComInvoiceHeader)).Object;

			Factory.ResetDatabaseLoadCount();

			using (RowFactory.SetCachedTables())
			{
				BusinessObjectFactory factory;
				var ignoreStackTraceBeforeThis = !TestingState.IsRunningOnDAT ? System.Environment.StackTrace.SplitByLine().Last() : null;
				var tablesToCollectQueriesFor = !TestingState.IsRunningOnDAT ? GetTablesToCollectQueriesForRegisterEditableChildObject(invoiceHeader).Union(RegisterEditableChildObjectExpectedHitCount.Keys).Distinct().ToArray() : null;
				using (AssertDbHitsForAllFactories("BaseJobComInvoiceHeaderDBHitsTest", RegisterEditableChildObjectExpectedHitCount, true, false, 1, stackTraceToIgnore: ignoreStackTraceBeforeThis, tablesToCollectQueriesFor: tablesToCollectQueriesFor, logNonPersistentTableHit: true))
				{
					factory = new BusinessObjectFactory()
					{
						NameForDebugging = "BaseJobComInvoiceHeaderDBHitsTestFactory"
					};
					var invoiceHeaderLoaded = factory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
					factory.ResetDatabaseLoadCount();
					invoiceHeaderLoaded.LoadChildEditableObjects();
				}
				FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>.RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
			}
		}

		Dictionary<string, int> RegisterEditableChildObjectExpectedHitCount =>
			new Dictionary<string, int>
			{
				{ JobComInvHeaderChargeSchema.Constants.TableName, 2 },
				{ ProcessTasksSchema.Constants.TableName, 2 },
				{ JobComInvoiceLineSchema.Constants.TableName, 1 }
			};

		protected virtual IEnumerable<string> GetTablesToCollectQueriesForRegisterEditableChildObject(BaseJobComInvoiceHeader invoiceHeader)
		{
			var result = FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader,
					BaseJobComInvoiceLine>.GetCommonTablesToCollectQueriesForRegisterEditableChildObject();

			if (invoiceHeader.SupportsChzPivotBetweenInvoiceHeaderAndPacking)
			{
				result.Add(CusHouseContPackInvoiceHeaderPivotSchema.Constants.TableName);
			}

			return result;
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory) => new CommercialInvoiceCollection(factory);

		void AssertInvoiceData(BaseJobComInvoiceHeader invoice)
		{
			AssertEquals("Were deleted because of non-standalone invoice", 0, invoice.Transports.Count);
			AssertEquals(2, invoice.InvoiceHeaderRefs.Count);
			AssertEquals(2, invoice.WorkflowItems.Count);
		}

		void AssertCacheConditionForFetchForValidation(bool useUniversalConditionCheck, string universalTariffType)
		{
			var (condition1, condition2) = CreateReferenceData(universalTariffType);

			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "111";
			invoiceLine1.JI_PrimaryPreference = "P1";
			invoiceLine1.JI_CountryOfOrigin = "ZA";
			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "222";
			invoiceLine2.JI_PrimaryPreference = "P1";
			invoiceLine2.JI_CountryOfOrigin = "ZA";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var invoiceHeaderInNewFactory = newFactory.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
			var strategy = new BaseJobComInvoiceHeaderFetchStrategy(invoiceHeaderInNewFactory);

			var query = new ZQuery();
			query.AddToFilter(RefCusConditionSchema.PK, new ZGuid[] { condition1.PK, condition2.PK });
			query.FetchOnlyFromLocalCache = true;

			CombineAssertions(() =>
			{
				AssertEquals("Precondition: not cached yet", 0, newFactory.Load<Universal.RefCusCondition>(query).Length);
				strategy.FetchForValidate();

				var conditions = newFactory.Load<Universal.RefCusCondition>(query);
				AssertEquals("After FetchForValidate: cached", useUniversalConditionCheck ? 2 : 0, conditions.Length);

				if (useUniversalConditionCheck)
				{
					AssertContainsExactElementsInAnyOrder("Conditions loaded and cached",
						new ZGuid[] { condition1.PK, condition2.PK }, conditions.Select(x => x.PK));
				}
			});
		}

		(Universal.RefCusCondition, Universal.RefCusCondition) CreateReferenceData(string universalTariffType)
		{
			var dataGrouping = GlbCompany.CurrentCompany.Country.Code;
			var startDate = ZDateTime.Today.AddYears(-1);
			var endDate = ZDateTime.Today.AddYears(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingDataGrouping(dataGrouping);
			var tariffType = helper.CreateTariffType(dataGrouping, universalTariffType);
			var stdTradeGroup = helper.CreateTradeGroup(dataGrouping, "STANDARD", startDate, endDate);
			var conditionType1 = helper.CreateOrGetExistingRefCusConditionType(dataGrouping, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Rate, "TSTR1", "Test Rate Condition Type 1");
			var preference1 = helper.CreatePreferenceForCountry("P1", "TestPreference1", "EUN");
			Factory.Save();

			var tariff = helper.CreateTariff(dataGrouping, tariffType.PK, "111", startDate, endDate);
			var tariff2 = helper.CreateTariff(dataGrouping, tariffType.PK, "222", startDate, endDate);
			helper.AddCountry(stdTradeGroup, "ZA");

			var condition1 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType1.PK, tariff.PK, "C1", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicability(condition1, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			var condition2 = helper.CreateOrGetExistingRefCusCondition(dataGrouping, conditionType1.PK, tariff2.PK, "C2", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, preferencePK: preference1.PK);
			helper.CreateCusApplicability(condition2, stdTradeGroup, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, additionalCode: "", orderNumber: "");
			Factory.Save();

			return (condition1, condition2);
		}

		BaseJobComInvoiceHeader AddInvoice(BaseJobDeclaration dec)
		{
			var invoice = dec.Invoices.AddNew();
			var transport1 = invoice.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "AUSYD";
			var transport2 = invoice.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";

			var ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNT002";

			var ref12 = invoice.InvoiceHeaderRefs.AddNew();
			ref12.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.CN;
			ref12.J2_ReferenceNumber = "CNT007";

			var workItem1 = invoice.WorkflowItems.AddNew();
			workItem1.P9_Description = "W1";

			var workItem2 = invoice.WorkflowItems.AddNew();
			workItem2.P9_Description = "W2";
			return invoice;
		}
	}
}
