using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class SerialNumberUniquenessChecker
	{
		public SerialNumberUniquenessChecker()
		{
		}

		public SerialNumberUniquenessChecker(WhsDocket docket)
		{
			Argument.NotNull(docket, nameof(docket));
			InitializeCache(docket);
		}

		void InitializeCache(WhsDocket docket)
		{
			serialNumbersCache = new Lazy<Dictionary<ZGuid, HashSet<ZString>>>(() =>
			{
				var serialNumbers = docket.Lines
					.Where(l => l.WE_TransactionQuantity > 0)
					.Cast<ISerialNumberParent>()
					.SelectMany(l => l.SerialNumbers.Cast<WhsSerialNumberPivot>())
					.Select(p => p.SerialNumber)
					.Where(s => !s.IsInDatabase)
					.Select(s => new { s.WSN_OP_Product, s.WSN_SerialNumber })
					.Distinct()
					.ToArray();

				var serialNumbersToCheck = LoadSerialNumbers(
					docket.Factory,
					docket.WD_OH_Client,
					serialNumbers.Select(l => l.WSN_OP_Product),
					serialNumbers.Select(s => s.WSN_SerialNumber),
					false);

				var serialNumberLookup = serialNumbersToCheck.ToLookup(s => s.WSN_SerialNumber);

				var groupedByProduct = serialNumbersToCheck
					.Where(s => serialNumberLookup[s.WSN_SerialNumber].Count() > 1) // Duplicate only
					.GroupBy(s => s.WSN_OP_Product)
					.ToDictionary(
						g => g.Key,
						g => g
							.Select(s => s.WSN_SerialNumber)
							.ToHashSet()
					);

				return groupedByProduct;
			});
		}

		static WhsSerialNumber[] LoadSerialNumbers(
			BusinessObjectFactory factory,
			ZGuid clientPK,
			IEnumerable<ZGuid> products,
			IEnumerable<ZString> serialNumbers,
			bool fetchOnlyFromLocalCache)
		{
			var query = new ZQuery() { AllowTableValuedParameters = true };
			query.AddToFilter(WhsSerialNumberSchema.WSN_SerialNumber, serialNumbers);
			query.AddToFilter(WhsSerialNumberSchema.WSN_IsInUse, ZBool.True);
			query.AddToFilter(WhsSerialNumberSchema.WSN_OH_Client, clientPK);

			if (WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct)
			{
				query.AddToFilter(WhsSerialNumberSchema.WSN_OP_Product, products);
			}

			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
			return factory.Load<WhsSerialNumber>(query);
		}

		public bool IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => IsSerialNumberAlreadyInUse(pivot, pivot.IsInDatabase);

		bool IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot, bool fetchFromMemoryOnly)
		{
			var result = false;
			var serialNumber = pivot.SerialNumber;

			if (serialNumbersCache != null)
			{
				if (serialNumbersCache.Value.TryGetValue(serialNumber.WSN_OP_Product, out var sns))
				{
					result = sns.Contains(serialNumber.WSN_SerialNumber);
				}
			}
			else
			{
				var serialNumbers = LoadSerialNumbers(
					pivot.Factory,
					serialNumber.WSN_OH_Client,
					[serialNumber.WSN_OP_Product],
					[serialNumber.WSN_SerialNumber],
					fetchFromMemoryOnly);

				result = serialNumbers.Length > 1; // To enhance the user experience for saved data.
			}

			return result;
		}

		Lazy<Dictionary<ZGuid, HashSet<ZString>>> serialNumbersCache;
	}
}
