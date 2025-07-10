using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class AirlineConnectValidation
	{
		public static ZString ValidateAirlineConnectPreRequisites(this ForwardingConsol consol)
		{
			var preAllocationIsEmpty = PreAllocationIsEmpty(consol);

			if (preAllocationIsEmpty
				&& consol.ShipmentCount == 0
				&& consol.Containers.Count == 0)
			{
				return ConsolPacklineHelper.NoPreAllocationShipmentAndContainerErrorMessage;
			}

			ZString preAllocationAndShipmentErrorMessage;
			if (PreAllocationHasValue(consol))
			{
				preAllocationAndShipmentErrorMessage = ZString.Empty;
			}
			else if (preAllocationIsEmpty)
			{
				preAllocationAndShipmentErrorMessage = consol.Shipments.Count == 0
					? ZString.Empty
					: consol.GetPacklinesValidationMessage();
			}
			else
			{
				preAllocationAndShipmentErrorMessage = ConsolPacklineHelper.MissingPreRequisitesErrorMessage;
			}

			if (!preAllocationAndShipmentErrorMessage.IsEmpty)
			{
				return preAllocationAndShipmentErrorMessage;
			}

			return consol.GetContainersValidationMessage();
		}

		static ZBool PreAllocationHasValue(ForwardingConsol consol)
		{
			return (!consol.JK_TotalShipmentCountCheck.IsEmpty)
				&& (!consol.JK_TotalShipmentActWeightCheck.IsEmpty)
				&& (!consol.JK_TotalShipmentActVolumeCheck.IsEmpty)
				&& (!consol.JK_MaximumAllowablePackageHeight.IsEmpty)
				&& (!consol.JK_MaximumAllowablePackageLength.IsEmpty)
				&& (!consol.JK_MaximumAllowablePackageWidth.IsEmpty);
		}

		static ZBool PreAllocationIsEmpty(ForwardingConsol consol)
		{
			return consol.JK_TotalShipmentCountCheck.IsEmpty
				&& consol.JK_TotalShipmentActWeightCheck.IsEmpty
				&& consol.JK_TotalShipmentActVolumeCheck.IsEmpty
				&& consol.JK_MaximumAllowablePackageHeight.IsEmpty
				&& consol.JK_MaximumAllowablePackageLength.IsEmpty
				&& consol.JK_MaximumAllowablePackageWidth.IsEmpty;
		}
	}
}
