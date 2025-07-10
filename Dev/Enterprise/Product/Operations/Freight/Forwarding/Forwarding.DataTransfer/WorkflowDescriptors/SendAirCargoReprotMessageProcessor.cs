using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class SendAirCargoReprotMessageProcessor : IProcessor
	{
		public SendAirCargoReprotMessageProcessor(BusinessObject businessObject, DocDataObjectReportSendingProvider reportSendingProvider, ZString location)
		{
			this.businessObject = Argument.NotNull(businessObject, nameof(businessObject));
			this.reportSendingProvider = Argument.NotNull(reportSendingProvider, nameof(reportSendingProvider));
			this.location = location;
		}

		readonly BusinessObject businessObject;
		readonly DocDataObjectReportSendingProvider reportSendingProvider;
		readonly ZString location;

		#region IProcessor Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		public void Process(INotifications notifications, CancellationToken token)
		{
			var logs = ((IStmALogProvider)businessObject).Logs;
			if (reportSendingProvider.SendMessage(businessObject, notifications))
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.MessageType, reportSendingProvider.MessageType),
					new KeyValuePair<string, string>(Params.Location, location)
				};

				logs.CreateOrRecreateEventLog(Events.MessageValidationPassed, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
			}
			else
			{
				var parameters = new[]
				{
					new KeyValuePair<string, string>(Params.MessageType, reportSendingProvider.MessageType),
					new KeyValuePair<string, string>(Params.Location, location),
					new KeyValuePair<string, string>(Params.Reason, Res.GetString("6660f284-1a9d-4d9b-8343-330861aa7c5f", "Check Advanced Air Cargo Report Electronic Messaging form for errors."))
				};

				logs.CreateOrRecreateEventLog(Events.MessageValidationFailed, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, parameters);
			}
		}

		#endregion
	}
}
