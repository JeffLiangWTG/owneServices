using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.FetchStrategies
{
	public class GlbBranchFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		#region Constructors

		public GlbBranchFetchStrategy(GlbBranch branch) : base(branch)
		{
		}

		#endregion

		#region FetchForLoad

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(GlbCompany), ((GlbBranch)BusinessObject).GB_GC);
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
		}

		#endregion

		#region FetchForValidate

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var branchPk = BusinessObject.PK;
			Factory.AddFetchHint(GenCustomAddOnRuleAckSchema.XK_ParentID, branchPk);
		}

		#endregion

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == GlbBranch.Schema.GB_Phone_IsManuallyVerified || column.ColumnName == GlbBranch.Schema.GB_Fax_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}
		}

		#endregion
	}
}