using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	static class FRExtensions
	{
		#region PortCommunicationSystem

		public static ZString GetPCS(this RefUNLOCO port)
		{
			return port?.RefLocoMaps.FirstOrDefault(x => Core.Constants.CountryCodes.IsFranceOrTerritory(x.Country.Code) && x.RY_SystemUsage == LocoMapSystemUsageList.Codes.PCS)?.RY_LocalPortCode ?? ZString.Empty;
		}

		#endregion

		#region GetShipmentNumbersWithSpecifiedRCNSent

		public static Dictionary<ZString, IEnumerable<ZString>> GetShipmentNumbersWithSpecifiedRCNSent(ForwardingShipment currentShipment, IEnumerable<ZString> rcns)
		{
			var cfsCountryCode = currentShipment.ExportReceivingDepot?.OA_RN_NKCountryCode ?? Core.Constants.CountryCodes.France;

			var entryNumberQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, ForwardingPackLine.Schema.TableName);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryStatus, SQLComparisonOperator.NotEqual, string.Empty);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, cfsCountryCode);
			entryNumberQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, rcns);

			var packLineQuery = new ZDBOnlySubQuery(typeof(ForwardingPackLine), JobPackLinesSchema.JL_JS);
			packLineQuery.AddSubQuery(entryNumberQuery, JoinCondition.And);

			var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
			shipmentQuery.AddToFilter(JobShipmentSchema.PK, SQLComparisonOperator.NotEqual, currentShipment.PK);
			shipmentQuery.AddToFilter(JobShipmentSchema.JS_TransportMode, SQLComparisonOperator.Equal, Core.Constants.TransportModes.Sea);
			shipmentQuery.AddSubQuery(packLineQuery, JoinCondition.And);

			var shipments = currentShipment.Factory.Load<ForwardingShipment>(shipmentQuery);

			return shipments.ToDictionary(s => s.JS_UniqueConsignRef, s => s.OuterPackLines
				.Cast<ForwardingPackLine>()
				.SelectMany(p => p.AdditionalReferenceNumbers
						.Cast<CusEntryNumber>()
						.Where(a => a.CE_EntryType == ForwardingPackingLineAdditionalReferenceNumberTypes.Codes.ERC && !a.CE_EntryStatus.IsEmpty && !a.CE_EntryNum.IsEmpty && a.CE_RN_NKCountryCode == cfsCountryCode && rcns.Contains(a.CE_EntryNum))
						.Select(a => a.CE_EntryNum))
				.Distinct());
		}

		#endregion
	}
}
