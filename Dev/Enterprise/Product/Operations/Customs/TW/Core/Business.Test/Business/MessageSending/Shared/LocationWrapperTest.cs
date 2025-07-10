using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LocationWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLocationWrapper()
		{
			ILocation location = new LocationWrapper();
			NUnit.Framework.Assert.That(location.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(location.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(location.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(location.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			location = new LocationWrapper("Test ID");
			NUnit.Framework.Assert.That(location.ID, NUnit.Framework.Is.EqualTo("Test ID").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(location.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(location.LoadingDateTime, NUnit.Framework.Is.EqualTo(ZDateTime.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(location.EstimatedLoadingCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);

			var testDatetime = new ZDate(2022, 6, 2);
			location = new LocationWrapper("Test ID", "Test Name", testDatetime, "Test Estimated Loading Code");
			NUnit.Framework.Assert.That(location.ID, NUnit.Framework.Is.EqualTo("Test ID").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(location.Name, NUnit.Framework.Is.EqualTo("Test Name").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(location.LoadingDateTime, NUnit.Framework.Is.EqualTo(testDatetime));
			NUnit.Framework.Assert.That(location.EstimatedLoadingCode, NUnit.Framework.Is.EqualTo("Test Estimated Loading Code").Using(CustomComparers.TypeComparison));
		}
	}
}
