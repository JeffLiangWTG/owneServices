using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business.Extensions;
using Enterprise.ZArchitecture.Business.UniversalCopy;

namespace Enterprise.Freight.Business
{
	public static class ConsolShipmentRelationshipHelper
	{
		public static void ShipmentAddedToConsol(CommonConsol consol, CommonShipment shipment)
		{
			UpdateFlags(consol, shipment);
			UpdateDates(consol, shipment);
			PackShipment(consol, shipment);
			AddAttachEvent(consol, shipment);
			AddSubShipmentsToConsol(consol, shipment);
		}

		public static void ShipmentRemovedFromConsol(CommonConsol consol, CommonShipment shipment)
		{
			UnpackShipment(consol, shipment);
			AddDetachEvent(consol, shipment);

			if (!consol.IsDeleted && !consol.JK_JK_MasterConsol.IsEmpty && consol.MasterConsol != null)
			{
				UnpackShipment(consol.MasterConsol, shipment);
			}

			if (shipment.DocsAndCartage != null)
			{
				shipment.DocsAndCartage.UpdateAvailabilityAndStorageDatesForContainers();
			}
		}

		#region Related Shipments

		static void AddSubShipmentsToConsol(CommonConsol consol, CommonShipment shipment)
		{
			if (shipment.IsLeadOrMaster)
			{
				foreach (CommonShipment subShipment in shipment.CoLoadShipments)
				{
					if (!subShipment.Consols.Contains(consol))
					{
						subShipment.Consols.Add(consol);
					}
				}
			}
		}

		#endregion

		#region Attach / Detach Event

		static void AddAttachEvent(CommonConsol consol, CommonShipment shipment)
		{
			var parameters = new Dictionary<string, string>()
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Constants.EventReferenceParameterTypes.Consol
			};

			shipment.Logs.AddATCEvent(consol.IsInDatabase, consol.LogReference(true), parameters);
		}

		static void AddDetachEvent(CommonConsol consol, CommonShipment shipment)
		{
			if (!shipment.IsDeleted && !consol.IsDeleted && shipment.IsInDatabase)
			{
				shipment.Logs.AddDTCEvent(consol.IsInDatabase, consol.LogReference(true), new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Consol));
			}
		}

		#endregion

		#region Pack / Unpack Shipment

		static void PackShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (consol.AutomaticallyUpdatePackLineContainers && !shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				consol.AllocateShipment(shipment);
			}
		}

		public static void UnpackShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (!shipment.IsDeleted && !shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				foreach (CommonContainer container in consol.Containers)
				{
					shipment.UnpackFromContainer(container);
				}
			}

			if (shipment.OuterPackLines.CurrentConsol == consol)
			{
				shipment.OuterPackLines.CurrentConsol = shipment.Consols.Count > 0 ? shipment.Consols[0] : null;
			}
		}

		#endregion

		#region Dates

		static void UpdateDates(CommonConsol consol, CommonShipment shipment)
		{
			if (!((ISupportDataImporting)shipment).IsImportingData)
			{
				if (!shipment.IsSuppressedETAETDOnAttachToConsol)
				{
					JobDocsAndCartage docsAndCartage = shipment.DocsAndCartage;
					Transport departureTransport = consol.Transports.DepartureTransport;
					Transport arrivalTransport = consol.Transports.ArrivalTransport;

					var updatedETA = false;
					var updatedETD = false;

					if (departureTransport != null && (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || !shipment.JS_E_DEP.IsValid))
					{
						UpdateIfEarlier(shipment.JS_E_DEPInfo, departureTransport.JW_ETD);
						updatedETD = true;
					}
					if (arrivalTransport != null && (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || !shipment.JS_E_ARV.IsValid))
					{
						UpdateIfLater(shipment.JS_E_ARVInfo, arrivalTransport.JW_ETA);
						updatedETA = true;
					}

					docsAndCartage.UpdateAvailabilityAndStorageDatesForContainers();

					if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || updatedETA)
					{
						shipment.UpdateETAWithPortDefaultDeliveryTimeIfEmpty();
					}
					if (!BusinessObjectUniversalCopyFactoryService.IsRunningUniversalCopy(shipment.Factory) || updatedETD)
					{
						shipment.UpdateETDeliveryWithPortDefaultDeliveryTime();
					}
				}
				else
				{
					shipment.IsSuppressedETAETDOnAttachToConsol = false;
				}
			}
		}

		static void UpdateIfLater(ZPropertyInfo targetDateInfo, ZDateTime sourceDate)
		{
			ZDateTime targetDate = (ZDateTime)targetDateInfo.Value;

			if (targetDate.IsEmpty || targetDate < sourceDate)
			{
				targetDateInfo.Value = sourceDate;
			}
		}

		static void UpdateIfEarlier(ZPropertyInfo targetDateInfo, ZDateTime sourceDate)
		{
			ZDateTime targetDate = (ZDateTime)targetDateInfo.Value;

			if (targetDate.IsEmpty || targetDate > sourceDate)
			{
				targetDateInfo.Value = sourceDate;
			}
		}

		#endregion

		#region Flags

		static void UpdateFlags(CommonConsol consol, CommonShipment shipment)
		{
			if (consol.JK_IsCFS && consol.JK_IsForwarding && shipment.JS_IsForwardRegistered)
			{
				shipment.JS_IsCFSRegistered = true;
			}
		}

		#endregion
	}
}
