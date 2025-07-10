using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusVehicle))]
sealed class CusVehicleTest : EU.Business.Testing.CusVehicleAbstractTest
{
	public void TestCVH_ModelYearMaxLength() => AssertEquals(4, Factory.New<CusVehicle>().CVH_ModelYearInfo.MaxLength);

	public void TestCVH_VehicleIdentificationNumberMaxLength() => AssertEquals(17, Factory.New<CusVehicle>().CVH_VehicleIdentificationNumberInfo.MaxLength);

	public void TestEngine()
	{
		CombineAssertions(() =>
		{
			var vehicle = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().FirstVehicle;
			var engine = vehicle.Engine;
			AssertType<CusEngine>("Generated new engine", engine);

			engine.Delete();
			AssertNotEquals("Previous engine was deleted new one should be generated", engine.PK, vehicle.Engine.PK);

			vehicle.CVH_ModelName = "123";
			engine = vehicle.Engine;
			engine.CEG_EngineNumber = "123";
			Factory.Save();
			vehicle = Factory.Load<CusVehicle>(vehicle.PK);
			AssertEquals("After load", engine.PK, vehicle.Engine.PK);
		});
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		var vehicle = invoiceLine.FirstVehicle;
		var engine = vehicle.Engine;

		vehicle.CVH_ModelName = "ABC";
		vehicle.CVH_VehicleIdentificationNumber = "123";
		vehicle.CVH_ModelYear = "2002";

		engine.CEG_EngineNumber = "123";
		engine.CEG_CapacityCC = 100;
		engine.CEG_EngineType = FuelTypeList.Codes.B;

		return vehicle;
	}

	public void TestCaptions()
	{
		var vehicle = Factory.New<CusVehicle>();
		CombineAssertions(() =>
		{
			AssertEquals("CVH_ModelYear caption", "Production Year", DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_ModelYearInfo).Caption);
			AssertEquals("CVH_VehicleIdentificationNumber caption", "VIN", DataBoundResourceStrings.GetDataForProperty(vehicle.CVH_VehicleIdentificationNumberInfo).Caption);
		});
	}

	public void TestSupportsClone()
	{
		var vehicle = Factory.New<CusVehicle>();
		Assert("PL Vehicle supports clone", vehicle.SupportsClone());
	}

	protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
	protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
	protected override EngineRelationshipType ExpectedEngineRelationship => EngineRelationshipType.One;
}
