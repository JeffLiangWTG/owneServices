using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Recruiter.Business
{
	public class HRJobApplicantFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public HRJobApplicantFetchStrategy(HRJobApplicant jobApplicant)
			: base(jobApplicant)
		{
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == HRJobApplicant.Schema.HA_FaxNum_IsManuallyVerified
					|| column.ColumnName == HRJobApplicant.Schema.HA_HomePhone_IsManuallyVerified
					|| column.ColumnName == HRJobApplicant.Schema.HA_MobilePhone_IsManuallyVerified
					|| column.ColumnName == HRJobApplicant.Schema.HA_WorkPhone_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}
		}
	}
}
