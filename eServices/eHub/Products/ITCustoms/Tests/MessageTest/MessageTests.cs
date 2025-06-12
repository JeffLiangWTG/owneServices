using CargoWise.eHub.Core.Orchestrations.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ITCustoms.Tests.MessageTest
{
	[TestClass]
	public class MessageTests
	{
		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageCountOfFinalStatusForIVISTOWithUnixNewLine()
		{
			// Arrange
			var message =
				"<?xml version='1.0' encoding='utf-8'?><soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'><soapenv:Body><ns2:getResponse xmlns:ns2='http://webservices.ftp.telematico.dogana.dogane.ag_dogane.finanze.it'><getReturn>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</getReturn></ns2:getResponse></soapenv:Body></soapenv:Envelope>";
			var responseMsg = CargoWise.eHub.Products.ITCustoms.Helpers.OrchestrationHelper.GetXpathValue(message, "/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='getResponse']/*[local-name()='getReturn']");
			responseMsg = CargoWise.eHub.Products.ITCustoms.Helpers.CertificateHelper.DecryptMessage(responseMsg);
			var finalStatus = 0;

			// Act
			// Assert
			TestHelper.AssertNoException<FatalMessageProcessingException>(() =>
			{
				finalStatus = CargoWise.eHub.Products.ITCustoms.Helpers.OrchestrationHelper.GetCountOfFinalStatusForIVISTO(
						responseMsg);
			});
			Assert.AreEqual(1, finalStatus);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageCountOfFinalStatusForIVISTOWithNonUnixNewLine()
		{
			// Arrange
			var responseMsg = @"0020            00200104.QPY210000096697371100    10715680152     001 00003    INVIO IN AMBIENTE REALE       
Data:10/01/2021  Ora:07:18:02
TIVISTO  21ITQ1V1T0000286E2IT371100IT279100MALPENSA                           07012021Uscita conclusa  ";
			var finalStatus = 0;

			// Act
			// Assert
			TestHelper.AssertNoException<FatalMessageProcessingException>(() =>
			{
				finalStatus = CargoWise.eHub.Products.ITCustoms.Helpers.OrchestrationHelper.GetCountOfFinalStatusForIVISTO(
						responseMsg);
			});
			Assert.AreEqual(1, finalStatus);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageCountOfFinalStatusForIRILDESWithUnixNewLine()
		{
			// Arrange
			var responseMsg = @"0020            00200108.LCR210000417200371100    01141410439     001 00004    INVIO IN AMBIENTE REALE       
Data:11/01/2021  Ora:08:54
TIE45    014156P21ITQ1V1T0008311T220IT01TR000001084Garanzia svincolata         110121CH004162";
			var finalStatus = 0;

			// Act
			// Assert
			TestHelper.AssertNoException<FatalMessageProcessingException>(() =>
			{
				finalStatus = CargoWise.eHub.Products.ITCustoms.Helpers.OrchestrationHelper.GetCountOfFinalStatusForIRILDES(
						responseMsg);
			});
			Assert.AreEqual(1, finalStatus);
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestMessageGetCountOfLines()
		{
			// Arrange
			var responseMsg = @"0020            00200108.LCR210000417200371100    01141410439     001 00004    INVIO IN AMBIENTE REALE       
Data:11/01/2021  Ora:08:54
TIE45    014156P21ITQ1V1T0008311T220IT01TR000001084Garanzia svincolata         110121CH004162";
			var count = 0;

			// Act
			// Assert
			TestHelper.AssertNoException<FatalMessageProcessingException>(() =>
			{
				count = CargoWise.eHub.Products.ITCustoms.Helpers.OrchestrationHelper.GetCountOfLines(
						responseMsg);
			});
			Assert.AreEqual(3, count);
		}
	}
}
