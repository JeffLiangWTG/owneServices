using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ExportAWBRateLineCollection))]
	sealed class ExportAWBRateLineCollectionBOTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ShipmentExportAWBHeader master = Factory.New<ShipmentExportAWBHeader>();
			return new ExportAWBRateLineCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ExportAWBRateLine>();
		}
	}
}
