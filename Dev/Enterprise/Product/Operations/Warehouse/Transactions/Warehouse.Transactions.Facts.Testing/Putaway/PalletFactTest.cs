using System;
using NUnit.Framework;
using WTG.ProductionRules.Business.ProductWarehousePutaway;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	public class PalletFactTest : TestCase
	{
		public void TestNullObject_Throws()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PalletFact(null));
		}

		public void TestPalletID_Empty()
		{
			AssertExceptionThrown<ArgumentException>(() => new PalletFact(string.Empty));
		}

		public void TestPalletID()
		{
			var palletFact = new PalletFact("PLT123");
			AssertEquals(nameof(IPalletFact.PalletID), "PLT123", palletFact.PalletID);
			AssertEquals(nameof(IPalletFact.PalletID), "PLT123", ((IPalletFact)palletFact).PalletID);
		}
	}
}
