using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.FetchStrategies.Testing
{
	public class JobComInvoiceLineFetchStrategyTest : BusinessObjectFetchStrategyTestCase
	{
		public void TestFetchForValidateCore()
		{
			CustomsDataRegistry.Instance.DefaultCurrencyToLocalCurrency.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			CombineAssertions(() =>
			{
				string ignoreStackTraceBeforeThis = !TestingState.IsRunningOnDAT ? System.Environment.StackTrace.SplitByLine().Last() : null;
				foreach (var testCase in SetupFetchForValidateTestCases())
				{
					Factory.Save();
					Factory.ResetDatabaseLoadCount();

					using (RowFactory.SetCachedTables())
					{
						BusinessObjectFactory factory = null;
						var tablesToCollectQueriesFor = !TestingState.IsRunningOnDAT ? GetTablesToCollectQueriesFor().Union(testCase.ExpectedHitCounts.Keys).Distinct().ToArray() : null;
						using (AssertDbHitsForAllFactories(testCase.Message, testCase.ExpectedHitCounts, true, false, 1, includeFactoryPredicate: f => f.NameForDebugging.Contains(testCase.Message), stackTraceToIgnore: ignoreStackTraceBeforeThis, tablesToCollectQueriesFor: tablesToCollectQueriesFor, logNonPersistentTableHit: true))
						{
							factory = new BusinessObjectFactory()
							{
								NameForDebugging = testCase.Message
							};
							var declaration = factory.Load<BaseJobDeclaration>(testCase.DeclarationPK);
							var invoiceLines = declaration.InvoiceLines;
							factory.ClearLoadedFetchHintCountForTable(JobComInvoiceLineSchema.Constants.TableName);
							((IBusiness)invoiceLines).RunPreSaveValidationFetch(false);
							invoiceLines.RunPreSaveValidation();
						}
						FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>.RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
					}
				}
			});
		}

		protected virtual IEnumerable<string> GetTablesToCollectQueriesFor()
		{
			return new[]
			{
				CusAddInfoSchema.Constants.TableName,
				CusCodeDataSchema.Constants.TableName,
				JobComInvoiceLineTaxSchema.Constants.TableName,
				JobComInvLineRefsSchema.Constants.TableName,
				TariffViewSchema.Constants.TableName,
				RateViewSchema.Constants.TableName,
				TariffAttributeViewSchema.Constants.TableName,
				TariffAdditionalCodeViewSchema.Constants.TableName,
				TariffRelationshipViewSchema.Constants.TableName,
				ZZRefCusCodeListCombinedSchema.Constants.TableName,
				ZZRefCusCodeListAttributeCombinedSchema.Constants.TableName,
				ZZRefCarrierCombinedSchema.Constants.TableName,
				ZZRefCarrierAttributeCombinedSchema.Constants.TableName
			};
		}

		public void TestFetchHintForRegisterEditableChildObject()
		{
			var helper = new FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>();
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory, false, false).ToArray();
			var invoiceLine = (BaseJobComInvoiceLine)data.First(o => o.Object is BaseJobComInvoiceLine).Object;
			var strategy = new JobComInvoiceLineFetchStrategy(invoiceLine);
			var count = Factory.ActiveTableFetchHints;
			strategy.FetchForLoadChildEditableObjects();
			AssertEquals("BaseJobDeclaration fetch hints count", count + FetchHintsIncrementCount, Factory.ActiveTableFetchHints);

			var factory = new BusinessObjectFactory();
			var invoiceLineLoaded = factory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
			invoiceLineLoaded.LoadChildEditableObjects();
			var dataSet = ((INeedDataSet)invoiceLineLoaded).Data;
			foreach (var o in data.Where(o => o.Object.PK != invoiceLineLoaded.PK))
			{
				AssertEquals($"row factory should have table:name={o.TableName}", 1, dataSet.Tables.Cast<DataTable>().Count(t => t.TableName == o.TableName));
				AssertEquals($"{o.TableName} should have row:{o.PKColumnName}={o.Object.PK}", 1, dataSet.Tables[o.TableName].Rows.Cast<DataRow>().Count(r => new ZGuid(r[o.PKColumnName]) == o.Object.PK));
			}
		}

		protected virtual int FetchHintsIncrementCount => 2;

		public void TestFetchHintForRegisterEditableChildObject_DbHits()
		{
			var helper = new FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>();
			var data = helper.SetupFetchHintForRegisterEditableChildObjectTestCase(Factory, false, false).ToArray();
			var invoiceLine = (BaseJobComInvoiceLine)data.First(o => o.Object is BaseJobComInvoiceLine).Object;

			Factory.ResetDatabaseLoadCount();

			using (RowFactory.SetCachedTables())
			{
				BusinessObjectFactory factory;
				var ignoreStackTraceBeforeThis = !TestingState.IsRunningOnDAT ? System.Environment.StackTrace.SplitByLine().Last() : null;
				var tablesToCollectQueriesFor = !TestingState.IsRunningOnDAT ? GetTablesToCollectQueriesForRegisterEditableChildObject(invoiceLine).Union(RegisterEditableChildObjectExpectedHitCount.Keys).Distinct().ToArray() : null;
				var factoryName = "BaseJobComInvoiceHeaderDBHitsTestFactory";
				using (AssertDbHitsForAllFactories("BaseJobComInvoiceHeaderDBHitsTest", RegisterEditableChildObjectExpectedHitCount, true, false, 1, includeFactoryPredicate: f => f.NameForDebugging.Contains(factoryName), stackTraceToIgnore: ignoreStackTraceBeforeThis, tablesToCollectQueriesFor: tablesToCollectQueriesFor, logNonPersistentTableHit: true))
				{
					factory = new BusinessObjectFactory()
					{
						NameForDebugging = factoryName
					};
					var invoiceReloaded = factory.Load<BaseJobComInvoiceLine>(invoiceLine.PK);
					factory.ResetDatabaseLoadCount();
					invoiceReloaded.LoadChildEditableObjects();
				}
				FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>.RemoveFactoryFromPersistentFactoryCacheManagerAndResetRegistryFactory(factory);
			}
		}

		protected virtual Dictionary<string, int> RegisterEditableChildObjectExpectedHitCount => new Dictionary<string, int>
		{
		};

		protected virtual IEnumerable<string> GetTablesToCollectQueriesForRegisterEditableChildObject(BaseJobComInvoiceLine invoiceLine)
		{
			var result = FetchForRegisterEditableChildObjectTestHelper<BaseJobDeclaration, BaseJobComInvoiceHeader, BaseJobComInvoiceLine>.GetCommonTablesToCollectQueriesForRegisterEditableChildObject();
			if (invoiceLine.SupportRulingConfigurations)
			{
				result.Add(CusRulingConfigCombinedSchema.Constants.TableName);
			}

			if (invoiceLine.SupportsAdditionalTariffs)
			{
				result.Add(CusLineTariffDetailSchema.Constants.TableName);
			}

			if (!(invoiceLine.InvoiceHeader?.HasFetchForLoadChildEditableObjectsBeenCalled ?? false) &&
				!(invoiceLine.Declaration?.HasFetchForLoadChildEditableObjectsBeenCalled ?? false))
			{
				result.Add(JobComInvLineComponentInventorySchema.Constants.TableName);

				if (invoiceLine.SupportInvoiceLineRefs)
				{
					result.Add(JobComInvLineRefsSchema.Constants.TableName);
				}

				if (!invoiceLine.ContainersPivotIsLoaded)
				{
					result.Add(CusContainerInvoiceLinePivotSchema.Constants.TableName);
				}

				if (invoiceLine.SupportsChcPivotBetweenInvoiceLineAndPacking)
				{
					result.Add(CusHouseContPackInvoiceLinePivotSchema.Constants.TableName);
				}

				if (invoiceLine.Declaration?.SupportsJobComInvoiceLineTax ?? false)
				{
					result.Add(JobComInvoiceLineTaxSchema.Constants.TableName);
				}
			}

			if (invoiceLine.JI_ParentID.IsValid)
			{
				result.Add(JobComInvoiceLineSchema.Constants.TableName);
			}

			return result;
		}

		protected virtual IEnumerable<TestCase> SetupFetchForValidateTestCases()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLines = new List<BaseJobComInvoiceLine>();
			for (var i = 0; i < 2; i++)
			{
				invoice.JobComInvoiceLines.Add(Factory.NewWithValidTestData<BaseJobComInvoiceLine>());
			}
			yield return new TestCase
			{
				Message = "Base Standalone Invoice Line",
				DeclarationPK = declaration.PK,
				ExpectedHitCounts = new Dictionary<string, int>
				{
					{ JobComInvoiceLineSchema.Constants.TableName, 1 },
					{ JobComInvoiceHeaderSchema.Constants.TableName, 1 }
				}
			};
		}

		public void TestFetchForView()
		{
			var invoice1 = Master.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew();

			var invoice2 = Master.Invoices.AddNew();
			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.Charges.AddNew();
			Factory.Save();

			TestFetchForView(invoiceLine1, invoiceLine2);
		}

		protected override IBusinessObjectCollection CreateCollectionToTest(BusinessObjectFactory factory)
		{
			var parent = factory.Load<BaseJobDeclaration>(Master.PK);
			return new InvoiceLineViewCollection<BaseJobComInvoiceLine>(parent).CollectionToFilter;
		}

		BaseJobDeclaration Master
		{
			get
			{
				return master ?? (master = Factory.New<BaseJobDeclaration>());
			}
		}
		BaseJobDeclaration master;

		public class TestCase
		{
			public string Message { get; set; }
			public ZGuid DeclarationPK { get; set; }
			public IDictionary<string, int> ExpectedHitCounts { get; set; }
		}
	}
}
