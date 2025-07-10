using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Constants = CargoWise.EventReference.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public class CertifiedPickupContainerEventHelperTest : TestCaseWithFactory
	{
		public void TestGetCurrentStatusFromEventsByLogs()
		{
			var consol = CreateConsol();

			var result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.NotApplicable, result.currentStatus);
			AssertNullOrEmpty(result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.Assigned, result.currentStatus);
			AssertEquals(EventCodes.Authorized, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.TransferSentAwaitingResponse, result.currentStatus);
			AssertEquals(EventCodes.MessageSent, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.TransferSent, result.currentStatus);
			AssertEquals(EventCodes.MessagePendingProcessing, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.Accepted, result.currentStatus);
			AssertEquals(EventCodes.MessageAccepted, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.Revoked, result.currentStatus);
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline, result.currentStatus);
			AssertEquals(EventCodes.MessageRejected, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke, result.currentStatus);
			AssertEquals(EventCodes.MessageRejected, result.eventTypeCode);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);

			result = CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0].Logs.GetAllLogs().Cast<StmALog>().OrderByDescending(log => log.SL_PostedTimeUtc));
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByOtherReason, result.currentStatus);
			AssertEquals(EventCodes.MessageRejected, result.eventTypeCode);
		}

		public void TestGetCurrentStatusFromEventsByContainer()
		{
			var consol = CreateConsol();

			AssertEquals(CertifiedPickupConstants.Status.NotApplicable, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			AssertEquals(CertifiedPickupConstants.Status.Assigned, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			AssertEquals(CertifiedPickupConstants.Status.TransferSentAwaitingResponse, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			AssertEquals(CertifiedPickupConstants.Status.TransferSent, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			AssertEquals(CertifiedPickupConstants.Status.Accepted, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			AssertEquals(CertifiedPickupConstants.Status.Revoked, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			AssertEquals(CertifiedPickupConstants.Status.DeclinedByOtherReason, CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0]));
		}

		public void TestCheckStatusIsReadyToShowInAcceptDeclineMode()
		{
			var consol = CreateConsol();

			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInAcceptDeclineMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));
		}

		public void TestCheckStatusIsReadyToShowInTransferMode()
		{
			var consol = CreateConsol();

			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInTransferMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));
		}

		public void TestCheckStatusIsReadyToShowInRevokeMode()
		{
			var consol = CreateConsol();

			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(consol.Containers[0]));
			Assert(!CertifiedPickupContainerEventHelper.CheckStatusIsReadyToShowInRevokeMode(CertifiedPickupContainerEventHelper.GetCurrentStatusFromEvents(consol.Containers[0])));
		}

		public void TestGetEventLogsInDescendingOrder()
		{
			var consol = CreateConsol();

			var logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(0, logs.Count());

			consol.Containers[0].Logs.AddNew(Events.MessageAccepted);
			Factory.Save();
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(0, logs.Count());

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Assigned);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(1, logs.Count());
			AssertEquals(EventCodes.Authorized, logs.First().SL_SE_NKEvent);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSentAwaitingResponse);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(2, logs.Count());
			AssertEquals(EventCodes.MessageSent, logs.First().SL_SE_NKEvent);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.TransferSent);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(3, logs.Count());
			AssertEquals(EventCodes.MessagePendingProcessing, logs.First().SL_SE_NKEvent);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Accepted);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(4, logs.Count());
			AssertEquals(EventCodes.MessageAccepted, logs.First().SL_SE_NKEvent);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.Revoked);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(5, logs.Count());
			AssertEquals(EventCodes.MessageWithdrawCancelAccepted, logs.First().SL_SE_NKEvent);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(6, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline, logs.First().SL_Reference);
			AssertContains(CertifiedPickupConstants.EventStatus.DeclinedByNextParty, logs.First().SL_Reference);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(7, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(CertifiedPickupConstants.ParameterMessageTypes.Transfer, logs.First().SL_Reference);
			AssertContains(CertifiedPickupConstants.EventStatus.DeclinedByNextParty, logs.First().SL_Reference);

			AddCertifiedPickupStatusLogForContainer(consol.Containers[0], CertifiedPickupConstants.Status.DeclinedByOtherReason);
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(8, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(CertifiedPickupConstants.ParameterMessageTypes.Transfer, logs.First().SL_Reference);
			AssertContains("ORG", logs.First().SL_Reference);

			consol.Containers[0].Logs.AddNew(Events.MessageRejected);
			Factory.Save();
			logs = CertifiedPickupContainerEventHelper.GetEventLogsInDescendingOrder(consol.Containers[0]);
			AssertEquals(8, logs.Count());
			AssertEquals(EventCodes.MessageRejected, logs.First().SL_SE_NKEvent);
			AssertContains(CertifiedPickupConstants.ParameterMessageTypes.Transfer, logs.First().SL_Reference);
			AssertContains("ORG", logs.First().SL_Reference);
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

		public static StmALog AddCertifiedPickupStatusLogForContainer(ForwardingContainer container, ZString containerStatus)
		{
			var parameters = new Dictionary<string, string>
			{
				[Constants.EventReferenceParameters.Codes.EquipmentReferenceNumber] = container.JC_ContainerNum,
			};

			Event eventToAdd;
			switch (containerStatus)
			{
				case CertifiedPickupConstants.Status.Accepted:
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline);
					eventToAdd = Events.MessageAccepted;
					break;
				case CertifiedPickupConstants.Status.Assigned:
					eventToAdd = Events.Authorised;
					parameters.Add(Constants.EventReferenceParameters.Codes.Type, CertifiedPickupConstants.ParameterTypes.ContainerRelease);
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.ReleaseRight);
					break;
				case CertifiedPickupConstants.Status.TransferSentAwaitingResponse:
					eventToAdd = Events.MessageSent;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(CertifiedPickupConstants.ContainerEventParameter.EventCode, CertifiedPickupConstants.ContainerEventParameter.Values.TransferSentAwaitingResponse);
					break;
				case CertifiedPickupConstants.Status.TransferSent:
					eventToAdd = Events.MessagePendingProcessing;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.Transferred);
					break;
				case CertifiedPickupConstants.Status.Revoked:
					eventToAdd = Events.MessageWithdrawCancelAccepted;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Revoke);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.RevokedByPreviousParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByNextPartyForAcceptDecline:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.AcceptDecline);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByNextPartyForTransferRevoke:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, CertifiedPickupConstants.EventStatus.DeclinedByNextParty);
					break;
				case CertifiedPickupConstants.Status.DeclinedByOtherReason:
					eventToAdd = Events.MessageRejected;
					parameters.Add(Constants.EventReferenceParameters.Codes.MessageType, CertifiedPickupConstants.ParameterMessageTypes.Transfer);
					parameters.Add(Constants.EventReferenceParameters.Codes.Status, "ORG");
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
