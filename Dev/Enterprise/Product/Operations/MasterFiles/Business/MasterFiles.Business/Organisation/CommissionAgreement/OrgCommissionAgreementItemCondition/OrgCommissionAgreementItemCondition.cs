using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCommissionAgreementItemCondition : AutoOrgCommissionAgreementItemCondition, ICommissionAgreementRelated<OrgCommissionAgreementItemCondition>
	{
		public OrgCommissionAgreementItemCondition(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CIC_Mode = OrgCommissionAgreementItemLookups.AllModesCode;
		}

		public override bool CanDelete
		{
			get
			{
				if (Parent == null)
				{
					return true;
				}

				return Parent.ConditionCollection.Count > 1;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("07D80280-5636-46A6-9CC1-5A536FA8DD5B", "Product of type {0} should have at least one Item Condition.", Parent.CAI_Code); }
		}

		public OrgCommissionAgreementItem Parent
		{
			get { return Factory.Load<OrgCommissionAgreementItem>(CIC_CAI); }
		}

		[List("Lookups.Modes")]
		public override ZString CIC_Mode
		{
			get { return base.CIC_Mode; }
			set
			{
				base.CIC_Mode = value;
				RefreshRatesAndTriggers();
			}
		}

		[List("Lookups.Locations")]
		public override ZString CIC_RL_NKOrigin
		{
			get { return base.CIC_RL_NKOrigin; }
			set
			{
				base.CIC_RL_NKOrigin = value;
				RefreshRatesAndTriggers();
			}
		}

		[List("Lookups.Locations")]
		public override ZString CIC_RL_NKDestination
		{
			get { return base.CIC_RL_NKDestination; }
			set
			{
				base.CIC_RL_NKDestination = value;
				RefreshRatesAndTriggers();
			}
		}

		void RefreshRatesAndTriggers()
		{
			var parent = Parent;
			if (parent != null && parent.CommissionAgreement != null)
			{
				foreach (var recipient in parent.CommissionAgreement.Recipients)
				{
					if (!recipient.CAR_IsCommissionRateOverriden)
					{
						recipient.SetDefaultsFromStaffCommissionRule();
					}
				}

				parent.CommissionAgreement.DefaultTriggerTypeAndCommissionBasisFromSalesPersonCommissionRules();
			}
		}

		public OrgCommissionAgreementItemCondition CreateDraft()
		{
			var draft = Factory.New<OrgCommissionAgreementItemCondition>();
			draft.CopyPersistentValuesFrom(this, GetDraftCloneArgs());

			return draft;
		}

		BusinessObjectCloneArgs GetDraftCloneArgs()
		{
			return new BusinessObjectCloneArgs(new[] { OrgCommissionAgreementItemCondition.Schema.CIC_CAI }, true);
		}

		public override bool IsSavedByFactory
		{
			get
			{
				return base.IsSavedByFactory && (IsDeleted || !this.IsUncommittedDraft());
			}
		}

		public OrgCommissionAgreementItemCondition ParentVersion => null;

		public OrgCommissionAgreement CommissionAgreement
		{
			get
			{
				return Parent?.CommissionAgreement;
			}
		}

		#region Testing
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var item = Factory.NewWithValidTestData<OrgCommissionAgreementItem>();
			CIC_CAI = item.PK;

			base.FillWithValidTestData(kind, propertyPath);
		}

#endif
		#endregion
	}
}
