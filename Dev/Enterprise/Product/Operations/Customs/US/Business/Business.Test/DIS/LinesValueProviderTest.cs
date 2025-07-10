using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.DIS.Testing
{
	sealed class LinesValueProviderTest : TestCaseWithFactory
	{
		public void TestDetailsWhenContainersExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DateOfArrival = ZDateTime.BrettsBirthday;
			declaration.US_SchDEntry = "3901";
			declaration.US_SchDArrival = "3902";

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX4327894";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "ABCD1327895";

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.JI_Description = "description";

			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine2.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;
			invoiceLine2.JI_Description = "description2";

			var disInvoiceLines = new LinesValueProvider(invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>()).Lines;

			AssertEquals(3, disInvoiceLines.Count());

			var disInvoiceLine1 = disInvoiceLines.ElementAt(0);
			AssertEquals(1, disInvoiceLine1.InvoiceLineNumber);
			AssertEquals("CRUX4327894", disInvoiceLine1.CommodityDetails.ContainerNumber);
			AssertEquals("description", disInvoiceLine1.CommodityDetails.CommodityDescription);
			AssertEquals(ZDateTime.BrettsBirthday, disInvoiceLine1.ArrivalDate);
			AssertEquals("3901", disInvoiceLine1.PortOfEntry);
			AssertEquals("3902", disInvoiceLine1.PortOfUnlading);

			var disInvoiceLine2 = disInvoiceLines.ElementAt(1);
			AssertEquals(2, disInvoiceLine2.InvoiceLineNumber);
			AssertEquals("CRUX4327894", disInvoiceLine2.CommodityDetails.ContainerNumber);
			AssertEquals("description2", disInvoiceLine2.CommodityDetails.CommodityDescription);
			AssertEquals(ZDateTime.BrettsBirthday, disInvoiceLine2.ArrivalDate);
			AssertEquals("3901", disInvoiceLine2.PortOfEntry);
			AssertEquals("3902", disInvoiceLine2.PortOfUnlading);

			var disInvoiceLine3 = disInvoiceLines.ElementAt(2);
			AssertEquals(2, disInvoiceLine3.InvoiceLineNumber);
			AssertEquals("ABCD1327895", disInvoiceLine3.CommodityDetails.ContainerNumber);
			AssertEquals("description2", disInvoiceLine3.CommodityDetails.CommodityDescription);
			AssertEquals(ZDateTime.BrettsBirthday, disInvoiceLine3.ArrivalDate);
			AssertEquals("3901", disInvoiceLine3.PortOfEntry);
			AssertEquals("3902", disInvoiceLine3.PortOfUnlading);
		}

		public void TestDetailsWhenVehicleLinesExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "description";

			var vehicle = invoiceLine.VehicleLines.AddNew();
			var vehicleDetail = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetail.US_EngineManufacturer = "AUDI";

			var vehicleDetail2 = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetail2.US_EngineManufacturer = "HONDA";

			var disInvoiceLines = new LinesValueProvider(invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>()).Lines;

			AssertEquals(2, disInvoiceLines.Count());

			var disInvoiceLine1 = disInvoiceLines.ElementAt(0);
			AssertEquals(1, disInvoiceLine1.InvoiceLineNumber);
			AssertEquals("description", disInvoiceLine1.CommodityDetails.CommodityDescription);
			AssertEquals("AUDI", disInvoiceLine1.CommodityDetails.VehicleData.EngineManufacturer);

			var disInvoiceLine2 = disInvoiceLines.ElementAt(1);
			AssertEquals(1, disInvoiceLine2.InvoiceLineNumber);
			AssertEquals("description", disInvoiceLine2.CommodityDetails.CommodityDescription);
			AssertEquals("HONDA", disInvoiceLine2.CommodityDetails.VehicleData.EngineManufacturer);
		}

		public void TestDetailsWhenVehicleAndContainersExist()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX4327894";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "ABCD1327895";

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[0].IsForInvoiceLine = true;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly[1].IsForInvoiceLine = true;

			invoiceLine.JI_Description = "description";

			var vehicle = invoiceLine.VehicleLines.AddNew();
			var vehicleDetail = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetail.US_EngineManufacturer = "AUDI";

			var vehicleDetail2 = vehicle.VehicleAndEngineDetails.AddNew();
			vehicleDetail2.US_EngineManufacturer = "HONDA";

			var disInvoiceLines = new LinesValueProvider(invoice.JobComInvoiceLines.Cast<JobComInvoiceLine>()).Lines;

			AssertEquals(4, disInvoiceLines.Count());

			var disInvoiceLine1 = disInvoiceLines.ElementAt(0);
			AssertEquals(1, disInvoiceLine1.InvoiceLineNumber);
			AssertEquals("AUDI", disInvoiceLine1.CommodityDetails.VehicleData.EngineManufacturer);
			AssertEquals("CRUX4327894", disInvoiceLine1.CommodityDetails.ContainerNumber);

			var disInvoiceLine2 = disInvoiceLines.ElementAt(1);
			AssertEquals(1, disInvoiceLine2.InvoiceLineNumber);
			AssertEquals("AUDI", disInvoiceLine2.CommodityDetails.VehicleData.EngineManufacturer);
			AssertEquals("ABCD1327895", disInvoiceLine2.CommodityDetails.ContainerNumber);

			var disInvoiceLine3 = disInvoiceLines.ElementAt(2);
			AssertEquals(1, disInvoiceLine3.InvoiceLineNumber);
			AssertEquals("HONDA", disInvoiceLine3.CommodityDetails.VehicleData.EngineManufacturer);
			AssertEquals("CRUX4327894", disInvoiceLine3.CommodityDetails.ContainerNumber);

			var disInvoiceLine4 = disInvoiceLines.ElementAt(3);
			AssertEquals(1, disInvoiceLine4.InvoiceLineNumber);
			AssertEquals("HONDA", disInvoiceLine4.CommodityDetails.VehicleData.EngineManufacturer);
			AssertEquals("ABCD1327895", disInvoiceLine4.CommodityDetails.ContainerNumber);
		}
	}
}
