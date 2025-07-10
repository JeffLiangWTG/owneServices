using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AddressWrapperTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLine()
		{
			NUnit.Framework.Assert.That(wrapper.Line, NUnit.Framework.Is.EqualTo("Line").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseLine()
		{
			NUnit.Framework.Assert.That(wrapper.ChineseLine, NUnit.Framework.Is.EqualTo("ChineseLine").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountryCode()
		{
			NUnit.Framework.Assert.That(wrapper.CountryCode, NUnit.Framework.Is.EqualTo("CountryCode").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionID()
		{
			NUnit.Framework.Assert.That(wrapper.CountrySubDivisionID, NUnit.Framework.Is.EqualTo("CountrySubDivisionID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCountrySubDivisionName()
		{
			NUnit.Framework.Assert.That(wrapper.CountrySubDivisionName, NUnit.Framework.Is.EqualTo("CountrySubDivisionName").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new AddressWrapper("Line", "ChineseLine", "CountryCode", "CountrySubDivisionID", "CountrySubDivisionName");
		}

		AddressWrapper wrapper;
	}
}
