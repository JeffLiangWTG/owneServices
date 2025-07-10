using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.QuotedBooking;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingContainerValidation : CommonContainerValidation
	{
		public ForwardingContainerValidation(ForwardingContainer parent)
			: base(parent)
		{
		}

		public new ForwardingContainer Parent
		{
			get { return (ForwardingContainer)base.Parent; }
		}

		protected override bool IsFcxContainterModeAllowed
		{
			get { return false; }
		}

		protected override void CheckJC_IsEmptyContainer()
		{
			base.CheckJC_IsEmptyContainer();

			if (Parent.JC_IsEmptyContainer)
			{
				if (Parent.PackLines.Any(x => ((ForwardingPackLine)x).JL_PackageCount > 0))
				{
					Parent.JC_IsEmptyContainerInfo.AddError(Res.GetString("efb8e417-1031-466c-b299-dd9bc330a2f0", "The container cannot be empty as it contains a pack line with non-zero pack count."));
				}
				else if (Parent.PackLines.Take(2).Count() > 1)
				{
					Parent.JC_IsEmptyContainerInfo.AddError(Res.GetString("22393437-bce3-44a7-96c9-91c08ebea1da", "The container cannot be empty as it contains more than one pack line."));
				}
			}
		}

		#region JC_TareWeight

		protected override void CheckJC_TareWeight()
		{
			base.CheckJC_TareWeight();
			if (Parent is ForwardingContainer container && container.GrossWeightUQShowTareWeightWarning)
			{
				Parent.JC_TareWeightInfo.AddWarning(Res.GetString("c5af7bba-7eba-1699-4c3b-037362500c51", "This field has not been converted when the UOM was changed."));
			}
		}

		#endregion

		#region JC_DunnageWeight

		protected override void CheckJC_DunnageWeight()
		{
			base.CheckJC_DunnageWeight();
			if (Parent is ForwardingContainer container && container.GrossWeightUQShowDunnageWeightWarning)
			{
				Parent.JC_DunnageWeightInfo.AddWarning(Res.GetString("2b383d84-1b4a-34a9-43ad-d9ae858ebcbf", "This field has not been converted when the UOM was changed."));
			}
		}

		#endregion

		#region JC_TotalDimensions

		protected override void CheckJC_TotalLength()
		{
			base.CheckJC_TotalLength();
		}

		protected override void CheckJC_TotalWidth()
		{
			base.CheckJC_TotalWidth();
		}

		protected override void CheckJC_TotalHeight()
		{
			base.CheckJC_TotalHeight();
		}

		#endregion

		#region JC_IsShipperOwned

		protected override void CheckJC_IsShipperOwned()
		{
			base.CheckJC_IsShipperOwned();

			if (Parent is ForwardingContainer container
				&& container.AllocationLine is not null
				&& !container.AllocationLine.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(container.AllocationLine, container))
			{
				Parent.JC_IsShipperOwnedInfo.AddError(CCAValidationMessageProvider.InvalidContainerOwnerForRoute(container.AllocationLine));
			}
		}

		#endregion

		protected override void CheckJC_ArrivalCTOStorageStartDate()
		{
			base.CheckJC_ArrivalCTOStorageStartDate();

			var (storageStart, availableDate, _) = Parent.NewContainerDefaultingStrategy()?.CalculateStorageStart(ContainerDetentionDirection.Import) ?? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);

			if (storageStart.IsValid && Parent.JC_ArrivalCTOStorageStartDate != storageStart)
			{
				Parent.JC_ArrivalCTOStorageStartDateInfo.AddWarning(Res.GetString("6412b2fd-ecbf-4396-b5b6-b5d3f3998340", "The availability date and applicable storage free days indicate that this should be '{0}'.", storageStart.ToShortDateString()));
			}
			else if (availableDate.IsValid && Parent.JC_ArrivalCTOStorageStartDate < availableDate)
			{
				Parent.JC_ArrivalCTOStorageStartDateInfo.AddWarning(Res.GetString("88aba214-5cf1-4921-8333-28f31fc55bfe", "The availability date and applicable storage free days indicate that this should be greater than '{0}'.", availableDate.ToShortDateString()));
			}
		}

		#region JC_RCA_AllocationLine

		protected override void CheckJC_RCA_AllocationLine()
		{
			base.CheckJC_RCA_AllocationLine();

			if (Parent.IsValidationSuspended
				|| Parent is not ForwardingContainer parentContainer)
			{
				return;
			}

			var consol = parentContainer.Consol;
			var quotedBooking = parentContainer.QuotedBooking;
			var validationData = consol as ICCACommonAssignmentValidationData ?? quotedBooking;
			if (validationData == null)
			{
				return;
			}

			if (parentContainer.JC_RCA_AllocationLine.IsEmpty)
			{
				if (validationData.AllocationRoute != null)
				{
					parentContainer.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidEmptyRouteOnContainer(validationData));
				}

				return;
			}

			if (validationData.AllocationRoute is IRatingContractAllocationLine parentAllocationRoute
				&& !parentContainer.IsRouteConsistentWithParent())
			{
				parentContainer.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.JobAndContainerAllocationRouteMismatch(parentAllocationRoute, validationData, parentContainer.AllocationLine.RCA_AllocationLineID));
			}

			if (CCARouteValidationHelper.IsRouteAssignmentMissingCarrierContract(validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.MissingContractWhenAllocatedToRoute(validationData));
				return;
			}

			if (CCARouteValidationHelper.IsInvalidAllocationRouteAssigned(Parent.JC_RCA_AllocationLine, validationData.Factory))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidAllocationRoute(validationData));
				return;
			}

			if (!CCARouteValidationHelper.IsRouteAssignedUnderParentContractAssigned(Parent.JC_RCA_AllocationLine, validationData.CarrierContract))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.AllocationRouteAndContractMismatch(validationData));
				return;
			}

			var allocationRoute = parentContainer.AllocationLine;
			if (validationData?.CarrierContract != null
				&& allocationRoute == null)
			{
				return;
			}

			if (!Parent.Consol?.AllocationRouteContainerWeightLimitHelper.CheckContainerWeightLimit(allocationRoute) ?? false)
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.GrossContainerWeightExceedsAllocationLimit(allocationRoute));
			}

			if (allocationRoute.RCA_HasBookingLimit)
			{
				var outstandingUtilization = ContractAllocationHelper.CalculateOutstandingUtilisation(Parent.Factory, allocationRoute);
				if (outstandingUtilization < 0)
				{
					if (allocationRoute.RCA_AllocatedUQ.EqualsIgnoringCase(Core.Constants.AllocationQuantityUnits.Containers))
					{
						Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.ContainerBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
					else
					{
						Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.TEUBookingLimitExceeded(allocationRoute, -outstandingUtilization));
					}
				}
			}

			if (allocationRoute.RCA_AllowGatewayConsolOnly
				&& !CCARouteValidationHelper.IsGatewayAgentAssigned(consol))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.MissingGatewayAgent(allocationRoute));
			}

			if (!allocationRoute.RCA_ContainerOwner.IsEmpty
				&& !CCARouteValidationHelper.IsValidContainerOwner(allocationRoute, parentContainer))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerOwnerForRoute(allocationRoute));
			}

			if (allocationRoute.RCA_AllowGroupageOnly
				&& !consol.IsGroupage)
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.GroupageContainerModeConsolsOnly(allocationRoute, consol));
			}

			if (CCARouteValidationHelper.IsContainerTypeInvalidForContract(allocationRoute.Contract, parentContainer))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerTypeForContract(allocationRoute, parentContainer.RefContainer, validationData));
			}
			else if (!allocationRoute.RCA_RC_ContainerType.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(parentContainer, allocationRoute))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerCodeForRoute(allocationRoute, parentContainer.RefContainer, validationData));
			}
			else if (!allocationRoute.RCA_StorageOrFreightRateClass.IsEmpty
				&& !ContractAllocationHelper.DoesContainerTypeMatchAllocation(parentContainer, allocationRoute))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidContainerClassForRoute(allocationRoute, parentContainer.RefContainer, validationData));
			}

			if (consol != null)
			{
				RunValidationsWhenParentIsConsol(parentContainer, consol, allocationRoute);
			}
			else if (quotedBooking != null)
			{
				RunValidationsWhenParentIsBooking(parentContainer, quotedBooking, allocationRoute);
			}
		}

		void ValidateForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			switch (validationData)
			{
				case ForwardingConsol consol:
					ValidateParentConsolForLinkedAllocationRoute(allocationRoute, consol);
					break;

				case IQuotedBooking booking:
					ValidateParentBookingForLinkedAllocationRoute(allocationRoute, booking);
					break;

				default:
					break;
			}
		}

		void ValidateParentConsolForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (CCARouteValidationHelper.DoAnyConsolTransportLegsMatchAllocationRouteSchedule(allocationRoute, consol))
			{
				return;
			}

			Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolContainerForLinkedAllocationRoute(allocationRoute));
		}

		void ValidateParentBookingForLinkedAllocationRoute(IRatingContractAllocationLine allocationRoute, IQuotedBooking booking)
		{
			if (booking.SailingJX == allocationRoute.RCA_JX_SailingSchedule)
			{
				return;
			}

			Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidBookingContainerForLinkedAllocationRoute(allocationRoute));
		}

		void ValidateForConsolParentForUnlinkedAllocationRoute(IRatingContractAllocationLine allocationRoute, ForwardingConsol consol)
		{
			if (!CCARouteValidationHelper.IsConsolValidForRouteLoadLocation(allocationRoute, consol))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidLoadPortForConsolContainer(allocationRoute));
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteDischargeLocation(allocationRoute, consol))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidDischargePortForConsolContainer(allocationRoute));
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteValidDateRanges(allocationRoute, consol))
			{
				if (CCARouteValidationHelper.BookingEmptyAndMBLEmptyOrAllocationChanged(consol))
				{
					Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolContainerDatesForAllocationRoute(allocationRoute));
				}
				else
				{
					Parent.JC_RCA_AllocationLineInfo.AddWarning(CCAValidationMessageProvider.InvalidConsolETDForAllocationRoute(allocationRoute, consol));
				}
			}

			if (!CCARouteValidationHelper.IsConsolValidForRouteScheduleDetails(allocationRoute, consol))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidScheduleDetailsOnConsolContainer(allocationRoute));
			}
		}

		void ValidateForBookingParentForUnlinkedAllocationRoute(IRatingContractAllocationLine allocationRoute, ICCACommonAssignmentValidationData validationData)
		{
			if (CCARouteValidationHelper.IsETDBeforeRouteStartDate(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.AllocationRouteStartDateViolated(allocationRoute, validationData));
			}
			else if (CCARouteValidationHelper.IsETDAfterRouteExpiryDate(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.AllocationRouteExpiryDateViolated(allocationRoute, validationData));
			}

			if (CCARouteValidationHelper.IsLoadPortNotCoveredByRouteLoadLocation(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidLoadPort(allocationRoute, validationData));
			}

			if (CCARouteValidationHelper.IsDischargePortNotCoveredByRouteDischargeLocation(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidDischargePort(allocationRoute, validationData));
			}

			if (CCARouteValidationHelper.IsVoyageNumberMismatch(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidVoyageNumber(allocationRoute, validationData));
			}

			if (CCARouteValidationHelper.IsVesselMismatch(allocationRoute, validationData))
			{
				Parent.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidVessel(allocationRoute, validationData));
			}
		}

		void RunValidationsWhenParentIsConsol(ForwardingContainer container, ForwardingConsol consol, IRatingContractAllocationLine allocationRoute)
		{
			if (consol?.CarrierContract == null || container.AllocationLine == null)
			{
				return;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				ValidateForConsolParentForUnlinkedAllocationRoute(allocationRoute, consol);
			}
			else
			{
				ValidateForLinkedAllocationRoute(allocationRoute, consol);
			}

			var shipments = consol.Shipments.Cast<ForwardingShipment>().ToArray();
			var namedAccounts = ContractAllocationHelper.GetAllocationRouteNamedAccountsWithFallback(container.AllocationLine);
			if (!ContractAllocationHelper.AllShipmentsHaveMatchingOrgInNamedAccountCollection(shipments, namedAccounts))
			{
				container.JC_RCA_AllocationLineInfo.AddWarning(CCAValidationMessageProvider.InvalidConsolContainerForAllocationRouteNamedAccounts(container.AllocationLine));
			}

			if (!CCARouteValidationHelper.IsValidForwarderForAllocationRouteAgents(consol, allocationRoute))
			{
				if ((consol.SendingForwarder != null && consol.ReceivingForwarder != null)
					|| (consol.SendingForwarder == null && consol.ReceivingForwarder == null))
				{
					container.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, consol, consol.SendingForwarder, consol.ReceivingForwarder));
				}
				else
				{
					var forwarder = consol.SendingForwarder is null
						? Res.GetString("bff681ac-e700-6295-4685-e39198f66cb4", $"Receiving Agent {consol.ReceivingForwarder.OH_Code}")
						: Res.GetString("068d96ac-eb21-5299-45b2-aa2ba5a2a392", $"Sending Agent {consol.SendingForwarder.OH_Code}");

					container.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidConsolForAllocationRouteAgents(allocationRoute, consol, forwarder));
				}
			}
		}

		void RunValidationsWhenParentIsBooking(ForwardingContainer container, IQuotedBooking quotedBooking, IRatingContractAllocationLine allocationRoute)
		{
			if (quotedBooking?.CarrierContract == null
				|| container.AllocationLine == null
				|| quotedBooking.ForwardingShipment as ForwardingShipment == null)
			{
				return;
			}

			if (allocationRoute.RCA_JX_SailingSchedule.IsEmpty)
			{
				ValidateForBookingParentForUnlinkedAllocationRoute(allocationRoute, quotedBooking);
			}
			else
			{
				ValidateForLinkedAllocationRoute(allocationRoute, quotedBooking);
			}

			var namedAccounts = container.AllocationLine.NamedAccountPivots.GetAllNamedAccounts();
			if (!CCABookingValidationHelper.AnyNamedAccountMatchesBooking(quotedBooking, ContractAllocationHelper.ToOrgHeaderCollection(namedAccounts)))
			{
				container.JC_RCA_AllocationLineInfo.AddError(CCAValidationMessageProvider.InvalidBookingContainerForAllocationRouteNamedAccounts(container.AllocationLine));
			}
		}

		#endregion

		#region JC_CLH_LoadListPlan

		protected override void CheckJC_CLH_LoadListPlan()
		{
			base.CheckJC_CLH_LoadListPlan();
			if (Parent.IsInDatabase && !Parent.JC_CLH_LoadListPlanInfo.HasChanges)
			{
				return;
			}

			if (Parent.Factory.Load<CFSContainerLoadList>(Parent.JC_CLH_LoadListPlan) is CFSContainerLoadList containerLoadPlan &&
				!IsValidRelatedContainerLoadPlan(containerLoadPlan.CLH_Status, containerLoadPlan.CLH_LoadMode))
			{
				Parent.JC_CLH_LoadListPlanInfo.AddError(Res.GetString(
					"F4562683-0A1B-4D5E-863B-9D7262FF4AE4",
					"Only incomplete, placed, rejected or approved Container Load Plan can be linked to containers.")
				);
			}
		}

		bool IsValidRelatedContainerLoadPlan(string status, string mode) => mode == ContainerLoadListHeaderLoadMode.ContainerFreightStation && (status == ContainerLoadListHeaderStatus.Incomplete || status == ContainerLoadListHeaderStatus.Placed || status == ContainerLoadListHeaderStatus.Rejected || status == ContainerLoadListHeaderStatus.Approved);

		#endregion

		#region CO2e

		public void ValidateTotalCO2eForEmptyPickupForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForEmptyPickupForBindingInfo);
		}

		protected void CheckTotalCO2eForEmptyPickupForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForEmptyPickupForBindingInfo, CO2eTypes.EmptyPickup);
		}

		public void ValidateTotalCO2eForEmptyReturnForBinding()
		{
			ValidateCalculatedProperty(Parent.TotalCO2eForEmptyReturnForBindingInfo);
		}

		protected void CheckTotalCO2eForEmptyReturnForBinding()
		{
			Parent.CheckCO2eStatus(Parent.TotalCO2eForEmptyReturnForBindingInfo, CO2eTypes.EmptyReturn);
		}

		#endregion

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateTotalCO2eForEmptyPickupForBinding();
			ValidateTotalCO2eForEmptyReturnForBinding();
		}
	}
}
