using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IOrgPatternMatchComparerTest : TestCaseWithFactory
	{
		public void TestEquals()
		{
			var paternMatchFred1 = GetNewPatternMatch("Fred");
			var paternMatchFred2 = GetNewPatternMatch("Fred");
			var paternMatchBarny = GetNewPatternMatch("Barny");

			var comparer = new IOrgPatternMatchComparer();
			AssertEquals("Fred1 and Fred2", true, comparer.Equals(paternMatchFred1, paternMatchFred2));
			AssertEquals("Fred1 and Barny", false, comparer.Equals(paternMatchFred1, paternMatchBarny));
			AssertEquals("Fred2 and Barny", false, comparer.Equals(paternMatchFred2, paternMatchBarny));
		}

		public void TestGetHashCode()
		{
			var paternMatchFred1 = GetNewPatternMatch("Fred");
			var paternMatchFred2 = GetNewPatternMatch("Fred");
			var paternMatchBarny = GetNewPatternMatch("Barny");

			var comparer = new IOrgPatternMatchComparer();
			AssertEquals("Fred1 and Fred2", comparer.GetHashCode(paternMatchFred1), comparer.GetHashCode(paternMatchFred2));
			AssertNotEquals("Fred1 and Barny", comparer.GetHashCode(paternMatchFred1), comparer.GetHashCode(paternMatchBarny));
			AssertNotEquals("Fred2 and Barny", comparer.GetHashCode(paternMatchFred2), comparer.GetHashCode(paternMatchBarny));
		}

		IOrgPatternMatch GetNewPatternMatch(ZString suffix)
		{
			var mock = new Mock<IOrgPatternMatch>();
			mock.Setup(m => m.OS_FullCompanyName).Returns("Company " + suffix);
			mock.Setup(m => m.OS_Address1).Returns("Address1 " + suffix);
			mock.Setup(m => m.OS_Address2).Returns("Address2 " + suffix);
			mock.Setup(m => m.OS_Address3).Returns("Address3 " + suffix);
			mock.Setup(m => m.OS_Address4).Returns("Address4 " + suffix);
			mock.Setup(m => m.OS_BusinessRegNo).Returns("BusinessRegNo " + suffix);
			return mock.Object;
		}
	}
}
