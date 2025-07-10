using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class UpdateProductMeasurablesAndUnitRatesTest : WhsSecureServiceTestCase
	{
		public void TestUpdate_JustProduct()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			AssertEquals("Precondition: OP_Width as expected", 0m, product.OP_Width);
			AssertEquals("Precondition: OP_Height as expected", 0m, product.OP_Height);
			AssertEquals("Precondition: OP_Depth as expected", 0m, product.OP_Depth);
			AssertEquals("Precondition: OP_MeasureUQ as expected", string.Empty, product.OP_MeasureUQ);

			product.OP_Weight = 0m;
			product.OP_WeightUQ = string.Empty;
			product.OP_Cubic = 0m;

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 3 part units", 3, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			productInfo.Width = 10m;
			productInfo.Height = 5m;
			productInfo.Depth = 3m;
			productInfo.MeasureUQ = "CM";

			productInfo.Weight = 25m;
			productInfo.WeightUQ = "G";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);

			AssertEquals("OP_Width as expected", 10m, product.OP_Width);
			AssertEquals("OP_Height as expected", 5m, product.OP_Height);
			AssertEquals("OP_Depth as expected", 3m, product.OP_Depth);
			AssertEquals("OP_MeasureUQ as expected", "CM", product.OP_MeasureUQ);

			AssertEquals("OP_Weight as expected", 25m, product.OP_Weight);
			AssertEquals("OP_WeightUQ as expected", "G", product.OP_WeightUQ);
		}

		public void TestUpdate_JustUnitRates()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 3 part units", 3, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			var rateToUpdate = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate.Depth = 12m;
			rateToUpdate.Height = 7m;
			rateToUpdate.Width = 45m;
			rateToUpdate.Weight = 10m;
			rateToUpdate.MeasurementUQ = "M";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT", 12m, 7m, 45m, 3780m, 10m);
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");
		}

		public void TestUpdate_TwoUnitRates()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			var extraPartUnit = product.PartUnits.AddNew();
			extraPartUnit.OF_ParentPackType = "PLT";
			extraPartUnit.OF_PackType = "CTN";
			extraPartUnit.OF_QuantityInParent = 50m;
			Helper.Factory.Save();

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 4 part units", 4, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "PLT", "CTN");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			var rateToUpdate1 = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate1.Depth = 15m;
			rateToUpdate1.Height = 5m;
			rateToUpdate1.Width = 25m;
			rateToUpdate1.Weight = 10m;
			rateToUpdate1.MeasurementUQ = "M";

			var rateToUpdate2 = productInfo.ProductUnits.First(r => r.Parent == "PLT");
			rateToUpdate2.Depth = 10m;
			rateToUpdate2.Height = 12m;
			rateToUpdate2.Width = 15m;
			rateToUpdate2.Weight = 75m;
			rateToUpdate2.MeasurementUQ = "M";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT", 15m, 5m, 25m, 1875m, 10m);
			AssertPartUnitExists(productPartUnits, "PLT", "CTN", 10m, 12m, 15m, 1800m, 75m);
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");
		}

		public void TestUpdate_Both()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			AssertEquals("Precondition: OP_Width as expected", 0m, product.OP_Width);
			AssertEquals("Precondition: OP_Height as expected", 0m, product.OP_Height);
			AssertEquals("Precondition: OP_Depth as expected", 0m, product.OP_Depth);
			AssertEquals("Precondition: OP_MeasureUQ as expected", string.Empty, product.OP_MeasureUQ);

			product.OP_Weight = 0m;
			product.OP_WeightUQ = string.Empty;

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 3 part units", 3, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			productInfo.Width = 10m;
			productInfo.Height = 5m;
			productInfo.Depth = 3m;
			productInfo.MeasureUQ = "CM";

			productInfo.Weight = 25m;
			productInfo.WeightUQ = "G";

			var rateToUpdate = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate.Depth = 12m;
			rateToUpdate.Height = 7m;
			rateToUpdate.Width = 45m;
			rateToUpdate.Weight = 10m;
			rateToUpdate.MeasurementUQ = "M";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);

			AssertEquals("OP_Width as expected", 10m, product.OP_Width);
			AssertEquals("OP_Height as expected", 5m, product.OP_Height);
			AssertEquals("OP_Depth as expected", 3m, product.OP_Depth);
			AssertEquals("OP_MeasureUQ as expected", "CM", product.OP_MeasureUQ);

			AssertEquals("OP_Weight as expected", 25m, product.OP_Weight);
			AssertEquals("OP_WeightUQ as expected", "G", product.OP_WeightUQ);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT", 12m, 7m, 45m, 3780m, 10m);
			AssertPartUnitExists(productPartUnits, "UNT", "G");
		}

		public void TestUpdate_Error_MissingProduct()
		{
			var webService = GetNewWebService();
			var response = webService.UpdateProductMeasurablesAndUnitRates(new WhsProductInfo());
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Product could not be found. Please restart the Unload and try again.", response.ErrorMessage);
		}

		public void TestUpdate_Error_ZSaveException()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				var row = ((INeedRow)data.Part1).Row;
				throw new ZSaveException(new DummyDataException(row, TestConnection), webService.Factory);
			}
			webService.Factory.Saving += action;
			var response = webService.UpdateProductMeasurablesAndUnitRates(new WhsProductInfo(data.Part1));
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Product could not be saved. Blah", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		public void TestUpdate_Error_ZCannotSaveException()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			void action(BusinessObjectFactory factory)
			{
				UnitTestUserNotification.Instance.ClearMessages();
				webService.Factory.Saving -= action;
				throw new ZCannotSaveException("Test - Cannot Save", "Test");
			}
			webService.Factory.Saving += action;
			var response = webService.UpdateProductMeasurablesAndUnitRates(new WhsProductInfo(data.Part1));
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Product could not be saved. Test - Cannot Save", response.ErrorMessage);
			Assert("There should be no error reported.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}

		public void TestUpdate_Error_VolumeValueToBig()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			var extraPartUnit = product.PartUnits.AddNew();
			extraPartUnit.OF_ParentPackType = "PLT";
			extraPartUnit.OF_PackType = "CTN";
			extraPartUnit.OF_QuantityInParent = 50m;
			Helper.Factory.Save();

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 4 part units", 4, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "PLT", "CTN");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			var rateToUpdate1 = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate1.Depth = 15m;
			rateToUpdate1.Height = 5m;
			rateToUpdate1.Width = 25m;
			rateToUpdate1.Weight = 10m;
			rateToUpdate1.MeasurementUQ = "M";

			var rateToUpdate2 = productInfo.ProductUnits.First(r => r.Parent == "PLT");
			rateToUpdate2.Depth = 210m;
			rateToUpdate2.Height = 212m;
			rateToUpdate2.Width = 315m;
			rateToUpdate2.Weight = 75m;
			rateToUpdate2.MeasurementUQ = "M";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Dimensions for Pack type(s) 'PLT' could not be saved. As Volume calculated is larger than max allowed value. Please enter dimensions of affected pack type(s) in the desktop platform.", response.ErrorMessage);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT", 15m, 5m, 25m, 1875m, 10m);
			AssertPartUnitExists(productPartUnits, "PLT", "CTN");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");
		}

		public void TestUpdate_Error_MultipleVolumeValueToBig()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			var extraPartUnit = product.PartUnits.AddNew();
			extraPartUnit.OF_ParentPackType = "PLT";
			extraPartUnit.OF_PackType = "CTN";
			extraPartUnit.OF_QuantityInParent = 50m;
			Helper.Factory.Save();

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 4 part units", 4, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "PLT", "CTN");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			var rateToUpdate1 = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate1.Depth = 215m;
			rateToUpdate1.Height = 225m;
			rateToUpdate1.Width = 225m;
			rateToUpdate1.Weight = 210m;
			rateToUpdate1.MeasurementUQ = "M";

			var rateToUpdate2 = productInfo.ProductUnits.First(r => r.Parent == "PLT");
			rateToUpdate2.Depth = 210m;
			rateToUpdate2.Height = 212m;
			rateToUpdate2.Width = 315m;
			rateToUpdate2.Weight = 75m;
			rateToUpdate2.MeasurementUQ = "M";

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);
			AssertEquals("Response shows correct error type", ErrorTypes.BusinessValidationError, response.Error);
			AssertEquals("Response shows correct error message", "Dimensions for Pack type(s) 'CTN, PLT' could not be saved. As Volume calculated is larger than max allowed value. Please enter dimensions of affected pack type(s) in the desktop platform.", response.ErrorMessage);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "PLT", "CTN");
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");
		}

		public void TestUpdate_OrgPartUnit_IgnoresUpdateIfNoMeasureUQ()
		{
			var webService = GetNewWebService();
			var data = new TestDataSimpleEnvironment(Helper.Factory);
			Helper.Factory.Save();

			var product = data.Part1;
			var extraPartUnit = product.PartUnits.AddNew();
			extraPartUnit.OF_ParentPackType = "PLT";
			extraPartUnit.OF_PackType = "CTN";
			extraPartUnit.OF_QuantityInParent = 50m;
			extraPartUnit.OF_Depth = 50m;
			extraPartUnit.OF_Width = 50m;
			extraPartUnit.OF_Height = 50m;
			extraPartUnit.OF_Cubic = 125000m;
			extraPartUnit.OF_Weight = 15m;
			Helper.Factory.Save();

			var productPartUnits = product.PartUnits.Cast<OrgPartUnit>();
			AssertEquals("Precondition: Part has 4 part units", 4, productPartUnits.Count());
			AssertPartUnitExists(productPartUnits, "CTN", "UNT");
			AssertPartUnitExists(productPartUnits, "PLT", "CTN", 50m, 50m, 50m, 125000m, 15m);
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");

			// Change productInfo like VolCam scanner will
			var productInfo = new WhsProductInfo(product);
			var rateToUpdate1 = productInfo.ProductUnits.First(r => r.Parent == "CTN");
			rateToUpdate1.Depth = 15m;
			rateToUpdate1.Height = 25m;
			rateToUpdate1.Width = 25m;
			rateToUpdate1.Weight = 210m;
			rateToUpdate1.MeasurementUQ = "CM";

			var rateToUpdate2 = productInfo.ProductUnits.First(r => r.Parent == "PLT");
			rateToUpdate2.Depth = 10m;
			rateToUpdate2.Height = 12m;
			rateToUpdate2.Width = 15m;
			rateToUpdate2.Weight = 75m;

			var response = webService.UpdateProductMeasurablesAndUnitRates(productInfo);
			AssertSuccessfulResponse(response, webService);

			AssertPartUnitExists(productPartUnits, "CTN", "UNT", 15, 25m, 25m, 0.009m, 210m);
			AssertPartUnitExists(productPartUnits, "PLT", "CTN", 50m, 50m, 50m, 125000m, 15m);
			AssertPartUnitExists(productPartUnits, "UNT", "KG");
			AssertPartUnitExists(productPartUnits, "UNT", "M3");
		}

		static void AssertPartUnitExists(IEnumerable<OrgPartUnit> partUnits, string parentPackType, string packType, decimal depth = 0m, decimal height = 0m, decimal width = 0m, decimal cubic = 0m, decimal weight = 0m)
		{
			AssertEquals(
				$"Collection contains unit rate as expected of ParentPackType: {packType}",
				true,
				partUnits.Any(a => a.OF_Cubic == cubic
													&& a.OF_Depth == depth
													&& a.OF_Height == height
													&& a.OF_Width == width
													&& a.OF_Weight == weight
													&& a.OF_ParentPackType == parentPackType
													&& a.OF_PackType == packType));
		}
	}
}
