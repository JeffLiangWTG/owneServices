using System;

namespace Enterprise.Customs.TR.Business
{
	public static class TRBaseMessageExtensions
	{
		public static bool IsMessageSigningRequired(Type ediMessageType, string messageType)
		{
			bool result = ediMessageType switch
			{
				Type t when t == typeof(ETradeEDIMessage) => IsMessageSigningRequiredForETrade(messageType),
				Type t when t == typeof(SPTSMessage) => IsMessageSigningRequiredForSPTS(messageType),
				Type t when t == typeof(TRManifestMessage) => IsMessageSigningRequiredForManifest(messageType),
				Type t when t == typeof(NCTSMessage) => IsMessageSigningRequiredForNCTS(messageType),
				Type t when t == typeof(TRImportExportMessage) => IsMessageSigningRequiredForImportExport(messageType),
				Type t when t == typeof(ExportUnionMessage) => IsMessageSigningRequiredForExportUnion(messageType),
				_ => false
			};

			return result;
		}

		static bool IsMessageSigningRequiredForETrade(string messageType)
		{
			return messageType == TRMessageTypes.Codes.TRE || messageType == TRMessageTypes.Codes.TRS || messageType == TRMessageTypes.Codes.TRD || messageType == TRMessageTypes.Codes.TCD;
		}

		static bool IsMessageSigningRequiredForManifest(string messageType)
		{
			return messageType != TRMessageTypes.Codes.T1O && messageType != TRMessageTypes.Codes.T2O && messageType != TRMessageTypes.Codes.T3O && messageType != TRMessageTypes.Codes.TRM;
		}

		static bool IsMessageSigningRequiredForSPTS(string messageType)
		{
			return messageType != TRMessageTypes.Codes.T1P;
		}

		static bool IsMessageSigningRequiredForNCTS(string messageType)
		{
			return messageType == TRMessageTypes.Codes.TRN || messageType == TRMessageTypes.Codes.TR5;
		}

		static bool IsMessageSigningRequiredForImportExport(string messageType)
		{
			return messageType == TRMessageTypes.Codes.DTE;
		}

		static bool IsMessageSigningRequiredForExportUnion(string messageType)
		{
			return messageType == TRMessageTypes.Codes.EUT;
		}
	}
}
