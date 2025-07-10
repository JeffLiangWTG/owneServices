using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(NX401ExportGoodsShipmentConsignmentBorderTransportMeans))]
	sealed class NX401ExportGoodsShipmentConsignmentBorderTransportMeansTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var transportMeans = new NX401ExportGoodsShipmentConsignmentBorderTransportMeans(declaration);
			NUnit.Framework.Assert.That(transportMeans.ID.ToString(), NUnit.Framework.Is.Null.Or.Empty, "Should be empty - should be [null] or [empty]");
		}
	}
}
