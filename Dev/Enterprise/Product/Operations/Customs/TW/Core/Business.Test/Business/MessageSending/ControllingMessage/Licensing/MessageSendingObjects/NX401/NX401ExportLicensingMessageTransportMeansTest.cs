using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportLicensingMessageTransportMeans))]
	sealed class NX401ExportLicensingMessageTransportMeansTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBorderTransportMeansArrivalDateTime()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_DateOfArrival = new ZDateTime(2024, 6, 28);
			var transportMeans = new NX401ExportLicensingMessageTransportMeans(declaration);
			NUnit.Framework.Assert.That(transportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(ZDate.Empty), "ArrivalDateTime");
		}
	}
}
