using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class CusEntryHeaderCharges : EU.Business.Declaration.CusEntryHeaderCharges, Integration.Customs.TR.ICusEntryHeaderCharges
	{
		public CusEntryHeaderCharges(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("567A33E0-3219-4FEE-A5EF-39795D711ADA", Caption = "Total Payment")]
		[DecimalPlaces(nameof(C1_ChargeAmountDecimalPlaces))]
		[ReadOnlyMember(nameof(C1_ChargeAmount_ReadOnly))]
		public override ZDecimal C1_ChargeAmount
		{
			get => base.C1_ChargeAmount;
			set => base.C1_ChargeAmount = value;
		}
		public bool C1_ChargeAmount_ReadOnly => IsExporterUnionType;
		public int C1_ChargeAmountDecimalPlaces => IsExporterUnionType ? 2 : 4;

		public bool IsExporterUnionType => C1_ChargeType == ExporterUnionType;
		public bool IsStampDutyType => C1_ChargeType == StampDutyType;
		const string ExporterUnionType = "EXU";
		const string StampDutyType = "89";

		[ResourceStringData("080A8A90-9E16-43C4-AD12-8F0191D43369", Caption = "Payment Type")]
		[ReadOnlyMember(nameof(C1_MethodOfPayment_ReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.PaymentMethodsList))]
		public override ZString C1_MethodOfPayment
		{
			get => base.C1_MethodOfPayment;
			set => base.C1_MethodOfPayment = value;
		}

		public bool C1_MethodOfPayment_ReadOnly => IsExporterUnionType;

		public ZString DescriptionOfChargeType => Lookups.TaxOrFeeCodeList.GetDescriptionFromCode(C1_ChargeType);

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.RateOverrideReasonCodeList))]
		[ResourceStringData("E1AC45FA-8394-43D7-9A2F-8D218C8BB3C0", Caption = "Action")]
		public override ZString C1_RateOverrideReasonCode { get => base.C1_RateOverrideReasonCode; set => base.C1_RateOverrideReasonCode = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryHeaderChargesLookups.TaxOrFeeCodeList))]
		public override ZString C1_ChargeType { get => base.C1_ChargeType; set => base.C1_ChargeType = value; }

		public new CusEntryHeaderChargesValidation Validation => (CusEntryHeaderChargesValidation)base.Validation;

		public new CusEntryHeaderChargesLookups Lookups => (CusEntryHeaderChargesLookups)base.Lookups;

		protected override Customs.Business.CusEntryHeaderChargesLookups GetNewLookups()
		{
			return new CusEntryHeaderChargesLookups(this);
		}

		protected override Customs.Business.CusEntryHeaderChargesValidation GetNewValidation()
		{
			return new CusEntryHeaderChargesValidation(this);
		}

		public new CusEntryHeader EntryHeader => (CusEntryHeader)base.EntryHeader;
	}
}
