using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class CarrierShipperReferenceMessageEventTransformerTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestTrasform_ToAddSTUEventForLowerVersionMessageReceived_OriginalVersion_RejectionMessage()
		{
			AssertTrasform(1, 0, Events.MessageRejectedCode, Events.StatusUpdatedCode,
				"|MST=Booking Request Rejection|RES=Received for previous 'reset to original' version.|TYP=Message Discarded");
		}

		public void TestTrasform_ToAddSTUEventForLowerVersionMessageReceived_OriginalVersion_ConfirmationMessage()
		{
			AssertTrasform(1, 0, Events.MessageAcceptedCode, Events.StatusUpdatedCode,
				"|MST=Booking Request Confirmation|RES=Received for previous 'reset to original' version. Check with carrier for possible duplication.|TYP=Message Discarded");
		}

		public void TestTrasform_ToAddSTUEventForLowerVersionMessageReceived_UpdatedVersion_RejectionMessage()
		{
			AssertTrasform(4, 3, Events.MessageRejectedCode, Events.StatusUpdatedCode,
				"|MST=Booking Request Rejection|RES=Received for previous 'reset to original' version.|TYP=Message Discarded");
		}

		public void TestTrasform_ToAddSTUEventForLowerVersionMessageReceived_UpdatedVersion_ConfirmationMessage()
		{
			AssertTrasform(4, 3, Events.MessageAcceptedCode, Events.StatusUpdatedCode,
				"|MST=Booking Request Confirmation|RES=Received for previous 'reset to original' version. Check with carrier for possible duplication.|TYP=Message Discarded");
		}

		public void TestTrasform_KeepOriginalEventWhenVersionMatched()
		{
			AssertTrasform(3, 3, Events.MessageRejectedCode, Events.MessageRejectedCode,
				"|DEP=Carrier|MST=Shipping Instruction|RES=Shipping Instruction Rejected, MISSED THE CUTOFF TIME");
		}

		void AssertTrasform(int currentVersion, int xmlVersion, string xmlEventCode, string expectedEventCode, string expectedEventReference)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001010";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "FUL423189120";

			var visualizerDocumentData = (BusinessObject)Factory.BOFactory.New<IVisualizerDocumentData>();
			visualizerDocumentData[JobDocumentDataSchema.JDD_ParentID] = consol.PK;
			visualizerDocumentData[JobDocumentDataSchema.JDD_ParentTableCode] = consol.TablePrefix;
			visualizerDocumentData[JobDocumentDataSchema.JDD_Name] = "SeaBookingRequest";

			var entryNum = consol.Numbers.AddNew();
			entryNum.CE_EntryType = ConsolNonCustomsAdditionalReferenceCodesCodeList.Codes.CarrierShipperReference;
			entryNum.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			entryNum.CE_EntryIsSystemGenerated = true;
			entryNum.CE_EntryNum = currentVersion == 0 ? "C00001010" : $"C00001010-V{currentVersion}";

			Factory.SaveForTesting();

			var responseMessage = GetQueuedUniversalEventMessage(string.Format(responseUniversalEvent, "Booking Request", xmlVersion == 0 ? "C00001010" : $"C00001010-V{xmlVersion}", xmlEventCode));

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(responseMessage);

			var consolSTULog = consol.Logs.Find(l => l.SL_SE_NKEvent == expectedEventCode).First();

			AssertMultilineASCIIEquals("Consol STU log reference", expectedEventReference, consolSTULog.SL_Reference);
		}

		const string responseUniversalEvent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
    <Event>
        <DataContext>
            <DocumentaryOverride>
                <DocumentName>{0}</DocumentName>
            </DocumentaryOverride>
            <DataTargetCollection>
                <DataTarget>
                    <Key>{1}</Key>
                    <Type>ForwardingConsol</Type>
                </DataTarget>
            </DataTargetCollection>
        </DataContext>
        <EventTime>2020-05-11T10:16:28.17</EventTime>
        <EventType>{2}</EventType>
        <EventParameters>
            <Department>Carrier</Department>
            <MessageType>Shipping Instruction</MessageType>
            <Reason>Shipping Instruction Rejected, MISSED THE CUTOFF TIME</Reason>
        </EventParameters>
        <EventReference />
    </Event>
</UniversalEvent>";
	}
}
