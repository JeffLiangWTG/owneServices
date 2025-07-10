using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
#if NETFRAMEWORK
using System.Web.Http.Results;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using CargoWise.Application;
using Enterprise.eTail.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class ETailPrintLabelControllerTest : TestCase
	{
		public void TestPrintRoutingLabel_Successful()
		{
			var testPrinter = new Guid();
			var mockPrinter = GetPrinterMock(testPrinter, string.Empty);

			var itemPK = new Guid();
			var labelData = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky")).ToArray();

			var mockProvider = new Mock<IHVLVRoutingLabelProvider>();
			mockProvider.SetupGet(m => m.RoutingLabel)
				.Returns(labelData);

			using (SubstituteRTUSBookingProvider(mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				var result = controller.PrintRoutingLabel(testPrinter, itemPK);
				AssertType<OkResult>(result);
			}
		}

		public void TestPrintRoutingLabel_WhenPrintFailed_HasErrorMessage()
		{
			var failedPrinter = new Guid();
			var mockPrinter = GetPrinterMock(failedPrinter, "Something wrong with the printing process", false);

			var itemPK = new Guid();
			var labelData = new ReadOnlyCollection<byte>(Encoding.UTF8.GetBytes("I'm a label and I'm sticky")).ToArray();

			var mockProvider = new Mock<IHVLVRoutingLabelProvider>();
			mockProvider.SetupGet(m => m.RoutingLabel)
				.Returns(labelData);

			using (SubstituteRTUSBookingProvider(mockProvider.Object))
			using (ObjectFactory.Substitute(mockPrinter.Object))
			{
				controller.PrintRoutingLabel(failedPrinter, itemPK)
					.AssertResultContains(HttpStatusCode.InternalServerError, "Failed to print label: Something wrong with the printing process");
			}
		}

		Mock<ILabelPrintingService> GetPrinterMock(Guid printerPK, string errorMesssage, bool successful = true)
		{
			var mockPrinter = new Mock<ILabelPrintingService>();

			mockPrinter.Setup(m => m.TryPrintLabel(It.IsAny<byte[]>(), It.IsAny<string>(), printerPK, out errorMesssage))
				.Returns(successful);

			return mockPrinter;
		}

		IDisposable SubstituteRTUSBookingProvider(IHVLVRoutingLabelProvider provider)
		{
			return ObjectFactory.Substitute("IHVLVRoutingLabelProvider", provider);
		}

		protected override void SetUp()
		{
			base.SetUp();
			controller = ControllerHelper.GetController<ETailPrintLabelController>();
		}

		ETailPrintLabelController controller;
	}
}
