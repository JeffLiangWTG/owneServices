using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.FR.Testing
{
	sealed class DemandeDeTracingMessageLogCreatorTest : TestCaseWithFactory
	{
		#region TestCreateMessageSentLog

		public void TestCreateMessageSentLog()
		{
			var consol = CreateConsol();

			var iTRCDetails = new ForwardingConsolTRCDetailsProvider(consol);
			var builder = new DemandeDeTracingBuilder(iTRCDetails, consol.Containers.OfType<ForwardingContainer>().ToList(), DemandeDeTracingDirection.Import);
			var demandeDeTracing = builder.Build();

			var documentData = CreateDocumentData(consol) as IStmALogParent;
			var data = demandeDeTracing.MakeDynamic();

			var logCreator = new DemandeDeTracingMessageLogCreator(iTRCDetails);
			logCreator.CreateMessageSentLog(documentData, data, "Tracing Request (TRC) - Import", "Bob");

			var msnEvents = documentData
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("MSN logs parameters on document data",
				new[]
				{
					"|DEP=Bob|MST=Tracing Request (TRC) - Import"
				},
				msnEvents.Select(log => log.SL_Reference));

			msnEvents = consol.Containers.OfType<ForwardingContainer>().First()
				.Logs
				.GetAllLogs()
				.OfType<StmALog>()
				.Where(log => log.SL_SE_NKEvent == Events.MessageSentCode)
				.ToArray();

			AssertContainsExactElementsInAnyOrder("MSN logs parameters on container",
				new[]
				{
					"|DEP=Bob|MST=Tracing Request (TRC) - Import"
				},
				msnEvents.Select(log => log.SL_Reference));
		}

		IVisualizerDocumentData CreateDocumentData(ForwardingConsol consol)
		{
			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentTableCode = JobConsolSchema.Constants.Prefix;
			documentData.JDD_ParentID = consol.PK;
			documentData.JDD_Name = "FR_TRC";

			return documentData;
		}

		#endregion

		ForwardingConsol CreateConsol(bool isAddressAvailableToCreate = true, bool isImport = true)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			if (isImport)
			{
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "FRPRA";
			}
			else
			{
				consol.JK_RL_NKLoadPort = "FRPRA";
				consol.JK_RL_NKDischargePort = "AUSYD";
			}

			consol.JK_BookingReference = "BookingReference";
			consol.JK_MasterBillNum = "BOL_Reference";

			if (isAddressAvailableToCreate)
			{
				CreateAddresses(consol);
			}

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "123";

			var container2 = Factory.New<ForwardingContainer>();
			container2.JC_ContainerNum = "456";

			var container3 = Factory.New<ForwardingContainer>();
			container3.JC_ContainerNum = "789";

			return consol;
		}

		void CreateAddresses(ForwardingConsol consol)
		{
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "MAERSK";
			carrier.OH_RL_NKClosestPort = "DKAAL";
			carrier.MainAddress.Address1 = "Unit 13";
			carrier.MainAddress.Address2 = "4 Lost Lane";
			carrier.MainAddress.City = "Aalborg";
			carrier.MainAddress.Postcode = "2000";
			carrier.MainAddress.OA_RN_NKCountryCode = "DK";

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var bookingAgent = Factory.New<OrgHeader>();
			bookingAgent.OH_FullName = "I'm Handling Stuff";
			bookingAgent.OH_RL_NKClosestPort = "AUSYD";
			bookingAgent.MainAddress.Address1 = "Unit 2";
			bookingAgent.MainAddress.Address2 = "60 What Lane";
			bookingAgent.MainAddress.City = "Sydney";
			bookingAgent.MainAddress.Postcode = "2023";
			bookingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address = bookingAgent.MainAddress.PK;

			var receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "YUMMY";
			receivingForwarder.OH_RL_NKClosestPort = "AUSYD";
			receivingForwarder.MainAddress.Address1 = "Unit 200";
			receivingForwarder.MainAddress.Address2 = "55 Why Lane";
			receivingForwarder.MainAddress.City = "Sydney";
			receivingForwarder.MainAddress.Postcode = "2000";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			var sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "YUMMY";
			sendingForwarder.OH_RL_NKClosestPort = "AUSYD";
			sendingForwarder.MainAddress.Address1 = "Unit 200";
			sendingForwarder.MainAddress.Address2 = "55 Why Lane";
			sendingForwarder.MainAddress.City = "Sydney";
			sendingForwarder.MainAddress.Postcode = "2000";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
		}
	}
}
