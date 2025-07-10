using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.DE
{
	sealed class AdvancedLogisticsPortOrderMessagingExtensions : BaseMessagingExtensions
	{
		public AdvancedLogisticsPortOrderMessagingExtensions(IDocument document, ForwardingConsol consol)
		{
			this.consol = consol;

			var advancedLogisticsPortOrder = document?.Data.Value as AdvancedLogisticsPortOrder;
			Argument.NotNull(advancedLogisticsPortOrder, nameof(advancedLogisticsPortOrder));
			bhtReferenz = advancedLogisticsPortOrder.ALPOReference;
		}

		readonly ForwardingConsol consol;
		readonly ZString bhtReferenz;

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var shlContainers = SHLEventsReceivedForContainers(consol.Containers);
			if (shlContainers.Count > 0)
			{
				var message = Res.GetString("7D9792E9-2C3D-4515-82D2-E753A43A8487", "A Customs Stop Loading (SHL – Held) event has been received for container('s) {0}. Please handle this request in ALPO.", string.Join(", ", shlContainers));
				var information = Res.GetString("cc9a905d-18fd-4f4f-a3ee-3316cbe45cb3", "Information");
				notifications?.ShowMessage(message, information);
				return false;
			}

			if (!EventMAAWithMatchingCRFExists(bhtReferenz))
			{
				var message = Res.GetString("612fdda9-5e17-46d8-ad22-d4fd9b9f75d1", "Message can be withdrawn only after you have received an acceptance response to the previous message.");
				var information = Res.GetString("cc9a905d-18fd-4f4f-a3ee-3316cbe45cb3", "Information");
				notifications?.ShowMessage(message, information);
				return false;
			}

			return true;
		}

		public override bool? IsSendingAmendment() => false;

		public static IEnumerable<StmALog> GetEventLogsInDescendingOrder(ForwardingContainer containerBO)
		{
			return containerBO
				.Logs?
				.GetAllLogs()
				.OfType<StmALog>()
				.OrderByDescending(log => log.SL_PostedTimeUtc)
				.Where(log => !log.SL_IsCancelled);
		}

		bool EventMAAWithMatchingCRFExists(string crfReference)
		{
			var documentData = GetDocumentData() as IStmALogParent;

			var scmLog = documentData.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
			if (crfReference == scmLog?.Parameters?.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber))
			{
				return true;
			}

			return false;
		}

		public static List<string> SHLEventsReceivedForContainers(ForwardingContainerCollection containers)
		{
			var shlContainers = new List<string>();
			foreach (ForwardingContainer container in containers)
			{
				var latestSHLEvent = GetEventLogsInDescendingOrder(container).FirstOrDefault(l => l.SL_SE_NKEvent == Events.HeldCode);
				var latestSCMEvent = GetEventLogsInDescendingOrder(container).FirstOrDefault(l => l.SL_SE_NKEvent == Events.ClearanceCompletedCode);

				if ((latestSHLEvent != null && latestSCMEvent == null) || (latestSHLEvent != null && latestSCMEvent != null && latestSHLEvent.SL_PostedTimeUtc > latestSCMEvent.SL_PostedTimeUtc))
				{
					shlContainers.Add(container.JC_ContainerNum);
				}
			}

			return shlContainers;
		}

		#region Implementation

		IVisualizerDocumentData GetDocumentData()
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load(consol, ConsolDocumentDataStoreNames.DEAdvancedLogisticsPortOrder);
		}

		#endregion
	}
}
