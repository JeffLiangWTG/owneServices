using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceHeaderApportionTest : TestCaseWithFactory
	{
		public void TestResumeApportionmentForStandAloneInvoices()
		{
			var invoice = Factory.New<BaseJobComInvoiceHeader>();

			var faker = new FakeDeclarationCreatorForInvoice(invoice);
			var declaration = (BaseJobDeclaration)faker.HeaderData;
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			AssertEquals("Apportionment is dirty", true, declaration.ApportionmentDirty);

			invoice.RunPreSaveValidation();
			AssertEquals("Apportionment should have been refreshed", false, declaration.ApportionmentDirty);
		}

		public void TestApportionedCommissionShouldNotBeIncludedInLinesByDefault()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
				testDec.AutoCreateChargesBasedOnIncoTerm = false;
				BaseJobComInvHeaderCharge groupCommission = testDec.JobComInvoiceGroupHeaders[0].Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
				PrepareCharge(groupCommission);
				groupCommission.J7_Amount = 100m;
				groupCommission.J7_RX_NKCurrency = testDec.LocalCurrencyCode;

				BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
				invoice.JZ_IncoTerm = "FOB";
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = testDec.LocalCurrencyCode;

				BaseJobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_LinePrice = 1000m;
				testDec.ResumeApportionment();

				AssertEquals("There is only one apportioned charge", 1, invoice.GroupCharges.Count);
				AssertEquals("Apportioned Charge is COM", CustomsChargeTypeList.Codes.Commission, invoice.GroupCharges[0].J7_ChargeType);
				AssertEquals("And the apportioned charge is not included in lines", false, invoice.GroupCharges[0].J7_IsIncludedInITOT);
			}
		}

		public void TestDiscountChargeApportionedAndJ7_IsIncludedInITOT()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				BaseGroupInvoiceCharge disCharge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				PrepareCharge(disCharge);
				disCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Discount;
				disCharge.J7_Amount = 100m;
				disCharge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
				disCharge.J7_IsIncludedInITOT = true;
				declaration.ResumeApportionment();
				AssertEquals("PreCondition:Apportioned charge is DIS", CustomsChargeTypeList.Codes.Discount, invoice.GroupCharges[0].J7_ChargeType);
				AssertEquals("IsIncludedInITOT", true, invoice.GroupCharges[0].J7_IsIncludedInITOT);

				disCharge.J7_IsIncludedInITOT = false;
				declaration.ResumeApportionment();
				AssertEquals("IsIncludedInITOT", false, invoice.GroupCharges[0].J7_IsIncludedInITOT);
			}
		}

		public void TestApportionOverseasFreight()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasFreight, false, true));
		}

		public void TestApportionOverseasInsurance()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OverseasInsurance, false, true));
		}

		public void TestApportionLandingCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.LandingCharges, false, false));
		}

		public virtual void TestApportionExWorksAmount()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ExWorks, true, true));
		}

		public void TestApportionForeignInlandFreight()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.ForeignInlandFreight, true, true));
		}

		public void TestApportionPackingCosts()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.PackingCost, true, true));
		}

		public virtual void TestApportionOtherCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.OtherCharges, true, true));
		}

		public void TestApportionDiscountCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Discount, false, false));
		}

		public void TestApportionCommissionCharges()
		{
			AssertApportion(new ChargeCodeChargeKey(CustomsChargeTypeList.Codes.Commission, true, true));
		}

		protected virtual void PrepareCharge(BaseJobComInvHeaderCharge charge)
		{
		}

		public void TestApportionChargeWhenChargeIsRelevantForIncoTerm()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupLanding = allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges);
				PrepareCharge(groupLanding);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				BaseJobComInvoiceHeader header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();

				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				header2.JZ_JE = testDeclaration.PK;
				header2.JZ_IncoTerm = "DDP";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.Count);
				AssertEquals("Landing Charge is apportioned to DDP invoice", 100m, header2.GroupCharges[0].J7_Amount);
			}
		}

		public void TestChangeIncoTermUpdateApportion()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobComInvHeaderCharge groupLanding = allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges);
				PrepareCharge(groupLanding);
				groupLanding.J7_Amount = 100m;
				groupLanding.J7_RX_NKCurrency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				BaseJobComInvoiceHeader header2 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
				header2.JZ_JE = testDeclaration.PK;

				header1.JZ_IncoTerm = "FOB";
				header1.JZ_InvoiceAmount = 2000m;
				header1.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;

				header2.JZ_IncoTerm = "DDP";
				header2.JZ_InvoiceAmount = 2000m;
				header2.JZ_RX_NKInvoice_Currency = allInvoicesGroup.JobDeclaration.LocalCurrencyCode;
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is not relavant for FOB invoice", 0, header1.GroupCharges.Count);
				AssertEquals("Landing Charge is apportioned to DDP invoice", 100m, header2.GroupCharges[0].J7_Amount);

				header1.JZ_IncoTerm = "DDP";
				testDeclaration.ResumeApportionment();
				AssertEquals("Landing Charge is apportioned to Header1", 50m, header1.GroupCharges[0].J7_Amount);
				AssertEquals("Landing Charge is apportioned to Header2", 50m, header2.GroupCharges[0].J7_Amount);
			}
		}

		public virtual void TestFOBWithPreFOBCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_RX_NKInvoice_Currency = header1.JobDeclaration.LocalCurrencyCode;
				header1.JZ_IncoTerm = "FOB";
				AssertEquals("FOB Value for $1000 FOB Invoice", 1000M, header1.JZ_Calc_FOBAmount);

				PrepareCharge(allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 100, header1.JobDeclaration.LocalCurrencyCode));
				testDeclaration.ResumeApportionment();
				AssertEquals("FOB value for $1000 FOB invoice", 1100m, header1.JZ_Calc_FOBAmount);
				AssertEquals("Line Total For this invoice", 1000m, header1.InvoiceLineTotal);

				allInvoicesGroup.Charges[0].J7_Amount = 0;

				PrepareCharge(allInvoicesGroup.Charges.AddNew(CustomsChargeTypeList.Codes.OtherCharges, 200, header1.JobDeclaration.LocalCurrencyCode));
				testDeclaration.ResumeApportionment();
				AssertEquals("FOB value for $1000 FOB invoice", 1200m, header1.JZ_Calc_FOBAmount);
				AssertEquals("Line Total For this invoice", 1000m, header1.InvoiceLineTotal);
			}
		}

		public void TestFOBCIFCurrencySameAsInvoiceCurr()
		{
			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			RefCurrency fOBCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_FOBCurrency);
			RefCurrency cIFCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_CIFCurrency);
			AssertEquals("FOB Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, fOBCurrency.RX_Code);
			AssertEquals("CIF Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, cIFCurrency.RX_Code);

			header1.JZ_RX_NKInvoice_Currency = uSDCurrency.RX_Code;
			fOBCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_FOBCurrency);
			cIFCurrency = Factory.Load<RefCurrency>(header1.JZ_Calc_CIFCurrency);
			AssertEquals("FOB Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, fOBCurrency.RX_Code);
			AssertEquals("CIF Currency set the same as invoice Curr", header1.JZ_RX_NKInvoice_Currency, cIFCurrency.RX_Code);
		}

		public void TestDefaultWeightForInvoiceHeader()
		{
			AssertEquals("New invoice has a weight UQ defaultly set 'KG'", "KG", header1.JZ_WeightUQ);

			header1.JZ_WeightUQ = "T";
			AssertEquals("Weight UQ overwritable", "T", header1.JZ_WeightUQ);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			BaseJobComInvoiceHeader loadedHeader = newFactory.Load<BaseJobComInvoiceHeader>(header1.PK);
			AssertEquals("Loaded Invoice should have a weight UQ", "T", loadedHeader.JZ_WeightUQ);
		}

		public void TestDefaultBranchForInvoiceHeader()
		{
			BaseJobComInvoiceHeader invoice = Factory.New<BaseJobComInvoiceHeader>();
			AssertEquals("JZ_GB", GlbBranch.CurrentBranch.PK, invoice.JZ_GB);
		}

		public virtual void TestInvoiceChargeSetBeforeInvoiceCurrency()
		{
			header1.JZ_RX_NKInvoice_Currency = ZString.Empty;
			BaseJobComInvHeaderCharge lCH = header1.Charges.AddNew(CustomsChargeTypeList.Codes.LandingCharges, 100);
			Assert("Landing Charges Currency Is not set", lCH.J7_RX_NKCurrency.IsEmpty);

			header1.JZ_RX_NKInvoice_Currency = aUDCurrency.RX_Code;
			AssertEquals("Landing Charges currency is set now", header1.Invoice_Currency.RX_Code, lCH.J7_RX_NKCurrency);
		}

		public void TestBalancePrice()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			var header = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line1 = header.JobComInvoiceLines.AddNew();
			line1.JI_LinePrice = 10m;
			var line2 = header.JobComInvoiceLines.AddNew();
			line2.JI_LinePrice = 20m;
			var line3 = header.JobComInvoiceLines.AddNew();
			line3.JI_LinePrice = 30m;
			header.BalancePrice();
			AssertEquals("JZ_InvoiceAmount", 60m, header.JZ_InvoiceAmount);

			line1.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			line2.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			line3.JI_InvoiceUQ = Core.Constants.Weight.Kilograms;
			Assert(!header.HasMultipleInvoiceUQs);
			line2.JI_InvoiceUQ = Core.Constants.Weight.Milligrams;
			Assert(header.HasMultipleInvoiceUQs);
		}

		public void TestOTHChargeApportionedAndJ7_IsIncludedInITOT()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

				invoice.JZ_InvoiceAmount = 10000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;

				BaseGroupInvoiceCharge charge = declaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
				PrepareCharge(charge);
				charge.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;
				charge.J7_Amount = 100m;
				charge.J7_RX_NKCurrency = declaration.LocalCurrencyCode;
				charge.J7_IsDutiable = true;
				charge.J7_IsGSTApplicable = true;
				charge.J7_IsIncludedInITOT = true;
				declaration.ResumeApportionment();
				AssertEquals("PreCondition:This is an OTH charge", CustomsChargeTypeList.Codes.OtherCharges, invoice.GroupCharges[0].J7_ChargeType);
				AssertEquals("IsIncludedInITOT calculated", true, invoice.GroupCharges[0].J7_IsIncludedInITOT);

				charge.J7_IsIncludedInITOT = false;
				declaration.ResumeApportionment();
				AssertEquals("IsIncludedInITOT calculated", false, invoice.GroupCharges[0].J7_IsIncludedInITOT);
			}
		}

		#region Implementation
		BaseJobDeclaration testDeclaration;
		BaseJobComInvoiceGroupHeader allInvoicesGroup;
		BaseJobComInvoiceHeader header1;
		RefCurrency aUDCurrency;
		RefCurrency uSDCurrency;

		protected virtual BaseJobDeclaration GetNewDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			testDeclaration = GetNewDeclaration();
			testDeclaration.AutoCreateChargesBasedOnIncoTerm = false;
			allInvoicesGroup = testDeclaration.JobComInvoiceGroupHeaders[0];
			header1 = allInvoicesGroup.JobComInvoiceHeaders.AddNew();
			header1.JZ_JE = testDeclaration.PK;
			aUDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
			uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
		}

		void AssertApportion(ChargeCodeChargeKey chargeKey)
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				header1.JZ_RX_NKInvoice_Currency = header1.JobDeclaration.LocalCurrencyCode;
				header1.JZ_InvoiceAmount = 1000m;
				header1.JZ_IncoTerm = "DDP";

				PrepareCharge(allInvoicesGroup.Charges.AddNew(chargeKey.ChargeCode, 150m, header1.JobDeclaration.LocalCurrencyCode));
				testDeclaration.ResumeApportionment();
				AssertEquals("Calculated " + chargeKey.ChargeCode + " - Apportioned", 150.0m, header1.GroupCharges[0].J7_Amount);
			}
		}
		#endregion
	}
}
