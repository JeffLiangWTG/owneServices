using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(CarrierLabelPrinterOption))]
	class CarrierLabelPrinterOptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCarrierLabelPrinterPK()
		{
			var printer1 = Factory.New<IStmPrintQueue>();
			printer1.QueueName = "PRINTER1";
			printer1.SQ_AllowPrinting = true;

			var printer2 = Factory.New<IStmPrintQueue>();
			printer2.QueueName = "PRINTER2";
			printer2.SQ_AllowPrinting = false;

			var printer3 = Factory.New<IStmPrintQueue>();
			printer3.QueueName = "PRINTER3";
			Env.Security.GetPrintQueueCheckPoint(printer3.PK.ToGuid(), printer3.SQ_DisplayName).IsAllowed = false;
			Factory.Save();

			var carrierLabelPrinterOption = new CarrierLabelPrinterOption(Factory);
			carrierLabelPrinterOption.CarrierLabelPrinterPK = printer1.PK;
			Assert("There should be no errors.", !carrierLabelPrinterOption.HasErrors);

			carrierLabelPrinterOption.CarrierLabelPrinterPK = ZGuid.Empty;
			AssertHasError(carrierLabelPrinterOption.CarrierLabelPrinterPKInfo,
				"Please select a Carrier Label Printer.");

			carrierLabelPrinterOption.CarrierLabelPrinterPK = ZGuid.BrettsGuid;
			AssertHasError(carrierLabelPrinterOption.CarrierLabelPrinterPKInfo, "Enter a valid Carrier Label Printer.");

			carrierLabelPrinterOption.CarrierLabelPrinterPK = printer2.PK;
			AssertHasError(carrierLabelPrinterOption.CarrierLabelPrinterPKInfo,
				"The printer you have selected is not currently installed. Please see your system administrator.");

			carrierLabelPrinterOption.CarrierLabelPrinterPK = printer3.PK;
			AssertHasError(carrierLabelPrinterOption.CarrierLabelPrinterPKInfo,
				"You do not have the security rights to print to the selected printer.");
		}

		public void TestPrinters()
		{
			var printQueue1 = Factory.New<IStmPrintQueue>();
			printQueue1.QueueName = "Printer1";
			printQueue1.SQ_AllowPrinting = true;

			var printQueue2 = Factory.New<IStmPrintQueue>();
			printQueue2.QueueName = "Printer2";
			printQueue2.SQ_AllowPrinting = true;
			Factory.Save();

			var carrierLabelPrinterOption = new CarrierLabelPrinterOption(Factory);
			var printers = carrierLabelPrinterOption.Printers;
			AssertEquals("There are 2 printers.", 2, printers.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "Printer1", "Printer2" },
				printers.ToArray().Cast<IStmPrintQueue>().Select(printer => printer.QueueName));
		}
	}
}
