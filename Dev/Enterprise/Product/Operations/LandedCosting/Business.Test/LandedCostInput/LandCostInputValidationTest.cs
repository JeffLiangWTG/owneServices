using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LandedCosting.Business.Testing
{
	sealed class LandCostInputValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateAll()
		{
			costInput.Validation.ValidateAll();
			costInput.LCGroupString = "";
			costInput.LinkedObjectUniqueCode = "";
			AssertEquals("Empty LCGroupString is an error", true, costInput.LCGroupStringInfo.HasErrors());
			AssertEquals("Empty LinkedObject code is an error", true, costInput.LinkedObjectUniqueCodeInfo.HasErrors());
		}

		public void TestValidateLCGroupString()
		{
			costInput.LCGroupString = "";
			AssertEquals("Empty is not valid", true, costInput.LCGroupStringInfo.HasErrors());

			costInput.LCGroupString = "A";
			AssertEquals("Expecting a number", true, costInput.LCGroupStringInfo.HasErrors());

			costInput.LCGroupString = "0";
			AssertEquals("Expecting  > 0", true, costInput.LCGroupStringInfo.HasErrors());
		}

		public void TestCheckLI_DistributeCostBy()
		{
			header.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;

			costInput.LI_DistributeCostBy = "XXX";
			AssertEquals("Invalid code", true, costInput.LI_DistributeCostByInfo.HasErrors());

			costInput.LI_DistributeCostBy = costInput.Lookups.DistributeCostBy[0].Code;
			AssertEquals("Valid", false, costInput.LI_DistributeCostByInfo.HasErrors());

			BusinessObject invoiceLine = (BusinessObject)Factory.New<Integration.Customs.IBaseJobComInvoiceLine>();
			costInput.LI_ParentID = invoiceLine.PK;
			costInput.LI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;

			costInput.LI_DistributeCostBy = "";
			AssertEquals("If the distribute level is ultimate distrubutee, then distribution by is not necessary", false, costInput.LI_DistributeCostByInfo.HasErrors());

			BusinessObject invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			costInput.LI_ParentID = invoice.PK;
			costInput.LI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;
			costInput.LI_DistributeCostBy = "";
			AssertEquals("Invoice is not IUltimateDistributee and needs DistibuteCost By", true, costInput.LI_DistributeCostByInfo.HasErrors());
		}

		public void TestCheckLI_AC_ChargeCode()
		{
			costInput.LI_AC_ChargeCode = ZGuid.NewZGuid();
			AssertEquals("Invalid charge", true, costInput.LI_AC_ChargeCodeInfo.HasErrors());

			costInput.LI_AC_ChargeCode = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK)).PK;
			AssertEquals("Valid", false, costInput.LI_AC_ChargeCodeInfo.HasErrors());
		}

		public void TestCheckLI_CostAmount()
		{
			costInput.LI_CostAmount = -100;
			AssertEquals("Cost amount should be greater than zero", false, costInput.LI_CostAmountInfo.HasErrors());

			costInput.LI_CostAmount = 0;
			AssertEquals("Cost amount should be greater than zero", true, costInput.LI_CostAmountInfo.HasErrors());

			costInput.LI_CostAmount = 100;
			AssertEquals("Cost amount should be greater than zero", false, costInput.LI_CostAmountInfo.HasErrors());
		}

		public void TestCheckLI_RX()
		{
			costInput.LI_RX_NKCostCurrency = "AAA";
			AssertEquals("Invalid", true, costInput.LI_RX_NKCostCurrencyInfo.HasErrors());
		}

		public void TestCheckLI_ServiceExRate()
		{
			costInput.LI_ServiceExRate = -100;
			AssertEquals("Ex rate > 0", true, costInput.LI_ServiceExRateInfo.HasErrors());

			costInput.LI_ServiceExRate = 0;
			AssertEquals("Ex rate > 0", true, costInput.LI_ServiceExRateInfo.HasErrors());

			costInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			costInput.LI_ServiceExRate = 2;
			AssertEquals("Local currency ex rate", true, costInput.LI_ServiceExRateInfo.HasErrors());
		}

		LandedCostHeader header;
		LandCostInput costInput;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<LandedCostHeader>();
			costInput = header.CostInputs.AddNew();
		}
	}
}
