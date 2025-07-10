using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class TransportMeansWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestJourneyID()
		{
			ITransportMeans transportMeans = new TransportMeansWrapper("JourneyID");
			NUnit.Framework.Assert.That(transportMeans.JourneyID, NUnit.Framework.Is.EqualTo("JourneyID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			ITransportMeans transportMeans = new TransportMeansWrapper("JourneyID", "Name");
			NUnit.Framework.Assert.That(transportMeans.Name, NUnit.Framework.Is.EqualTo("Name").Using(CustomComparers.TypeComparison));
		}
	}
}
