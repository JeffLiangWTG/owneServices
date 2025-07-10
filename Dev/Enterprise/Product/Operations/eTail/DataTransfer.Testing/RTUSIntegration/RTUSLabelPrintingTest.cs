using System;
using System.IO;
using System.Net;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.eTail.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.TransportCommon.Registry;
using WTG.RTUS.Interface;
using WTG.RTUS.Printing;

namespace Enterprise.eTail.DataTransfer.Testing
{
	class RTUSLabelPrintingTest : TestCaseWithFactory
	{
		public void TestWebExceptionWhilePrinting()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "Money printer";
			printer.SQ_ServerName = "Bank";

			var webException = new WebException("Bad Route", new Exception("inner Ex"), WebExceptionStatus.ConnectFailure, new WebResposneForTest());
			var printingProvider = new SMSLabelPrintingForTest(Factory) { RTUSPrinter_Exposed = new RTUSPrinterForTest() { ExpectedException = webException } };

			var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var errorMessage);
			Assert(!result);
			AssertEquals("Failed to connect to remote printer due to network error: Bad Route, connection status: ConnectFailure, response: You don't even have internet!", errorMessage);
		}

		public void TestURIExceptionWhenNoURLSet()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "eCommerce Special Printer";
			printer.SQ_ServerName = "WiseTech Global Official Printing Server";

			using (TransportRegistry.Instance.RemotePrintServerURL.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var printingProvider = new RTUSLabelPrinting(Factory);
				var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var errorMessage);
				Assert(!result);
				AssertEquals("Invalid URI: The URI is empty.\r\nPlease ensure the URL in 'Registry > Transport > RTUS > Remote Print Server URL for RTUS' is valid.", errorMessage);
			}
		}

		public void TestReportUnexpectedException()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "Money printer";
			printer.SQ_ServerName = "Bank";

			var exception = new InvalidOperationException("Someone messed up something");
			var printingProvider = new SMSLabelPrintingForTest(Factory) { RTUSPrinter_Exposed = new RTUSPrinterForTest() { ExpectedException = exception } };

			var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var errorMessage);
			Assert(!result);
			AssertEquals(@"Error occurred while printing label.

Printer: Money printer@Bank", errorMessage);
			AssertEquals(exception, ErrorReporter.LastExceptionReported);
			AssertEquals("eTailLabelPrinting|ExceptionMessage", ErrorReporter.LastKeyReported);
			AssertEquals($@"Unexpected exception while printing: 
File size: -1
File type: PDF
Printer PK: {printer.PK}
Printer name: Money printer
Printer host: Bank
Remote printing server: http://not.exists.com/
Last action: 


Last error message: ",
ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestGetPrinterFromObjectFactory()
		{
			var printer = ObjectFactory.New<ILabelPrintingService>(Factory);
			AssertNotNull(printer);
			AssertType<RTUSLabelPrinting>(printer);
		}

		public void TestPrintLabel()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "Money printer";
			printer.SQ_ServerName = "Bank";
			var rtusPrinter = new RTUSPrinterForTest();
			var printingProvider = new SMSLabelPrintingForTest(Factory) { RTUSPrinter_Exposed = rtusPrinter };

			var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var _);
			Assert(result);
			AssertEquals("A PDF print job was send to Money printer on Bank", rtusPrinter.LastPrintResult);
		}

		public void TestPrintLabel_Failed_PrinterNotAvailable()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "Money printer";
			printer.SQ_ServerName = "Bank";
			printer.SQ_AllowPrinting = false;

			var rtusPrinter = new RTUSPrinterForTest();
			var printingProvider = new SMSLabelPrintingForTest(Factory) { RTUSPrinter_Exposed = rtusPrinter };

			var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var errorMessage);
			Assert(!result);
			AssertNull(rtusPrinter.LastPrintResult);
			AssertEquals($"Cannot find printer [{printer.PK}]", errorMessage);
		}

		public void TestPrintLabel_Failed_RTUSPrinterDidNotPrint()
		{
			var printer = Factory.New<IStmPrintQueue>();
			printer.QueueName = "Money printer";
			printer.SQ_ServerName = "Bank";

			var rtusPrinter = new RTUSPrinterForTest() { Faulty = true };
			var printingProvider = new SMSLabelPrintingForTest(Factory) { RTUSPrinter_Exposed = rtusPrinter };

			var result = printingProvider.TryPrintLabel(null, "PDF", printer.PK.ToGuid(), out var errorMessage);
			Assert(!result);
			AssertNull(rtusPrinter.LastPrintResult);
			AssertEquals("Error occurred while printing label.\r\n\r\nPrinter: Money printer@Bank", errorMessage);
		}
	}

	public class SMSLabelPrintingForTest : RTUSLabelPrinting
	{
		public ETailRTUSPrinter RTUSPrinter_Exposed { get; set; }

		public SMSLabelPrintingForTest(BusinessObjectFactory factory) : base(factory) { }

		protected override ETailRTUSPrinter GetRTUSPrinter() => RTUSPrinter_Exposed;
	}

	public class RTUSPrinterForTest : ETailRTUSPrinter
	{
		public RTUSPrinterForTest() : base(new RemotePrintServerConfig(new Uri("http://not.exists.com"), "test", "test")) { }

		protected override bool PrintCore(FileType fileType, byte[] binaryData, string printerName, string printerServer)
		{
			if (!Faulty)
			{
				LastPrintResult = $"A {fileType} print job was send to {printerName} on {printerServer}";
			}

			if (ExpectedException != null)
			{
				throw ExpectedException;
			}

			return !Faulty;
		}

		public Exception ExpectedException { get; set; }

		public bool Faulty { get; set; }

		public string LastPrintResult { get; private set; }
	}

	public class WebResposneForTest : WebResponse
	{
		public override Stream GetResponseStream()
		{
			return new MemoryStream(Encoding.Default.GetBytes("You don't even have internet!"));
		}
	}
}
