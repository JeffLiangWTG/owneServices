using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public partial class BaseCusClassPartPivot
	{
		public new partial class Schema : AutoCusClassPartPivot.Schema
		{
			public const string NewLookUpPK = "NewLookUpPK";
			public const string NewLookUpCode = "NewLookUpCode";
			public const string NewTariffNum = "NewTariffNum";
			public const string OldClassificationCode = "OldClassificationCode";
			public const string OldTariffCode = "OldTariffCode";
		}

		#region TariffBulkChange

		public ZGuid NewLookUpPK
		{
			get
			{
				return TBCPivotManager != null ? TBCPivotManager.NewLookupCode : ZGuid.Empty;
			}
			set
			{
				if (TBCPivotManager != null)
				{
					TBCPivotManager.NewLookupCode = value;
					fNewClassification = null;
				}
				NewLookUpPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NewLookUpPKInfo
		{
			get { return GetZPropertyInfo(Schema.NewLookUpPK); }
		}

		[MaxLength(BaseCusClassification.Schema.CC_TariffNumMaxLength)]
		public ZString NewTariffNum
		{
			get
			{
				if (TBCPivotManager != null && !TBCPivotManager.NewTariffNum.IsEmpty)
				{
					return TBCPivotManager.NewTariffNum;
				}
				else if (NewClassification != null)
				{
					return NewClassification.NewTariffNum.IsEmpty ? NewClassification.CC_FormattedTariffNum : NewClassification.NewTariffNum;
				}
				else if (OrigClassification != null)
				{
					return OrigClassification.NewTariffNum;
				}
				else
				{
					return ZString.Empty;
				}
			}
			set
			{
				if (TBCPivotManager != null)
				{
					CheckMaximumLength(NewTariffNumInfo, value);
					TBCPivotManager.NewTariffNum = value;
				}
				this.NewTariffNumInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo NewTariffNumInfo
		{
			get { return GetZPropertyInfo(Schema.NewTariffNum); }
		}

		[MaxLength(BaseCusClassification.Schema.CC_LookupCodeMaxLength)]
		public ZString NewLookUpCode
		{
			get
			{
				if (TBCPivotManager != null && !TBCPivotManager.NewTariffNum.IsEmpty)
				{
					return ZString.Empty;
				}
				else if (NewClassification != null)
				{
					return NewClassification.NewLookupCode.IsEmpty ? NewClassification.CC_LookupCode : NewClassification.NewLookupCode;
				}
				else if (OrigClassification != null)
				{
					return OrigClassification.NewLookupCode;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public ZPropertyInfo NewLookUpCodeInfo
		{
			get { return GetZPropertyInfo(Schema.NewLookUpCode); }
		}

		[MaxLength(BaseCusClassification.Schema.CC_LookupCodeMaxLength)]
		public ZString OldClassificationCode
		{
			get { return OrigClassification != null ? OrigClassification.CC_LookupCode : ZString.Empty; }
		}

		public ZPropertyInfo OldClassificationCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OldClassificationCode); }
		}

		[MaxLength(BaseCusClassification.Schema.CC_TariffNumMaxLength)]
		public ZString OldTariffCode
		{
			get { return OrigClassification != null ? OrigClassification.CC_FormattedTariffNum : CI_FormattedTariffNum; }
		}

		public ZPropertyInfo OldTariffCodeInfo
		{
			get { return GetZPropertyInfo(Schema.OldTariffCode); }
		}

		public TariffBulkChange.TBCClassification NewClassification
		{
			get { return fNewClassification ?? (fNewClassification = Factory.Load<TariffBulkChange.TBCClassification>(NewLookUpPK)); }
		}
		TariffBulkChange.TBCClassification fNewClassification;

		TariffBulkChange.TBCClassification OrigClassification
		{
			get { return fOrigClassification ?? (fOrigClassification = Factory.Load<TariffBulkChange.TBCClassification>(CI_CC)); }
		}
		TariffBulkChange.TBCClassification fOrigClassification;

		public BaseTariffBulkChange.TBCPivotManager TBCPivotManager
		{
			get { return fTBCPivotManager ?? (fTBCPivotManager = new BaseTariffBulkChange.TBCPivotManager(this)); }
		}
		BaseTariffBulkChange.TBCPivotManager fTBCPivotManager;

		#endregion

		#region ForTesting
#if DEBUG
		public void ResetTBCCacheForTesting()
		{
			fNewClassification = null;
			fOrigClassification = null;
			fTBCPivotManager = null;
		}
#endif
		#endregion
	}
}
