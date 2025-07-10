using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseInvoiceChargePrepaidCollectTest : TestCaseWithFactory
	{
		public void TestChargesPrepaidCollectWithEXW()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.ExWorks;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);

			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Collect, eXW.J7_PrepaidCollect);

			AssertEquals("OFT is Collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithFCA()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeCarrier;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("OFT is Collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithFAS()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeAlongsideShip;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);

			AssertEquals("OFT is Collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);

			invoice.Charges.RemoveAndDelete(fIFT);
			testDec.ResumeApportionment();
			fIFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[2].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithFOB()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.FreeOnBoard;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);

			AssertEquals("OFT is Collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithCFR()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.CostAndFreight;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);

			invoice.GroupCharges.Sort("J7_PrepaidCollect");
			AssertEquals("OFT is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithCIF()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.CostInsuranceAndFreight;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);

			AssertEquals("OFT is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithCPT()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.CarriagePaidTo;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			invoice.GroupCharges.Sort("J7_PrepaidCollect");
			AssertEquals("OFT is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
			AssertEquals("ONS is Collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithCIP()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.CarriageAndInsurancePaidTo;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge oTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("OTH is prepaid", Core.Constants.PaymentType.Prepaid, oTH.J7_PrepaidCollect);
			AssertEquals("EXW is prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);

			AssertEquals("OFT is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestChargesPrepaidCollectWithDDP()
		{
			invoice.JZ_IncoTerm = Constants.IncoTerms.DeliveredDutyPaid;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge dutyOTH = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200);
			BaseJobComInvHeaderCharge nDOth = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 250);
			nDOth.J7_IsDutiable = false;
			nDOth.J7_IsGSTApplicable = false;

			BaseJobComInvHeaderCharge eXW = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ExWorks, 300);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight, 350);

			BaseJobComInvHeaderCharge oFT = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 400, invoice.JobDeclaration.LocalCurrencyCode);
			BaseJobComInvHeaderCharge oNS = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 500, invoice.JobDeclaration.LocalCurrencyCode);
			testDec.ResumeApportionment();

			CombineAssertions(() =>
			{
				AssertEquals("PC is Prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
				AssertEquals("OTH is Prepaid", Core.Constants.PaymentType.Prepaid, dutyOTH.J7_PrepaidCollect);
				AssertEquals("NDOth is Prepaid", Core.Constants.PaymentType.Prepaid, nDOth.J7_PrepaidCollect);
				AssertEquals("EXW is Prepaid", Core.Constants.PaymentType.Prepaid, eXW.J7_PrepaidCollect);
				AssertEquals("FIFT is Prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
				AssertEquals("OFT is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[0].J7_PrepaidCollect);
				AssertEquals("ONS is Prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
			});
		}

		public void TestDefaultForOverseasFreight()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.OverseasFreight, false, true);
		}

		public void TestDefaultForOverseasInsurance()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.OverseasInsurance, false, true);
		}

		public void TestDefaultForCommission()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.Commission, true, true);
		}

		public void TestDefaultForExWorks()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.ExWorks, true, true);
		}

		public void TestDefaultForPackingCost()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.PackingCost, true, true);
		}

		public void TestDefaultForLandingCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.LandingCharges, false, false);
		}

		public void TestDefaultForAddtionCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.AdditionCharge, true, true);
		}

		public void TestDefaultForDeductionCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.DeductionCharge, false, false);
		}

		public void TestDefaultForDiscount()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.Discount, false, false);
		}

		public void TestDefaultForForeignInlandFreight()
		{
			AssertDefaultFlagsForInvoiceCharge(CustomsChargeTypeList.Codes.ForeignInlandFreight, true, true);
		}

		#region Implementation

		protected BaseJobDeclaration testDec;
		protected BaseJobComInvoiceGroupHeader groupHeader;
		protected BaseJobComInvoiceHeader invoice;

		protected override void SetUp()
		{
			base.SetUp();
			SetupAllTestObjects();
			distributeByForExport = DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value);
		}

		protected override void TearDown()
		{
			base.TearDown();
			distributeByForExport?.Dispose();
		}

		IDisposable distributeByForExport;

		protected virtual void SetupAllTestObjects()
		{
			testDec = Factory.New<BaseJobDeclaration>();
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 10000;
			invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;
		}

		protected virtual BaseJobComInvHeaderCharge GetNewInvoiceChargeOnInvoice(string chargeName)
		{
			return invoice.Charges.AddNew(chargeName);
		}

		void AssertDefaultFlagsForInvoiceCharge(string chargeName, bool isDutiable, bool isGSTApplicable)
		{
			BaseJobComInvHeaderCharge charge = GetNewInvoiceChargeOnInvoice(chargeName);
			AssertEquals("Dutiable", isDutiable, charge.J7_IsDutiable);
			AssertEquals("GST", isGSTApplicable, charge.J7_IsGSTApplicable);
		}
		#endregion
	}
}
