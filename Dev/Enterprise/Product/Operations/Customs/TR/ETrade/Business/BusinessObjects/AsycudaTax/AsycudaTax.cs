using System;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaTax : ASYCUDA.Business.AsycudaTax
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			this.AET_MethodOfCalculation = "%";
		}

		public new partial class Schema : ManifestBase.AutoAsycudaTax.Schema
		{
			public const string AET_TypeDescription = "AET_TypeDescription";
		}

		[MaxLength(2)]
		[ResourceStringData("AsycudaTax.AET_ChargeType", Caption = "Tax Code")]
		public override ZString AET_ChargeType { get => base.AET_ChargeType; set => base.AET_ChargeType = value; }

		[ResourceStringData("AsycudaTax.AET_TypeDescription", Caption = "Description")]
		public ZString AET_TypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(AET_ChargeType) ?? ZString.Empty;

		[ResourceStringData("AsycudaTax.AET_RateOverrideReasonCode", Caption = "Overridden")]
		public override ZString AET_RateOverrideReasonCode
		{
			get => base.AET_RateOverrideReasonCode;
			set => base.AET_RateOverrideReasonCode = value;
		}

		[ReadOnlyMember(nameof(ReadOnlyStatusForAET_RateOverrideReasonCode))]
		[DecimalPlaces(2)]
		[ResourceStringData("AsycudaTax.AET_BaseValue", Caption = "Tax Base")]
		public override ZDecimal AET_BaseValue
		{
			get => base.AET_BaseValue;
			set
			{
				base.AET_BaseValue = value;
				CalculateAmount();
				AET_BaseValueInfo.RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(ReadOnlyStatusForAET_RateOverrideReasonCode))]
		[DecimalPlaces(2)]
		[ResourceStringData("AsycudaTax.AET_Rate", Caption = "Tax Percent/Quantity", ShortCaption = "Tax Percent/Qty")]
		public override ZDecimal AET_Rate
		{
			get => base.AET_Rate;
			set
			{
				base.AET_Rate = value;
				CalculateAmount();
				AET_RateInfo.RefreshBinding();
			}
		}

		[ReadOnlyMember(nameof(ReadOnlyStatusForAET_RateOverrideReasonCode))]
		[ResourceStringData("AsycudaTax.AET_ChargeAmount", Caption = "Tax Amount")]
		[DecimalPlaces(2)]
		public override ZDecimal AET_ChargeAmount
		{
			get => base.AET_ChargeAmount;
			set
			{
				base.AET_ChargeAmount = value;
				CalculateRate();
				AET_ChargeAmountInfo.RefreshBinding();
			}
		}

		[MaxLength(3)]
		[ResourceStringData("AsycudaTax.AET_MethodOfPayment", Caption = "Payment Type")]
		public override ZString AET_MethodOfPayment { get => base.AET_MethodOfPayment; set => base.AET_MethodOfPayment = value; }

		public ZBool IsStampTax => AET_ChargeType == TaxCodeList.Codes.StampTax;
		public ZBool IsTRTBandrolTax => AET_ChargeType == TaxCodeList.Codes.TRTBandrol;

		public new AsycudaTaxValidation Validation => (AsycudaTaxValidation)base.Validation;
		protected override ManifestBase.AsycudaTaxValidation GetNewValidation() => new AsycudaTaxValidation(this);
		public new AsycudaTaxLookups Lookups => (AsycudaTaxLookups)base.Lookups;
		protected override ManifestBase.AsycudaTaxLookups GetNewLookups() => new AsycudaTaxLookups(this);

		void CalculateAmount()
		{
			if (!IsOnCalculation)
			{
				using (SuspendOnCalculation())
				{
					if (AET_Rate != 0)
					{
						AET_ChargeAmount = IsTRTBandrolTax ? new ZDecimal(AET_BaseValue * AET_Rate) : new ZDecimal(AET_BaseValue * AET_Rate / 100.0m);

						AppendLog();
					}
				}
			}
		}

		void CalculateRate()
		{
			if (!IsOnCalculation)
			{
				using (SuspendOnCalculation())
				{
					if (!AET_ChargeAmount.IsEmpty && !AET_BaseValue.IsEmpty)
					{
						AET_Rate = IsTRTBandrolTax ? new ZDecimal(AET_ChargeAmount / AET_BaseValue) : new ZDecimal(AET_ChargeAmount / AET_BaseValue * 100.0m);
					}
				}
			}
		}

		int inCalculation;
		public bool IsOnCalculation => inCalculation > 0;
		public IDisposable SuspendOnCalculation() => new DisposableAction(() => inCalculation++, () => inCalculation--);

		void AppendLog()
		{
			if (AET_ChargeAmount != (ZDecimal)AET_ChargeAmountInfo.OriginalValue)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, GetUpdateMessage(AET_ChargeAmountInfo.OriginalValue.ToString(), AET_ChargeAmount.ToString(), Schema.AET_ChargeAmount));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		ZString GetUpdateMessage(string originalValue, string newValue, string fieldName)
		{
			return ZString.Format((NoResString)"{0} has been updated. Was {1} Now {2}", fieldName, originalValue, newValue);
		}

		ZBool ReadOnlyStatusForAET_RateOverrideReasonCode => AET_RateOverrideReasonCode == ManifestBase.RateOverrideReasonCodeList.Codes.Override ? ZBool.False : ZBool.True;
	}
}
