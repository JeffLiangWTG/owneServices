using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class ExportNotificationMessageProcessor : IProcessor
	{
		public ExportNotificationMessageProcessor(BusinessObject businessObject, DocDataObjectReportSendingProvider reportSendingProvider)
		{
			this.businessObject = Argument.NotNull(businessObject, nameof(businessObject));
			this.reportSendingProvider = Argument.NotNull(reportSendingProvider, nameof(reportSendingProvider));
		}

		readonly BusinessObject businessObject;
		readonly DocDataObjectReportSendingProvider reportSendingProvider;

		#region IProcessor Members

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var logs = ((IStmALogProvider)businessObject).Logs;
			if (reportSendingProvider.SendMessage(businessObject, notifications))
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.Department, (NoResString)"Terminal"),
					new KeyValuePair<string, string>(Params.MessageType, (NoResString)"Export Notification (755)"),
				};

				logs.CreateOrRecreateEventLog(Events.MessageValidationPassed, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
			}
			else
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.Department, (NoResString)"Terminal"),
					new KeyValuePair<string, string>(Params.MessageType, (NoResString)"Export Notification (755)"),
					new KeyValuePair<string, string>(Params.Reason, (NoResString)"Check Export Notification (755) Report Electronic Messaging form for errors.")
				};

				logs.CreateOrRecreateEventLog(Events.MessageValidationFailed, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
			}
		}

		#endregion
	}
}
