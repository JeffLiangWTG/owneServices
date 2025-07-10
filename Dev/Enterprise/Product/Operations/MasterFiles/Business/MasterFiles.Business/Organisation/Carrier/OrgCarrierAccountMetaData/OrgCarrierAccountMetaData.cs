using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class OrgCarrierAccountMetaData : AutoOrgCarrierAccountMetaData
	{
		public OrgCarrierAccountMetaData(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
