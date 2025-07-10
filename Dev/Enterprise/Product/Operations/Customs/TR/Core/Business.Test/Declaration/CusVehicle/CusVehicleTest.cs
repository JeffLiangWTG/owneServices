using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(CusVehicle))]
	class CusVehicleTest : EU.Business.Testing.CusVehicleAbstractTest
	{
		public void TestCVH_SerialNumber()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<MaxLengthAttribute>(vehicle.GetType(), "CVH_SerialNumber", false, attr => attr.MaxLength == 30);
		}

		public void TestCVH_ModelYear()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<MaxLengthAttribute>(vehicle.GetType(), "CVH_ModelYear", false, attr => attr.MaxLength == 4);
		}

		public void TestCVH_ModelName()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<MaxLengthAttribute>(vehicle.GetType(), "CVH_ModelName", false, attr => attr.MaxLength == 20);
		}

		public void TestCVH_Color()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<MaxLengthAttribute>(vehicle.GetType(), "CVH_Color", false, attr => attr.MaxLength == 15);
		}

		public void TestCVH_VehicleIdentificationNumber()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<MaxLengthAttribute>(vehicle.GetType(), "CVH_VehicleIdentificationNumber", false, attr => attr.MaxLength == 17);
		}

		public void TestCVH_BrandName()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals(60, vehicle.CVH_BrandNameInfo.MaxLength);
		}

		public void TestCVH_IMEINo()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals(15, vehicle.CVH_IMEINoInfo.MaxLength);
		}

		public void TestGears()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertHasCustomAttribute<ListAttribute>(vehicle.GetType(), "Gears", false, attr => attr.ListDataSourceMember == "Lookups.GearsDescriptionList");

			vehicle.Gears = "Automatic";
			AssertEquals(1, vehicle.CVH_Gears.ToZInt());

			vehicle.Gears = "XXX";
			AssertEquals(1, vehicle.CVH_Gears.ToZInt());

			vehicle.Gears = "";
			AssertEquals(0, vehicle.CVH_Gears.ToZInt());

			vehicle.CVH_Gears = 1;
			AssertEquals("Automatic", vehicle.Gears);

			vehicle.CVH_Gears = 0;
			AssertEquals("", vehicle.Gears);

			vehicle.CVH_Gears = 9;
			AssertEquals("", vehicle.Gears);
		}

		public void TestEngine()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals(true, vehicle.Engine.PK.IsValid);

			vehicle = Factory.New<CusVehicle>();
			var engine = Factory.New<CusEngine>();
			engine.CEG_ParentID = vehicle.PK;
			engine.CEG_ParentTableCode = CusVehicleSchema.Constants.Prefix;
			AssertSame(engine, vehicle.Engine);
		}

		public void TestInvoiceLine()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertNull(vehicle.InvoiceLine);

			var invoiceLine = Factory.New<JobComInvoiceLine>();
			vehicle.CVH_ParentTableCode = invoiceLine.TablePrefix;
			vehicle.CVH_ParentID = invoiceLine.PK;
			AssertSame(invoiceLine, vehicle.InvoiceLine);
		}

		public void TestInvoiceLineProperties()
		{
			var vehicle = Factory.New<CusVehicle>();
			AssertEquals(0m, vehicle.BrandValue);
			AssertEquals(0m, vehicle.BrandValueInTRY);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			vehicle = Factory.New<CusVehicle>();
			vehicle.CVH_ParentTableCode = invoiceLine.TablePrefix;
			vehicle.CVH_ParentID = invoiceLine.PK;
			invoiceHeader.JZ_InvoiceCurrExRate = 9.985700m;
			invoiceLine.JI_LinePrice = 20000.13m;
			AssertEquals(20000.13m, vehicle.BrandValue);
			var trCurrency = RefCurrency.LoadFromCurrencyCode(Factory, "TRY");
			AssertEquals(199715.30m, vehicle.BrandValueInTRY.Round(trCurrency.Decimals));
		}

		protected override Type ExpectedLookupsType => typeof(CusVehicleLookups);
		protected override Type ExpectedValidationType => typeof(CusVehicleValidation);
		protected override EngineRelationshipType ExpectedEngineRelationship => EngineRelationshipType.One;
		protected override BusinessObject GetVehicleParent(BusinessObjectFactory factory) => factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
	}
}
