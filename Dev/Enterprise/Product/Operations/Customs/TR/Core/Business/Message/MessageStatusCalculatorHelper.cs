using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Customs.TR.Messaging;

namespace Enterprise.Customs.TR.Business
{
	public static class MessageStatusCalculatorHelper
	{
		public static readonly ImmutableDictionary<(string MessageType, string MessageStatus), (string RegistrationStatus, string MessageMode)> ImportStatusInfos =
			new Dictionary<(string MessageType, string MessageStatus), (string RegistrationStatus, string MessageMode)>
				{
					{ (TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.TRS, TRMessageTypes.Codes.TRQ) },
					{ (TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.TRR, TRMessageTypes.Codes.TRE) },
					{ (TRMessageTypes.Codes.TRQ, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QRS, TRMessageTypes.Codes.TRI) },
					{ (TRMessageTypes.Codes.TRQ, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QRR, TRMessageTypes.Codes.TRQ) },
					{ (TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QIS, TRMessageTypes.Codes.TRL) },
					{ (TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QIR, TRMessageTypes.Codes.TRI) },
					{ (TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QLS, TRMessageTypes.Codes.TRB) },
					{ (TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QLR, TRMessageTypes.Codes.TRL) },
					{ (TRMessageTypes.Codes.TRB, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QBS, TRMessageTypes.Codes.TRD) },
					{ (TRMessageTypes.Codes.TRB, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QBR, TRMessageTypes.Codes.TRB) },
					{ (TRMessageTypes.Codes.TRD, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.DLS, TRMessageTypes.Codes.TCD) },
					{ (TRMessageTypes.Codes.TRD, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.DLR, TRMessageTypes.Codes.TRD) },
					{ (TRMessageTypes.Codes.TCD, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.CDS, TRMessageTypes.Codes.CPL) },
					{ (TRMessageTypes.Codes.TCD, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.CDR, TRMessageTypes.Codes.TCD) },
				}.ToImmutableDictionary();

		public static readonly ImmutableDictionary<(string MessageType, string MessageStatus), (string RegistrationStatus, string MessageMode)> ExportStatusInfos =
			new Dictionary<(string MessageType, string MessageStatus), (string RegistrationStatus, string MessageMode)>
				{
					{ (TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.TRS, TRMessageTypes.Codes.TRS) },
					{ (TRMessageTypes.Codes.TRE, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.TRR, TRMessageTypes.Codes.TRE) },
					{ (TRMessageTypes.Codes.TRS, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.RNS, TRMessageTypes.Codes.TRI) },
					{ (TRMessageTypes.Codes.TRS, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.RNR, TRMessageTypes.Codes.TRS) },
					{ (TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QIS, TRMessageTypes.Codes.TRL) },
					{ (TRMessageTypes.Codes.TRI, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QIR, TRMessageTypes.Codes.TRI) },
					{ (TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Accepted), (CustomsStatusList.Codes.QLS, TRMessageTypes.Codes.CPL) },
					{ (TRMessageTypes.Codes.TRL, TRMessageStatusCodeList.Codes.Error), (CustomsStatusList.Codes.QLR, TRMessageTypes.Codes.TRL) },
				}.ToImmutableDictionary();

		public static (string RegistrationStatus, string MessageMode) CalculateMessageStatusAndMode(string messageType, string messageStatus, bool isImport)
		{
			var statusInfos = isImport ? ImportStatusInfos : ExportStatusInfos;
			statusInfos.TryGetValue((messageType, messageStatus), out var result);
			return result;
		}
	}
}
