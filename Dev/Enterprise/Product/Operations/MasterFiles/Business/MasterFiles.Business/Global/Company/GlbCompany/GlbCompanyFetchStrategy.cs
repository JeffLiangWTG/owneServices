using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class GlbCompanyFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		#region Constructors

		public GlbCompanyFetchStrategy(GlbCompany company) : base(company)
		{
		}

		#endregion

		#region FetchForLoad

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

		#endregion

		#region FetchForValidate

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
			var companyPk = BusinessObject.PK;
			Factory.AddFetchHint(GenCustomAddOnRuleAckSchema.XK_ParentID, companyPk);
		}

		#endregion

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == GlbCompany.Schema.GC_Phone_IsManuallyVerified || column.ColumnName == GlbCompany.Schema.GC_Fax_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}
		}

		#endregion
	}
}