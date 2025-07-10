using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(DutyTaxFeeChargeCollection))]
	sealed class DutyTaxFeeAndChargeCollectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DutyTaxFeeChargeCollection>
	{
		protected override DutyTaxFeeChargeCollection GetCollectionToTest()
		{
			return new DutyTaxFeeChargeCollection(Header);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DutyTaxFeeCharge(Header);
		}

		CusEntryHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<CusEntryHeader>();
				}

				return header;
			}
		}

		CusEntryHeader header;
		public void TestShouldRebuildElements()
		{
			var header = Factory.New<CusEntryHeader>();
			var dutyTaxFeeCharges = new DutyTaxFeeChargeCollection(header);
			var line = header.MergedLines.AddNew();
			var invoiceLine = line.InvoiceLines.AddNew() as JobComInvoiceLine;
			var lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			lineFee.CF_Rate = 0.1M;
			lineFee.CF_ChargeAmount = 2M;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			lineFee.CF_Rate = 0.1M;
			lineFee.CF_ChargeAmount = 2M;
			var charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTA;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 2M;
			charge = header.Charges.AddNew();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.DTS;
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			charge.C1_ChargeAmount = 2M;
			CombineAssertions(() =>
			{
				AssertEquals(0, dutyTaxFeeCharges.Count);
				Assert(dutyTaxFeeCharges.ShouldRebuildElements());
				AssertEquals(2, dutyTaxFeeCharges.Count);
				Assert(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A10" && x.MethodOfPayment == "CAS" && x.ChargeAmount == 6M));
				Assert(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "A19" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 2M));
				AssertEquals(false, dutyTaxFeeCharges.ShouldRebuildElements());
			});
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			AssertEquals(true, dutyTaxFeeCharges.ShouldRebuildElements());
			charge.C1_MethodOfPayment = EntryChargePaymentMethod.Codes.CAS;
			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.TAT;
			lineFee.CF_MethodOfPayment = EntryChargePaymentMethod.Codes.DEF;
			lineFee.CF_Rate = 0.1M;
			lineFee.CF_ChargeAmount = 2M;
			CombineAssertions(() =>
			{
				AssertEquals(2, dutyTaxFeeCharges.Count);
				Assert(dutyTaxFeeCharges.ShouldRebuildElements());
				AssertEquals(3, dutyTaxFeeCharges.Count);
				Assert(dutyTaxFeeCharges.Cast<DutyTaxFeeCharge>().Any(x => x.ChargeType == "B69" && x.MethodOfPayment == "DEF" && x.ChargeAmount == 2M));
			});

			lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ADT;
			lineFee.CF_MethodOfPayment = ZString.Empty;
			lineFee.CF_Rate = 0m;
			lineFee.CF_ChargeAmount = 0m;
			CombineAssertions(() =>
			{
				AssertEquals(3, dutyTaxFeeCharges.Count);
				Assert(dutyTaxFeeCharges.ShouldRebuildElements());
				AssertEquals(3, dutyTaxFeeCharges.Count);
			});

			lineFee.CF_ChargeAmount = 1m;
			CombineAssertions(() =>
			{
				Assert(dutyTaxFeeCharges.ShouldRebuildElements());
				AssertEquals(4, dutyTaxFeeCharges.Count);
			});
		}

		public void TestGetHashCode()
		{
			var header = Factory.New<CusEntryHeader>();
			var dutyTaxFeeCharges = new DutyTaxFeeChargeCollection(header);
			var line = header.MergedLines.AddNew();
			var lineFee = line.Fees.AddNew();
			lineFee.CF_ChargeType = "DTA";
			lineFee.CF_MethodOfPayment = "A";
			lineFee.CF_ChargeAmount = 2M;
			var charge = header.Charges.AddNew();
			charge.C1_ChargeType = "DTA";
			charge.C1_MethodOfPayment = "A";
			charge.C1_ChargeAmount = 2M;
			var map = new Dictionary<ZPropertyInfo, IZType> { { charge.C1_ChargeTypeInfo, new ZString("DTS") }, { charge.C1_MethodOfPaymentInfo, new ZString("B") }, { charge.C1_ChargeAmountInfo, new ZDecimal(4m) } };
			foreach (var infoAndValue in map)
			{
				var oldHasCode = dutyTaxFeeCharges.GetHashCode();
				infoAndValue.Key.Value = infoAndValue.Value;
				AssertNotEquals(oldHasCode, dutyTaxFeeCharges.GetHashCode());
			}

			map = new Dictionary<ZPropertyInfo, IZType> { { lineFee.CF_ChargeTypeInfo, new ZString("DTS") }, { lineFee.CF_MethodOfPaymentInfo, new ZString("B") }, { lineFee.CF_ChargeAmountInfo, new ZDecimal(4m) } };
			foreach (var infoAndValue in map)
			{
				var oldHasCode = dutyTaxFeeCharges.GetHashCode();
				infoAndValue.Key.Value = infoAndValue.Value;
				AssertNotEquals(oldHasCode, dutyTaxFeeCharges.GetHashCode());
			}
		}

		public void TestHashCodeInaccurateWhenOnlyChangePaymentMethod()
		{
			var header = Factory.New<CusEntryHeader>();
			var dutyTaxFeeCharges = new DutyTaxFeeChargeCollection(header);
			var line = header.MergedLines.AddNew();
			var lineFee1 = line.Fees.AddNew();
			lineFee1.CF_ChargeType = "DAT";
			lineFee1.CF_MethodOfPayment = "CAS";
			lineFee1.CF_ChargeAmount = 0;
			var lineFee2 = line.Fees.AddNew();
			lineFee2.CF_ChargeType = "HAS";
			lineFee2.CF_MethodOfPayment = "DEF";
			lineFee2.CF_ChargeAmount = 500000M;
			var lineFee3 = line.Fees.AddNew();
			lineFee3.CF_ChargeType = "TAT";
			lineFee3.CF_MethodOfPayment = "DEF";
			lineFee3.CF_ChargeAmount = 795000M;
			var lineFee4 = line.Fees.AddNew();
			lineFee4.CF_ChargeType = "VAT";
			lineFee4.CF_MethodOfPayment = "DEF";
			lineFee4.CF_ChargeAmount = 64750M;
			var oldHasCode = dutyTaxFeeCharges.GetHashCode();
			lineFee2.CF_MethodOfPayment = "CAS";
			lineFee3.CF_MethodOfPayment = "CAS";
			AssertNotEquals(oldHasCode, dutyTaxFeeCharges.GetHashCode());
		}
	}
}
