using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public static class IEventDataContextManagerWithTriggeringLogExtensions
	{
		#region GetHoldCodeChangedEventContexts

		public static IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetHoldCodeChangedEventContexts(this IEventDataContextManagerWithTriggeringLog dataContextManager, WhsDocket originalDocket)
		{
			IEnumerable<KeyValuePair<TypeWithDescription, IZType>> result = null;

			if (originalDocket != null && dataContextManager != null && dataContextManager.TriggeringLogForUseInPopulatingEventContext != null)
			{
				var triggeringLog = dataContextManager.TriggeringLogForUseInPopulatingEventContext;
				if (!triggeringLog.SL_IsCancelled
					&& PropagationHandler.IsPropagatedEventLog(triggeringLog)
					&& triggeringLog.IsHoldCodeChangeEvent())
				{
					string oldHoldCode = GetValueOrEmptyIfDoesNotExist(triggeringLog, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old);
					string newHoldCode = GetValueOrEmptyIfDoesNotExist(triggeringLog, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New);
					var allDocketLinesFromOriginalDocket = GetAllDocketLinesThatCouldPropagateToOriginalDocket(originalDocket);
					var quantityChanged = GetQuantityChangedGroupedByDocketLineWithUniqueProductAndAttribs(triggeringLog, allDocketLinesFromOriginalDocket, oldHoldCode, newHoldCode);
					var holdCodeChangeContexts = new List<KeyValuePair<TypeWithDescription, IZType>>();

					foreach (var quantityChange in quantityChanged)
					{
						AddHoldCodeChangedContext(holdCodeChangeContexts, oldHoldCode, quantityChange.Key, quantityChange.Value);
					}

					result = holdCodeChangeContexts;
				}
			}

			return result ?? Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		static string GetValueOrEmptyIfDoesNotExist(BaseStmALog log, string eventParameter)
		{
			string value;
			return log.Parameters.TryGetValue(eventParameter, out value) ? value : "";
		}

		static IEnumerable<WhsDocketLine> GetAllDocketLinesThatCouldPropagateToOriginalDocket(WhsDocket originalDocket)
		{
			var query = new ZDBOnlyQuery(typeof(WhsDocketLine));
			query.AddToFilter(WhsDocketLineSchema.WE_WD, originalDocket.PK);

			var originalDocketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.PK);
			originalDocketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WD, originalDocket.PK);

			query.AddSubQuery(WhsDocketLineSchema.WE_WE_OriginalDocketLineForRating, originalDocketLineSubQuery, JoinCondition.Or);

			return originalDocket.Factory.Load<WhsDocketLine>(query);
		}

		#region GetQuantityChangedGroupedByInventoryWithUniqueProductAndAttribs

		static Dictionary<WhsDocketLine, ZDecimal> GetQuantityChangedGroupedByDocketLineWithUniqueProductAndAttribs(BaseStmALog triggeringLog, IEnumerable<WhsDocketLine> allDocketLinesFromOriginalDocket, string oldHoldCode, string newHoldCode)
		{
			var query = new ZQuery();
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ChangeOfIdentifierCode);
			query.AddToFilter(StmALogSchema.SL_EventTime, triggeringLog.SL_EventTime);

			var quantityChanged = new Dictionary<WhsDocketLine, ZDecimal>(new DocketLineComparer());
			var docketLineWithChangedHeldCodes = allDocketLinesFromOriginalDocket
				.Where(i => i.Logs.Find(query).Any(l => LogHasCorrectParameters(l, oldHoldCode, newHoldCode)));

			foreach (var docketLine in docketLineWithChangedHeldCodes)
			{
				ZDecimal currentCount;
				if (quantityChanged.TryGetValue(docketLine, out currentCount))
				{
					quantityChanged[docketLine] = currentCount + docketLine.WE_TransactionQuantity;
				}
				else
				{
					quantityChanged[docketLine] = docketLine.WE_TransactionQuantity;
				}
			}

			return quantityChanged;
		}

		static bool LogHasCorrectParameters(StmALog log, ZString oldHoldCode, ZString newHoldCode)
		{
			return
				log.IsHoldCodeChangeEvent() &&
				oldHoldCode.EqualsIgnoringCase(GetValueOrEmptyIfDoesNotExist(log, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old)) &&
				newHoldCode.EqualsIgnoringCase(GetValueOrEmptyIfDoesNotExist(log, CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New));
		}

		#region DocketLineComparer

		class DocketLineComparer : IEqualityComparer<WhsDocketLine>
		{
			bool IEqualityComparer<WhsDocketLine>.Equals(WhsDocketLine x, WhsDocketLine y)
			{
				return x.WE_OP == y.WE_OP && AttributeComparer.CompareWithoutBondedEntryKey(x, y);
			}

			int IEqualityComparer<WhsDocketLine>.GetHashCode(WhsDocketLine docketLine)
			{
				return AttributeComparer.GetHashCodeForConsolidation(docketLine).GetHashCode();
			}
		}

		#endregion

		#endregion

		#region AddHoldCodeChangedContext

		static void AddHoldCodeChangedContext(List<KeyValuePair<TypeWithDescription, IZType>> holdCodeChangeContexts, string oldHoldCode, WhsDocketLine docketLine, ZDecimal quantityChange)
		{
			var attributes = new ZStringBuilder();
			attributes.AppendIfNotEmpty("PA1=", docketLine.WE_PartAttrib1);
			attributes.AppendIfNotEmpty("PA2=", docketLine.WE_PartAttrib2);
			attributes.AppendIfNotEmpty("PA3=", docketLine.WE_PartAttrib3);
			attributes.AppendIfNotEmpty("SN=", docketLine.WE_SerialNumber);
			attributes.AppendIfNotEmpty("EXP=", docketLine.WE_ExpiryDate.ToShortDateString());
			attributes.AppendIfNotEmpty("PCK=", docketLine.WE_PackingDate.ToShortDateString());

			var attribInfo = attributes.IsEmpty ? "" : string.Format(CultureInfo.CurrentCulture, " ({0})", attributes.ToStringWithDelimiterBetweenAppends(", "));
			var statusChangeInfo = Res.GetString("3148cb9d-abad-4e6c-a1df-5f1fc7f315a8", "{0}x {1}{2} changed from '{3}' to '{4}'",
				quantityChange.ToStringTrimZeros(), docketLine.ProductCode, attribInfo, oldHoldCode, docketLine.WE_WHC_NKCurrentInventoryHeldCode);

			holdCodeChangeContexts.Add(new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription("InventoryWithChangedHoldCode"), (ZString)statusChangeInfo));
		}

		#endregion

		#endregion
	}
}
