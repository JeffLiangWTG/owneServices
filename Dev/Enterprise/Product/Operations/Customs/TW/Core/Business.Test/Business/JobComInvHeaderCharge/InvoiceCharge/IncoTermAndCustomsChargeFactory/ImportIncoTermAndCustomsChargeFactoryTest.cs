using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(ImportIncoTermAndCustomsChargeFactory))]
	sealed class ImportIncoTermAndCustomsChargeFactoryTest : Common.Testing.IncoTermAndCustomsChargeFactoryTest
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
		protected override string IncoTermAndCustomsChargeConfigurationFilename => BaseSourcePath + @"Enterprise\Product\Operations\Customs\TW\Core\Business.Test\Business\JobComInvHeaderCharge\InvoiceCharge\TestFile\ImportIncoTermAndCustomsChargeConfiguration.csv";
		JobDeclaration testDec;
		JobComInvoiceGroupHeader testGroupInvoice;
		JobComInvoiceHeader testInvoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testGroupInvoice = testDec.JobComInvoiceGroupHeaders[0];
			testInvoice = testGroupInvoice.JobComInvoiceHeaders.AddNew();
			testInvoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
		}

		protected override string GetCountryContext()
		{
			return base.GetCountryContext() + Common.Shared.SharedJobMessageTypeList.Codes.Import;
		}

		public override void TestGetCharge()
		{
			AssertGetCharge(CustomsChargeTypeList.Codes.PackingCost, ImportIncoTermAndCustomsChargeFactory.PackingCost);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasFreight, ImportIncoTermAndCustomsChargeFactory.OverseasFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.OverseasInsurance, ImportIncoTermAndCustomsChargeFactory.OverseasInsurance);
			AssertGetCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, ImportIncoTermAndCustomsChargeFactory.ForeignInlandFreight);
			AssertGetCharge(CustomsChargeTypeList.Codes.LandingCharges, ImportIncoTermAndCustomsChargeFactory.LandingCharges);
			AssertGetCharge(CustomsChargeTypeList.Codes.AdditionCharge, ImportIncoTermAndCustomsChargeFactory.AdditionCharge);
			AssertGetCharge(CustomsChargeTypeList.Codes.DeductionCharge, ImportIncoTermAndCustomsChargeFactory.DeductionCharge);
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
