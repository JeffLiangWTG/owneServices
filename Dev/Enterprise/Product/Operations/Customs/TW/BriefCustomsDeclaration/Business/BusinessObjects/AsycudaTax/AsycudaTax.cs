using System;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaTax : ASYCUDA.Business.AsycudaTax
	{
		public AsycudaTax(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new AsycudaTaxLookups Lookups => (AsycudaTaxLookups)base.Lookups;

		protected override ManifestBase.AsycudaTaxLookups GetNewLookups() => new AsycudaTaxLookups(this);

		protected override ManifestBase.AsycudaTaxValidation GetNewValidation() => new AsycudaTaxValidation(this);

		[ResourceStringData("0BC96921-9D8A-475C-92A3-5F898D5AE683", Caption = "Action")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.RateOverrideReasonCodeList))]
		public override ZString AET_RateOverrideReasonCode
		{
			get => base.AET_RateOverrideReasonCode;
			set => base.AET_RateOverrideReasonCode = value;
		}

		[ResourceStringData("C62B6B05-5980-4837-AF09-30D07AD14109", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.MethodOfPaymentList))]
		public override ZString AET_MethodOfPayment
		{
			get => base.AET_MethodOfPayment;
			set
			{
				if (base.AET_MethodOfPayment != value)
				{
					base.AET_MethodOfPayment = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateAET_ChargeType();
					}
				}
			}
		}

		public bool AET_ChargeTypeReadOnly => AET_MethodOfPayment.IsEmpty;

		[ResourceStringData("69A81115-EDE8-49DE-A6B9-29A82EE0479A", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.ChargeTypeList))]
		[ReadOnlyMember(nameof(AET_ChargeTypeReadOnly))]
		public override ZString AET_ChargeType
		{
			get => base.AET_ChargeType;
			set => base.AET_ChargeType = value;
		}

		[ResourceStringData("80BC57B0-5D94-4DF4-BD68-9BE844C19A16", Caption = "Description")]
		[ReadOnly(true)]
		public ZString ChargeTypeDescription => Lookups.ChargeTypeAllList.GetDescriptionFromCode(base.AET_ChargeType) ?? ZString.Empty;

		public ZPropertyInfo ChargeTypeDescriptionInfo => GetZPropertyInfo(nameof(ChargeTypeDescription));

		public bool AET_ChargeAmountReadOnly => AET_ChargeType.IsEmpty;

		[ResourceStringData("704E273F-83A1-4DC7-84A6-BC6260A1B705", Caption = "Amount")]
		[ReadOnlyMember(nameof(AET_ChargeAmountReadOnly))]
		public override ZDecimal AET_ChargeAmount
		{
			get => base.AET_ChargeAmount;
			set => base.AET_ChargeAmount = value;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AET_MethodOfCalculation = UniversalReferenceConstants.MethodOfCalculation.Percentage;
		}

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax|AET_BaseValue", Caption = "Base Amount", MediumCaption = "Base Amount", ShortCaption = "Base Amount", FullDescription = "Base Amount for duties, taxes and fees.")]
		public override ZDecimal AET_BaseValue
		{
			get => base.AET_BaseValue;
			set
			{
				var oldValue = AET_BaseValue;
				base.AET_BaseValue = value;
				if (!IsCopying && oldValue != AET_BaseValue)
				{
					CalculateChargeAmountIfNeeded();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaTaxLookups.MethodOfCalculationList))]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax|AET_MethodOfCalculation", Caption = "Method of Calculation", MediumCaption = "Method of Calculation", ShortCaption = "Method of Calculation", FullDescription = "Calculation Method for duties, taxes and fees.")]
		public override ZString AET_MethodOfCalculation { get => base.AET_MethodOfCalculation; set => base.AET_MethodOfCalculation = value; }

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaTax|AET_Rate", Caption = "Rate", MediumCaption = "Rate", ShortCaption = "Rate", FullDescription = "Rate of duties, taxes and fees.")]
		public override ZDecimal AET_Rate
		{
			get => base.AET_Rate;
			set
			{
				var oldValue = AET_Rate;
				base.AET_Rate = value;
				if (!IsCopying && oldValue != AET_Rate)
				{
					CalculateChargeAmount();
				}
			}
		}

		void CalculateChargeAmountIfNeeded()
		{
			if (AET_ChargeAmount.IsEmpty)
			{
				CalculateChargeAmount();
			}
		}

		void CalculateChargeAmount()
		{
			AET_ChargeAmount = Math.Floor(AET_BaseValue * AET_Rate);
		}
	}
}
