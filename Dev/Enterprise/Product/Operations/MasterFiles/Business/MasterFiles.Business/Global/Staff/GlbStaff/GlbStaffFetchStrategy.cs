using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.FetchStrategies
{
	public class GlbStaffFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		#region Constructors

		public GlbStaffFetchStrategy(GlbStaff staff) : base(staff)
		{
		}

		#endregion

		#region FetchForLoad

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(GlbBranch), ((GlbStaff)BusinessObject).GS_GB_HomeBranch);
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
			var staffPk = BusinessObject.PK;
			Factory.AddFetchHint(GenCustomAddOnRuleAckSchema.XK_ParentID, staffPk);
		}

		#endregion

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == GlbStaff.Schema.GS_HomePhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_WorkPhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_MobilePhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_FaxNum_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_Pager_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_NextOfKinHomePhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_NextOfKinWorkPhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_EmergencyHomePhone_IsManuallyVerified ||
					column.ColumnName == GlbStaff.Schema.GS_EmergencyWorkPhone_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}

			var staff = (GlbStaff)BusinessObject;
			Factory.AddFetchHint(typeof(GlbPerson), staff.GS_PER);
		}

		#endregion
	}
}
