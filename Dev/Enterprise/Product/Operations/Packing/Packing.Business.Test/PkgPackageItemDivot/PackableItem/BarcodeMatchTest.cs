using System;
using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	public class BarcodeMatchTest : TestCase
	{
		#region TestConstructor

		public void TestConstructorThrowsExceptionOnEmptyPackType()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => new BarcodeMatch(true, "", 5m));
		}

		public void TestContructorDefaultsEmptyQtyTo1()
		{
			AssertEquals(1m, new BarcodeMatch(true, "BOX", 0m).QtyToPack);
		}

		#endregion

		#region TestYes

		public void TestYes()
		{
			AssertEquals(true, BarcodeMatch.Yes.IsMatch);
			AssertEquals(false, BarcodeMatch.Yes.IsTUN);
			AssertEquals("", BarcodeMatch.Yes.PackTypeToPackInto);
			AssertEquals(1m, BarcodeMatch.Yes.QtyToPack);
			AssertEquals(BarcodeMatch.Yes, BarcodeMatch.Yes);
		}

		#endregion

		#region TestNo

		public void TestNo()
		{
			AssertEquals(false, BarcodeMatch.No.IsMatch);
			AssertEquals(false, BarcodeMatch.No.IsTUN);
			AssertEquals("", BarcodeMatch.No.PackTypeToPackInto);
			AssertEquals(1m, BarcodeMatch.No.QtyToPack);
			AssertEquals(BarcodeMatch.No, BarcodeMatch.No);
		}

		#endregion

		#region TestBarcodeMatch

		public void TestBarcodeMatch()
		{
			var match = new BarcodeMatch(true);
			AssertEquals(true, match.IsMatch);
			AssertEquals("", match.PackTypeToPackInto);
			AssertEquals(1m, match.QtyToPack);

			var noMatch = new BarcodeMatch(false, "KEG", 2.5m);
			AssertEquals(false, noMatch.IsMatch);
			AssertEquals("KEG", noMatch.PackTypeToPackInto);
			AssertEquals(2.5m, noMatch.QtyToPack);
		}

		#endregion

		#region TestIsTUN

		public void TestIsTUN()
		{
			AssertEquals(false, new BarcodeMatch(true).IsTUN);
			AssertEquals(true, new BarcodeMatch(true, "PLT", 5m).IsTUN);
		}

		#endregion
	}
}
