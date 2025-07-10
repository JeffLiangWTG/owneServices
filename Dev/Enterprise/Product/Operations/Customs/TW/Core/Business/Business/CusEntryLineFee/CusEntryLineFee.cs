using System;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.TW.Business
{
	public partial class CusEntryLineFee : AutoCusEntryLineFee
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool ShouldDeleteIfChargeAmountIsZero => !IsDuty && base.ShouldDeleteIfChargeAmountIsZero;

		protected override bool ShouldResetDataOnMergingCore => false;

		bool IsDuty => !IsDeleted && (CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTA || CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.DTS);

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		public override ZString CF_ChargeType
		{
			get => base.CF_ChargeType;
			set
			{
				var oldValue = CF_ChargeType;
				base.CF_ChargeType = value;
				if (oldValue != CF_ChargeType && !IsCopying)
				{
					SetPaymentMethod();
				}
			}
		}

		void SetPaymentMethod()
		{
			if (!IsActionBlank && !IsExport)
			{
				if (IsCashOnlyChargeTypes)
				{
					TW_MethodOfPayment = DutyTaxPaymentMethodList.Codes.CashPayment;
				}
				else
				{
					TW_MethodOfPayment = ZString.Empty;
				}
			}
		}

		public bool IsActionBlank => CF_RateOverrideReasonCode.IsEmpty;

		public bool MethodOfPaymentReadOnly => IsExport || IsCashOnlyChargeTypes || TW_RateOverride.IsEmpty;

		ZBool IsCashOnlyChargeTypes
		{
			get
			{
				var chargeType = CF_ChargeType;
				switch (chargeType)
				{
					case UniversalReferenceConstants.RefCusRateCodes.ADD:
					case UniversalReferenceConstants.RefCusRateCodes.RTD:
					case UniversalReferenceConstants.RefCusRateCodes.CVD:
					case UniversalReferenceConstants.RefCusRateCodes.ADT:
						return true;
					default:
						return false;
				}
			}
		}

		ZBool IsExport => EntryLine?.Header?.IsExport ?? false;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_MethodOfPayment", Caption = "Method of Payment", FullDescription = "The payment method of the duties, taxes and fees.")]
		[ReadOnlyMember(nameof(MethodOfPaymentReadOnly))]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfPaymentList))]
		public ZString TW_MethodOfPayment
		{
			get => CF_MethodOfPayment;
			set => CF_MethodOfPayment = value;
		}

		public ZPropertyInfo TW_MethodOfPaymentInfo => CF_MethodOfPaymentInfo;

		public bool RateDutyReadOnly => IsActionBlank || IsExport;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_RateDuty", Caption = "Method of Calculation", FullDescription = "Calculation Method for duties, taxes and Fees.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfCalculationList))]
		[ReadOnlyMember(nameof(RateDutyReadOnly))]
		public ZString TW_RateDuty
		{
			get => CF_MethodOfCalculation;
			set => CF_MethodOfCalculation = value;
		}

		public ZPropertyInfo TW_RateDutyInfo => CF_MethodOfCalculationInfo;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_RateSuspension", Caption = "Tax Rate", FullDescription = "Rate of duties, taxes and Fees.")]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZString TW_RateSuspension
		{
			get => (CF_Rate.IsEmpty ? ZDecimal.Zero : CF_Rate).ToString("0.000000#");
			set => CF_Rate = ZDecimal.ParseSafe(value, 0m);
		}

		public ZPropertyInfo TW_RateSuspensionInfo => CF_RateInfo;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_RateOverride", Caption = "Action", FullDescription = "Action Type.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.RateOverrideReasonList))]
		public ZString TW_RateOverride
		{
			get => CF_RateOverrideReasonCode;
			set => CF_RateOverrideReasonCode = value;
		}

		public ZPropertyInfo TW_RateOverrideInfo => CF_RateOverrideReasonCodeInfo;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_Type", Caption = "Type", FullDescription = "The code of the duties, taxes and fees.")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZString TW_Type
		{
			get => CF_ChargeType;
			set => CF_ChargeType = value;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_TypeDescription", Caption = "Description", FullDescription = "The description of the duties, taxes and fees.")]
		public ZString TW_TypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(TW_Type);

		public ZPropertyInfo TW_TypeInfo => CF_ChargeTypeInfo;

		public bool TotalAmountReadOnly => !(CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Additional ||
				CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Override);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_Amount", Caption = "Total Amount", FullDescription = "The amount of the duties, taxes and fees.")]
		[ReadOnlyMember(nameof(TotalAmountReadOnly))]
		public ZString TW_Amount
		{
			get => CF_ChargeAmount.IsEmpty ? "0.0000" : CF_ChargeAmount.ToString("#.0000", CultureInfo.CurrentCulture);
			set => CF_ChargeAmount = ZDecimal.ParseSafe(value, 0m);
		}

		public ZPropertyInfo TW_AmountInfo => CF_ChargeAmountInfo;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusEntryLineFee|TW_BaseAmount", Caption = "Base Amount", FullDescription = "Base Amount for duties, taxes and Fees.")]
		[DecimalPlaces(4)]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZDecimal TW_BaseAmount
		{
			get => CF_BaseValue;
			set => CF_BaseValue = value;
		}

		public ZPropertyInfo TW_BaseAmountInfo => CF_BaseValueInfo;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CF_RateOverrideReasonCodeInfo.ValueChanged += CF_RateOverrideReasonCodeInfo_ValueChanged;
			CF_RateOverrideReasonCode = TWRateOverrideReasonList.Codes.Additional;
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			CF_RateOverrideReasonCodeInfo.ValueChanged += CF_RateOverrideReasonCodeInfo_ValueChanged;
			CF_RateOverrideReasonCodeInfo_ValueChanged(null, null);
		}

		void CF_RateOverrideReasonCodeInfo_ValueChanged(object sender, EventArgs e)
		{
			if (CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Additional ||
				CF_RateOverrideReasonCode == TWRateOverrideReasonList.Codes.Override)
			{
				AttachManualCalculation();
			}
			else
			{
				DetachManualCalculation();
			}
		}

		void AttachManualCalculation()
		{
			CF_BaseValueInfo.ValueChanged += CalculateManualTotal;
			CF_MethodOfCalculationInfo.ValueChanged += CalculateManualTotal;
			CF_RateInfo.ValueChanged += CalculateManualTotalOnRateChanged;
		}

		void DetachManualCalculation()
		{
			CF_BaseValueInfo.ValueChanged -= CalculateManualTotal;
			CF_MethodOfCalculationInfo.ValueChanged -= CalculateManualTotal;
			CF_RateInfo.ValueChanged -= CalculateManualTotalOnRateChanged;
		}

		void CalculateManualTotal(object sender, EventArgs args)
		{
			if (CF_ChargeAmount.IsEmpty)
			{
				CF_ChargeAmount = CF_BaseValue * CF_Rate;
			}
		}

		void CalculateManualTotalOnRateChanged(object sender, EventArgs args)
		{
			CF_ChargeAmount = CF_BaseValue * CF_Rate;
		}
	}
}
