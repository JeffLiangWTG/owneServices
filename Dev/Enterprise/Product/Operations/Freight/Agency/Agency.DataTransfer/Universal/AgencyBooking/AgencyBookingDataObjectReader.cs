using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Universal
{
	class AgencyBookingDataObjectReader : AgencyShipmentDataObjectReader<AgencyBooking>
	{
		public AgencyBookingDataObjectReader(UniversalShipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, IAgencyShipmentReadStrategy<AgencyBooking> readStrategy = null)
			: base(dataObject, logger, factory, readStrategy)
		{
		}

		#region Implementation

		public override DataContextType DataContextType
		{
			get { return DataContextType.AgencyBooking; }
		}

		protected override IAgencyShipmentReadStrategy<AgencyBooking> GetAgencyShipmentReadStrategy()
		{
			return new AgencyShipmentReadStrategy<AgencyBooking, AgencyBookingContainer, AgencyBookingPackLine>(logger, factory, dataObject);
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(AgencyBooking matchedShipment)
		{
			var shipmentStatus = dataObject.ShipmentStatus != null && dataObject.ShipmentStatus.Code.HasValue ?
				dataObject.ShipmentStatus.Code.Value.ToString().Trim() : null;

			if (!string.IsNullOrEmpty(shipmentStatus) && !ShipmentStatusHelperMethods.IsBookingStage(shipmentStatus))
			{
				return Res.GetString("d4ea2d49-d7e7-48d1-8cbe-5d6bb82dd987", "XML file contains shipment status [{0}] that is invalid in this scope.", shipmentStatus);
			}

			if ((!AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value
					|| !(IsFromCarrier && IsElectronicBooking)
					|| IsUnknownMessagePurpose)
				&& matchedShipment != null && matchedShipment.IsBillOfLadingStage)
			{
				return Res.GetString("ecc8c29c-d7d0-45dd-a792-ee5ad2a61bf4", "{0} has already been confirmed.", matchedShipment.HumanReadableName);
			}

			var reason = base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(matchedShipment);
			if (!reason.IsEmpty)
			{
				return reason;
			}

			if (matchedShipment == null)
			{
				if (References.BookingPartyPK.IsEmpty)
				{
					return Res.GetString("396e8c60-2c48-446f-b7df-1fb00a25a7f5", "XML file does not contain Booking Party.");
				}
			}

			return ZString.Empty;
		}

		protected override void ReadShipmentStatus(AgencyBooking agencyShipment)
		{
			if (IsElectronicBooking)
			{
				var status = ShipmentStatusList.Codes.ElectronicBooking;
				if (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.Value && IsWithdrawalMessage)
				{
					status = ShipmentStatusList.Codes.EBookingCancellationRequest;
				}
				SetValue(agencyShipment, JobShipmentSchema.JS_ShipmentStatus, status);
			}
			else
			{
				base.ReadShipmentStatus(agencyShipment);
			}
		}

		public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
		{
			yield break;
		}

		#endregion
	}
}




