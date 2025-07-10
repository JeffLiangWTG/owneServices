using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public class ReconEntryOriginalCharge : CusCodeData, IFee
	{
		public new class Schema : CusCodeData.Schema
		{
			public const string CY_Amount = "CY_Amount";
			public const string CY_Description = "CY_Description";
			public const string CY_SelectedRateType = "CY_SelectedRateType";
		}

		public ReconEntryOriginalCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region CY_Code

		[List(nameof(Lookups) + "." + nameof(ReconEntryOriginalChargeLookups.CY_CodeList))]
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
						Parent.UpdateChargeDetails();
					}
				}
			}
		}

		void UpdateCY_SelectedRateTypeBasedOnDutyRateType()
		{
			if (!IsSpecificSpecificTaxFee)
			{
				CY_SelectedRateType = ZString.Empty;
			}
		}

		public bool CY_Code_ReadOnly => !CY_IsOverridden && CY_ParentTableCode == CusEntryHeaderSchema.Constants.Prefix && !Parent.DefaultValueForOverridenForNewChild;

		#endregion

		#region CY_Amount

		[DecimalPlaces(2)]
		public ZDecimal CY_Amount
		{
			get { return ZDecimal.ParseSafe(CY_Data.SubstringSafe(1), ZDecimal.Zero); }
			set
			{
				var hasChanges = CY_Amount != value;
				UpdateCY_Data(CY_SelectedRateType, value);
				CY_AmountInfo.RefreshBinding();

				if (hasChanges && !IsCopying)
				{
					RefreshEntryWithTotalOriginalCustomsFees();
					RefreshMPC();
					if (Parent != null)
					{
						Parent.UpdateChargeDetails();
					}
				}
				Validation.ValidateCY_Amount();
			}
		}

		public ZPropertyInfo CY_AmountInfo
		{
			get { return GetZPropertyInfo(Schema.CY_Amount); }
		}

		public bool CY_Amount_ReadOnly
		{
			get
			{
				return !CY_IsOverridden
					&& (Parent == null || Parent.ShouldCalculateOrigDuty || Not499ReadOnlyForEntryHeader || _499ReadOnlyForEntryHeader)
					&& !IsMerchandiseProcessingMonthlyFiling;
			}
		}

		#endregion

		public override ZBool CY_IsOverridden
		{
			get { return base.CY_IsOverridden; }
			set
			{
				var oldValue = base.CY_IsOverridden;
				base.CY_IsOverridden = value;
				if (oldValue != base.CY_IsOverridden && !IsCopying && Parent != null)
				{
					Parent.UpdateChargeDetails();
				}
			}
		}

		bool IsMerchandiseProcessingMonthlyFiling => CY_Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing && Parent.MonthlyFiling;

		bool Not499ReadOnlyForEntryHeader => CY_ParentTableCode == CusEntryHeaderSchema.Constants.Prefix
									&& CY_Code != Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing
									&& !Parent.DefaultValueForOverridenForNewChild;

		bool _499ReadOnlyForEntryHeader => CY_ParentTableCode == CusEntryHeaderSchema.Constants.Prefix
									&& CY_Code == Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing
									&& !Parent.MonthlyFiling
									&& !Parent.DefaultValueForOverridenForNewChild;

		#region CY_SelectedRateType

		[ReadOnlyMember(nameof(CY_SelectedRateType_ReadOnly))]
		[MaxLength(1)]
		public ZString CY_SelectedRateType
		{
			get { return CY_Data.Left(1).TrimEnd(' '); }
			set
			{
				CheckMaximumLength(CY_SelectedRateTypeInfo, value);
				UpdateCY_Data(value, CY_Amount);
				CY_SelectedRateTypeInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					Validation.ValidateCY_SelectedRateType();
				}
			}
		}

		public bool CY_SelectedRateType_ReadOnly
		{
			get { return !IsSpecificSpecificTaxFee || (Parent is JobComInvoiceLine line && line.US_R_OrigTaxApply == TaxApplyList.Codes.Override); }
		}

		public ZPropertyInfo CY_SelectedRateTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CY_SelectedRateType); }
		}

		#endregion

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

		public USCTariffDutyRate TariffDutyRate
		{
			get { return (Parent != null && Parent.Tariff != null) ? Parent.Tariff.DutyRates.GetRateForTaxFeeClassCode(CY_Code) : null; }
		}

		public bool IsSpecificSpecificTaxFee
		{
			get
			{
				USCTariffDutyRate tariffDutyRate = TariffDutyRate;
				return (tariffDutyRate != null && tariffDutyRate.IsSpecificSpecificTaxFee);
			}
		}

		void UpdateCY_Data(ZString selectedRateType, ZDecimal feeAmount)
		{
			CY_Data = selectedRateType.Left(1).PadRight(1) + feeAmount.ToString(5);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.ReconEntryOriginalCharge;
		}

		protected override TypeLoaderCollection parentLoaders
		{
			get { return new TypeLoaderCollection(typeof(CusEntryHeader), typeof(JobComInvoiceLine)); }
		}

		protected override CusCodeDataLookups GetNewLookups()
		{
			return new ReconEntryOriginalChargeLookups(this);
		}

		protected override CusCodeDataValidation GetNewValidation()
		{
			return new ReconEntryOriginalChargeValidation(this);
		}

		public new ReconEntryOriginalChargeLookups Lookups
		{
			get { return (ReconEntryOriginalChargeLookups)base.Lookups; }
		}

		public new ReconEntryOriginalChargeValidation Validation
		{
			get { return (ReconEntryOriginalChargeValidation)base.Validation; }
		}

		public new IReconOriginalChargeParent Parent
		{
			get { return (IReconOriginalChargeParent)base.Parent; }
			set { base.Parent = (BusinessObject)value; }
		}

		#region Overriden Methods

		public override void Delete()
		{
			RefreshEntryWithTotalOriginalCustomsFees();
			base.Delete();
		}

		#endregion

		#region New Methods

		void RefreshEntryWithTotalOriginalCustomsFees()
		{
			var invoiceLine = Parent as JobComInvoiceLine;
			if (invoiceLine == null && CY_ParentID.IsEmpty && !CY_ParentIDInfo.OriginalValue.IsEmpty)
			{
				invoiceLine = Factory.Load<JobComInvoiceLine>((ZGuid)CY_ParentIDInfo.OriginalValue);
			}

			if (invoiceLine != null && invoiceLine.InvoiceHeader != null)
			{
				var reconOriginalEntry = invoiceLine.InvoiceHeader.ReconOriginalEntry;
				if (reconOriginalEntry != null)
				{
					reconOriginalEntry.RefreshEntryWithTotalOriginalCustomsFees();
				}
			}
		}

		void RefreshMPC()
		{
			if (CY_Code == Core.Constants.USCustoms.FeeCodes.MPC)
			{
				var entryHeader = Parent as CusEntryHeader;
				if (entryHeader != null)
				{
					var reconOriginalEntry = entryHeader.ReconOriginalEntry;
					if (reconOriginalEntry != null)
					{
						reconOriginalEntry.MPCInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region IFee Members

		ZString IFee.Code
		{
			get { return CY_Code; }
			set { CY_Code = value; }
		}

		ZDecimal IFee.Amount
		{
			get { return CY_Amount; }
			set { CY_Amount = value; }
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
