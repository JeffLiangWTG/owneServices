using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class SortByPropertyComparerTest : TestCase
	{
		#region TestValueComparer

		public void TestValueComparer()
		{
			TestSortByPropertyComparer sortComparer = new TestSortByPropertyComparer();
			ZString str1, str2;
			str1 = ZString.Empty;
			str2 = ZString.Empty;
			AssertEquals(0, sortComparer.Compare(str1, str2));
			str1 = "1";
			AssertEquals(-1, sortComparer.Compare(str1, str2));
			str2 = str1;
			AssertEquals(0, sortComparer.Compare(str1, str2));
			str2 = "2";
			AssertEquals(-1, sortComparer.Compare(str1, str2));
			str1 = ZString.Empty;
			AssertEquals(1, sortComparer.Compare(str1, str2));
		}

		#endregion

		#region Implementation

		public class TestSortByPropertyComparer : SortByPropertiesComparer<IZType>
		{
			protected override IEnumerable<IComparer<IZType>> GetElementaryComparers()
			{
				yield return new ValueComparer(delegate(IZType value)
				{ return value; });
			}
		}

		#endregion
	}
}
