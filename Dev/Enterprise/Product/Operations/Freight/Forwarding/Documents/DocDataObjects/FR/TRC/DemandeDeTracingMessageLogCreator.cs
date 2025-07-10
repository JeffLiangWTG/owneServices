using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public class DemandeDeTracingMessageLogCreator : IMessageLogCreator
	{
		public DemandeDeTracingMessageLogCreator(ITRCDetails tRCDetails)
		{
			this.tRCDetails = tRCDetails;
		}
		readonly ITRCDetails tRCDetails;

		public bool CreateMessageSentLog(object logParent, IDynamicData data, string documentName, string recipient)
		{
			if (logParent is IVisualizerDocumentData documentData
				&& documentData.Parent is BusinessObject
				&& data?.Value is DemandeDeTracing demandeDeTracing)
			{
				AddMessageSentEvent((IStmALogParent)logParent, recipient, documentName);

				foreach (var container in GetSentContainers(tRCDetails, demandeDeTracing))
				{
					AddMessageSentEvent(container, recipient, documentName);
				}

				return true;
			}

			return false;
		}

		public bool CreateResetToOriginalLog(object logParent, IDynamicData data, string documentName) => false;

		public bool CreateWithdrawalSentLog(object logParent, IDynamicData data, string documentName, string recipient, object reasonForSending) => false;

		IEnumerable<CommonContainer> GetSentContainers(ITRCDetails trcDetails, DemandeDeTracing demandeDeTracing)
		{
			return trcDetails.Containers.Where(c => demandeDeTracing.Containers.Any(x => (ZGuid)x.Identifier == c.PK));
		}

		void AddMessageSentEvent(IStmALogParent logParent, string recipient, string documentName)
		{
			logParent?.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				GetParametersForEvent(recipient, documentName));
		}

		KeyValuePair<string, string>[] GetParametersForEvent(string recipient, string documentName)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
				documentName));

			result.Add(new KeyValuePair<string, string>(
				CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
				recipient));

			return result.ToArray();
		}
	}
}
