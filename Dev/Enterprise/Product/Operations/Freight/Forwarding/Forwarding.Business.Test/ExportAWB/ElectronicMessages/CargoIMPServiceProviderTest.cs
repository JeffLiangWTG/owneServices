using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.AWB.Messaging;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	sealed class CargoIMPServiceProviderTest : TestCaseWithFactory
	{
		public void TestGetAirlineOrder()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var awb = consol.AWBHeader as ConsolExportAWBHeader;
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";

			string header = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			Assert(header.Contains("QFSYD"));

			consol.JK_MasterBillNum = "01412345";

			header = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			Assert("GetAirline method should search airline using EH_By1st first then EH_AirlinePrefix", header.Contains("QFSYD"));

			awb.EH_By1st = ZString.Empty;

			header = CargoIMPServiceProvider.CCN.GetTransmissionMethodHeader(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			Assert("No EH_By1st. Should use EH_AirlinePrefix", header.Contains("ACSYD"));
		}

		public void TestGetTransmissionMethodFooter()
		{
			var awb = Factory.New<ConsolExportAWBHeader>();
			awb.EH_AWBOriginCode = "SYD";
			awb.EH_By1st = "QF";

			var messageFWB = EDIMessageTestFactory.New(Factory);
			messageFWB.EM_MessageType = "FWB";

			string result = CargoIMPServiceProvider.CCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.Get("XYZ").GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x03\r\n\n\n\x04\n", result);

			result = CargoIMPServiceProvider.BTviaCCN.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.SMTP);
			AssertEquals("\x04", result);

			result = CargoIMPServiceProvider.IATA.GetTransmissionMethodFooter(awb, messageFWB, CargoIMPTransmissionMethod.FTP);
			AssertEquals("\x04", result);
		}
	}
}
