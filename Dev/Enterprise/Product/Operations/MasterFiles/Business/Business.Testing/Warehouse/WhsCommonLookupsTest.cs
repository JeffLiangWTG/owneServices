using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WhsCommonLookupsTest : TestCaseWithFactory
	{
		#region TestGetPrintersList

		public void TestGetPrintersList()
		{
			var printer1 = Factory.New<IStmPrintQueue>();
			var printer2 = Factory.New<IStmPrintQueue>();
			var printer3 = Factory.New<IStmPrintQueue>();
			printer1.SQ_AllowPrinting = true;
			printer2.SQ_AllowPrinting = true;
			printer3.SQ_AllowPrinting = false;
			((BusinessObject)printer2)[StmPrintQueueSchema.SQ_QueueDeleted] = ZDateTime.Now.AddYears(-1);

			var printersList = WhsCommonLookups.GetPrintersList(Factory);
			AssertNotNull("Should not return null collection.", printersList);
			AssertEquals("Collection should have correct Type.", ObjectFactory.GetType<IStmPrintQueueCollection>(), printersList.GetType());
			AssertContainsExactElementsInAnyOrder("Should only Container non-deleted printers that are allowed to print.", new[] { printer1 }, printersList);
		}

		#endregion
	}
}
