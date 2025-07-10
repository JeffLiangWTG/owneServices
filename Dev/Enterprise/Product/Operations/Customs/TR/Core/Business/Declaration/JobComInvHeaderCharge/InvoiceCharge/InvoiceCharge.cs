using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.TR.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ReadOnlyMember(nameof(ChargeTypeReadonly))]
		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var oldValue = base.J7_ChargeType;
				base.J7_ChargeType = value;
				if (oldValue != value && !IsCopying)
				{
					if (value == TRIncotermChargeCodeList.Codes.LocalTotalCharges || TRIncotermChargeCodeList.IsLocalCharge(value))
					{
						J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Turkey;
					}
					else if (value == TRIncotermChargeCodeList.Codes.TotalForeignCharges || TRIncotermChargeCodeList.IsForeignCharge(value))
					{
						J7_RX_NKCurrency = Invoice.JZ_RX_NKInvoice_Currency;
					}

					if (Invoice is JobComInvoiceHeader invoice)
					{
						invoice.OnChargeTypeSet(this);
					}
				}
			}
		}

		public bool ChargeTypeReadonly => true;

		public override ZDecimal J7_Amount
		{
			get => base.J7_Amount;
			set
			{
				var oldValue = base.J7_Amount;
				base.J7_Amount = value;
				if (oldValue != value && !IsCopying)
				{
					if (Invoice is JobComInvoiceHeader invoice)
					{
						if (this.IsForeignCharge())
						{
							invoice.ForeignChargesEnteredChanged();
						}
						else if (this.IsLocalCharge())
						{
							invoice.LocalChargesEnteredChanged();
						}
					}
				}
			}
		}

		public override ZDecimal J7_Percentage
		{
			get => base.J7_Percentage;
			set
			{
				var oldValue = base.J7_Percentage;
				base.J7_Percentage = value;
				if (oldValue != value && !IsCopying)
				{
					Invoice.Charges.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("AA966C73-5D1D-4E09-B901-FF3DAFF9F2AF", ShortCaption = "Desc.", Caption = "Description")]
		public new ZString ChargeCodeDescription => base.ChargeCodeDescription;

		[ResourceStringData("AEAEBBB7-0B66-499F-9211-4E204D8F3C04", ShortCaption = "In Invoice Amt", Caption = "Included in Invoice Amount")]
		public override ZBool J7_Calc_IsIncludedInInvoiceAmount { get => base.J7_Calc_IsIncludedInInvoiceAmount; set => base.J7_Calc_IsIncludedInInvoiceAmount = value; }

		[ResourceStringData("71F33E23-D4D0-4767-AAB9-22FD015BEE06", ShortCaption = "In FOB", Caption = "Included In FOB")]
		public override ZBool J7_IsStatisticalValueApplicable { get => base.J7_IsStatisticalValueApplicable; set => base.J7_IsStatisticalValueApplicable = value; }

		protected override bool GetJ7_IsStatisticalValueApplicable_ReadOnly() => J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges || base.GetJ7_IsStatisticalValueApplicable_ReadOnly();

		protected override bool GetJ7_RX_NKCurrency_ReadOnly()
			=> base.GetJ7_RX_NKCurrency_ReadOnly() || J7_ChargeType == TRIncotermChargeCodeList.Codes.LocalTotalCharges || this.IsLocalCharge();

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly() => this.IsTotalCharge() || base.GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly();
	}
}
