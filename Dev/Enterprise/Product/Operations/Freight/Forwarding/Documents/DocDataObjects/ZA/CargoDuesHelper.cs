using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.ZA
{
	public static class CargoDuesHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public static class ContextCollectionTypes
		{
			public const string TotalCargoDuesAmount = "Total Cargo Dues Amount";
			public const string ChargeLine = "ChargeLine";
			public const string Description = "Description";
			public const string Amount = "Amount";
		}

		public static void SetDuesCollectionElement(int elementID, DuesCollectionRow element, List<UniversalDataBuss.DataObjects.Universal.Context> contextCollection)
		{
			var subContextCollection = contextCollection.FirstOrDefault(c => c.Type == ContextCollectionTypes.ChargeLine && c.Value.ToString() == elementID.ToString())?.SubContextCollection;
			if (subContextCollection != null)
			{
				var description = subContextCollection.FirstOrDefault(c => c.Type == ContextCollectionTypes.Description);
				var amount = subContextCollection.FirstOrDefault(c => c.Type == ContextCollectionTypes.Amount);
				element.Description = description != null ? description.Value.ToString() : string.Empty;
				element.Amount = amount != null ? amount.Value.ToString() : string.Empty;
			}
		}

		public static ZDecimal ParseStringToDecimal(ZString value)
		{
			ZDecimal decimalValue;
			decimalValue = ZDecimal.TryParse(value, out decimalValue) ? decimalValue : ZDecimal.Zero;
			return decimalValue;
		}

		public static ZDecimal RoundToTwoDecimals(ZDecimal value)
		{
			return Decimal.Round(value, 2, MidpointRounding.AwayFromZero);
		}

		public static decimal GetTaxRateForCountryOfCurrentCompany(BusinessObjectFactory factory)
		{
			var taxRate = AccTaxRate.Helper.FindTaxRate(factory, AccTaxRate.Helper.MainGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
			if (taxRate != null)
			{
				return taxRate.GetRate(ZDate.Today) / 100;
			}

			return ZDecimal.Zero;
		}

		public static IEnumerable<StmALog> GetMessageAcceptedUniversalEventLogsInDescendingOrder(Logs logs, string documentTitle)
		{
			if (logs != null && documentTitle != null && !string.IsNullOrEmpty(documentTitle))
			{
				foreach (var log in logs.GetAllLogs()
							.OfType<StmALog>()
							.Where(log => log.SL_SE_NKEvent == Events.MessageAcceptedCode
								&& log.RelatedEDIMessage?.Message?.EM_MessageSubType.ToString() == EDIMessageSubTypeList.Codes.XmlUniversalEvent)
							.OrderByDescending(log => log.SL_PostedTimeUtc))
				{
					var messageType = log.Parameters.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.MessageType);

					if (string.Compare(messageType, documentTitle, StringComparison.OrdinalIgnoreCase) == 0)
					{
						yield return log;
					}
				}
			}
		}
	}
}
