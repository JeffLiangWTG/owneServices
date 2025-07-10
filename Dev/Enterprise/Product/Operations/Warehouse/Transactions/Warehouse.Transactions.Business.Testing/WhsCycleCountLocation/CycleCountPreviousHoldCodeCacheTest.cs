using CargoWise.Types;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(CycleCountPreviousHoldCodeCache))]
	public class CycleCountPreviousHoldCodeCacheTest : TestCase
	{
		#region TestCycleCountPreviousHoldCodeCache_SerialNumberCaching

		public void TestCycleCountPreviousHoldCodeCache_SerialNumberCaching_MatchingDetails()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var serialNumber = "S1";

			cache.CacheSerialNumberPreviousHoldCode(clientPK, productPK, serialNumber, "HELD");

			AssertEquals("Cache should retrieve serial number.",
				"HELD",
				cache.RetrieveHoldCodeForSerialNumber(clientPK, productPK, serialNumber));
		}

		public void TestCycleCountPreviousHoldCodeCache_SerialNumberCaching_MatchingDetails_CaseInsensitive()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var serialNumber = "SER1";

			cache.CacheSerialNumberPreviousHoldCode(clientPK, productPK, serialNumber, "held");

			AssertEquals("Cache should retrieve serial number.",
				"HELD",
				cache.RetrieveHoldCodeForSerialNumber(clientPK, productPK, "ser1"));

			AssertEquals("Cache should retrieve serial number.",
				"HELD",
				cache.RetrieveHoldCodeForSerialNumber(clientPK, productPK, "SeR1"));
		}

		public void TestCycleCountPreviousHoldCodeCache_SerialNumberCaching_MismatchedClientPK()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var serialNumber = "S1";

			cache.CacheSerialNumberPreviousHoldCode(clientPK, productPK, serialNumber, "HELD");

			AssertEquals("Cache should not retrieve serial number if mismatch.",
				ZString.Empty,
				cache.RetrieveHoldCodeForSerialNumber(ZGuid.NewZGuid(), productPK, serialNumber));
		}

		public void TestCycleCountPreviousHoldCodeCache_SerialNumberCaching_MismatchedProductPK()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var serialNumber = "S1";

			cache.CacheSerialNumberPreviousHoldCode(clientPK, productPK, serialNumber, "HELD");

			AssertEquals("Cache should not retrieve serial number if mismatch.",
				ZString.Empty,
				cache.RetrieveHoldCodeForSerialNumber(clientPK, ZGuid.NewZGuid(), serialNumber));
		}

		public void TestCycleCountPreviousHoldCodeCache_SerialNumberCaching_MismatchedSerial()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();
			var serialNumber = "S1";

			cache.CacheSerialNumberPreviousHoldCode(clientPK, productPK, serialNumber, "HELD");

			AssertEquals("Cache should not retrieve serial number if mismatch.",
				ZString.Empty,
				cache.RetrieveHoldCodeForSerialNumber(clientPK, productPK, "S2"));
		}

		#endregion

		#region TestCycleCountPreviousHoldCodeCache_PalletIDCaching

		#region TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MatchingDetails

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MatchingDetails()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			AssertEquals("Cache should retrieve previous hold code.",
				"HELD",
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					whsPK,
					productPK,
					"PALLETID",
					"PA1",
					"PA2",
					"PA3",
					date,
					date));
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MatchingDetails_CaseInsensitive()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"held");

			AssertEquals("Cache should retrieve previous hold code.",
				"HELD",
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					whsPK,
					productPK,
					"palletid",
					"pa1",
					"pa2",
					"pa3",
					date,
					date));

			AssertEquals("Cache should retrieve previous hold code.",
				"HELD",
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					whsPK,
					productPK,
					"PaLlEtId",
					"pA1",
					"Pa2",
					"pA3",
					date,
					date));
		}

		#endregion

		#region TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_MismatchClient()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchClientPK: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_MismatchProduct()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchProductPK: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_MismatchAttrib1()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchAttrib1: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_MismatchAttrib2()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchAttrib2: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_MismatchAttrib3()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchAttrib3: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_ExpiryDate()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchExpiryDate: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCode_PackingDate()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(matchPackingDate: false);
		}

		void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_SingleHoldCodeCore(
			bool matchClientPK = true,
			bool matchProductPK = true,
			bool matchAttrib1 = true,
			bool matchAttrib2 = true,
			bool matchAttrib3 = true,
			bool matchPackingDate = true,
			bool matchExpiryDate = true)
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			AssertEquals("Cache should still retrieve previous pallet hold code if single hold code mapped.",
				"HELD",
				cache.RetrieveHoldCodeForPalletID(
					matchClientPK ? clientPK : ZGuid.NewZGuid(),
					whsPK,
					matchProductPK ? productPK : ZGuid.NewZGuid(),
					"PALLETID",
					matchAttrib1 ? "PA1" : "ABC",
					matchAttrib2 ? "PA2" : "ABC",
					matchAttrib3 ? "PA3" : "ABC",
					matchExpiryDate ? date : date.AddDays(1),
					matchPackingDate ? date : date.AddDays(1)));
		}

		#endregion

		#region TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_MismatchClient()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchClientPK: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_MismatchProduct()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchProductPK: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_MismatchAttrib1()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchAttrib1: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_MismatchAttrib2()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchAttrib2: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_MismatchAttrib3()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchAttrib3: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_ExpiryDate()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchExpiryDate: false);
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCode_PackingDate()
		{
			TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(matchPackingDate: false);
		}

		void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodeCore(
			bool matchClientPK = true,
			bool matchProductPK = true,
			bool matchAttrib1 = true,
			bool matchAttrib2 = true,
			bool matchAttrib3 = true,
			bool matchPackingDate = true,
			bool matchExpiryDate = true)
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"DAM");

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				ZString.Empty);

			AssertEquals("Cache should not retrieve previous pallet hold code if mixed hold codes mapped to Pallet ID.",
				InventoryHoldCodes.Codes.Held,
				cache.RetrieveHoldCodeForPalletID(
					matchClientPK ? clientPK : ZGuid.NewZGuid(),
					whsPK,
					matchProductPK ? productPK : ZGuid.NewZGuid(),
					"PALLETID",
					matchAttrib1 ? "PA1" : "ABC",
					matchAttrib2 ? "PA2" : "ABC",
					matchAttrib3 ? "PA3" : "ABC",
					matchExpiryDate ? date : date.AddDays(1),
					matchPackingDate ? date : date.AddDays(1)));
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MixedHoldCodesWithoutMismatch()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"DAM");

			AssertEquals("Cache should not retrieve any previous pallet hold code if mixed hold codes mapped.",
				InventoryHoldCodes.Codes.Held,
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					whsPK,
					productPK,
					"PALLETID",
					"PA1",
					"PA2",
					"PA3",
					date,
					date));
		}

		#endregion

		#region TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_Unmatchable

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MismatchPalletID()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			AssertEquals("Cache should not retrieve previous pallet hold code if pallet ID mismatch.",
				ZString.Empty,
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					whsPK,
					productPK,
					"ABCPallet",
					"PA1",
					"PA2",
					"PA3",
					date,
					date));
		}

		public void TestCycleCountPreviousHoldCodeCache_PalletIDCaching_MismatchingDetails_MismatchWarehouse()
		{
			var cache = new CycleCountPreviousHoldCodeCache();
			var clientPK = ZGuid.NewZGuid();
			var whsPK = ZGuid.NewZGuid();
			var productPK = ZGuid.NewZGuid();

			var date = ZDate.Today;

			cache.CachePalletIDPreviousHoldCode(
				clientPK,
				whsPK,
				productPK,
				"PALLETID",
				"PA1",
				"PA2",
				"PA3",
				date,
				date,
				"HELD");

			AssertEquals("Cache should not retrieve previous pallet hold code if warehouse mismatch.",
				ZString.Empty,
				cache.RetrieveHoldCodeForPalletID(
					clientPK,
					ZGuid.NewZGuid(),
					productPK,
					"PALLETID",
					"PA1",
					"PA2",
					"PA3",
					date,
					date));
		}

		#endregion

		#endregion
	}
}
