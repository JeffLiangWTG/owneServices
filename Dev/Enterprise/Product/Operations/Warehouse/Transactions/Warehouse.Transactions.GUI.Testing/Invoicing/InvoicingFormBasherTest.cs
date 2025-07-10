using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Cartonisation.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(InvoicingForm))]
	public class InvoicingFormBasherTest : ZFormBasherTest
	{
		public void TestAutorate_AdditionalJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-20), ZDate.Empty);
			Helper.CreateRateLine(rateEntry, chargeCode, "", 10m, FlatCalculator.Code);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1000m);

			var today = ZDateTime.Today;
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "ORDER1", today.AddDays(-10).ToOffset(), data.Part1, 10m);

			AssertNull("Precondition: WhsOrder has no JobHeader", order.JobHeader);

			Factory.Save();

			var pick = Helper.CreatePickNew(new[] { order });
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			var cartonisationMock = new Mock<ICartonisation>();
			using (ObjectFactory.Substitute(cartonisationMock.Object))
			{
				pick.AllocatePackageLabels();
			}

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-12), today.AddDays(-6));
			Factory.Save();
			AssertAutorate_AdditionalJobs(invoice, new string[] { "OHAN => 10.00" }, "Autorate should have OHAN charge for WhsOrder");

			var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
			AssertAutorate_AdditionalJobs(invoiceInOtherFactory, new string[] { "OHAN => 10.00" }, "Re-autorate should give same result as before i.e autorate should have OHAN charge for WhsOrder");
			cartonisationMock.VerifyAll();
		}

		static void AssertAutorate_AdditionalJobs(WhsInvoice invoice, string[] expectedCharges, string message)
		{
			using (var form = new InvoicingForm(invoice))
			{
				form.Show();

				var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
				var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
				menuItem.PerformClick();

				AssertContainsExactElementsInAnyOrder
				(
					message,
					expectedCharges,
					((WhsInvoice)form.BusinessEntity).JobHeader.Charges.Select(charge => $"{charge.ChargeCode.AC_Code} => {charge.JR_OSSellAmt}")
				);
			}
		}

		#region TestPerformance

		public void TestPerformance_Autorate()
		{
			TestPerformance_SetupAutoratingFor10OrdersWith3PackagesEach((invoice, orders) =>
			{
				// create existing "CAR" events to trigger StmALog DB Hits.
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
					menuItem.PerformClick();

					Factory.Save();
				}

				foreach (var order in orders)
				{
					((Job)order.JobHeader).Charges.DeleteAll();
				}
				Factory.Save();

				var expectedHits = new Dictionary<string, int>();
				expectedHits.Add(GenAddOnColumnSchema.Constants.TableName, 0);
				expectedHits.Add(JobHeaderSchema.Constants.TableName, 6);
				expectedHits.Add(PkgPackageSchema.Constants.TableName, 1);
				expectedHits.Add(PkgPackageJobSchema.Constants.TableName, 1);
				expectedHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedHits.Add(StmALogSchema.Constants.TableName, 1);
				expectedHits.Add(WhsLoadPkgPackagePivotSchema.Constants.TableName, 1);
				expectedHits.Add(WhsPickByLabelLabelSchema.Constants.TableName, 0);
				expectedHits.Add(WhsPickTrolleySlotSchema.Constants.TableName, 0);

				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
				using (var form = new InvoicingForm(invoiceInOtherFactory))
				{
					using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
					{
						form.Show();

						var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
						var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
						menuItem.PerformClick();
					}
				}
			});
		}

		[TestDate(2019, 05, 05)]
		public void TestTimeComplexity_Autorate()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-20), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, "PK"); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			var inv1NumberOfOrders = 10;
			var inv2NumberOfOrders = 100;

			var refDate1 = ZDate.Today.AddDays(-12);
			var refDate2 = ZDate.Today.AddDays(-5);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, (inv1NumberOfOrders + inv2NumberOfOrders) * 25m);

			var orders1 = new WhsOrder[inv1NumberOfOrders];
			var orders2 = new WhsOrder[inv2NumberOfOrders];

			for (int i = 0; i < inv1NumberOfOrders; i++)
			{
				orders1[i] = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "S" + i, refDate1.ToZDateTime().ToOffset(), data.Part1, 25m);
				AssertNotNull(Helper.CreateJobHeader(orders1[i]));
			}

			for (int i = 0; i < inv2NumberOfOrders; i++)
			{
				orders2[i] = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "L" + i, refDate2.ToZDateTime().ToOffset(), data.Part1, 25m);
				AssertNotNull(Helper.CreateJobHeader(orders2[i]));
			}

			Factory.Save();

			var allOrders = new List<WhsOrder>(orders1);
			allOrders.AddRange(orders2);

			var pick = Helper.CreatePickNew(allOrders.ToArray());
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			var mock = MockAndCallAllocatePackageLabels(pick);
			foreach (var order in allOrders)
			{
				var packageJob = order.PackageJob;
				var packages = packageJob.Packages;
				AssertEquals("Precondition - should create 2x cases and 1x split case.", 3, packages.Count);
				AssertEquals("Precondition", 2, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
				AssertEquals("Precondition", 1, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton));
			}

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			var invoice1 = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, refDate1.AddDays(-2), refDate1.AddDays(4));
			var invoice2 = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, refDate2.AddDays(-2), refDate2.AddDays(4));
			Factory.Save();

			var otherFactory1 = new BusinessObjectFactory() { RefreshEnabled = false };
			var otherFactory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var inv1 = otherFactory1.Load<WhsInvoice>(invoice1.PK);
			var inv2 = otherFactory2.Load<WhsInvoice>(invoice2.PK);

			double measureAutoratingPerformance(WhsInvoice invoice, int numberOfChargesExpected)
			{
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");

					var sw = new Stopwatch();
					sw.Start();
					menuItem.PerformClick();
					sw.Stop();

					AssertEquals(numberOfChargesExpected, invoice.JobHeader.Charges.Count);
					return Convert.ToDouble(sw.ElapsedMilliseconds);
				}
			}

			var inv1Autorate_ms = measureAutoratingPerformance(inv1, inv1NumberOfOrders);
			var inv2Autorate_ms = measureAutoratingPerformance(inv2, inv2NumberOfOrders);

			var inv1TimePerOrder = inv1Autorate_ms / Convert.ToDouble(inv1NumberOfOrders);
			var inv2TimePerOrder = inv2Autorate_ms / Convert.ToDouble(inv2NumberOfOrders);

			Assert($@"Performance per order should improve if we autorate larger batch of orders/charges.
Small batch time per order, ms: {inv1TimePerOrder}
Large batch time per order, ms: {inv2TimePerOrder}", inv1TimePerOrder > inv2TimePerOrder);
			mock.VerifyAll();
		}

		public void TestPerformance_ReAutorate()
		{
			TestPerformance_SetupAutoratingFor10OrdersWith3PackagesEach((invoice, orders) =>
			{
				// create existing "CAR" events to trigger StmALog DB Hits.
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
					menuItem.PerformClick();
					Factory.Save();
				}

				var expectedHits = new Dictionary<string, int>();
				expectedHits.Add(GenAddOnColumnSchema.Constants.TableName, 0);
				expectedHits.Add(JobPaymentBasisSchema.Constants.TableName, 1);
				expectedHits.Add(JobHeaderSchema.Constants.TableName, 6);
				expectedHits.Add(PkgPackageSchema.Constants.TableName, 1);
				expectedHits.Add(WhsLoadPkgPackagePivotSchema.Constants.TableName, 1);
				expectedHits.Add(ProcessTasksSchema.Constants.TableName, 1);
				expectedHits.Add(StmALogSchema.Constants.TableName, 1);
				expectedHits.Add(StmDocDataOverrideSchema.Constants.TableName, 4);
				expectedHits.Add(StmNoteSchema.Constants.TableName, 3);
				expectedHits.Add(StmUniversalCopySchema.Constants.TableName, 2);
				expectedHits.Add(WhsPickByLabelLabelSchema.Constants.TableName, 0);
				expectedHits.Add(WhsPickTrolleySlotSchema.Constants.TableName, 0);

				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
				using (var form = new InvoicingForm(invoiceInOtherFactory))
				{
					using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
					{
						form.Show();

						var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
						var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
						menuItem.PerformClick();
					}
				}
			});
		}

		public void TestPerformance_Save()
		{
			// need to have any triggers with conditional filter to reproduce DB hits to ProcessTaskNotification
			var warehouseReleaseTemplate = Helper.CreateWorkflowTemplate("234", WorkflowDescriptors.WhsOrderWorkflowDescriptorCode);
			var warehouseReleaseTrigger = Helper.CreateWorkflowTrigger(warehouseReleaseTemplate.WorkflowItems, "TR2", 1, AutoEvents.DataImportCode);
			((ITemplateConditional)warehouseReleaseTrigger).TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			((ITemplateConditional)warehouseReleaseTrigger).TemplateCondition2Value = "\"1\"==\"1\"";
			Helper.CreateWorkflowNotification(warehouseReleaseTrigger, WorkflowTriggerActionTypeConstants.Codes.SetField, fieldName: "WD_PackingAfterPickingRequired", fieldValue: "true");
			Factory.Save();

			TestPerformance_SetupAutoratingFor10OrdersWith3PackagesEach((invoice, orders) =>
			{
				// create existing "CAR" events to trigger StmALog DB Hits.
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
					menuItem.PerformClick();
					Factory.Save();
				}

				foreach (var order in orders)
				{
					((Job)order.JobHeader).Charges.DeleteAll();
				}
				Factory.Save();

				// autorate job to be saved
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
				using (var form = new InvoicingForm(invoiceInOtherFactory))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
					menuItem.PerformClick();

					var expectedHits = new Dictionary<string, int>();
					expectedHits.Add(AccTransactionLinesSchema.Constants.TableName, 1);
					expectedHits.Add(GenAddOnColumnSchema.Constants.TableName, 1);
					expectedHits.Add(JobHeaderSchema.Constants.TableName, 7);
					expectedHits.Add(PkgPackageSchema.Constants.TableName, 2);
					expectedHits.Add(WhsLoadPkgPackagePivotSchema.Constants.TableName, 1);
					expectedHits.Add(ProcessTaskNotificationSchema.Constants.TableName, 1);
					expectedHits.Add(ProcessTasksSchema.Constants.TableName, 2);
					expectedHits.Add(StmALogSchema.Constants.TableName, 1);
					expectedHits.Add(WhsPickByLabelLabelSchema.Constants.TableName, 0);
					expectedHits.Add(WhsPickTrolleySlotSchema.Constants.TableName, 0);

					using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
					{
						form.FireSaveButton();
					}
				}
			});
		}

		public void TestPerformance_Post()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			AccountingConfigurationRegistry.Instance.PeriodicBillingChargePostingPerformanceImprovementConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5); // ensure that "optimisation" is enabled for 10 orders

			TestPerformance_SetupAutoratingFor10OrdersWith3PackagesEach((invoice, orders) =>
			{
				// make client a debtor to allow posting
				invoice.Client.OH_IsDebtor = true;

				// autorate to have stuff to post
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Autorate Costs and Revenue");
					menuItem.PerformClick();

					// add currency exchanges to trigger currency convertions
					foreach (var order in orders)
					{
						((Job)order.JobHeader).Charges.Cast<Charge>().ForEach(charge =>
						{
							charge.JR_RX_NKSellCurrency = "USD";
							charge.JR_OSSellExRate = 1;
						});
					}

					Factory.Save();
				}

				// autorate job to be saved
				var otherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);

				using (var form = new InvoicingForm(invoiceInOtherFactory))
				{
					form.Show();

					var expectedHitsForAllFactory = new Dictionary<string, int>
					{
						{ AccAllowedBranchDepartmentComboSchema.Constants.TableName, 1 },
						{ AccChargeCodeSchema.Constants.TableName, 1 },
						{ AccChargeGLPostingOverrideSchema.Constants.TableName, 4 },
						{ AccChargeRevRecOverrideSchema.Constants.TableName, 1 },
						{ AccChargeTaxOverrideSchema.Constants.TableName, 1 },
						{ AccChargeTypeOverrideSchema.Constants.TableName, 2 },
						{ AccGLHeaderSchema.Constants.TableName, 1 },
						{ AccGLHeaderSubAccountSchema.Constants.TableName, 1 },
						{ AccPeriodManagementSchema.Constants.TableName, 2 },
						{ AccSurchargeApplicationSchema.Constants.TableName, 1 },
						{ AccTaxConfigurationSchema.Constants.TableName, 1 },
						{ AccTaxRateSchema.Constants.TableName, 1 },
						{ AccTransactionHeaderSchema.Constants.TableName, 2 },
						{ AccTransactionLinesSchema.Constants.TableName, 1 },
						{ JobCartageSchema.Constants.TableName, 1 },
						{ JobChargeRevRecognitionSchema.Constants.TableName, 1 },
						{ JobChargeTargetSchema.Constants.TableName, 1 },
						{ JobChargeSchema.Constants.TableName, 3 },
						{ JobExRateSchema.Constants.TableName, 1 },
						{ JobHeaderSchema.Constants.TableName, 6 },
						{ JobStorageSchema.Constants.TableName, 1 },
						{ StmModuleFilterSchema.Constants.TableName, 1 },
						{ OrgAddressSchema.Constants.TableName, 2 },
						{ OrgAddressCapabilitySchema.Constants.TableName, 1 },
						{ OrgARTermsSchema.Constants.TableName, 1 },
						{ OrgCommissionAgreementSchema.Constants.TableName, 1 },
						{ OrgCompanyDataSchema.Constants.TableName, 2 },
						{ OrgCusCodeSchema.Constants.TableName, 1 },
						{ OrgHeaderSchema.Constants.TableName, 3 },
						{ OrgInvoiceTypeSchema.Constants.TableName, 1 },
						{ OrgMiscServSchema.Constants.TableName, 1 },
						{ OrgRelatedPartySchema.Constants.TableName, 1 },
						{ OrgStaffAssignmentsSchema.Constants.TableName, 1 },
						{ ProcessTasksSchema.Constants.TableName, 1 },
						{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
						{ "RefDatabase_RefAccTaxRate", 2 },
						{ "RefDatabase_RefDataGrouping", 1 },
						{ StmDataSchema.Constants.TableName, 1 },
						{ ViewGenericJobSchema.Constants.TableName, 2 },
						{ WhsAdHocServiceJobSchema.Constants.TableName, 1 },
						{ WhsDocketSchema.Constants.TableName, 1 },
						{ WhsVASOrderSchema.Constants.TableName, 1 },
					};

					var expectedHits = new Dictionary<string, int>
					{
						{ AccTransactionLinesSchema.Constants.TableName, 2 },
						{ AccTaxRecordTransactionLinePivotSchema.Constants.TableName, 1 },
						{ JobChargeSchema.Constants.TableName, 3 },
						{ JobExRateSchema.Constants.TableName, 1 },
						{ JobHeaderSchema.Constants.TableName, 4 },
					};

					var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
					var menuItem = jobInvoicing.MenuItems.FindByText("Post All Charges and Costs");
					using (AssertDbHitsForAllFactories(expectedHitsForAllFactory, useOnlyNewFactories: true, tablesToCollectQueriesFor: expectedHitsForAllFactory.Keys.ToArray()))
					using (AssertDbHitsWithUsefulQueryInformation(expectedHits, otherFactory, ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
					{
						menuItem.PerformClick();
					}
				}

				var assertFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var invoiceInAssertFactory = assertFactory.Load<WhsInvoice>(invoice.PK);
				AssertEquals("All charges posted.", true, invoiceInAssertFactory.JobHeader.Charges.Cast<Charge>().All(c => c.IsRevenuePosted));
			});
		}

		void TestPerformance_SetupAutoratingFor10OrdersWith3PackagesEach(Action<WhsInvoice, WhsOrder[]> doPerformanceTest)
		{
			const int NumberOfOrdersToCreate = 10;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType = UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType = UOMPackTypesList.Codes.SplitCase;

			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);

			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres, Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;

			var chargeCode = Helper.CreateChargeCode("OHAN", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");
			var clientRate = Helper.CreateClientRate(data.Org1);
			var rateEntry = Helper.CreateRateEntry(clientRate, ZDate.Today.AddDays(-20), ZDate.Empty);
			var rateLine = rateEntry.AddRateLine(chargeCode, WarehousePackCalculator.Code, "PK"); // Package

			var calculator = (WarehousePackCalculator)rateLine.Calculator;
			calculator.AddRateLineItem(Constants.PkgUnit.Box, 0m, 3.50m); // $3.50 per Box
			calculator.AddRateLineItem(Constants.PkgUnit.Carton, 0m, 5.00m); // $5.00 per Carton

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, NumberOfOrdersToCreate * 25m);

			var today = ZDateTime.Today;
			var orders = new WhsOrder[NumberOfOrdersToCreate];
			for (int i = 0; i < NumberOfOrdersToCreate; i++)
			{
				orders[i] = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, today.AddDays(-10).ToOffset(), data.Part1, 25m);
				Helper.CreateJobHeader(orders[i]);
			}
			Factory.Save();

			var pick = Helper.CreatePickNew(orders);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();

			var mock = MockAndCallAllocatePackageLabels(pick);
			foreach (var order in orders)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Precondition - should create 2x cases and 1x split case.", 3, packages.Count);
				AssertEquals("Precondition", 2, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
				AssertEquals("Precondition", 1, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton));
			}

			pick.FinaliseAllOrders();
			pick.FinalisePick();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(pick);

			var invoice = Helper.CreateInvoice(Factory, data.Org1, data.Whs1, today.AddDays(-12), today.AddDays(-6));
			Factory.Save();

			doPerformanceTest(invoice, orders);
			mock.VerifyAll();
		}

		#endregion

		#region TestDeletionOfPeriodicInvoiceThrowsNoException

		public void TestDeletionOfPeriodicInvoiceThrowsNoException()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var periodicInvoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, new ZDateTime(2013, 1, 2), new ZDateTime(2013, 4, 1));
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.WhsInvoicing);

			using (var deleteForm = controller.ShowDeleteForm(periodicInvoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, periodicInvoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", deleteForm);
			}
		}

		#endregion

		#region TestInvoiceDeleteAllRelatedAdjustment

		public void TestInvoiceDeleteAllRelatedAdjustment()
		{
			var today = ZDateTime.Today;
			var data = new TestDataSimpleEnvironment(Factory);
			var inventoryLocation = data.Whs1.FindLocation("A");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, inventoryLocation);
			receive.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Receive should be finalised.", true, receive.IsFinalised);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1, "ADJ-1");
			var line = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -10, inventoryLocation);
			adjustment.FinaliseDocketWithoutUserConfirmation();

			data.Org1.CompanyData.OB_ARWhsStorageCalcMethod = OrgCompanyDataLookups.WarehouseSplitPeriodBilling;
			var periodicInvoice = Helper.CreateInvoiceWithJobHeader(data.Org1, data.Whs1, today.AddDays(-1), today.AddDays(5));
			AssertContainsExactElementsInAnyOrder(new WhsDocket[] { receive, adjustment },
				((IRatingSupporter)periodicInvoice).AdaptersProvider.GetAdditionalJobs());
			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.WhsInvoicing);

			using (var deleteForm = controller.ShowDeleteForm(periodicInvoice))
			{
				AssertEquals("JobHeader of Invoice connot be deleted.", false, periodicInvoice.JobHeader.CanDelete);
				AssertNull("DeleteForm should be null because job can not be delete", deleteForm);
			}
		}

		#endregion

		#region Test eDocs ViewMode

		public void TestInvoice_eDocs_EditMode()
		{
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, false ? new JobHeaderStatusList() : null);

			new AccountingPeriodTestHelper().SetupPeriods();
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var jobHeader = Helper.CreateRatingJob(receive, "WD");
			var charge = Helper.CreateJobCharge(jobHeader, TestObjectCreator.CC1, 10m);
			receive.FinaliseDocket();
			receive.WD_FinalisedDate = now.AddDays(-2);
			Factory.Save();

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, now.AddDays(-8).Date, now.AddDays(-1).Date);

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.WhsInvoicing);
			var form = controller.ShowEditForm(invoice) as InvoicingForm;

			using (form)
			{
				invoice = (WhsInvoice)form.BusinessEntity;
				AssertNotNull(invoice);

				var eDocControl = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn).UserControl as eDocsUserControl;
				AssertNotNull(eDocControl);

				var docManagerInfo = invoice.DocManagerInfo();
				AssertNotNull(docManagerInfo);

				AssertEquals($"Invoice ReadOnly should have been false", false, docManagerInfo.ReadOnly);
			}
		}

		public void TestInvoice_eDocs_ViewMode()
		{
			TestInvoice_eDocs_Delete_And_ReadOnly(ODisplayMode.ReadOnly);
		}

		public void TestInvoice_eDocs_DeleteMode()
		{
			TestInvoice_eDocs_Delete_And_ReadOnly(ODisplayMode.Delete);
		}

		void TestInvoice_eDocs_Delete_And_ReadOnly(ODisplayMode expectedDisplayMode)
		{
			var today = CargoWise.Types.ZDateTime.Today;
			var warehouse = Helper.CreateWarehouse("Whs1");
			var client = Helper.CreateClient("Client");
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(warehouse.PK, client.PK, today.AddDays(-6), today);

			Factory.Save();

			var controller = ZControllerFactory.Create(ControllerIDs.WhsInvoicing);

			var form = null as InvoicingForm;

			switch (expectedDisplayMode)
			{
				case ODisplayMode.ReadOnly:
					form = controller.ShowViewForm(invoice) as InvoicingForm;
					break;
				case ODisplayMode.Delete:
					form = controller.ShowDeleteForm(invoice) as InvoicingForm;
					break;
				default:
					break;
			}

			AssertNotNull(form);
			using (form)
			{
				invoice = (WhsInvoice)form.BusinessEntity;
				AssertNotNull(invoice);

				var docManagerInfo = invoice.DocManagerInfo();
				AssertNotNull(docManagerInfo);

				AssertEquals($"Invoice ReadOnly should have been true", true, docManagerInfo.ReadOnly);
			}
		}

		#endregion

		#region TestCustomNoteTypes

		public void TestCustomNoteTypes()
		{
			var invoice = Factory.New<WhsInvoice>();

			// Check the initial note types count
			int initialNoteTypesCount = invoice.NoteTypes.Count;

			// Create a custom note for the invoice module
			var customNoteRegistry = new CustomNoteModuleAndCountry();
			customNoteRegistry.ModuleIDName = ModuleIDs.WhsInvoicing.Name;
			customNoteRegistry.CountryCode = "ALL";

			CustomNoteTypeItem noteType = customNoteRegistry.CustomNoteTypesList.AddNew();
			noteType.IsTextOnly = true;
			noteType.DefaultVisibility = nameof(CargoWise.Definitions.StmNoteVisibility.PUB);
			noteType.NoteName = "My Custom Note";

			var customNoteTypes = new CustomNoteTypes();
			customNoteTypes.NoteModuleAndCountryList.Add(customNoteRegistry);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customNoteTypes);

			// Open the invoicing form and select the notes tab in order to fire the custom note delegate done in the datasource binding
			using (var form = new InvoicingFormForTest(invoice))
			{
				form.Show();
				form.ClickNotesTab();
				AssertEquals("The custom note is missing!", initialNoteTypesCount + 1, invoice.NoteTypes.Count);
			}
		}

		#endregion

		#region TestPostingChargesWithDifferentStatus

		[TestDate(2012, 11, 12)]
		public void TestPostingChargesWithDifferentStatus_JobStatusToInvoicedWhenFirstARInvoicePostedTurnedOff()
		{
			AssertPostingChargesWithDifferentStatus(false);
		}

		[TestDate(2012, 11, 12)]
		public void TestPostingChargesWithDifferentStatus_JobStatusToInvoicedWhenFirstARInvoicePostedTurnedOn()
		{
			AssertPostingChargesWithDifferentStatus(true);
		}

		void AssertPostingChargesWithDifferentStatus(bool changeInvoiceStatusToInvoiced)
		{
			TestObjectCreator.SetupRegistrySetJobStatusToInvoicedWhenFirstARInvoicePosted(Env.CurrentCompany.PK, !changeInvoiceStatusToInvoiced ? new JobHeaderStatusList() : null);

			new AccountingPeriodTestHelper().SetupPeriods();
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var jobHeader = Helper.CreateRatingJob(receive, "WD");
			var charge = Helper.CreateJobCharge(jobHeader, TestObjectCreator.CC1, 10m);
			receive.FinaliseDocket();
			receive.WD_FinalisedDate = now.AddDays(-2);
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, now.AddDays(-8).Date, now.AddDays(-1).Date);
			AssertEquals("Precondition: 1 charge should come from receive.", 1, invoice.JobHeader.Charges.Count);
			invoice.JobHeader.JH_Status = JobHeaderStatus.Working.Code;
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			receiveInNewFactory.JobHeader.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			newFactory.Save();

			var newFactoryForInvoice = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactoryForInvoice.Load<WhsInvoice>(invoice.PK);
			using (var form = new InvoicingFormForTest(invoiceInNewFactory))
			{
				form.Show();
				var jobInvoicing = form.Menu.MenuItems.FindByText("Job Invoicing");
				var postAllCharges = jobInvoicing.MenuItems.FindByText("Post All Charges and Costs");
				postAllCharges.PerformClick();
				invoiceInNewFactory.JobHeader.Charges.Reload(); // Posting is in another factory
				invoiceInNewFactory.JobHeader.Reload(); // Posting is in another factory

				AssertNotEquals("Please try to post these charges again. Posting has failed. One or more of the charges you are attempting to post has been modified by another user. The other users changes have been merged with yours.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Charge should be posted.", true, ((Charge)invoiceInNewFactory.JobHeader.Charges.Single()).IsRevenuePosted);
				AssertEquals("1 Posted transaction should be created.", 1, invoiceInNewFactory.JobHeader.PrintingFilter.Transactions.Count);

				var receiveLoadedInInvoicingFactory = newFactoryForInvoice.Load<WhsReceive>(receiveInNewFactory.PK);
				if (changeInvoiceStatusToInvoiced)
				{
					AssertEquals("Invoice status should be changed to Job Invoiced.", JobHeaderStatus.JobInvoiced.Code, invoiceInNewFactory.JobHeader.JH_Status);
					AssertEquals("Receive status should be changed to Job invoiced.", JobHeaderStatus.JobInvoiced.Code, receiveLoadedInInvoicingFactory.JobHeader.JH_Status);
				}
				else
				{
					AssertEquals("Receive status should not change.", JobHeaderStatus.JobReadyForCostPosting.Code, receiveLoadedInInvoicingFactory.JobHeader.JH_Status);
					AssertEquals("Invoice status should be Working.", JobHeaderStatus.Working.Code, invoiceInNewFactory.JobHeader.JH_Status);
				}
			}
		}

		#endregion

		#region TestInvoiceReadonlyByJobStatus

		public void TestInvoiceReadonlyByJobStatus()
		{
			var today = CargoWise.Types.ZDateTime.Today;
			var warehouse = Helper.CreateWarehouse("Whs1");
			var client = Helper.CreateClient("Client");
			var invoice = (WhsInvoice)Helper.CreateWhsInvoice(warehouse.PK, client.PK, today.AddDays(-6), today);
			Factory.Save();

			using (var jobHeader = new Job.Loader(invoice).TryCreateWithMutex())
			{
				jobHeader.JH_Status = JobHeaderStatus.Working.Code;

				using (var form = new InvoicingFormForTest(invoice))
				{
					var invoicingPlugin = form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
					var dropEdit = ((ZDropEdit)invoicingPlugin.UserControl.Controls.Find("JobStatusDropDownEdit", true)[0]);
					form.Show();
					AssertReadOnly("Precondition - Status is not 'CLS' form should be editable.", form, expectedReadOnly: false);

					SetStatusCodeAndTabOff(dropEdit, JobHeaderStatus.JobInvoiced.Code);
					AssertReadOnly("Status is changed form should be editable.", form, expectedReadOnly: false);

					PerformSave(form);
					AssertReadOnly("Status is not 'CLS' form should be editable.", form, expectedReadOnly: false);

					SetStatusCodeAndTabOff(dropEdit, JobHeaderStatus.Closed.Code);
					AssertReadOnly("Status is changed and has not been saved, form should be editable.", form, expectedReadOnly: false);

					PerformSave(form);
					AssertReadOnly("Status is not 'CLS', form should **not** be editable.", form, expectedReadOnly: true);

					SetStatusCodeAndTabOff(dropEdit, JobHeaderStatus.JobInvoiced.Code);
					AssertReadOnly("Status is changed form should be editable.", form, expectedReadOnly: false);
				}
			}
		}

		static void AssertReadOnly(string message, InvoicingFormForTest form, bool expectedReadOnly)
		{
			var invoice = (WhsInvoice)form.BusinessEntity;
			AssertEquals(message, expectedReadOnly, invoice.ReadOnly);
			AssertEquals(message, expectedReadOnly, invoice.ET_StorageToDateInfo.ReadOnly);
			AssertEquals(message, expectedReadOnly, invoice.ET_StorageFromDateInfo.ReadOnly);
			AssertEquals(message, expectedReadOnly, ((ZDateEdit)form.Controls.Find("FromDateEdit", true)[0]).ReadOnly);
			AssertEquals(message, expectedReadOnly, ((ZDateEdit)form.Controls.Find("ToDateEdit", true)[0]).ReadOnly);
		}

		static void SetStatusCodeAndTabOff(ZDropEdit dropEdit, string code)
		{
			dropEdit.CodeBox.Text = code;
			KeySender.PostKeyDown(dropEdit.CodeBox, dropEdit.CodeBox.Handle, Keys.Tab);
			Application.DoEvents();
		}

		static void PerformSave(InvoicingForm form)
		{
			// ValidateAndSave does not similate what the user is doing.
			AssertEquals("Precondition - form has changes button should be save", "&Save", ((IPostingButtonsProvider)form).CommandButtonApply.Text);
			((IPostingButtonsProvider)form).CommandButtonApply.PerformClick();
		}

		#endregion

		#region TestHookEvents_NullBusinessEntity

		public void TestHookEvents_NullBusinessEntity()
		{
			var invoice = Factory.New<WhsInvoice>();
			using (var formWithNullBusinessEntityOnFormShown = new InvoicingFormForTestingNullBusinessEntityOnFormShown(invoice))
			{
				formWithNullBusinessEntityOnFormShown.Show();
				AssertNoExceptionThrown("There should be no exception thrown.", Application.DoEvents);
				AssertEquals("Unable to create the invoice, please close the form and then try again", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region TestShouldNotOpenMessageInFactorySave

		[DoNotAllowNotificationDuringTransaction]
		public void TestShouldNotOpenMessageInFactorySave()
		{
			new AccountingPeriodTestHelper().SetupPeriods();
			var now = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.OH_IsDebtor = true;

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 1m, data.Whs1.DefaultLocation);
			var jobHeader = Helper.CreateRatingJob(receive, "WD");
			var charge = Helper.CreateJobCharge(jobHeader, TestObjectCreator.CC1, 10m);
			receive.FinaliseDocket();
			receive.WD_FinalisedDate = now.AddDays(-2);
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(receive);

			var invoice = Helper.CreateInvoiceWithJobHeader(Factory, data.Org1, data.Whs1, now.AddDays(-8).Date, now.AddDays(-1).Date);
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			receiveInNewFactory.JobHeader.JH_Status = JobHeaderStatus.Codes.Closed;
			newFactory.Save();

			var isOnCannotChangeStatusUserMessageCalled = false;
			var newFactoryForInvoice = new BusinessObjectFactory();
			var invoiceInNewFactory = newFactoryForInvoice.Load<WhsInvoice>(invoice.PK);
			invoiceInNewFactory.JobHeader.JH_Status = JobHeaderStatus.Codes.JobReadyForCostPosting;
			var receiveJobHeader = ((Job)newFactoryForInvoice.Load<WhsReceive>(receive.PK).JobHeader);
			receiveJobHeader.SecurityOverrideProvider = new InteractiveSecurityOverrideProvider();
			receiveJobHeader.OnCannotChangeStatusUserMessage += (s, e) => { isOnCannotChangeStatusUserMessageCalled = true; };

			Env.Security.ReopenJob.IsAllowed = false;
			AssertNoExceptionThrown("Should not try to open a messagebox (reopen receive complete job) in transcation during the factory save.", newFactoryForInvoice.Save);
			AssertEquals("It should call the massage but not in trasaction.", true, isOnCannotChangeStatusUserMessageCalled);
		}

		#endregion

		#region Implementation

		static Mock<ICartonisation> MockAndCallAllocatePackageLabels(WhsPick pick)
		{
			var cartonisationMock = new Mock<ICartonisation>();
			cartonisationMock.CallBase = false;
			cartonisationMock.Setup(m => m.CartoniseItems(It.IsAny<IEnumerable<ICartonisableItem>>(), It.IsAny<IEnumerable<ICartonDefinition>>()))
				.Returns((IEnumerable<ICartonisableItem> pickLines1, IEnumerable<ICartonDefinition> cartonSizes) =>
				{
					var pickLines = (pickLines1).Cast<WhsPickLine>().ToArray();

					var cartonisationResults = new List<IContentResult>(pickLines.Length);
					foreach (var pickLine in pickLines)
					{
						var result = new Mock<IContentResult>();
						result.CallBase = true;
						result.Setup(m => m.CartonisableItemPK).Returns(pickLine.PK.ToGuid());
						result.Setup(m => m.Quantity).Returns((decimal)pickLine.WZ_Units);

						cartonisationResults.Add(result.Object);
					}

					var cartonisationResult = new Mock<ICartonWithItems>();
					cartonisationResult.CallBase = true;
					cartonisationResult.Setup(m => m.CartonPK).Returns(cartonSizes.First().PK);
					cartonisationResult.Setup(m => m.Items).Returns(cartonisationResults);

					return new[] { cartonisationResult.Object };
				});

			using (ObjectFactory.Substitute(cartonisationMock.Object))
			{
				pick.AllocatePackageLabels();
			}
			return cartonisationMock;
		}

		protected override Form GetFormToBashCore()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.HasChanges = false;
			return new InvoicingForm(invoice);
		}

		class InvoicingFormForTest : InvoicingForm
		{
			public InvoicingFormForTest(WhsInvoice docket)
				: base(docket)
			{
				this.ControllerID = ControllerIDs.WhsInvoicing;
			}

			public void ClickNotesTab()
			{
				this.TopLevelTabControl.SelectTab(zStmNoteTabPage1);
			}
		}

		class InvoicingFormForTestingNullBusinessEntityOnFormShown : InvoicingForm
		{
			public InvoicingFormForTestingNullBusinessEntityOnFormShown(WhsInvoice docket)
				: base(docket)
			{
				this.ControllerID = ControllerIDs.WhsInvoicing;
			}

			protected override void OnShown(EventArgs e)
			{
				this.SetDataBinding(null, null);
				base.OnShown(e);
			}
		}

		#endregion

		#region TestObjectCreator

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator_cached ?? (testObjectCreator_cached = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator_cached;

		#endregion

		#region Helper

		public WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion
	}
}
