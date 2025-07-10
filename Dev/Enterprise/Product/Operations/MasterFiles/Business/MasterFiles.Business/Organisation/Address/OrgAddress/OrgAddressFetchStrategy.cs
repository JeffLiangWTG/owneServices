using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgAddressFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgAddressFetchStrategy(OrgAddress address)
			: base(address)
		{
		}

		OrgAddress Address
		{
			get { return BusinessObject as OrgAddress; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(typeof(OrgHeader), Address.OA_OH);
			Factory.AddFetchHint(typeof(OrgAddressCapability), OrgAddressCapabilitySchema.PZ_OA, Address.PK);
			Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, Address.PK);
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
				if (column.ColumnName == OrgAddress.Schema.OA_Phone_IsManuallyVerified
					|| column.ColumnName == OrgAddress.Schema.OA_Fax_IsManuallyVerified
					|| column.ColumnName == OrgAddress.Schema.OA_Mobile_IsManuallyVerified)
				{
					Factory.AddFetchHint(typeof(GenCustomAddOnRuleAck), GenCustomAddOnRuleAckSchema.XK_ParentID, BusinessObject.PK);
				}
			}
		}
	}
}
