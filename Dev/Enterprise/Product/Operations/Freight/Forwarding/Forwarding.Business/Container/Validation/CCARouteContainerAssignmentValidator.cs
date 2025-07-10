using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class CCARouteConsolContainerAssignmentValidator
	{
		public bool IsAllowedToAllocateToRoute(
			ForwardingContainer container,
			IRatingContractAllocationLine route,
			out Notification notification)
		{
			notification = null;
			var consol = container.Consol;

			if (consol is not ICCACommonAssignmentValidationData validationData ||
				route is not IRatingContractAllocationLine allocationRoute)
			{
				return true;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				if (!ContractAllocationHelper.ConsolWillHaveLoadDetailsDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidLoadPortForConsolContainer(allocationRoute));
					return false;
				}

				if (!ContractAllocationHelper.ConsolWillHaveDischargeDetailsDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidDischargePortForConsolContainer(allocationRoute));
					return false;
				}

				if (ContractAllocationHelper.SingleLegLoadWillBeDefaulted(consol, allocationRoute)
					|| ContractAllocationHelper.SingleLegDischargeWillBeDefaulted(consol, allocationRoute))
				{
					if (!CCARouteValidationHelper.TransportLegIsValidForRouteDates(allocationRoute, consol.Transports[0]))
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolContainerDatesForAllocationRoute(allocationRoute));
						return false;
					}
				}
				else if (!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, isAttaching: true))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolContainerDatesForAllocationRoute(allocationRoute));
					return false;
				}

				if (!ContractAllocationHelper.CanSingleLegScheduleDetailsBeDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteScheduleDetails(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidScheduleDetailsOnConsolContainer(allocationRoute));
					return false;
				}
			}
			else if (!CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidConsolContainerForLinkedAllocationRoute(allocationRoute));
				return false;
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(container.Factory, allocationRoute);
				if (outstandingUtilization < 0)
				{
					if (allocationRoute.RCA_AllocatedUQ.EqualsIgnoringCase(Core.Constants.AllocationQuantityUnits.Containers))
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.ContainerBookingLimitExceeded(allocationRoute, -outstandingUtilization));
						return false;
					}
					else
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.TEUBookingLimitExceeded(allocationRoute, -outstandingUtilization));
						return false;
					}
				}
			}

			if (allocationRoute.RCA_AllowGatewayConsolOnly
				&& !CCARouteValidationHelper.IsGatewayAgentAssigned(consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.MissingGatewayAgent(allocationRoute));
				return false;
			}

			if (!allocationRoute.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerOwnerForRoute(allocationRoute));
				return false;
			}

			if (allocationRoute.RCA_AllowGroupageOnly
				&& !consol.IsGroupage)
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.GroupageContainerModeConsolsOnly(allocationRoute, consol));
				return false;
			}

			if (CCARouteValidationHelper.IsContainerTypeInvalidForContract(allocationRoute.Contract, container))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerTypeForContract(allocationRoute, container.RefContainer, validationData));
				return false;
			}
			else if (!route.RCA_RC_ContainerType.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerCodeForRoute(allocationRoute, container.RefContainer, consol));
				return false;
			}
			else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(container, allocationRoute))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerClassForRoute(allocationRoute, container.RefContainer, consol));
				return false;
			}

			if (CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, consol))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidConsolContainerForAllocationRouteNamedAccounts(allocationRoute));
				return false;
			}

			if (!CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute))
			{
				if ((consol.SendingForwarder != null && consol.ReceivingForwarder != null)
					|| (consol.SendingForwarder == null && consol.ReceivingForwarder == null))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, consol, consol.SendingForwarder, consol.ReceivingForwarder));
					return false;
				}
				else
				{
					var forwarder = consol.SendingForwarder is null
						? Res.GetString("49f75f92-6184-08ba-426e-0aa015cab832", $"Receiving Agent {consol.ReceivingForwarder.OH_Code}")
						: Res.GetString("0080de96-b546-8b81-4754-fee7a4729b75", $"Sending Agent {consol.SendingForwarder.OH_Code}");

					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, consol, forwarder));
					return false;
				}
			}

			if (!consol.AllocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute));
				return false;
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				if (!CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(route, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfReceiptAndFirstLoadForConsol(route, consol));
					return false;
				}

				if (!CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(route, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfDeliveryAndLastDischargeForConsol(route, consol));
					return false;
				}
			}

			return true;
		}
	}
}
