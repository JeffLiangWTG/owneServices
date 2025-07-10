using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ContainerEventsDataImporterTest : TestCaseWithFactory
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			container.JC_ContainerNum = "CONTAINER45";
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2006, 12, 17);
			consol.Transports.MostInterestingTransport.JW_Vessel = "VesselName";
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "Voyage";
			consol.Containers.Add(container);
			Factory.Save();

			Assert("FCLStorageCommences before import", container.JC_ArrivalCTOStorageStartDate.IsEmpty);

			ContainerEventsDataImporter importer = new ContainerEventsDataImporter();
			NotificationBuffer notifications = new NotificationBuffer();
			importer.ImportData(Path.Combine(pathToFile, "ContainerEvents.xml"), notifications, SourceInfo.EmptySourceInfo);

			Assert("Should not be errors", !notifications.HasErrors);
			AssertEquals("FCLStorageCommences after import", new ZDateTime(2006, 12, 19), container.JC_ArrivalCTOStorageStartDate.Date);
			Assert(notifications.AsString.Contains("Container with Job='C00001000' Container Number='CONTAINER45' - FCLStorageCommences updated"));
			Assert(notifications.AsString.Contains("Could not find container with Job='C00001000' Container Number='CONTAINER34'"));

			importer.ImportData(Path.Combine(pathToFile, "ContainerEventsWithInvalidDate.xml"), notifications, SourceInfo.EmptySourceInfo);

			Assert("Should be errors", notifications.HasErrors);
			AssertEquals("FCLStorageCommences should remain unchanged", new ZDateTime(2006, 12, 19), container.JC_ArrivalCTOStorageStartDate.Date);
			Assert(notifications.AsString.Contains("The string '2013-05-02T00:00:00.0000000+9:30' is not a valid AllXsd value."));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
		readonly string pathToFile = BaseSourcePath + @"Enterprise\Product\Operations\Freight\Forwarding\Forwarding.DataTransfer.Test\TestFiles\";
	}
}
