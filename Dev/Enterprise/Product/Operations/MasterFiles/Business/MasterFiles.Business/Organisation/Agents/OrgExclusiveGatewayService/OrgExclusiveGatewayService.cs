using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgExclusiveGatewayService : AutoOrgExclusiveGatewayService
	{
		public OrgExclusiveGatewayService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		OrgHeader Header => AgentPort?.Header;

		#region IReadOnlySecurity Members

		protected virtual bool GetReadOnlySecurity(PropertyDescriptor property) =>
			(Header != null && !Header.SecurityProvider.HasModifyForwarderDetailsSecurity) ||
			MetaData.GetReadOnlyExcludingMethodProvider(this, property);

		#endregion
	}
}
