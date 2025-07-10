using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class JobComInvoiceLineDeepCloneStrategyTest : TestCaseWithFactory
{
	public void TestVehiclesAreCopiedForImpDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var vehicle = invoiceLine.FirstVehicle;
		var engine = vehicle.Engine;

		vehicle.CVH_ModelName = "ABC";
		vehicle.CVH_VehicleIdentificationNumber = "123";
		vehicle.CVH_ModelYear = "2002";
		engine.CEG_EngineNumber = "123";
		engine.CEG_CapacityCC = 100;
		engine.CEG_EngineType = FuelTypeList.Codes.B;

		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var declarationCopy = (JobDeclaration)declaration.TemplateCopy();
		var invoiceLineCopy = declarationCopy.Invoices[0].InvoiceLines[0];

		CombineAssertions(() =>
		{
			AssertEquals("Vehicle info is copied for IMP dec", 1, invoiceLineCopy.Vehicles.Count);

			var vehicleCopy = invoiceLineCopy.FirstVehicle;
			AssertEquals("Vehicle CVH_ModelName", "ABC", vehicleCopy.CVH_ModelName);
			AssertEquals("Vehicle CVH_VehicleIdentificationNumber", "123", vehicleCopy.CVH_VehicleIdentificationNumber);
			AssertEquals("Vehicle CVH_ModelYear", "2002", vehicleCopy.CVH_ModelYear);

			var engineCopy = vehicleCopy.Engine;
			AssertEquals("Engine CEG_EngineNumber", "123", engineCopy.CEG_EngineNumber);
			AssertEquals("Engine CEG_CapacityCC", new ZDecimal(100), engineCopy.CEG_CapacityCC);
			AssertEquals("Engine CEG_EngineType", FuelTypeList.Codes.B, engineCopy.CEG_EngineType);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declarationCopy = (JobDeclaration)declaration.TemplateCopy();
			invoiceLineCopy = declarationCopy.Invoices[0].InvoiceLines[0];
			AssertEquals("Vehicle Info not copied for EXP dec", 0, invoiceLineCopy.Vehicles.Count);
		});
	}
}
