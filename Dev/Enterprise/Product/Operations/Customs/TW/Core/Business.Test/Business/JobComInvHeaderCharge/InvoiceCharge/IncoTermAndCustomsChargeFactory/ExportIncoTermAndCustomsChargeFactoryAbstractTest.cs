using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class ExportIncoTermAndCustomsChargeFactoryAbstractTest<TExportIncoTermAndCustomsChargeFactory> : Common.Testing.IncoTermAndCustomsChargeFactoryTest
		where TExportIncoTermAndCustomsChargeFactory : ExportIncoTermAndCustomsChargeFactory
	{
		public override void TestGetAllIncoTerms()
		{
			var list = new ZString[] { "CIF", "CFR", "FOB", "C&I", "FAS", "EXW", "FCA", "CPT", "CIP", "DAT", "DAP", "DDP", "DPU" };
			var incoTerms = incoTermAndChargeFactory.GetAllIncoTerms();
			AssertEquals("Count", 13, incoTerms.Length);
			AssertEquals("Count", 13, incoTerms.Where(x => list.Contains(x)).Count());
		}

		public override void TestGetAllCharges()
		{
			AssertEquals("There should be 7 charges", 7, incoTermAndChargeFactory.GetAllCharges().Length);
		}

		public override void TestIsThisChargeDiscount()
		{
			AssertEquals(true, incoTermAndChargeFactory.IsThisChargeDiscount(CustomsChargeTypeList.Codes.Discount));
			AssertEquals(true, incoTermAndChargeFactory.IsThisChargeDiscount(CustomsChargeTypeList.Codes.DeductionCharge));
		}

		#region Implementation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\TW\Core\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ExportIncoTermAndCustomsChargeConfiguration.csv";

		public override void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, ExportIncoTermAndCustomsChargeFactory.PackingCost);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, ExportIncoTermAndCustomsChargeFactory.OverseasFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, ExportIncoTermAndCustomsChargeFactory.OverseasInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, ExportIncoTermAndCustomsChargeFactory.ForeignInlandFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.LandingCharges, ExportIncoTermAndCustomsChargeFactory.LandingCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, ExportIncoTermAndCustomsChargeFactory.AdditionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, ExportIncoTermAndCustomsChargeFactory.DeductionCharge);
		}

		public void TestChargeParentTypes()
		{
			var charges = incoTermAndChargeFactory.GetAllCharges();
			foreach (var charge in charges)
			{
				AssertEquals(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} charge code shuold be available for Group Inovice, Invoice, and Invoice Line", charge.Code), ChargeParentTypes.GroupInvoice | ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine, charge.ParentTypes);
			}
		}
		#endregion
	}
}
