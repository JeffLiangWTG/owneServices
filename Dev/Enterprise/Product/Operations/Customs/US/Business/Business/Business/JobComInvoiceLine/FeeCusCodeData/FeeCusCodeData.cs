using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FeeCusCodeData : Customs.Business.CusCodeData, IFee
	{
		public FeeCusCodeData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : Customs.Business.CusCodeData.Schema
		{
			public const string CY_FeeAmount = "CY_FeeAmount";
			public const string CY_Description = "CY_Description";
			public const string CY_SelectedRateType = "CY_SelectedRateType";
			public const string CY_SelectedRate = "CY_SelectedRate";
			public const int CY_FeeAmountDecimalPlaces = 5;
		}

		#region CY_Description

		public ZString CY_Description
		{
			get { return Lookups.CY_CodeList.GetDescriptionFromCode(CY_Code); }
		}

		public ZPropertyInfo CY_DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CY_Description); }
		}

		#endregion

		#region CY_FeeAmount

		[ReadOnlyMember(nameof(CY_FeeAmount_ReadOnly))]
		[DecimalPlaces(Schema.CY_FeeAmountDecimalPlaces)]
		public ZDecimal CY_FeeAmount
		{
			get { return ZDecimal.ParseSafe(CY_Data.SubstringSafe(1), ZDecimal.Zero); }
			set
			{
				var oldValue = CY_FeeAmount;
				UpdateCY_Data(CY_SelectedRateType, value.Round(Schema.CY_FeeAmountDecimalPlaces));
				if (oldValue != CY_FeeAmount && !IsCopying && Parent != null)
				{
					Parent.UpdateReconChargeDetails();
				}
				CY_FeeAmountInfo.RefreshBinding();
			}
		}

		public bool CY_FeeAmount_ReadOnly
		{
			get { return !CY_IsOverridden; }
		}

		public ZPropertyInfo CY_FeeAmountInfo
		{
			get { return GetZPropertyInfo(Schema.CY_FeeAmount); }
		}

		#endregion

		#region CY_SelectedRateType

		[ReadOnlyMember(nameof(CY_SelectedRateType_ReadOnly))]
		[MaxLength(1)]
		public ZString CY_SelectedRateType
		{
			get { return CY_Data.Left(1).TrimEnd(' '); }
			set
			{
				CheckMaximumLength(CY_SelectedRateTypeInfo, value);
				UpdateCY_Data(value, CY_FeeAmount);
				CY_SelectedRateTypeInfo.RefreshBinding();
				CY_SelectedRateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCY_SelectedRateType();
				}
			}
		}

		public bool CY_SelectedRateType_ReadOnly
		{
			get { return !IsSpecificSpecificTaxFee || (Parent is JobComInvoiceLine line && line.US_TaxApply == TaxApplyList.Codes.Override); }
		}

		public ZPropertyInfo CY_SelectedRateTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CY_SelectedRateType); }
		}

		#endregion

		#region CY_SelectedRate

		public ZString CY_SelectedRate
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsSpecificSpecificTaxFee && !CY_SelectedRateType.IsEmpty && TariffDutyRate != null)
				{
					ZDecimal rate = CY_SelectedRateType == RateTypeList.Codes.Secondary ? TariffDutyRate.UD_TaxFeeAdvalorem : TariffDutyRate.UD_TaxFeeSpecificRate;

					result = rate.ToString();
				}

				return result;
			}
		}

		public ZPropertyInfo CY_SelectedRateInfo
		{
			get { return GetZPropertyInfo(Schema.CY_SelectedRate); }
		}

		#endregion

		public override ZString CY_Code
		{
			get { return base.CY_Code; }
			set
			{
				bool isDiff = base.CY_Code != value;
				base.CY_Code = value;
				if (isDiff && !IsCopying)
				{
					UpdateCY_SelectedRateTypeBasedOnDutyRateType();
					if (Parent != null)
					{
						Parent.UpdateReconChargeDetails();
					}
				}
			}
		}

		public override ZBool CY_IsOverridden
		{
			get { return base.CY_IsOverridden; }
			set
			{
				var oldValue = base.CY_IsOverridden;
				base.CY_IsOverridden = value;

				if (oldValue != base.CY_IsOverridden && !IsCopying && Parent != null)
				{
					Parent.UpdateReconChargeDetails();
				}
			}
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
			set { base.Parent = value; }
		}

		public USCTariffDutyRate TariffDutyRate
		{
			get { return (Parent != null && Parent.ImportTariff != null) ? Parent.ImportTariff.DutyRates.GetRateForTaxFeeClassCode(CY_Code) : null; }
		}

		#region Implementation

		internal bool IsMandatory => Parent != null && !Parent.US_TaxCode.IsEmpty && Parent.IsTaxRateApplicable;

		internal bool IsSpecificSpecificTaxFee
		{
			get
			{
				USCTariffDutyRate tariffDutyRate = TariffDutyRate;
				return (tariffDutyRate != null && tariffDutyRate.IsSpecificSpecificTaxFee);
			}
		}

		void UpdateCY_SelectedRateTypeBasedOnDutyRateType()
		{
			if (!IsSpecificSpecificTaxFee)
			{
				CY_SelectedRateType = ZString.Empty;
			}
		}

		void UpdateCY_Data(ZString selectedRateType, ZDecimal feeAmount)
		{
			CY_Data = selectedRateType.Left(1).PadRight(1) + feeAmount.ToString(Schema.CY_FeeAmountDecimalPlaces);
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(JobComInvoiceLine)); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.Fee;
		}

		#endregion

		public new FeeCusCodeDataLookups Lookups
		{
			get { return (FeeCusCodeDataLookups)base.Lookups; }
		}

		protected override Customs.Business.CusCodeDataLookups GetNewLookups()
		{
			return new FeeCusCodeDataLookups(this);
		}

		public new FeeCusCodeDataValidation Validation
		{
			get { return (FeeCusCodeDataValidation)base.Validation; }
		}

		protected override Customs.Business.CusCodeDataValidation GetNewValidation()
		{
			return new FeeCusCodeDataValidation(this);
		}

		#region IFee Members

		ZString IFee.Code
		{
			get { return CY_Code; }
			set { CY_Code = value; }
		}

		ZDecimal IFee.Amount
		{
			get { return CY_FeeAmount; }
			set { CY_FeeAmount = value; }
		}

		void IFee.Delete()
		{
			Delete();
		}

		ZString IFee.SelectedRateType
		{
			get { return CY_SelectedRateType; }
			set { CY_SelectedRateType = value; }
		}

		ZBool IFee.IsOverridden => CY_IsOverridden;

		#endregion
	}
}
