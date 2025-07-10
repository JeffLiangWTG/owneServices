using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Category = Enterprise.Rating.Business.RatingConstants.RateCategory;

namespace Enterprise.Rating.Business.Testing
{
	sealed class QuickCalculatorTests : BaseRatingIntegrationTest
	{
		[GuiTest]
		public void TestQuickCalculator_Shipment_EstimatedCostWhenSaved()
		{
			var origin = "USHIT";
			var destination = "NOSHT";

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(Category.LCL, RateMode.LCL, origin, destination, "FRT", 222);
			Helper.NewClientRate(NewClient).AddRateEntryWithFlatRateLine(Category.LCL, RateMode.LCL, origin, destination, "FRT", 111);
			Factory.Save();

			var shipment =
				CreateForwardingShipment("SEA", NewClient.PK, ZGuid.Empty, origin, destination, 50m);
			shipment.AddPackLine(1, "PLT", weight: 200);
			var jobCode = shipment.RatingAdapter.OperationalJobCode;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 111,
					JR_OSCostAmt = 222,
					JR_EstimatedRevenue = 111,
					JR_EstimatedCost = 222,
				}
			};
			AutorateAndAssert(expected, shipment, NewClient);

			Factory.Save();

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var charge = job.Charges.Cast<Charge>().First();

				CombineAssertions("Preconditions", () =>
				{
					AssertEquals("Same as cost from autorating", (ZDecimal)222, charge.JR_EstimatedCost);
					AssertEquals("Same as revenue from autorating", (ZDecimal)111, charge.JR_EstimatedRevenue);
				});

				//---------------------------------------------------------------------------------
				charge.JR_EstimatedCost = 0;
				charge.JR_EstimatedRevenue = 0;

