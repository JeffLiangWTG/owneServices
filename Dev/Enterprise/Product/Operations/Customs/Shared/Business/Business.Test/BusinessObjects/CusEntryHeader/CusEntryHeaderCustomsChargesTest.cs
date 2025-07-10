using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class CusEntryHeaderCustomsChargesTest : TestCaseWithFactory
	{
		public abstract CusEntryHeader GetEntryHeader();
		public abstract void AddCountrySpecificCharges(CusEntryHeader entryHeader);
		public abstract void AssertCustomsCharges(CustomsCharge[] charges);

		public void TestCustomsChargesContainAllChargesPaidWhenMessageClears()
		{
			CusEntryHeader entryHeader = GetEntryHeader();
			AddCountrySpecificCharges(entryHeader);
			ICustomsCharges entryAsICustomsCharges = ServiceLocator.GetService<ICustomsCharges>(entryHeader);
			CustomsCharge[] charges = entryAsICustomsCharges.GetCustomsCharges(null);
			Array.Sort(charges, new CustomsChargeComparerForTest());
			AssertCustomsCharges(charges);
		}

		public void AssertCustomsCharge(CustomsCharge charge, ZDecimal expectedAmount, ZString expectedDescription)
		{
			AssertCustomsCharge(charge, expectedAmount, 0.00m, expectedDescription);
		}

		public void AssertCustomsCharge(CustomsCharge charge, ZDecimal expectedAmount, ZDecimal expectedGST, ZString expectedDescription)
		{
			AssertEquals("Charge.Amount", expectedAmount, charge.Amount);
			AssertEquals("Charge.GST", expectedGST, charge.GST);
			AssertEquals("Charge.Description", expectedDescription, charge.Description);
		}
	}

	public class CustomsChargeComparerForTest : IComparer
	{
		public int Compare(object x, object y)
		{
			return CompareCustomsCharge(x as CustomsCharge, y as CustomsCharge);
		}

		int CompareCustomsCharge(CustomsCharge x, CustomsCharge y)
		{
			return x.Amount.CompareTo(y.Amount);
		}
	}
}
