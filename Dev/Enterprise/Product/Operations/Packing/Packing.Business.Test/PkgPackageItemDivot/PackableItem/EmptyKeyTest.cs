using NUnit.Framework;

namespace Enterprise.Packing.Business.Testing
{
	class EmptyKeyTest : TestCase
	{
		#region TestGetHashCode

		public void TestGetHashCode()
		{
			AssertEquals(-1, EmptyKey.Instance.GetHashCode());
		}

		#endregion

		#region TestEquals

		public void TestEquals()
		{
			AssertEquals(true, EmptyKey.Instance.Equals(EmptyKey.Instance));

			var instance = EmptyKey.Instance;
			AssertEquals(true, instance == EmptyKey.Instance);
		}

		#endregion

		#region TestInstance

		public void TestInstance()
		{
			AssertEquals(EmptyKey.Instance, EmptyKey.Instance);
		}

		#endregion

		#region TestTemporaryProperGroupingKeyForWhs_DoNotUseCore

		public void TestTemporaryProperGroupingKeyForWhs_DoNotUseCore()
		{
			AssertEquals(true, EmptyKey.Instance.IsSimilarItem_DoNotUse(EmptyKey.Instance));
		}

		#endregion
	}
}
