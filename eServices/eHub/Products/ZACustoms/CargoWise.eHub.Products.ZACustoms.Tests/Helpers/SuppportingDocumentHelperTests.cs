using System;
using System.IO;
using System.Text;
using System.Xml;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ZACustoms.Helpers;
using CargoWise.eHub.Products.ZACustoms.Tests.TestFiles;
using CargoWise.eHub.Shared.Crypto;
using CargoWise.eHub.Shared.Mime;
using Common.Logging.Simple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Microsoft.XLANGs.BaseTypes;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ZACustoms.Tests.Helpers
{
	[TestClass]
	public class SupportingDocumentHelperTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void VerifyDocumentSize()
		{
            SupportingDocumentHelper.VerifyDocumentSize(LoadDocument("Success.xml"));
            SupportingDocumentHelper.VerifyDocumentSize(LoadDocument("SuccessEdgeCase.xml"));

            AssertException(() => SupportingDocumentHelper.VerifyDocumentSize(LoadDocument("Fail.xml")));
            AssertException(() => SupportingDocumentHelper.VerifyDocumentSize(LoadDocument("FailEdgeCase.xml")));
		}

        XLANGMessage LoadDocument(String filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "CargoWise.eHub.Products.ZACustoms.Tests.Helpers.TestFiles." + filename;

            var stream = assembly.GetManifestResourceStream(resourceName);

            var mockPart = MockRepository.GenerateMock<XLANGPart>();
            mockPart.Stub(x => x.RetrieveAs(typeof(Stream))).Return(stream);

            var mockMessage = MockRepository.GenerateMock<XLANGMessage>();
            mockMessage.Stub(x => x[0]).Return(mockPart);
            return mockMessage;
        }

        void AssertException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(Exception));
                return;
            }

            Assert.Fail("Expected exception of type Exception was not thrown.");
        }
	}
}
