using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class EHBLUniversalEventSenderHelperTest : TestCaseWithFactory
	{
		[TestDate(2024, 11, 15)]
		public void TestSendUniversalEvent()
		{
			var originalIsInteractive = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInteractive))
			{
				Globals.IsUserInteractive = true;

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;

				Factory.Save();

				var helper = new EHBLUniversalEventSenderHelper()
				{
					DirectXTClientID = "DIRECT_XT_CLIENT_5",
					MessageBroker = EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface
				};

				var parameters = new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(Params.Type, Core.Constants.BillStatusUpdatedTypes.AmendmentDenied),
					new KeyValuePair<string, string>(Params.Department, ElectronicBOLConstants.EHBLEventDepartments.Carrier),
					new KeyValuePair<string, string>(Params.ReferenceNumber, "WTLDAUILA_S00001001_1"),
					new KeyValuePair<string, string>(Params.RequestNumber, "1234"),
				};

				var errorMessage = helper.SendUniversalEvent(shipment, AutoEvents.BillStatusUpdatedCode, parameters, "The requested changes are too comprehensive");
				Assert(errorMessage.IsEmpty);

				var factory2 = new BusinessObjectFactory();
				var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);

				var dex = shipment2.Logs.MostRecentLogByEventTime(AutoEvents.DataExport);
				AssertNotNull(dex);

				var message = dex.RelatedEDIMessage;
				AssertNotNull(message);

				AssertEquals("Send to Direct XT", EDICommunicationsModeCommunicationsTransportList.Codes.XTInterface, message.Message.Interchange.EI_TransportType);
				AssertEquals("Direct XT client name", "DIRECT_XT_CLIENT_5", message.Message.Interchange.EI_To);

				AssertContains("<EventType>BLU</EventType>", message.Message.EM_MessageText);
				AssertContains(@"<Context>
        <Type>NotificationDetails</Type>
        <Value>The requested changes are too comprehensive</Value>
      </Context>", message.Message.EM_MessageText);
				AssertContains("<ReferenceNumber>WTLDAUILA_S00001001_1</ReferenceNumber>", message.Message.EM_MessageText);
				AssertContains("<RequestNumber>1234</RequestNumber>", message.Message.EM_MessageText);
				AssertContains("<Type>Amendment Denied</Type>", message.Message.EM_MessageText);
				AssertContains("<Department>Carrier</Department>", message.Message.EM_MessageText);
				AssertNotContains("<EventReference>", message.Message.EM_MessageText);

				var msn = shipment2.Logs.MostRecentLogByEventTime(AutoEvents.MessageSent);
				AssertNotNull(msn);
				AssertContains("|DEP=Title Registry|RFN=WTLDAUILA_S00001001_1|RQN=1234|TYP=Amendment Denied", msn.SL_Reference);
			}
		}
	}
}
