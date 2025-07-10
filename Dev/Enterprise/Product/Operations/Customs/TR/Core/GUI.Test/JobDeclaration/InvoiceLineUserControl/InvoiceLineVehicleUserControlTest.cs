using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	class InvoiceLineVehicleUserControlTest : TestCaseWithFactory
	{
		public void TestRegistrationNumber()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_RegistrationNumber, "Registration No", 100);
		}

		public void TestBrandName()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_BrandName, "Brand Name", 80);
		}

		public void TestBrandValue()
		{
			AssertGridDetails(CusVehicle.Schema.BrandValue, "Brand Value", 80);
		}

		public void TestBrandValueInTRY()
		{
			AssertGridDetails(CusVehicle.Schema.BrandValueInTRY, "Brand Value in TRY", 110);
		}

		public void TestSerialNumber()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_SerialNumber, "Reference No", 120);
		}

		public void TestModelYear()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_ModelYear, "Model Year", 80);
		}

		public void TestModelName()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_ModelName, "Model Name", 80);
		}

		public void TestEngineCC()
		{
			using (var control = new InvoiceLineVehicleUserControl())
			{
				var grid = control.FindSingle<ZGrid>("VehicleGrid");
				var columnStyle = grid.GetColumnStyle("Engine+CEG_CapacityCC");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Motor CC", columnStyle.CaptionResourceString.Caption);
					AssertEquals("Width", 80, columnStyle.Width);
					AssertEquals("Max Value", 9999m, ((ZCalcEditColumnStyleInfo)columnStyle).MaxValue);
				});
			}
		}

		public void TestCylinderQty()
		{
			using (var control = new InvoiceLineVehicleUserControl())
			{
				var grid = control.FindSingle<ZGrid>("VehicleGrid");
				var columnStyle = grid.GetColumnStyle("Engine+CEG_Cylinders");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Cylinder Qty", columnStyle.CaptionResourceString.Caption);
					AssertEquals("Width", 80, columnStyle.Width);
					AssertEquals("Max Value", 9m, ((ZCalcEditColumnStyleInfo)columnStyle).MaxValue);
				});
			}
		}

		public void TestColor()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_Color, "Color", 80);
		}

		public void TestEngineType()
		{
			AssertGridDetails("Engine+CEG_EngineType", "Engine Type", 80);
		}

		public void TestVehicleIdentificationNumber()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_VehicleIdentificationNumber, "VIN", 120);
		}

		public void TestEngineHP()
		{
			using (var control = new InvoiceLineVehicleUserControl())
			{
				var grid = control.FindSingle<ZGrid>("VehicleGrid");
				var columnStyle = grid.GetColumnStyle("Engine+CEG_CapacityHP");
				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Engine HP", columnStyle.CaptionResourceString.Caption);
					AssertEquals("Width", 80, columnStyle.Width);
					AssertEquals("Max Value", 9999m, ((ZCalcEditColumnStyleInfo)columnStyle).MaxValue);
				});
			}
		}

		public void TestGears()
		{
			AssertGridDetails(CusVehicle.Schema.Gears, "Gear Type", 80);
		}

		public void TestIMEINo()
		{
			AssertGridDetails(CusVehicle.Schema.CVH_IMEINo, "IMEI No", 105);
		}

		void AssertGridDetails(string columnName, string caption, int width)
		{
			using (var control = new InvoiceLineVehicleUserControl())
			{
				var grid = control.FindSingle<ZGrid>("VehicleGrid");
				var columnStyle = grid.GetColumnStyle(columnName);
				CombineAssertions(() =>
				{
					AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
					AssertEquals("Width", width, columnStyle.Width);
				});
			}
		}
	}
}
