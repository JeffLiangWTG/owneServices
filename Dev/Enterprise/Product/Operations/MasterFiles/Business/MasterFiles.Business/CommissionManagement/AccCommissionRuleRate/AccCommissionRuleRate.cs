using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class AccCommissionRuleRate : AutoAccCommissionRuleRate, ICommissionRateOverridable
	{
		#region Constructor

		public AccCommissionRuleRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Properties

		#region Decimals

		public int CommissionAmountDecimalPlaces => CommissionCurrency?.Decimals ?? LocalDecimals;

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int PercentageDecimals => Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages;

		#endregion

		#region ACT_CRO

		public override ZGuid ACT_ACM
		{
			get { return base.ACT_ACM; }
			set
			{
				base.ACT_ACM = value;
				if (!value.IsEmpty)
				{
					base.ACT_CRO = ZGuid.Empty;
				}
			}
		}

		public override ZGuid ACT_CRO
		{
			get { return base.ACT_CRO; }
			set
			{
				base.ACT_CRO = value;
				if (!value.IsEmpty)
				{
					base.ACT_ACM = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region RootRule

		public AccCommissionRule RootRule
		{
			get
			{
				var parentRule = CommissionRule;
				if (parentRule != null)
				{
					return parentRule;
				}

				var parentRuleOverride = CommissionRuleStaffOverride;
				if (parentRuleOverride != null)
				{
					return parentRuleOverride.ParentRule;
				}

				return null;
			}
		}

		#endregion

		#region ParentCommissionRatesProvider

		public ICommissionRuleRatesProvider ParentCommissionRatesProvider
		{
			get
			{
				if (!ACT_ACM.IsEmpty)
				{
					return CommissionRule;
				}
				else if (!ACT_CRO.IsEmpty)
				{
					return CommissionRuleStaffOverride;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region ACT_CommissionType

		[List("Lookups.CommissionTypes")]
		public override ZString ACT_CommissionType
		{
			get { return base.ACT_CommissionType; }
			set
			{
				if (base.ACT_CommissionType != value)
				{
					base.ACT_CommissionType = value;
					CommissionRuleHelper.SetCommissionTypeDefaults(this);
				}
			}
		}

		#endregion

		#region ACT_CommissionPercentage

		[DecimalPlaces(nameof(PercentageDecimals))]
		public override ZDecimal ACT_CommissionPercentage
		{
			get { return base.ACT_CommissionPercentage; }
			set { base.ACT_CommissionPercentage = value; }
		}

		protected bool ACT_CommissionPercentage_ReadOnly
		{
			get { return !this.IsPercentageCommissionType(); }
		}

		#endregion

		#region ACT_RX_NKCommissionCurrency

		[List("Lookups.CommissionCurrencies")]
		public override ZString ACT_RX_NKCommissionCurrency
		{
			get { return base.ACT_RX_NKCommissionCurrency; }
			set { base.ACT_RX_NKCommissionCurrency = value; }
		}

		protected bool ACT_RX_NKCommissionCurrency_ReadOnly
		{
			get { return !this.IsCurrencyCommissionType(); }
		}

		#endregion

		#region ACT_CommissionAmount

		[DecimalPlaces(nameof(CommissionAmountDecimalPlaces))]
		public override ZDecimal ACT_CommissionAmount
		{
			get { return base.ACT_CommissionAmount; }
			set { base.ACT_CommissionAmount = value; }
		}

		protected bool ACT_CommissionAmount_ReadOnly
		{
			get { return !this.IsAmountCommissionType(); }
		}

		#endregion

		#region ACT_CommissionPeriod

		[List("Lookups.EnabledCommissionPeriods")]
		public override ZString ACT_CommissionPeriod
		{
			get { return base.ACT_CommissionPeriod; }
			set { base.ACT_CommissionPeriod = value; }
		}

		public ZString ACT_CommissionPeriodDescription
		{
			get { return this.GetCommissionPeriodDescription(); }
		}

		#endregion

		#endregion

		#region ICommissionRateOverridable Members

		GlbCompany ICommissionRateOverridable.Company
		{
			get
			{
				var rootRule = RootRule;
				return rootRule != null ? rootRule.Company : null;
			}
		}

		ZString ICommissionRateOverridable.CommissionType
		{
			get { return ACT_CommissionType; }
			set { ACT_CommissionType = value; }
		}

		ZDecimal ICommissionRateOverridable.CommissionPercentage
		{
			get { return ACT_CommissionPercentage; }
			set { ACT_CommissionPercentage = value; }
		}

		ZString ICommissionRateOverridable.CommissionCurrency
		{
			get { return ACT_RX_NKCommissionCurrency; }
			set { ACT_RX_NKCommissionCurrency = value; }
		}

		ZDecimal ICommissionRateOverridable.CommissionAmount
		{
			get { return ACT_CommissionAmount; }
			set { ACT_CommissionAmount = value; }
		}

		ZString ICommissionRateOverridable.CommissionPeriod
		{
			get { return ACT_CommissionPeriod; }
			set { ACT_CommissionPeriod = value; }
		}

		#endregion

		#region Test
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			if (ACT_ACM.IsEmpty)
			{
				var rule = Factory.NewWithValidTestData<AccCommissionRule>();
				rule.ACM_GS_NKStaff = GlbStaff.CurrentUser.GS_Code;
				ACT_ACM = rule.PK;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

#endif
		#endregion
	}
}
