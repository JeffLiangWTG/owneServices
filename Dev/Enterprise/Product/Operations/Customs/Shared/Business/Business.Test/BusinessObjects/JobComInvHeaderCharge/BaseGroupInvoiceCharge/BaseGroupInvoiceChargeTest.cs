using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseGroupInvoiceCharge))]
	public class BaseGroupInvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultCurrencyOnAmountChanged()
		{
			TestDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "NZD";
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;

			var groupCharge = GroupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			groupCharge.J7_Amount = 100m;
			CombineAssertions(() =>
			{
				AssertEquals("It should be defaulted to NZD", "NZD", groupCharge.J7_RX_NKCurrency);

				invoice.JZ_RX_NKInvoice_Currency = "AUD";
				AssertEquals("PreCondition:Currency is still NZD", "NZD", groupCharge.J7_RX_NKCurrency);

				groupCharge.J7_Amount = 200m;
				AssertEquals("Calcualted currency is AUD", "AUD", new GroupChargeCurrencyCalculator(GroupHeader).GetDefaultCurrency(GetOverseasFreightCharge()));
				AssertEquals("Currency should not change", "NZD", groupCharge.J7_RX_NKCurrency);
			});
		}

		protected virtual ICustomsChargeCode GetOverseasFreightCharge()
		{
			return CustomsChargeCodeProvider.OverseasFreight;
		}

		public void TestJ7_Calc_IsIncludedInInvoiceAmount()
		{
			var charge = GroupHeader.Charges.AddNew();
			charge.J7_IsIncludedInITOT = true;
			CombineAssertions(() =>
			{
				AssertEquals(true, charge.J7_Calc_IsIncludedInInvoiceAmount);
				AssertEquals("always readonly", true, charge.J7_Calc_IsIncludedInInvoiceAmountInfo.ReadOnly);

				charge.J7_IsIncludedInITOT = false;
				AssertEquals(false, charge.J7_Calc_IsIncludedInInvoiceAmount);
			});
		}

		public void TestTypeDeciderForCharge()
		{
			AssertEquals("Type decider type", typeof(BaseGroupInvoiceChargeTypeDecider), BaseGroupInvoiceCharge.TypeDecider.GetType());
		}

		public void TestSetDefaultValues()
		{
			var charge = Factory.New<BaseGroupInvoiceCharge>();
			CombineAssertions(() =>
			{
				AssertEquals("IsApportioned should have been set", false, charge.J7_IsApportionedCharge);

				charge.Parent = GroupHeader;
				AssertEquals("ParentTableCode", "JZ", charge.J7_ParentTableCode);
			});
		}

		public void TestReadOnlyOfAmountCurrencyIfPercentage()
		{
			var groupCharge = GroupHeader.Charges.AddNew();
			groupCharge.J7_ChargeType = CustomsChargeTypeList.Codes.Commission;

			groupCharge.J7_Percentage = 10m;
			CombineAssertions(() =>
			{
				AssertEquals("Amount is calculated and should be readonly", true, groupCharge.J7_AmountInfo.ReadOnly);
				AssertEquals("Currency is calculated and should be readonly", true, groupCharge.J7_RX_NKCurrencyInfo.ReadOnly);

				groupCharge.J7_Percentage = 0m;
				AssertEquals("% is zero and should not be readonly", false, groupCharge.J7_AmountInfo.ReadOnly);
				AssertEquals("% is zero and should not be readonly", false, groupCharge.J7_RX_NKCurrencyInfo.ReadOnly);
			});
		}

		public void TestSettingPercentagePropagateIntoInvoiceThatDoesntHaveAmountOfTheCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			var inv1Charge = invoice1.Charges.AddNew(CustomsChargeTypeList.Codes.Commission, 100m, GroupHeader.JobDeclaration.LocalCurrencyCode);

			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceAmount = 1000;
			invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var cOM = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
			cOM.J7_Percentage = 20;
			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("Invoice1 doesnt have % value", 0m, inv1Charge.J7_Percentage);
				AssertEquals("Invoice 2 assigned", 20m, invoice2.GroupCharges[0].J7_Percentage);
			});
		}

		public void TestChangingPercentagePropagatedIntoInvoiceAndLines()
		{
			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var cOM = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
			cOM.J7_Percentage = 20;
			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("Invoice's COM", 20m, invoice.GroupCharges[0].J7_Percentage);

				cOM.J7_Percentage = 25;
				TestDec.ResumeApportionment();
				AssertEquals("Invoice's COM", 25m, invoice.GroupCharges[0].J7_Percentage);
			});
		}

		public void TestDeletingPercentagePropagedIntoInvoiceAndLines()
		{
			var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice.JZ_InvoiceAmount = 1000;
			invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

			var cOM = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
			cOM.J7_Percentage = 20;
			TestDec.ResumeApportionment();
			CombineAssertions(() =>
			{
				AssertEquals("Invoice's COM", 20m, invoice.GroupCharges[0].J7_Percentage);

				cOM.Delete();
				TestDec.ResumeApportionment();
				AssertEquals("Invoice charges", 0, invoice.GroupCharges.Count);
			});
		}

		public void TestValidPercentageClearsAmountCurrency()
		{
			var cOM = GroupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.Commission);
			cOM.J7_Amount = 1000m;
			cOM.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;
			cOM.J7_Percentage = 20;
			CombineAssertions(() =>
			{
				AssertEquals("Percentage entered-> Clear Amount", 0m, cOM.J7_Amount);
				AssertEquals("Percentage entered-> Clear Currency", ZString.Empty, cOM.J7_RX_NKCurrency);
			});
		}

		public void TestDefaultPrepaidCollectForGroupCharge()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			AssertEquals("Overseas freight charge is Collect", Core.Constants.PaymentType.Collect, oFT.J7_PrepaidCollect);
		}

		public virtual void TestDefaultPrepaidCollectForGroupCharge2()
		{
			var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();
			var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_IncoTerm = "CIF";

			var oFT = GroupHeader.Charges.AddNew();
			oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
			AssertEquals("Overseas freight charge is Prepaid", Core.Constants.PaymentType.Prepaid, oFT.J7_PrepaidCollect);
		}

		public virtual void TestChargePrepaidCollectCommittedToApportionedCharge()
		{
			using (Customs.DataRegistry.Business.CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var invoice1 = GroupHeader.JobComInvoiceHeaders.AddNew();
				invoice1.JZ_IncoTerm = GetOverseasFreightIncoTermForTest();
				invoice1.JZ_InvoiceAmount = 1000;
				invoice1.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				var invoice2 = GroupHeader.JobComInvoiceHeaders.AddNew();
				invoice2.JZ_IncoTerm = "CIF";
				invoice2.JZ_InvoiceAmount = 1000;
				invoice2.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				var oFT = GroupHeader.Charges.AddNew();
				oFT.J7_ChargeType = GetOverseasFreightChargeCodeForTest();
				oFT.J7_Amount = 100;
				oFT.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				TestDec.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition:OFT is Prepaid", Core.Constants.PaymentType.Prepaid, oFT.J7_PrepaidCollect);
					AssertEquals("1 apportioned Charge", 1, invoice1.GroupCharges.Count);
					AssertEquals("1 apportioned Charge", 1, invoice2.GroupCharges.Count);

					oFT.J7_PrepaidCollect = Core.Constants.PaymentType.Collect;
					TestDec.ResumeApportionment();
					AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice1.GroupCharges[0].J7_PrepaidCollect);
					AssertEquals("Now it is collect", Core.Constants.PaymentType.Collect, invoice2.GroupCharges[0].J7_PrepaidCollect);
				});
			}
		}

		public virtual void TestApportionChargeWithSameChargeTypeWithDifferntKeys()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var invoice = GroupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 1000;
				invoice.JZ_RX_NKInvoice_Currency = GroupHeader.JobDeclaration.LocalCurrencyCode;

				//Dutiable FIFT
				GroupCharge.J7_ChargeType = ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys;
				GroupCharge.J7_Amount = 100;
				GroupCharge.J7_RX_NKCurrency = GroupHeader.JobDeclaration.LocalCurrencyCode;
				TestDec.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertEquals("PreCondition: Dutiable", true, GroupCharge.J7_IsDutiable);

					var nonDutyFIFT = GroupHeader.Charges.AddNew(ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys, 200, GroupHeader.JobDeclaration.LocalCurrencyCode);
					nonDutyFIFT.J7_IsDutiable = false;
					TestDec.ResumeApportionment();
					AssertEquals("Two apportioned Charges", 2, invoice.GroupCharges.Count);
				});
			}
		}

		protected virtual string ChargeCodeForTestApportionChargeWithSameChargeTypeWithDifferntKeys => CustomsChargeTypeList.Codes.ForeignInlandFreight;

		#region Implementation

		protected virtual string GetOverseasFreightChargeCodeForTest()
		{
			return CustomsChargeTypeList.Codes.OverseasFreight;
		}

		protected virtual string GetOverseasFreightIncoTermForTest()
		{
			return Core.Constants.IncoTerms.FreeOnBoard;
		}

		protected BaseJobDeclaration TestDec
		{
			get
			{
				if (fTestDec == null)
				{
					fTestDec = GetNewDeclarationForTest();
				}
				return fTestDec;
			}
		}

		protected virtual BaseJobDeclaration GetNewDeclarationForTest() => Factory.New<BaseJobDeclaration>();

		BaseJobDeclaration fTestDec;

		protected BaseJobComInvoiceGroupHeader GroupHeader
		{
			get
			{
				if (fGroupHeader == null)
				{
					fGroupHeader = TestDec.JobComInvoiceGroupHeaders[0];
				}
				return fGroupHeader;
			}
		}
		BaseJobComInvoiceGroupHeader fGroupHeader;

		protected BaseGroupInvoiceCharge GroupCharge
		{
			get
			{
				using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
				{
					if (fGroupCharge == null)
					{
						fGroupCharge = GroupHeader.Charges.AddNew();
					}
					return fGroupCharge;
				}
			}
		}
		BaseGroupInvoiceCharge fGroupCharge;

		protected override BusinessObject GetNewBusinessObject()
		{
			return GroupCharge;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GroupCharge;
		}
		#endregion
	}
}
