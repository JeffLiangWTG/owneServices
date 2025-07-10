using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(PrinterInfo))]
	class PrinterInfoTest : DataObjectInfoTestCase<PrinterInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var printer = Helper.CreatePrintQueue("Printer");
			printer.SQ_DisplayName = "Display Name";

			var printerInfo = new PrinterInfo(printer);
			AssertEquals("Printer Display Name", "Display Name", printerInfo.Name);
			AssertEquals("Printer PK", printer.PK, printerInfo.PK);
		}

		#endregion

		#region TestName

		public void TestName()
		{
			var printerInfo = new PrinterInfo();
			AssertEquals("", printerInfo.Name);

			printerInfo.Name = "ABC";
			AssertEquals("ABC", printerInfo.Name);
		}

		#endregion

		#region TestPK

		public void TestPK()
		{
			var printerInfo = new PrinterInfo();
			AssertEquals(Guid.Empty, printerInfo.PK);

			var guid = Guid.NewGuid();
			printerInfo.PK = guid;
			AssertEquals(guid, printerInfo.PK);
		}

		#endregion

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new PrinterInfo();
		}

		#endregion
	}
}
