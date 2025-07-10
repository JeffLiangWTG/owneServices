using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	sealed class QuotedBookingContractAllocationFormActions : IRatingContractSimulationFormActions
	{
		public QuotedBookingContractAllocationFormActions(QuotedBooking quotedBooking)
		{
			this.quotedBooking = Argument.NotNull(quotedBooking, nameof(quotedBooking));
		}

		public QuotedBookingContractAllocationFormActions(ForwardingContainer container)
		{
			this.container = Argument.NotNull(container, nameof(container));
			quotedBooking = Argument.NotNull(container.QuotedBooking as QuotedBooking, nameof(container.QuotedBooking));
		}

		readonly QuotedBooking quotedBooking;
		readonly ForwardingContainer container;

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToContract => true;

		bool IRatingContractSimulationFormActions.IsEnabledAllocationToRoute => true;

		bool IRatingContractSimulationFormActions.TryAllocateToContract(IRatingContract contract)
		{
			if (contract == null)
			{
				return false;
			}

			var contractCarrier = contract.RCT_OH;
			if (quotedBooking != null
				&& !quotedBooking.OH_Carrier.IsEmpty
				&& quotedBooking.OH_Carrier != contractCarrier
				&& ZDialogResult.No == Globals.Message.Show(
					Res.GetString(
						"f35235d1-93cf-5198-4453-49c6d93c6723",
						"Service Provider {0} of Contract {1} to be allocated is different from the Carrier {2} on {3}.\r\n\r\nDo you want to override the Carrier on the {3} with {4}?",
						contract.ServiceProvider.OH_Code,
						contract.RCT_ContractNumber,
						quotedBooking.Carrier.OH_Code,
						quotedBooking.HumanReadableNameWithoutID,
						contract.ServiceProvider.OH_Code),
					Res.GetString("96fdd2e8-01d3-14ab-48cb-52de27e04685", "Override the Carrier on the {0}", quotedBooking.HumanReadableNameWithoutID),
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Warning))
			{
				return false;
			}

			if (quotedBooking != null)
			{
				quotedBooking.OH_Carrier = contractCarrier;
				quotedBooking.CarrierContractNumber = contract.RCT_ContractNumber;

				quotedBooking.AllocationLinePK = ZGuid.Empty;
			}

			if (container != null)
			{
				container.JC_RCA_AllocationLine = ZGuid.Empty;
			}

			return true;
		}

		bool IRatingContractSimulationFormActions.TryAllocateToAllocationRoute(IRatingContractAllocationLine allocationRoute)
		{
			if (allocationRoute == null)
			{
				return false;
			}

			var allocationRouteCarrier = allocationRoute.Contract?.RCT_OH ?? ZGuid.Empty;
			if (quotedBooking != null
				&& !quotedBooking.OH_Carrier.IsEmpty
				&& quotedBooking.OH_Carrier != allocationRouteCarrier
				&& ZDialogResult.No == Globals.Message.Show(
					Res.GetString(
						"8b88533f-1e74-589f-4036-64bab040d4c1",
						"Service Provider {0} of Contract {1} and Allocation Route {2} is different from the Carrier {3} on {4}.\r\n\r\nDo you want to override the Carrier on the {4} with {5}?",
						allocationRoute.Contract.ServiceProvider.OH_Code,
						allocationRoute.Contract.RCT_ContractNumber,
						allocationRoute.RCA_AllocationLineID,
						quotedBooking.Carrier.OH_Code,
						quotedBooking.HumanReadableNameWithoutID,
						allocationRoute.Contract.ServiceProvider.OH_Code),
					Res.GetString("a1260f87-549e-c98d-4e8c-9d953d07140a", "Override the Carrier on the {0}", quotedBooking.HumanReadableNameWithoutID),
					ZMessageBoxButtons.YesNo,
					ZMessageBoxIcon.Warning))
			{
				return false;
			}

			if (quotedBooking != null)
			{
				quotedBooking.OH_Carrier = allocationRouteCarrier;
				quotedBooking.CarrierContractNumber = allocationRoute.Contract?.RCT_ContractNumber ?? ZString.Empty;
			}

			if (container != null)
			{
				container.JC_RCA_AllocationLine = allocationRoute.PK;
			}
			else if (quotedBooking != null)
			{
				quotedBooking.AllocationLinePK = allocationRoute.PK;
			}

			return true;
		}
	}
}
