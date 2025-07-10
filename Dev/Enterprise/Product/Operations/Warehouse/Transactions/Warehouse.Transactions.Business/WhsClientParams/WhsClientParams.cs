using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsClientParams : NonPersistentBusinessObject
	{
		#region Constructors

		WhsClientParams(OrgHeader parent)
		{
			Client = parent;
		}

		#endregion

		public OrgHeader Client { get; }

		#region GetClientParams

		public static WhsClientParams GetClientParams(OrgHeader client)
		{
			Argument.NotNull(client, nameof(client));
			return client.Factory.GetCachedValue($"WhsClientParams|{client.PK}", () => new WhsClientParams(client));
		}

		#endregion

		#region Related Business Objects

		public WhsClientParameterByWarehouseCollection ClientParametersByWarehouse
		{
			get
			{
				if (clientParametersByWarehouse == null)
				{
					clientParametersByWarehouse = new WhsClientParameterByWarehouseCollection(Client);
					RegisterEditableChildObject(clientParametersByWarehouse);
				}
				return clientParametersByWarehouse;
			}
		}
		WhsClientParameterByWarehouseCollection clientParametersByWarehouse;

		#endregion
	}
}
