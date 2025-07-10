using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class OCBEventProcessorHelper
	{
		public static void AddOCBEvent(ForwardingConsol consol, string documentName, string dataStoreName, bool isMessageWithdrawalSent, string action = "")
		{
			if (consol != null && !consol.IsCoLoad && consol.ShippingLineIsShippingLine)
			{
				var factory = new BusinessObjectFactory();
				consol = factory.Load<ForwardingConsol>(consol.PK);

				var documentPurpose = GetMessagePurpose(GetDocumentData(consol, dataStoreName), documentName, isMessageWithdrawalSent);

				if (documentPurpose.IsNullOrEmpty())
				{
					return;
				}

				var totalTEU = OCBEventParameterHelper.GetTotalTEU(consol);
				var totalTEUString = totalTEU.ToString();
				var scac = OCBEventParameterHelper.GetSCAC(consol);

				var lastOCBlog = consol.Logs.MostRecentLogByPostedTime(Events.OceanCarrierBookingByTEU, l => !l.IsCancelled);

				var previousNew = lastOCBlog?.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New) ?? "0";
				decimal.TryParse(lastOCBlog?.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Maximum) ?? "0", out decimal previousMax);
				var previousQty = lastOCBlog?.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Quantity) ?? "0";
				var max = Math.Max(totalTEU, previousMax);

				consol.Logs.CreateOrRecreateEventLog(Events.OceanCarrierBookingByTEU, EstimateActual.Actual, ZDateTimeOffset.Now, ZString.Empty, GetParametersForEvent(documentName, totalTEUString, previousNew, max.ToString(), Math.Max(max - previousMax, 0).ToString(), documentPurpose, scac, action));

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
			}
		}

		static string GetMessagePurpose(IVisualizerDocumentData documentData, string documentName, bool isMessageWithdrawal)
		{
			if (documentData != null && !documentName.IsNullOrEmpty())
			{
				if (isMessageWithdrawal)
				{
					return MessagePurposes.Codes.Withdrawal;
				}

				var dataVersion = documentData.CalculateDataVersion(documentName, false) - 1;
				// Need minus one. Because AddOCBEvent() is invoked by OnMessageSent(). The logs (MSN DEX) of this round are saved before running OnMessageSent().

				if (dataVersion > 1)
				{
					return MessagePurposes.Codes.Amendment;
				}
				else
				{
					return MessagePurposes.Codes.Original;
				}
			}

			return string.Empty;
		}

		static IVisualizerDocumentData GetDocumentData(ForwardingConsol consol, string dataStoreName)
		{
			var documentDataLoader = ObjectFactory.Get<IVisualizerDocumentDataLoader>();
			return documentDataLoader.Load(consol, dataStoreName);
		}

		static KeyValuePair<string, string>[] GetParametersForEvent(string typeValue, string newValue, string old, string maximum, string quantity, string status, string company, string action)
		{
			var result = new List<KeyValuePair<string, string>>();

			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, typeValue));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, newValue));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, old));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Maximum, maximum));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Quantity, quantity));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Status, status));
			result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Company, company));

			if (!string.IsNullOrEmpty(action))
			{
				result.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Action, action));
			}

			return result.ToArray();
		}
	}
}
