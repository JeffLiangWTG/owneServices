using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class CommonShipmentJobDatesProvider : ShipmentJobDatesProvider<CommonShipment>
	{
		public CommonShipmentJobDatesProvider(CommonShipment shipment)
			: base(shipment) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			var result = ZDateTime.Empty;
			var correctConsol = Parent.Consols
				.Cast<CommonConsol>()
				.FirstOrDefault(consol => consol.Transports.ArrivalTransport != null && consol.Transports.ArrivalTransport.DiscPort == Parent.RatingAdapter.Destination);

			if (correctConsol != null)
			{
				if (correctConsol.Transports.ArrivalTransport.JW_ATA.IsValid)
				{
					result = correctConsol.Transports.ArrivalTransport.JW_ATA;
				}
				else if (correctConsol.Transports.ArrivalTransport.JW_ETA.IsValid)
				{
					result = correctConsol.Transports.ArrivalTransport.JW_ETA;
				}
			}

			if (result.IsEmpty && Parent.JS_E_ARV.IsValid)
			{
				result = Parent.JS_E_ARV;
			}

			return result;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			var result = ZDateTime.Empty;
			var correctConsol = Parent.Consols
				.Cast<CommonConsol>()
				.FirstOrDefault(consol => consol.Transports.DepartureTransport != null && consol.Transports.DepartureTransport.LoadPort == Parent.RatingAdapter.Origin);

			if (correctConsol != null)
			{
				if (correctConsol.Transports.DepartureTransport.JW_ATD.IsValid)
				{
					result = correctConsol.Transports.DepartureTransport.JW_ATD;
				}
				else if (correctConsol.Transports.DepartureTransport.JW_ETD.IsValid)
				{
					result = correctConsol.Transports.DepartureTransport.JW_ETD;
				}
			}

			if (result.IsEmpty && Parent.JS_E_DEP.IsValid)
			{
				result = Parent.JS_E_DEP;
			}

			return result;
		}

		protected override ZDateTime GetEstimatedArrivalDateCore()
		{
			var result = ZDateTime.Empty;
			var correctConsol = Parent.Consols.Cast<CommonConsol>()
				.FirstOrDefault(consol => consol.Transports.ArrivalTransport != null && consol.Transports.ArrivalTransport.DiscPort == Parent.RatingAdapter.Destination);

			if (correctConsol != null && correctConsol.Transports.ArrivalTransport.JW_ETA.IsValid)
			{
				result = correctConsol.Transports.ArrivalTransport.JW_ETA;
			}

			if (result.IsEmpty && Parent.JS_E_ARV.IsValid)
			{
				result = Parent.JS_E_ARV;
			}

			return result;
		}

		protected override ZDateTime GetEstimatedDepartureDateCore()
		{
			var result = ZDateTime.Empty;
			var correctConsol = Parent.Consols.Cast<CommonConsol>()
				.FirstOrDefault(consol => consol.Transports.DepartureTransport != null && consol.Transports.DepartureTransport.LoadPort == Parent.RatingAdapter.Origin);

			if (correctConsol != null && correctConsol.Transports.DepartureTransport.JW_ETD.IsValid)
			{
				result = correctConsol.Transports.DepartureTransport.JW_ETD;
			}

			if (result.IsEmpty && Parent.JS_E_DEP.IsValid)
			{
				result = Parent.JS_E_DEP;
			}

			return result;
		}

		protected override ZDateTime GetAWBIssueDateCore()
		{
			CommonConsol consol = null;
			if (Parent.Consols.Count == 1)
			{
				consol = Parent.Consols[0];
			}
			else if (Parent.Consols.Count > 1)
			{
				consol = Parent.FindCorrectConsol();
			}

			if (consol != null)
			{
				return consol.JK_MasterBillIssueDate;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetCustomsClearanceDateCore()
		{
			var result = ZDateTime.Empty;
			var mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
										?? Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
										?? Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true))
										?? Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));

			if (mostRecentClearedLog != null)
			{
				result = mostRecentClearedLog.SL_EventTime;
			}

			return result;
		}

		protected override ZDateTime GetCustomsClearanceDateByDirectionCore(string direction)
		{
			var result = ZDateTime.Empty;
			StmALog mostRecentClearedLog = null;

			if (direction == Constants.FreightShipmentDirection.Code.Import)
			{
				mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
										?? Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
			}
			else if (direction == Constants.FreightShipmentDirection.Code.Export)
			{
				mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
										?? Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
			}
			else
			{
				mostRecentClearedLog = Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
											?? Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, false))
											?? Parent.Logs.MostRecentLogByEventTime(Events.CustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true))
											?? Parent.Logs.MostRecentLogByEventTime(Events.ExportCustomsCleared, new ZQuery(StmALogSchema.SL_IsEstimate, true));
			}

			if (mostRecentClearedLog != null)
			{
				result = mostRecentClearedLog.SL_EventTime;
			}

			return result;
		}

		protected override ZDateTime GetPickupDateCore()
		{
			if (!Parent.IsDeleted
				&& (Parent.DocsAndCartage.JP_PickupCartageCompleted.IsValid || Parent.DocsAndCartage.JP_EstimatedPickup.IsValid))
			{
				return Parent.DocsAndCartage.JP_PickupCartageCompleted.IsValid ? Parent.DocsAndCartage.JP_PickupCartageCompleted : Parent.DocsAndCartage.JP_EstimatedPickup;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetDeliveryDateCore()
		{
			if (!Parent.IsDeleted
				&& (Parent.DocsAndCartage.JP_DeliveryCartageCompleted.IsValid || Parent.DocsAndCartage.JP_EstimatedDelivery.IsValid))
			{
				return Parent.DocsAndCartage.JP_DeliveryCartageCompleted.IsValid ? Parent.DocsAndCartage.JP_DeliveryCartageCompleted : Parent.DocsAndCartage.JP_EstimatedDelivery;
			}

			return ZDateTime.Empty;
		}

		protected override ZDateTime GetHouseBillIssueDateCore()
		{
			return Parent.JS_HouseBillIssueDate.IsValid
				? Parent.JS_HouseBillIssueDate
				: ZDateTime.Empty;
		}

		protected override ZDateTime GetFirstContainerGateInDateCore()
		{
			return GetEarliestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		protected override ZDateTime GetLastContainerGateInDateCore()
		{
			return GetLatestValidDate(Parent.Containers.Cast<CommonContainer>().Select(c => c.JC_FCLWharfGateIn));
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ZDateTime GetHBLPlaceOfReceiptArrivalDateCore()
		{
			var result = ZDateTime.Empty;
			switch (Parent.JS_HBLContainerPackModeOverride)
			{
				case Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR:
				case Core.Constants.HBLDeliveryModes.Codes.CY_DOOR:
				case Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR:
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR:
					result = GetPickupDateCore();
					break;

				case Core.Constants.HBLDeliveryModes.Codes.CY_CFS:
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS:
				case Core.Constants.HBLDeliveryModes.Codes.CFS_CFS:
					var routeSets = ((IRoutingSupport)Parent.GetFirstOrCorrectConsol())?.TransportsIncludingRelated.RouteSets ?? new List<RouteSet>();
					if (routeSets.Any())
					{
						result = routeSets.Select(x => x.ReferenceLeg).First().JW_DepotReceivalCommences;
					}
					break;

				case Core.Constants.HBLDeliveryModes.Codes.CFS_CY:
				case Core.Constants.HBLDeliveryModes.Codes.DOOR_CY:
				case Core.Constants.HBLDeliveryModes.Codes.CY_CY:
					result = GetLastContainerGateInDateCore();
					break;
			}

			return result;
		}

		protected override ZDateTime InterimReceiptDateCore()
		{
			return Parent.JS_A_RCV.IsValid
				? Parent.JS_A_RCV
				: ZDateTime.Empty;
		}

		protected override ZDateTime GetRevenueAutoratingDateOverrideCore()
		{
			return Parent.RevenueAutoratingDate;
		}
	}
}
