using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsClientPickingParams : NonPersistentBusinessObject
	{
		WhsClientPickingParams(OrgHeader client)
		{
			this.client = client;
		}

		readonly OrgHeader client;

		#region GetClientPickingParams

		public static WhsClientPickingParams GetClientPickingParams(OrgHeader client)
		{
			Argument.NotNull(client, "client");
			return client.Factory.GetCachedValue("WhsClientPickingParams|" + client.PK, () => GetClientPickingParamsCore(client));
		}

		static WhsClientPickingParams GetClientPickingParamsCore(OrgHeader client)
		{
			var result = new WhsClientPickingParams(client);
			client.RegisterEditableChildObject(result);

			return result;
		}

		#endregion

		#region Related Entities

		#region Client

		public OrgHeader Client
		{
			get { return client; }
		}

		#endregion

		#region WarehousePickPackParams

		public WhsClientPickPackParamsByWhsCollection WarehousePickPackParams
		{
			get
			{
				if (warehousePickPackParams == null)
				{
					warehousePickPackParams = new WhsClientPickPackParamsByWhsCollection(Client);
					RegisterEditableChildObject(warehousePickPackParams);
				}

				return warehousePickPackParams;
			}
		}

		WhsClientPickPackParamsByWhsCollection warehousePickPackParams;

		#endregion

		#endregion
	}
}
