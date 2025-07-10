using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsClientPickPackParamsByWhsCollection : ActiveBusinessObjectCollection<WhsClientPickPackParamsByWhs>
	{
		public WhsClientPickPackParamsByWhsCollection(OrgHeader client)
			: base(client.Factory, client, null, WhsClientPickPackParamsByWhsSchema.WPP_OH_Client)
		{
		}
	}
}
