using System;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class AttributeComparerTest : WhsTestCaseWithFactory
	{
		#region TestGetHashCodeForConsolidation

		public void TestGetHashCodeForConsolidation()
		{
			var line = new TestILineAttributes();
			line.ExpiryDate = new ZDate(2023, 01, 02);
			line.PackingDate = new ZDate(2021, 05, 06);
			line.PartAttrib1 = "PA1";
			line.PartAttrib2 = "PA2";
			line.PartAttrib3 = "PA3";
			line.SerialNumber = "SN";
			line.AllocationKey = "ALO";
			AssertEquals("02-Jan-23°06-May-21°pa1°pa2°pa3°sn°alo", AttributeComparer.GetHashCodeForConsolidation(line));

			using (WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("02-Jan-23°06-May-21°pa1°pa2°pa3°alo", AttributeComparer.GetHashCodeForConsolidation(line));
			}
		}

		#endregion

		#region TestCompare

		public void TestCompare()
		{
			var src = new TestILineAttributes();
			var with = new TestILineAttributes();
			IPartAttributes srcAsPartAttribs = src;

			AssertEquals(true, AttributeComparer.Compare(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));

			with.BondedEntryKey = "BEK";
			with.ExpiryDate = ZDate.Today.AddMonths(1);
			with.PackingDate = ZDate.Today.AddMonths(-1);
			with.PartAttrib1 = "PA1";
			with.PartAttrib2 = "PA2";
			with.PartAttrib3 = "PA3";
			with.SerialNumber = "SERN";

			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));

			src.BondedEntryKey = "BEK";
			src.ExpiryDate = ZDate.Today.AddMonths(1);
			src.PackingDate = ZDate.Today.AddMonths(-1);
			src.PartAttrib1 = "PA1";
			src.PartAttrib2 = "PA2";
			src.PartAttrib3 = "PA3";
			src.SerialNumber = "SERN";

			AssertEquals(true, AttributeComparer.Compare(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));

			src.BondedEntryKey = ""; // BondedEntryKey is special case, it only needs to match if it is not empty
			AssertEquals(false, AttributeComparer.Compare(src, with));
			src.BondedEntryKey = "BEK";

			with.BondedEntryKey = "BK";
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.BondedEntryKey = "BEK";

			with.ExpiryDate = ZDate.Today.AddMonths(2);
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.ExpiryDate = ZDate.Today.AddMonths(1);

			with.PackingDate = ZDate.Today.AddMonths(-2);
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.PackingDate = ZDate.Today.AddMonths(-1);

			with.PartAttrib1 = "P1";
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.PartAttrib1 = "PA1";

			with.PartAttrib2 = "P2";
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.PartAttrib2 = "PA2";

			with.PartAttrib3 = "PA33";
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.PartAttrib3 = "PA3";

			with.SerialNumber = "SERNN";
			AssertEquals(false, AttributeComparer.Compare(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(src, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
			with.SerialNumber = "SERN";
		}

		#endregion

		#region TestCompareWithoutBondedEntryKey

		public void TestCompareWithoutBondedEntryKey()
		{
			var year = ZDateTime.Now.Year;
			var source = new TestILineAttributes("123-1", new ZDate(year, 1, 1), new ZDate(year, 1, 1), "1", "2", "3", "SN");
			var with = new TestILineAttributes("123-2", new ZDate(year, 1, 1), new ZDate(year, 1, 1), "1", "2", "3", "SN");
			AssertEquals(false, AttributeComparer.Compare(source, with));
			AssertEquals(true, AttributeComparer.CompareWithoutBondedEntryKey(source, with));
		}

		#endregion

		#region TestCompare_AllocationKey

		public void TestCompare_AllocationKey()
		{
			var empty = new TestILineAttributes();
			var source = new TestILineAttributes { AllocationKey = "A" };
			var with = new TestILineAttributes { AllocationKey = "B" };
			IPartAttributes srcAsPartAttribs = source;

			AssertEquals(false, AttributeComparer.Compare(source, with));
			AssertEquals(false, AttributeComparer.CompareWithoutBondedEntryKey(source, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(source, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));

			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(empty, with));
			AssertEquals(false, AttributeComparer.CompareWithIsEmptyCheck(with, empty));

			with.AllocationKey = "A";
			AssertEquals(true, AttributeComparer.Compare(source, with));
			AssertEquals(true, AttributeComparer.CompareWithoutBondedEntryKey(source, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(source, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));

			with.AllocationKey = "a";
			AssertEquals(true, AttributeComparer.Compare(source, with));
			AssertEquals(true, AttributeComparer.CompareWithoutBondedEntryKey(source, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(source, with));
			AssertEquals(true, AttributeComparer.CompareWithIsEmptyCheck(srcAsPartAttribs, with));
		}

		#endregion
	}
}
