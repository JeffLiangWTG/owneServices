using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgTradeValue : AutoOrgTradeValue
	{
		public OrgTradeValue(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		#region SupportsNotes

		public override bool SupportsNotes => false;

		#endregion

		#region ReadOnly

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool shouldBeReadOnly = false;
			if (TradePeriod?.TradeDetail?.Parent?.ParentOrganisation != null)
			{
				shouldBeReadOnly = !TradePeriod.TradeDetail.Parent.ParentOrganisation.SecurityProvider.HasModifySalesTradeProfileSecurity;
			}
			return shouldBeReadOnly || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
