using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class NZPortMessageValidationTest : BaseAgencyTest
	{
		public void TestValidateCTO()
		{
			var voyage = Factory.New<JobVoyage>();

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "UAIEV";

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "UAIEV";
			destination.JB_OA_ArrivalCTOAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

			var message = new NZPortMessage(voyage);
			message.Port = "UAIEV";

			var validation = new NZPortMessageValidation(message);

			message.Direction = Constants.PortDirection.Load;
			validation.ValidateCTO();
			AssertHasErrors("CTO", message.CTOInfo);

			message.Direction = Constants.PortDirection.Discharge;
			validation.ValidateCTO();
			AssertNoErrors("CTO", message.CTOInfo);
		}

		public void TestValidateMessageType()
		{
			var now = ZDateTimeOffset.Now;

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "UAIEV";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "UAIEV";

			var message = new NZPortMessage(voyage);
			message.Port = "UAIEV";
			message.Direction = Constants.PortDirection.Load;

			// There are no any message sent
			message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoWarnings("There are no any message sent", message.MessageTypeInfo);

			message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertHasWarning("There are no any message sent", message.MessageTypeInfo, "No messages have been sent for this port/direction, so there is nothing to replace.");

			message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertHasWarning("There are no any message sent", message.MessageTypeInfo, "No messages have been sent for this port/direction, so there is nothing to cancel.");

			// Original message has been sent
			voyage.Logs.AddNew(AutoEvents.MessageSent, now, Params.Location.AsKeyFor("UAIEV"), Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.LoadManifest));
			Factory.Save();

			AssertEquals("The last message is Original", PortMessageTypeList.Codes.Original, message.LastMessageSent);

			message.MessageType = PortMessageTypeList.Codes.Original;
			AssertHasWarning("The last message is Original", message.MessageTypeInfo, "An uncanceled message has already been sent for this port/direction.");

			message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertNoWarnings("The last message is Original", message.MessageTypeInfo);

			message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertNoWarnings("The last message is Original", message.MessageTypeInfo);

			// Replace message has been sent
			voyage.Logs.AddNew(AutoEvents.MessageSent, now.AddMinutes(1), Params.Location.AsKeyFor("UAIEV"), Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.LoadManifestReplacement));
			Factory.Save();

			AssertEquals("The last message is Replace", PortMessageTypeList.Codes.Replace, message.LastMessageSent);

			message.MessageType = PortMessageTypeList.Codes.Original;
			AssertHasWarning("The last message is Replace", message.MessageTypeInfo, "An uncanceled message has already been sent for this port/direction.");

			message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertNoWarnings("The last message is Replace", message.MessageTypeInfo);

			message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertNoWarnings("The last message is Replace", message.MessageTypeInfo);

			// Cancel message has been sent
			voyage.Logs.AddNew(AutoEvents.MessageWithdrawCancelRequest, now.AddMinutes(2), Params.Location.AsKeyFor("UAIEV"), Params.MessageType.AsKeyFor(Constants.EventReferenceMessageTypes.LoadManifestCancellation));
			Factory.Save();

			AssertEquals("The last message is Cancellation", PortMessageTypeList.Codes.Cancellation, message.LastMessageSent);

			message.MessageType = PortMessageTypeList.Codes.Original;
			AssertNoWarnings("The last message is Cancellation", message.MessageTypeInfo);

			message.MessageType = PortMessageTypeList.Codes.Replace;
			AssertHasWarning("The last message is Cancellation", message.MessageTypeInfo, "The last message sent for this port/direction was a cancellation, so there is nothing to replace.");

			message.MessageType = PortMessageTypeList.Codes.Cancellation;
			AssertHasWarning("The last message is Cancellation", message.MessageTypeInfo, "A cancellation has already been sent for this port/direction.");
		}
	}
}
