using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	class CycleCountPreviousHoldCodeCache
	{
		#region Constructor

		public CycleCountPreviousHoldCodeCache()
		{
			SerialNumberCache = new Dictionary<(ZGuid, ZGuid, ZString), ZString>();

			PalletIDAndAttributesCache = new Dictionary<PreviousHoldCodeForPalletIDKey, ZString>();
			PalletIDToHoldCodesMapping = new Dictionary<(ZGuid, ZString), HashSet<ZString>>();
			PalletsToHoldWithDefaultCode = new HashSet<(ZGuid, ZString)>();
		}

		#endregion

		#region SerialNumberCache

		Dictionary<(ZGuid, ZGuid, ZString), ZString> SerialNumberCache { get; }

		public void CacheSerialNumberPreviousHoldCode(ZGuid clientPK, ZGuid productPK, ZString serial, ZString previousHoldCode)
			=> SerialNumberCache.Add((clientPK, productPK, serial.ToUpper()), previousHoldCode.ToUpper());

		public ZString RetrieveHoldCodeForSerialNumber(ZGuid clientPK, ZGuid productPK, ZString serial)
		{
			SerialNumberCache.TryGetValue((clientPK, productPK, serial.ToUpper()), out var result);
			return result;
		}

		#endregion

		#region PalletIDAndAttributesCache

		Dictionary<PreviousHoldCodeForPalletIDKey, ZString> PalletIDAndAttributesCache { get; }

		Dictionary<(ZGuid, ZString), HashSet<ZString>> PalletIDToHoldCodesMapping { get; }

		// Some Pallets may have the same inventory attributes held with different hold codes (or partially available)
		// We will default them to simply be HEL - Held
		HashSet<(ZGuid, ZString)> PalletsToHoldWithDefaultCode { get; }

		public void CachePalletIDPreviousHoldCode(
				ZGuid clientPK,
				ZGuid whsPK,
				ZGuid productPK,
				ZString palletID,
				ZString partAttrib1,
				ZString partAttrib2,
				ZString partAttrib3,
				ZDate expiryDate,
				ZDate packingDate,
				ZString previousHoldCode)
		{
			(palletID, partAttrib1, partAttrib2, partAttrib3, previousHoldCode) = ToUpperInputs(palletID, partAttrib1, partAttrib2, partAttrib3, previousHoldCode);

			if (!PalletsToHoldWithDefaultCode.Contains((whsPK, palletID)))
			{
				var key = new PreviousHoldCodeForPalletIDKey(
					clientPK,
					whsPK,
					productPK,
					palletID.ToUpper(),
					partAttrib1.ToUpper(),
					partAttrib2.ToUpper(),
					partAttrib3.ToUpper(),
					expiryDate,
					packingDate);

				if (!PalletIDAndAttributesCache.TryGetValue(key, out var cachedHoldCode))
				{
					PalletIDAndAttributesCache.Add(key, previousHoldCode);
				}
				else if (cachedHoldCode != previousHoldCode)
				{
					PalletsToHoldWithDefaultCode.Add((whsPK, palletID));
				}

				UpdatePalletIDToHoldCodesMapping(whsPK, palletID, previousHoldCode);
			}
		}

		void UpdatePalletIDToHoldCodesMapping(ZGuid whsPK, ZString palletID, ZString previousHoldCode)
		{
			if (PalletIDToHoldCodesMapping.TryGetValue((whsPK, palletID), out var mapping))
			{
				mapping.Add(previousHoldCode);
			}
			else
			{
				PalletIDToHoldCodesMapping.Add((whsPK, palletID), new HashSet<ZString> { previousHoldCode });
			}
		}

		public ZString RetrieveHoldCodeForPalletID(
				ZGuid clientPK,
				ZGuid whsPK,
				ZGuid productPK,
				ZString palletID,
				ZString partAttrib1,
				ZString partAttrib2,
				ZString partAttrib3,
				ZDate expiryDate,
				ZDate packingDate)
		{
			(palletID, partAttrib1, partAttrib2, partAttrib3, _) = ToUpperInputs(palletID, partAttrib1, partAttrib2, partAttrib3, ZString.Empty);

			var key = new PreviousHoldCodeForPalletIDKey(clientPK, whsPK, productPK, palletID, partAttrib1, partAttrib2, partAttrib3, expiryDate, packingDate);

			ZString result;
			if (PalletsToHoldWithDefaultCode.Contains((whsPK, palletID)))
			{
				result = InventoryHoldCodes.Codes.Held;
			}
			else if (!PalletIDAndAttributesCache.TryGetValue(key, out result))
			{
				result = RetrieveSingularHoldCodeForPalletID(whsPK, palletID);
			}

			return result;
		}

		ZString RetrieveSingularHoldCodeForPalletID(ZGuid whsPK, ZString palletID)
		{
			var result = ZString.Empty;

			if (PalletIDToHoldCodesMapping.TryGetValue((whsPK, palletID), out var mappedHoldCodes) && mappedHoldCodes.Count == 1)
			{
				result = mappedHoldCodes.Single();
			}

			return result;
		}

		(ZString, ZString, ZString, ZString, ZString) ToUpperInputs(ZString palletID, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString previousHoldCode)
		{
			return (palletID.ToUpper(), partAttrib1.ToUpper(), partAttrib2.ToUpper(), partAttrib3.ToUpper(), previousHoldCode.ToUpper());
		}

		#region Key Implementation

		class PreviousHoldCodeForPalletIDKey
		{
			public PreviousHoldCodeForPalletIDKey(
				ZGuid clientPK,
				ZGuid whsPK,
				ZGuid productPK,
				ZString palletID,
				ZString partAttrib1,
				ZString partAttrib2,
				ZString partAttrib3,
				ZDate expiryDate,
				ZDate packingDate)
			{
				ClientPK = clientPK;
				WhsPK = whsPK;
				ProductPK = productPK;
				PalletID = palletID;
				PartAttrib1 = partAttrib1;
				PartAttrib2 = partAttrib2;
				PartAttrib3 = partAttrib3;
				ExpiryDate = expiryDate;
				PackingDate = packingDate;
			}

			ZGuid ClientPK { get; }
			ZGuid WhsPK { get; }
			ZGuid ProductPK { get; }
			ZString PalletID { get; }
			ZString PartAttrib1 { get; }
			ZString PartAttrib2 { get; }
			ZString PartAttrib3 { get; }
			ZDate ExpiryDate { get; }
			ZDate PackingDate { get; }

			public override bool Equals(object obj)
			{
				var isEqual = base.Equals(obj);

				if (obj is PreviousHoldCodeForPalletIDKey key)
				{
					isEqual =
						ClientPK.Equals(key.ClientPK)
						&& WhsPK.Equals(key.WhsPK)
						&& ProductPK.Equals(key.ProductPK)
						&& PalletID.EqualsIgnoringCase(key.PalletID)
						&& PartAttrib1.EqualsIgnoringCase(key.PartAttrib1)
						&& PartAttrib2.EqualsIgnoringCase(key.PartAttrib2)
						&& PartAttrib3.EqualsIgnoringCase(key.PartAttrib3)
						&& ExpiryDate.Equals(key.ExpiryDate)
						&& PackingDate.Equals(key.PackingDate);
				}

				return isEqual;
			}

			public override int GetHashCode()
			{
				return ClientPK.GetHashCode()
					^ WhsPK.GetHashCode()
					^ ProductPK.GetHashCode()
					^ PalletID.ToUpper().GetHashCode()
					^ PartAttrib1.ToUpper().GetHashCode()
					^ PartAttrib2.ToUpper().GetHashCode()
					^ PartAttrib3.ToUpper().GetHashCode()
					^ ExpiryDate.GetHashCode()
					^ PackingDate.GetHashCode();
			}
		}

		#endregion

		#endregion
	}
}
