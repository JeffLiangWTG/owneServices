using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.TransportCommon.DataTransfer.Universal.Test
{
	public class DataTypeExtensionTest : TestCaseWithFactory
	{
		#region TestGetHashCodeForNullableDataTypes

		public void TestGetHashCodeForNullableDataTypes()
		{
			ZString? nullableString = null;
			AssertEquals(0, nullableString.GetHashCodeForNullableDataTypes());

			string testValue = "TEST";
			nullableString = testValue;
			AssertEquals(testValue.GetHashCode(), nullableString.GetHashCodeForNullableDataTypes());
		}

		#endregion
	}
}
