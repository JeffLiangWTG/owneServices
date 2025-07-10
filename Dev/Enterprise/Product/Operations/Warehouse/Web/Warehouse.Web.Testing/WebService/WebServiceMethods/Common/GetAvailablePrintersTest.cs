using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class GetAvailablePrintersTest : WhsSecureServiceTestCase
	{
		#region TestGetAvailablePrinters

		public void TestGetAvailablePrinters_NoPrinters()
		{
			var webService = GetNewWebService();
			PopulateSecurityHeader(webService);

			var response = webService.GetAvailablePrinters();
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Printers);
			AssertEquals("With no Printers Available, the Printers collection should be empty.", 0, response.Printers.Length);
		}

		public void TestGetAvailablePrinters()
		{
			var printer1 = Helper.CreatePrintQueue("1");
			var printer2 = Helper.CreatePrintQueue("2");
			var printer3 = Helper.CreatePrintQueue("3");
			printer3.SQ_AllowPrinting = false;
			((BusinessObject)printer2)[StmPrintQueueSchema.SQ_QueueDeleted] = ZDateTime.Now.AddYears(-1);
			Helper.Factory.Save();

			var webService = GetNewWebService();
			var response = webService.GetAvailablePrinters();
			AssertSuccessfulResponse(response, webService);
			AssertNotNull(response.Printers);
			AssertContainsExactElementsInAnyOrder(new[] { Guid.Empty, printer1.PK }, response.Printers.Select(p => p.PK));
		}

		#endregion
	}
}
