using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgAirlineBranchAccount : AutoOrgAirlineBranchAccount
	{
		public OrgAirlineBranchAccount(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return Carrier != null && (!Carrier.SecurityProvider.HasModifyCarrierSecurityAir || MetaData.GetReadOnlyExcludingMethodProvider(this, property));
		}

		#endregion
	}
}
