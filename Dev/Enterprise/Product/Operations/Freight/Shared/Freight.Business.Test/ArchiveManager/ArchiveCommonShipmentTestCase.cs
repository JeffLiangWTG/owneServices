using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using NUnit.Framework;

namespace Enterprise.Freight.Business.ArchiveManager.Testing
{
	sealed class ArchiveCommonShipmentTestCase : TestCase
	{
		[ExpectNoExceptions]
		public void TestArchiveCommonShipmentOrderReferences()
		{
			var bizOFactory = new BusinessObjectFactory();
			var commonShipment = CommonShipment.New(bizOFactory);
			commonShipment.DocsAndCartage.JP_OrderItemsAsString = "123451234512345123451234512345123451234512345123451234512345,12345123451234512345123451234512345123451234512345123451234512345123";
			ArchiveCommonShipment archiveCommonShipment = new ArchiveCommonShipment(commonShipment);

			IArchiveableBusinessObject archiveable = archiveCommonShipment;
			var additionalKeys = archiveable.AdditionalKeys;
			AssertEquals("should be two order references in collections", 2, additionalKeys.Count());
			Assert(ErrorReporter.TotalErrorCount == 0);
		}
	}
}
