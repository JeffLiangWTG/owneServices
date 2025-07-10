using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ConsolPacklineHelper
	{
		public static string MissingPreRequisitesErrorMessage => BaseErrorMessage + " " + Res.GetString("b97175a5-53d3-429e-b49d-5189d597d8a0", "Please adjust the details in the preallocations tab of the consol.");
		public static string TotalShipmentWeightErrorMessage => BaseErrorMessage + " " + Res.GetString("7f92f295-0c56-42a2-8632-49b8d93de8bd", "Total shipment weight must be greater than zero.");
		public static string PackLinesErrorMessage => BaseErrorMessage + " " + Res.GetString("a0612de2-cc9c-4fdf-9f27-15fc22201444", "Please review and adjust the packline details on the following attached shipments:");
		public static string ContainersErrorMessage => Res.GetString("369a7c9c-09a6-4c17-9363-8a2e825fc49d", "AirlineConnect is not available due to incomplete, incorrect or missing container details. Please review the container tab on the consol and the container type record used.");
		public static string NoPreAllocationShipmentAndContainerErrorMessage => Res.GetString("0faddcb1-26db-470d-a830-2476cec759a0", "To proceed with AirlineConnect complete the Pre-Allocation tab and/or the container tab or add shipments to your consol.");
		static string BaseErrorMessage => Res.GetString("1ed208a5-3610-4ee1-ad79-65848369d50f", "AirlineConnect is not available due to incorrect/missing weight/dimensions or UOM details.");

		public static ZString GetPacklinesValidationMessage(this ForwardingConsol consol)
		{
			var packLines = consol
				.TopLevelShipments.Cast<ForwardingShipment>()
				.SelectMany(shipment => shipment.OuterPackLines)
				.Cast<ForwardingPackLine>();

			if (!packLines.Any())
			{
				return MissingPreRequisitesErrorMessage;
			}

			if (consol.JK_TotalShipmentWeight <= 0 && !EnableVolumetricWeightDistribution())
			{
				return TotalShipmentWeightErrorMessage;
			}

			var errorMessageBuilder = new ZStringBuilder();
			var shipmentRefs = new HashSet<ZString>();

			foreach (var packLine in packLines)
			{
				if ((packLine.JL_Length <= 0 || packLine.JL_Width <= 0 || packLine.JL_Height <= 0 || packLine.JL_PackageCount <= 0 || packLine.JL_ActualVolume <= 0))
				{
					shipmentRefs.Add(packLine.Shipment.JS_UniqueConsignRef);
				}
			}

			foreach (var shipmentRef in shipmentRefs)
			{
				errorMessageBuilder.Append("- " + shipmentRef);
			}

			return !errorMessageBuilder.IsEmpty
				? PackLinesErrorMessage + "\r\n" + errorMessageBuilder.ToStringWithNewLineBetweenAppends()
				: string.Empty;
		}

		public static ZString GetContainersValidationMessage(this ForwardingConsol consol)
		{
			var forwardingContainers = consol.Containers.Cast<ForwardingContainer>();

			if (!forwardingContainers.Any())
			{
				return ZString.Empty;
			}

			foreach (var forwardingContainer in forwardingContainers)
			{
				if (!forwardingContainer.JC_ContainerNum.IsEmpty
					&& !CommonContainerValidation.ULDRegex.IsMatch(forwardingContainer.JC_ContainerNum)
					|| forwardingContainer.JC_ContainerCount <= 0
					|| forwardingContainer.JC_GrossWeight <= 0
					|| forwardingContainer.JC_Calc_ActualCapacity <= 0)
				{
					return ContainersErrorMessage;
				}
			}

			return ZString.Empty;
		}

		static ZBool EnableVolumetricWeightDistribution()
		{
			return FreightDataRegistry.Instance.PacklineWeightDistribution.Value.EnableVolumetricWeightDistribution;
		}
	}
}