				CombineAssertions("Saved, Not posted, Estimate 0, Manual-change => Updated", () =>
				{
					charge.JR_OSCostAmt = 555;
					AssertEquals("Estimate changes", (ZDecimal)555, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 666;
					AssertEquals("Estimate changes", (ZDecimal)666, charge.JR_EstimatedRevenue);
				});
				charge.JR_EstimatedCost = 0;
				charge.JR_EstimatedRevenue = 0;
				CombineAssertions("Saved, Not posted, Estimate 0, Quick-calculator => Updated", () =>
				{
					SetCostAndRevenue(NewClient2, charge, jobCode, costAmount: 777, revenueAmount: 888);
					AssertEquals("Estimate changes", (ZDecimal)777, charge.JR_EstimatedCost);
					AssertEquals("Estimate changes", (ZDecimal)888, charge.JR_EstimatedRevenue);
				});

				//---------------------------------------------------------------------------------
				charge.JR_EstimatedCost = 1111;
				charge.JR_EstimatedRevenue = 2222;
				CombineAssertions("Saved, Not posted, Estimate non-zero, Manual-change => fixed", () =>
				{
					charge.JR_OSCostAmt = 999;
					AssertEquals("Estimate unchanged", (ZDecimal)1111, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 999;
					AssertEquals("Estimate unchanges", (ZDecimal)2222, charge.JR_EstimatedRevenue);
				});
				charge.JR_EstimatedCost = 3333;
				charge.JR_EstimatedRevenue = 4444;
				CombineAssertions("Saved, Not posted, Estimate non-zero, Quick-calculate => fixed", () =>
				{
					SetCostAndRevenue(NewClient2, charge, jobCode, costAmount: 999, revenueAmount: 999);
					AssertEquals("Estimate unchanges", (ZDecimal)3333, charge.JR_EstimatedCost);
					AssertEquals("Estimate unchanges", (ZDecimal)4444, charge.JR_EstimatedRevenue);
				});

				//---------------------------------------------------------------------------------
				charge.JR_EstimatedCost = 5555;
				charge.JR_EstimatedRevenue = 6666;

				PostTheCharge(charge);

				CombineAssertions("Saved, Posted, Estimate 0, Manual-change => Fixed", () =>
				{
					charge.JR_OSCostAmt = 999;
					AssertEquals("Estimate changes", (ZDecimal)5555, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 999;
					AssertEquals("Estimate changes", (ZDecimal)6666, charge.JR_EstimatedRevenue);
				});
				charge.JR_EstimatedCost = 7777;
				charge.JR_EstimatedRevenue = 8888;
				CombineAssertions("Saved, Posted, Estimate 0, Quick-calculator => Fixed", () =>
				{
					SetCostAndRevenue(NewClient2, charge, jobCode, costAmount: 333, revenueAmount: 444);
					charge.JR_OSCostAmt = 999;
					AssertEquals("Estimate changes", (ZDecimal)7777, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 999;
					AssertEquals("Estimate changes", (ZDecimal)8888, charge.JR_EstimatedRevenue);
				});
			}
		}

		public void TestQuickCalculator_Shipment_EstimatedCostWhenNotSaved()
		{
			var origin = "USHIT";
			var destination = "NOSHT";

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(Category.LCL, RateMode.LCL, origin, destination, "FRT", 333);
			Helper.NewClientRate(NewClient).AddRateEntryWithFlatRateLine(Category.LCL, RateMode.LCL, origin, destination, "FRT", 666);
			Factory.Save();

			var shipment =
				CreateForwardingShipment("SEA", NewClient.PK, ZGuid.Empty, origin, destination, 50m);
			shipment.AddPackLine(1, "PLT", weight: 200);
			var jobCode = shipment.RatingAdapter.OperationalJobCode;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 666,
					JR_OSCostAmt = 333,
					JR_EstimatedCost = 333,
					JR_EstimatedRevenue = 666
				}
			};
			AutorateAndAssert(expected, shipment, NewClient);

			using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
			{
				var charge = job.Charges.Cast<Charge>().First();

				CombineAssertions("Preconditions", () =>
				{
					AssertEquals("Same as cost from autorating", (ZDecimal)333, charge.JR_EstimatedCost);
					AssertEquals("Same as revenue from autorating", (ZDecimal)666, charge.JR_EstimatedRevenue);
				});

				//---------------------------------------------------------------------------------
				CombineAssertions("No save, Not posted, Manual-change => Updated", () =>
				{
					charge.JR_OSCostAmt = 111;
					AssertEquals("Estimate updated", (ZDecimal)111, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 222;
					AssertEquals("Estimate updated", (ZDecimal)222, charge.JR_EstimatedRevenue);
				});
				CombineAssertions("No save, Not posted, Quick-calculator => Updated", () =>
				{
					SetCostAndRevenue(NewClient2, charge, jobCode, costAmount: 333, revenueAmount: 444);
					AssertEquals("Estimate updated", (ZDecimal)333, charge.JR_EstimatedCost);
					AssertEquals("Estimate updated", (ZDecimal)444, charge.JR_EstimatedRevenue);
				});

				//---------------------------------------------------------------------------------
				charge.JR_EstimatedCost = 777;
				charge.JR_EstimatedRevenue = 777;
				PostTheCharge(charge);

				CombineAssertions("No save, Posted, Manual-change => Fixed", () =>
				{
					charge.JR_OSCostAmt = 111;
					AssertEquals("Estimate fixed", (ZDecimal)777, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 222;
					AssertEquals("Posted revenue can still have estimate updated.", (ZDecimal)222, charge.JR_EstimatedRevenue);
				});
				CombineAssertions("No save, Posted, Quick-calculator => Fixed", () =>
				{
					SetCostAndRevenue(NewClient2, charge, jobCode, costAmount: 333, revenueAmount: 444);
					charge.JR_OSCostAmt = 333;
					AssertEquals("Estimate fixed", (ZDecimal)777, charge.JR_EstimatedCost);
					charge.JR_OSSellAmt = 444;
					AssertEquals("Posted revenue can still have estimate updated.", (ZDecimal)444, charge.JR_EstimatedRevenue);
				});
			}
		}

		[GuiTest]
		public void TestQuickCalculator_Consol()
		{
			var frtChargeCode = Helper.ChargeCodes["FRT"];

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = "FCL";

			consol.Shipments.AddNew();

			var apps = new ApportionmentListing(Factory, consol);
			var consolCost = apps.CostsCollection.TryAddNew();
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;
			consolCost.E6_AC_ChargeCode = frtChargeCode.PK;

			using (var form = new ZForm(apps))
			using (var grid = new ZGrid())
			{
				grid.BindTo = "CostsCollection";
				grid.Columns.AddCalcEditColumn(JobConsolCostSchema.E6_OSCostAmount.Name, 20, 2);
				form.Controls.Add(grid);
				form.Show();
				grid.SetDataBinding(apps, "CostsCollection");

				new QuickCalculateMenuItemManager(grid, consol).AddMenuItem();
				var quickCalculateMenuItem = grid.ContextMenu.MenuItems.FindByText("Quick Calculate");

				grid.Select(0);
				quickCalculateMenuItem.PerformClick();

				var qcForm = ZFormModaliser.ActiveForm as JobChargeQuickCalculateForm;
				AssertNotNull(qcForm);

				var qc = qcForm.BusinessEntity as JobChargeQuickCalculateBusinessObject;
				AssertNotNull(qcForm);
				qc.QuantityDescription = "Custom";
				qc.Quantity = 10m;

				qc.UpdateCost = true;
				qc.CostRate = 6m;

				qcForm.AcceptButton.PerformClick();
				((IZForm)qcForm).Dispose();
			}

			apps.ReleaseMutexes();
			AssertEquals(60m, consolCost.E6_OSCostAmount);
			AssertEquals(1, ((IPaymentBasisViewCharge)consolCost).CostPaymentBasesView.Count);
		}

		public void TestQuickCalculator_Consol_ContainerCountTEU()
		{
			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_OneTimeQuote = true;

			var oneOff = Factory.NewWithValidTestData<RateOneOffShipment>();
			oneOff.TT_TransportMode = TransportModes.Sea;
			oneOff.TT_ContainerMode = RateMode.SEA;

			var oneOffAdapter = new RateOneOffShipmentRatingAdapter(oneOff);
			var oneOffContainer = oneOff.Containers.AddNew();

			oneOffContainer.TC_ContainerCount = 2;
			oneOffContainer.TC_RC = GP40.PK;

			var newContainer = Factory.New<RefContainer>();
			newContainer.RC_HandlingRateClass = "YMK";
			newContainer.RC_Code = "Z1";
			newContainer.RC_TEU = 0;

			var noTeuContainer = oneOff.Containers.AddNew();
			noTeuContainer.TC_ContainerCount = 2;
			noTeuContainer.TC_RC = newContainer.PK;

			Factory.Save();

			var job = Factory.NewJobForTesting<Job>();
			var charge = job.Charges.AddNew();
			var quickCalculateBO = new JobChargeQuickCalculateBusinessObject(oneOffAdapter, charge);
			quickCalculateBO.QuantityDescription = "TEU";

			AssertEquals("Should be 2x2 TEU per 4 Containers", 4m, quickCalculateBO.Quantity);
		}

		void PostTheCharge(Charge charge)
		{
			AccTransactionLines transactionLine;
			if (charge.IsInDatabase)
			{
				transactionLine = Factory.Load<AccTransactionLines>(charge.JR_AL_APLine);
			}
			else
			{
				transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_Ledger = LedgerTypes.AccountsPayable;
				invoice.AH_InvoiceAmount = charge.JR_OSCostAmt;
				invoice.AH_OutstandingAmount = charge.JR_OSCostAmt;
				transactionLine.AL_AH = invoice.PK;
				charge.JR_AL_APLine = transactionLine.PK;
			}
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_OSAmount = charge.JR_OSCostAmt;

			if (charge.IsInDatabase)
			{
				transactionLine = Factory.Load<AccTransactionLines>(charge.JR_AL_ARLine);
			}
			else
			{
				transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
				var invoice = Factory.NewWithValidTestData<ARInvoice>();
				invoice.AH_InvoiceAmount = charge.JR_OSSellAmt;
				invoice.AH_OutstandingAmount = charge.JR_OSSellAmt;
				invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
				transactionLine.AL_AH = invoice.PK;
				charge.JR_AL_ARLine = transactionLine.PK;
			}
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			transactionLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			transactionLine.AL_OSAmount = charge.JR_OSSellAmt;
		}

		void SetCostAndRevenue(OrgHeader org, Charge charge, ZString jobCode, ZDecimal costAmount, ZDecimal revenueAmount)
		{
			var quickCalculator = (IQuickCalculatorCharge)charge;

			var costInfo = new AutoRateInfo(Factory);
			costInfo.ChargeCode = charge.ChargeCode;
			costInfo.HasExplicitZeroAmount = false;
			costInfo.Currency = "USD";
			costInfo.AddFlatPaymentBasis(costAmount, jobCode);

			var revenueInfo = new AutoRateInfo(Factory);
			revenueInfo.ChargeCode = charge.ChargeCode;
			revenueInfo.HasExplicitZeroAmount = false;
			revenueInfo.Currency = "USD";
			revenueInfo.AddFlatPaymentBasis(revenueAmount, jobCode);

			quickCalculator.SetAmount(CostSell.Cost, costInfo, org, recalculateOverriden: true);
			quickCalculator.SetAmount(CostSell.Revenue, revenueInfo, org, recalculateOverriden: true);
		}
	}
}
