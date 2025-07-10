using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgContactFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgContactFetchStrategy(OrgContact contact)
			: base(contact)
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
			var contactPk = BusinessObject.PK;
			Factory.AddFetchHint(OrgDocumentSchema.OD_OC, contactPk);
			Factory.AddFetchHint(OrgContactItemSchema.OI_OC, contactPk);
			Factory.AddFetchHint(GenCustomAddOnRuleAckSchema.XK_ParentID, contactPk);
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			foreach (var column in columns)
			{
				if (column.ColumnName == OrgContact.Schema.OC_Fax_IsManuallyVerified
					|| column.ColumnName == OrgContact.Schema.OC_HomePhone_IsManuallyVerified
					|| column.ColumnName == OrgContact.Schema.OC_Mobile_IsManuallyVerified
					|| column.ColumnName == OrgContact.Schema.OC_OtherPhone_IsManuallyVerified
					|| column.ColumnName == OrgContact.Schema.OC_Pager_IsManuallyVerified
					|| column.ColumnName == OrgContact.Schema.OC_Phone_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}
		}
	}
}
