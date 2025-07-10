using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class Logger
	{
		#region MSN

		public static bool CreateMessageSentLog(this IStmALogParent logParent, string documentName)
		{
			if (logParent == null)
			{
				return false;
			}

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment)
			};

			logParent.Logs.CreateOrRecreateEventLog(
				Events.MessageSent,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			return true;
		}

		#endregion

		#region MSW

		public static bool CreateMessageWithdrawalLog(this IStmALogParent logParent, string documentName, string reasonForSending)
		{
			if (logParent == null)
			{
				return false;
			}

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					AirBookingLogConstants.CarrierDepartment),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason,
					reasonForSending)
			};

			logParent.Logs.CreateOrRecreateEventLog(
				Events.MessageWithdrawCancelRequest,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			return true;
		}

		#endregion

		#region STU

		public static bool CreateStatusUpdateLog(this IStmALogParent logParent, string documentName)
		{
			if (logParent == null)
			{
				return false;
			}

			var eventParameters = new[]
			{
				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType,
					documentName),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type,
					Enterprise.Core.Constants.EventReferenceMessageTypes.ResetToOriginal),

				new KeyValuePair<string, string>(
					CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Department,
					Env.CurrentUser.FullName)
			};

			logParent.Logs.CreateOrRecreateEventLog(
				Events.StatusUpdated,
				EstimateActual.Actual,
				ZDateTimeOffset.Now,
				ZString.Empty,
				eventParameters);

			return true;
		}

		#endregion

		#region DEX

		public static bool CreateDataExportEventLog(this IStmALogParent logParent, string apiMessage)
		{
			if (logParent == null)
			{
				return false;
			}

			var factory = logParent.Factory;
			var interchangeBuilder = new InterchangeBuilder(factory);

			if (!interchangeBuilder.TryParseUniversalXml(apiMessage, InterchangeBuilder.MessageDirection.Transmit, out var interchange))
			{
				return false;
			}

			var dataExportLog = logParent.Logs.AddNew(Events.DataExport);

			var messageQuery = new ZQuery(EDIMessageSchema.EM_EI, interchange.PK);
			messageQuery.FetchOnlyFromLocalCache = true;

			var messages = factory.Load<IEDIMessage>(messageQuery);

			foreach (var message in messages)
			{
				var messageLogPivot = factory.New<IGenPivot>();
				messageLogPivot.XX_RelationType = Core.Constants.GenPivotTypes.XmlEdiMessage;
				messageLogPivot.XX_Relation1ID = dataExportLog.PK;
				messageLogPivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
				messageLogPivot.XX_Relation2ID = message.PK;
				messageLogPivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			}

			return true;
		}

		#endregion
	}
}
