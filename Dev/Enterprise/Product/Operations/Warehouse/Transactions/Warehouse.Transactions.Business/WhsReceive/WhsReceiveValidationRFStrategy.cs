using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveValidationRFStrategy : WhsReceiveValidationStrategy
	{
		public WhsReceiveValidationRFStrategy(WhsReceive parent)
			: base(parent)
		{
		}

		#region CheckSerialNumberIsUnique

		protected override CheckSerialNumberIsUniqueDelegate CreateCheckSerialNumberIsUniqueDelegate()
		{
			CheckSerialNumberIsUniqueDelegate result = (client, inv, checkInDB) =>
			{
				return CheckSerialNumberIsUniqueFromRF(client, inv);
			};

			return result;
		}

		bool CheckSerialNumberIsUniqueFromRF(OrgHeader client, WhsInventoryView inventory)
		{
			var result = true;
			if (ShouldIncludeInventoryForSerialNumberCheck(inventory))
			{
				result = CheckSerialNumberIsUniqueFromRFCore(client, inventory);
			}

			return result;
		}

		protected override bool ShouldIncludeInventoryForSerialNumberCheck(WhsInventoryView inventory) => inventory.WI_TotalUnits > 0;

		bool CheckSerialNumberIsUniqueFromRFCore(OrgHeader client, WhsInventoryView inventory)
		{
			var result = false;

			var query = new ZQuery();
			query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, client.PK);
			query.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
			query.AddToFilter(WhsInventoryViewSchema.PK, SQLComparisonOperator.NotEqual, inventory.PK);

			if (WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct)
			{
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, inventory.WI_OP);
			}

			var product = inventory.Product;
			var serialNo = inventory.WI_SerialNumber;
			if (serialNo.IsEmpty)
			{
				result = true; // if serial numbers are not used or SN attributes is not entered.
			}
			else
			{
				var attributeQuery = new ZQuery();

				if (product.IsSerialNumberUsed(client))
				{
					attributeQuery.AddToFilter(JoinCondition.Or, WhsInventoryViewSchema.WI_SerialNumber, serialNo);
				}

				query.AddToFilter(attributeQuery);
			}

			if (!result)
			{
				result = Parent.Factory.LoadTop1<WhsInventoryView>(query) == null;
			}

			return result;
		}

		#endregion
	}
}
