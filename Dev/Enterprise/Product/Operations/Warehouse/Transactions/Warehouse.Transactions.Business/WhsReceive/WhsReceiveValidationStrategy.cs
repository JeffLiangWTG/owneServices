using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveValidationStrategy
	{
		public WhsReceiveValidationStrategy(WhsReceive parent)
		{
			Parent = Argument.NotNull(parent, nameof(parent));
		}

		protected WhsReceive Parent { get; }

		#region CheckSerialNumberIsUnique

		public CheckSerialNumberIsUniqueDelegate CheckSerialNumberIsUnique
		{
			get { return checkSerialNumberIsUnique ?? (checkSerialNumberIsUnique = CreateCheckSerialNumberIsUniqueDelegate()); }
		}

		CheckSerialNumberIsUniqueDelegate checkSerialNumberIsUnique;

		public void ClearCheckSerialNumberIsUniqueCache()
		{
			checkSerialNumberIsUnique = null; // clear closure cache before each validation.
		}

		public delegate bool CheckSerialNumberIsUniqueDelegate(OrgHeader client, WhsInventoryView inventory, bool checkInDB);

		protected virtual CheckSerialNumberIsUniqueDelegate CreateCheckSerialNumberIsUniqueDelegate()
		{
			// using Closure to avoid creating and clearing of global cache. 
			// http://www.codeproject.com/Articles/375166/Functional-programming-in-Csharp
			Dictionary<string, IEnumerable<ZGuid>> nonUniqueSerialNumbers = null; // cached inside of the closure until checkSerialNumberIsUnique is nulled out
			CheckSerialNumberIsUniqueDelegate result = (client, inv, checkInDB) =>
			{
				var isValid = true;

				if (!Parent.IsGeneratingSerialNumbers) // skip CheckSerialNumberIsUniqueCore if it's generating serial numbers.
				{
					if (nonUniqueSerialNumbers == null || !((IBusinessObjectInternals)Parent).IsInPreSaveValidation) // update closure cache if not in RunPreSaveValidation.
					{
						nonUniqueSerialNumbers = GetNonUniqueSerialNumbers(client, checkInDB);
					}

					isValid = CheckSerialNumberIsUniqueCore(nonUniqueSerialNumbers, inv);
				}
				return isValid;
			};

			return result;
		}

		#region GetNonUniqueSerialNumbers

		Dictionary<string, IEnumerable<ZGuid>> GetNonUniqueSerialNumbers(OrgHeader client, bool checkInDB)
		{
			var relevantSerialNumbers = new Dictionary<string, Dictionary<ZGuid, List<WhsInventoryView>>>(); // SN - Product PK - Inventories
			AddSerialsToTheDictionary(client, relevantSerialNumbers, Parent.Inventory.Cast<WhsInventoryView>());

			if (checkInDB && relevantSerialNumbers.Count > 0)
			{
				var inventoriesFromDB = GetInventoriesWithSameSerialsFromDB(client, relevantSerialNumbers);
				AddSerialsToTheDictionary(client, relevantSerialNumbers, inventoriesFromDB);
			}

			return GetNonUniqueSerialNumbers(relevantSerialNumbers);
		}

		#region AddSerialsToTheDictionary

		void AddSerialsToTheDictionary(OrgHeader client, Dictionary<string, Dictionary<ZGuid, List<WhsInventoryView>>> result, IEnumerable<WhsInventoryView> inventoriesToAdd)
		{
			var inventoryGroupedByProduct = GetInventoriesGroupedByProduct(inventoriesToAdd);
			foreach (var group in inventoryGroupedByProduct)
			{
				var listOfInventories = group.Value;
				var product = WhsProduct.GetWhsProduct(Parent.Factory, group.Key);
				if (product.IsSerialNumberUsed(client))
				{
					foreach (var inventory in listOfInventories.Where(inv => ShouldIncludeInventoryForSerialNumberCheck(inv)))
					{
						AddPartAttribSerialNumbers(result, inventory, inventory.WI_SerialNumber);
					}
				}
			}
		}

		#endregion

		#region ShouldIncludeInventoryForSerialNumberCheck

		protected virtual bool ShouldIncludeInventoryForSerialNumberCheck(WhsInventoryView inventory)
		{
			var shouldIncludeSerialNumberCheck = inventory.WI_TotalUnits > 0;

			if (!shouldIncludeSerialNumberCheck)
			{
				var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.PK) { FetchOnlyFromLocalCache = true };
				var pickLine = inventory.Factory.LoadTop1<WhsPickLine>(query);
				if (pickLine != null && pickLine.IsPickedInMemory)
				{
					shouldIncludeSerialNumberCheck = false;
				}
				else
				{
					shouldIncludeSerialNumberCheck =
						Parent.PK != inventory.WI_WD_Proxy
						|| (!Parent.StartedReceiving && inventory.WI_ExpectedReceiptQuantity > 0);
				}
			}
			return shouldIncludeSerialNumberCheck;
		}

		#endregion

		#region GetInventoriesGroupedByProduct

		static Dictionary<ZGuid, List<WhsInventoryView>> GetInventoriesGroupedByProduct(IEnumerable<WhsInventoryView> inventories)
		{
			var result = new Dictionary<ZGuid, List<WhsInventoryView>>();
			foreach (var inventory in inventories)
			{
				if (inventory.WI_OP.IsValid)
				{
					List<WhsInventoryView> inventoriesWithSameProduct;
					if (!result.TryGetValue(inventory.WI_OP, out inventoriesWithSameProduct))
					{
						inventoriesWithSameProduct = new List<WhsInventoryView>();
						result.Add(inventory.WI_OP, inventoriesWithSameProduct);
					}
					inventoriesWithSameProduct.Add(inventory);
				}
			}

			return result;
		}

		#endregion

		#region AddPartAttribSerialNumbers

		static void AddPartAttribSerialNumbers(Dictionary<string, Dictionary<ZGuid, List<WhsInventoryView>>> existingSerialNumbers, WhsInventoryView inventory, ZString serialNumber)
		{
			if (!serialNumber.IsEmpty)
			{
				Dictionary<ZGuid, List<WhsInventoryView>> inventoriesWithTheSerialNumber;
				if (!existingSerialNumbers.TryGetValue(serialNumber, out inventoriesWithTheSerialNumber))
				{
					inventoriesWithTheSerialNumber = new Dictionary<ZGuid, List<WhsInventoryView>>();
					existingSerialNumbers.Add(serialNumber, inventoriesWithTheSerialNumber);
				}

				List<WhsInventoryView> inventoryList;
				if (!inventoriesWithTheSerialNumber.TryGetValue(inventory.WI_OP, out inventoryList))
				{
					inventoryList = new List<WhsInventoryView>();
					inventoriesWithTheSerialNumber.Add(inventory.WI_OP, inventoryList);
				}

				if (!inventoryList.Contains(inventory))
				{
					inventoryList.Add(inventory);
				}
			}
		}

		#endregion

		#region GetNonUniqueSerialNumbers

		Dictionary<string, IEnumerable<ZGuid>> GetNonUniqueSerialNumbers(Dictionary<string, Dictionary<ZGuid, List<WhsInventoryView>>> currentSerialNumbers)
		{
			var nonUniqueSerialNumbers = new Dictionary<string, IEnumerable<ZGuid>>();
			var isUniquePerProduct = WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct;

			foreach (var serialGroup in currentSerialNumbers)
			{
				var inventoriesGroupedByProduct = serialGroup.Value;
				var inventoriesForTheFirstProduct = inventoriesGroupedByProduct.First().Value;
				// SN unique per client and number of products that have the SN > 1 or inventories that have the SN > 1
				if (!isUniquePerProduct && (inventoriesGroupedByProduct.Count > 1 || inventoriesForTheFirstProduct.Count > 1))
				{
					nonUniqueSerialNumbers.Add(serialGroup.Key, inventoriesGroupedByProduct.Keys);
				}
				else if (isUniquePerProduct)
				{
					var productPKs = inventoriesGroupedByProduct.Where(sn => sn.Value.Count > 1).Select(sn => sn.Key).ToArray(); // more that 1 inventory for the same product and SN
					nonUniqueSerialNumbers.Add(serialGroup.Key, productPKs);
				}
			}

			return nonUniqueSerialNumbers;
		}

		#endregion

		#region CheckSerialNumberIsUniqueCore

		bool CheckSerialNumberIsUniqueCore(Dictionary<string, IEnumerable<ZGuid>> nonUniqueSerialNumbers, WhsInventoryView inventory) =>
			!nonUniqueSerialNumbers.ContainsKey(inventory.WI_SerialNumber) || !nonUniqueSerialNumbers[inventory.WI_SerialNumber].Contains(inventory.WI_OP);

		#endregion

		#region GetInventoriesWithSameSerialsFromDB

		WhsInventoryView[] GetInventoriesWithSameSerialsFromDB(OrgHeader client, Dictionary<string, Dictionary<ZGuid, List<WhsInventoryView>>> currentSerialNumbers)
		{
			var serialNumbersFromTheJob = currentSerialNumbers.Keys.ToList();

			var inventoryWithMatchingSerialsSQL = client.PartAttributeManager.IsSerialNumberUsedByOrganisation
				? GetSerialNumberQueryForPartAttribute()
				: "";

			var rawSql = string.Format(Culture.Invariant, @"
WI_PK IN
(
	SELECT
		WE_PK
	FROM
		dbo.WhsDocketLine
	WHERE
		WE_PK IN
		(
			{0}
		) AND
		WE_StockOnHand > 0
)", inventoryWithMatchingSerialsSQL);

			var sqlParams = new ZSqlParameterCollection
			{
				{ "@ClientPK", client.PK, WhsDocketSchema.WD_OH_Client },
				ZSqlParameter.New("@SerialNumbersFromTheJob", serialNumbersFromTheJob, WhsDocketLineSchema.WE_SerialNumber, isTableValued: true)
			};

			var query = new ZDBOnlyQuery(typeof(WhsInventoryView));
			query.AddFilterAndZSQLParameterCollection(rawSql, sqlParams);

			return Parent.Factory.Load<WhsInventoryView>(query);
		}

		static string GetSerialNumberQueryForPartAttribute()
		{
			return Invariant($@"
			SELECT
				WE_PK
			FROM
				dbo.WhsDocketLine
				JOIN dbo.WhsDocket ON WD_PK = WE_WD
				JOIN dbo.OrgPartRelation ON OU_OP = WE_OP AND OU_OH = @ClientPK AND OU_Relationship IN ('{OrgPartRelation.RelationshipTypes.Owner}', '{OrgPartRelation.RelationshipTypes.Both}')
			WHERE
				WD_OH_Client = @ClientPK AND
				OU_UseSerialNumber = 1 AND
				WE_SerialNumber <> '' AND
				WE_SerialNumber IN (SELECT Value FROM @SerialNumbersFromTheJob)");
		}

		#endregion

		#endregion

		#endregion
	}
}
