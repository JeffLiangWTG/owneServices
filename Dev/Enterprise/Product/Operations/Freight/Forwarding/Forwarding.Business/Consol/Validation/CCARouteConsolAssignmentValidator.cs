using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class CCARouteConsolAssignmentValidator
	{
		public CCARouteConsolAssignmentValidator(bool scheduleMismatchIsError = false)
		{
			this.scheduleMismatchIsError = scheduleMismatchIsError;
		}

		readonly bool scheduleMismatchIsError;

		public bool IsAllowedToAllocateToRoute(
			IForwardingConsol forwardingConsol,
			IRatingContractAllocationLine route,
			out Notification notification)
		{
			notification = null;

			if (forwardingConsol is not ForwardingConsol consol ||
				route is not IRatingContractAllocationLine allocationRoute)
			{
				return true;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				if (!ContractAllocationHelper.ConsolWillHaveLoadDetailsDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidLoadPortForConsol(allocationRoute, consol));
					return false;
				}

				if (!ContractAllocationHelper.ConsolWillHaveDischargeDetailsDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidDischargePortForConsol(allocationRoute, consol));
					return false;
				}

				if (ContractAllocationHelper.SingleLegLoadWillBeDefaulted(consol, allocationRoute)
					|| ContractAllocationHelper.SingleLegDischargeWillBeDefaulted(consol, allocationRoute))
				{
					if (!CCARouteValidationHelper.TransportLegIsValidForRouteDates(allocationRoute, consol.Transports[0]))
					{
						notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolDatesForAllocationRoute(allocationRoute));
						return false;
					}
				}
				else if (!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol, isAttaching: true))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolDatesForAllocationRoute(allocationRoute));
					return false;
				}

				if (!ContractAllocationHelper.CanSingleLegScheduleDetailsBeDefaulted(consol, allocationRoute)
					&& !CCARouteValidationHelper.IsConsolValidForRouteScheduleDetails(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidScheduleDetailsOnConsol(allocationRoute, consol));
					return false;
				}
			}
			else if (!CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol))
			{
				notification = new Notification(scheduleMismatchIsError ? NotificationType.Error : NotificationType.Warning, CCAValidationMessageProvider.InvalidConsolForLinkedAllocationRoute(allocationRoute));
				return false;
			}

			if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerCodeInCollection(allocationRoute, consol));
				return false;
			}
			else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
				&& CCARouteValidationHelper.JobHasContainersNotMatchingAllocationContainerType(allocationRoute, consol))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerClassInCollection(allocationRoute, consol));
				return false;
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(consol.Factory, allocationRoute);
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

			if (allocationRoute.RCA_AllowGroupageOnly
				&& !consol.IsGroupage)
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.GroupageContainerModeConsolsOnly(allocationRoute, consol));
				return false;
			}

			if (CCARouteValidationHelper.IsConsolInvalidForAllocationRouteNamedAccounts(allocationRoute, consol))
			{
				notification = new Notification(NotificationType.Warning, CCAValidationMessageProvider.InvalidConsolForAllocationRouteNamedAccounts(allocationRoute));
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
						? Res.GetString("90673a16-20e7-309f-4473-29f60b11abc6", $"Receiving Agent {consol.ReceivingForwarder.OH_Code}")
						: Res.GetString("9a547155-6bb8-bc98-4c28-02887112b7b1", $"Sending Agent {consol.SendingForwarder.OH_Code}");

					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, consol, forwarder));
					return false;
				}
			}

			if (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.Value)
			{
				if (!CCARouteValidationHelper.IsConsolFirstLoadValidForPlaceOfReceipt(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfReceiptAndFirstLoadForConsol(allocationRoute, consol));
					return false;
				}

				if (!CCARouteValidationHelper.IsConsolDischargeValidForPlaceOfDelivery(allocationRoute, consol))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidPlaceOfDeliveryAndLastDischargeForConsol(allocationRoute, consol));
					return false;
				}
			}

			foreach (var container in consol.Containers.OfType<ForwardingContainer>())
			{
				if (CCARouteValidationHelper.IsContainerTypeInvalidForContract(allocationRoute.Contract, container))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerTypeForContract(allocationRoute, container.RefContainer, consol));
					return false;
				}
				else if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
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

				if (!allocationRoute.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, container))
				{
					notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.InvalidContainerOwnerInCollection(allocationRoute));
					return false;
				}
			}

			if (!consol.AllocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute))
			{
				notification = new Notification(NotificationType.Error, CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute));
				return false;
			}

			return true;
		}
	}
}
