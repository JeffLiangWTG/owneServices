using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class SecureContainerReleaseContainerEventHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrentStatusFromEventsByContainer()
		{
			var consol = CreateConsol();

			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.NotApplicable, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			consol.Containers[0].Logs.AddNew(Events.MessageAccepted);
			Factory.Save();
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.NotApplicable, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Assigned, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Accepted, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferSent, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferRejected, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeSent, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeRejected, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Revoked, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			consol.Containers[0].Logs.AddNew(Events.MessageRejected);
			Factory.Save();
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Revoked, SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));
		}

		public void TestGetCurrentStatusFromEventsByLogs()
		{
			var consol = CreateConsol();

			var result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.NotApplicable, result.currentStatus);
			AssertNullOrEmpty(result.eventTypeCode);

			consol.Containers[0].Logs.AddNew(Events.MessageSent);
			Factory.Save();

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.NotApplicable, result.currentStatus);
			AssertNullOrEmpty(result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Assigned, result.currentStatus);
			AssertEquals(EventCodes.Authorized, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Accepted, result.currentStatus);
			AssertEquals(EventCodes.MessageAccepted, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse, result.currentStatus);
			AssertEquals(EventCodes.MessageSent, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferSent, result.currentStatus);
			AssertEquals(EventCodes.MessagePendingProcessing, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.TransferRejected, result.currentStatus);
			AssertEquals(EventCodes.MessageRejected, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse, result.currentStatus);
			AssertEquals(EventCodes.MessageSent, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeSent, result.currentStatus);
			AssertEquals(EventCodes.MessageWithdrawCancelRequest, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.RevokeRejected, result.currentStatus);
			AssertEquals(EventCodes.MessageRejected, result.eventTypeCode);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Revoked, result.currentStatus);
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, result.eventTypeCode);

			consol.Containers[0].Logs.AddNew(Events.MessageRejected);
			Factory.Save();

			result = SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(TMiningConstants.SecureContainerReleaseStatus.Revoked, result.currentStatus);
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, result.eventTypeCode);
		}

		public void TestGetEventLogsInDescendingOrder()
		{
			var consol = CreateConsol();

			var logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(0, logs.Count());

			consol.Containers[0].Logs.AddNew(Events.MessageAccepted);
			Factory.Save();
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(0, logs.Count());

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(1, logs.Count());
			AssertEquals(EventCodes.Authorized, logs.First().SL_SE_NKEvent);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(2, logs.Count());
			AssertEquals(EventCodes.MessageAccepted, logs.First().SL_SE_NKEvent);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(3, logs.Count());
			AssertEquals(EventCodes.MessageSent, logs.First().SL_SE_NKEvent);
			AssertContains(TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, logs.First().SL_Reference);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(4, logs.Count());
			AssertEquals(EventCodes.MessagePendingProcessing, logs.First().SL_SE_NKEvent);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(5, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer, logs.First().SL_Reference);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(6, logs.Count());
			AssertEquals(EventCodes.MessageSent, logs.First().SL_SE_NKEvent);
			AssertContains(TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, logs.First().SL_Reference);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(7, logs.Count());
			AssertEquals(EventCodes.MessageWithdrawCancelRequest, logs.First().SL_SE_NKEvent);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(8, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke, logs.First().SL_Reference);

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(9, logs.Count());
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, logs.First().SL_SE_NKEvent);

			consol.Containers[0].Logs.AddNew(Events.MessageRejected);
			Factory.Save();
			logs = SecureContainerReleaseContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(9, logs.Count());
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, logs.First().SL_SE_NKEvent);
		}

		public void TestCheckStatusIsReadyToShowInTransferMode()
		{
			var consol = CreateConsol();

			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));
		}

		public void TestCheckStatusIsReadyToShowInRevokeMode()
		{
			var consol = CreateConsol();

			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Assigned);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Accepted);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferSent);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.TransferRejected);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeSent);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.RevokeRejected);
			Assert(SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddSecureContainerReleaseStatusLogForContainer(consol.Containers[0], TMiningConstants.SecureContainerReleaseStatus.Revoked);
			Assert(!SecureContainerReleaseContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(SecureContainerReleaseContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));
		}

		ForwardingConsol CreateConsol()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "BOL_Reference";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1111111";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2222222";

			Factory.Save();

			return consol;
		}

		public static StmALog AddSecureContainerReleaseStatusLogForContainer(ForwardingContainer container, ZString containerStatus)
		{
			var parameters = new Dictionary<string, string>
			{
				[Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber] = container.JC_ContainerNum,
			};

			Event eventToAdd;
			switch (containerStatus)
			{
				case TMiningConstants.SecureContainerReleaseStatus.Assigned:
					eventToAdd = Events.Authorised;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerRelease);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.Accepted:
					eventToAdd = Events.MessageAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferRejected:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeRejected:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferSent:
					eventToAdd = Events.MessagePendingProcessing;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeSent:
					eventToAdd = Events.MessageWithdrawCancelRequest;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.Revoked:
					eventToAdd = Events.MessageWithdrawCancelAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.TransferSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseTransfer);
					break;

				case TMiningConstants.SecureContainerReleaseStatus.RevokeSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, TMiningConstants.ParameterMessageTypes.SecureContainerReleaseRevoke);
					break;
				default:
					eventToAdd = Events.MessageAccepted;
					break;
			}

			var log = container.Logs.AddNew(eventToAdd, parameters.ToArray());
			container.Factory.Save();
			Thread.Sleep(100);
			return log;
		}
	}
}
