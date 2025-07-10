using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.TransportBookings.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.TransportBookings.Document
{
	public class DtbBookingConfirmationFFMDetailsProvider : IFFMDetails
	{
		readonly DtbBookingConfirmation confirmation;
		readonly IForwardingConsol parentConsol;
		const string NonConsolParentDefaultGoodsDescription = "OTHER";

		public DtbBookingConfirmationFFMDetailsProvider(DtbBookingConfirmation confirmation)
		{
			this.confirmation = confirmation;
			if (confirmation.Booking.ParentJob?.ParentWithWorkflow is IForwardingConsol bookingConsolParent)
			{
				parentConsol = bookingConsolParent;
			}
			else
			{
				parentConsol = null;
			}
		}

		public BusinessObject BusinessObject => confirmation;

		public ZString VoyageFlightNo => confirmation.Booking.Schedule?.VL_VoyageFlight ?? ZString.Empty;

		public ZDateTime FlightDate => confirmation.Booking.Schedule?.VL_ETD ?? ZDateTime.Empty;

		public IRefUNLOCO AirportOfDestinationCode => confirmation.Booking.Schedule?.Discharge;

		public IRefUNLOCO PortOfLoading => confirmation.Booking.Schedule?.Load;

		public ZString DriverDocumentID => confirmation.KK_DocumentID;

		public IList<IFFMPackageDetails> Packages
		{
			get
			{
				IEnumerable<DtbBookingInstructionPkgDivot> packageDivots = confirmation.PackageDivot != null ? new List<DtbBookingInstructionPkgDivot> { confirmation.PackageDivot } : confirmation.Instruction.PackageDivots;
				var packageDetails = new List<IFFMPackageDetails>();
				foreach (var divot in packageDivots)
				{
					if (divot.Package.IsContainer)
					{
						packageDetails.AddRange(GetPackageDetailsForContainer(divot.Package));
					}
					else
					{
						packageDetails.Add(GetPackageDetailsForLoose(divot));
					}
				}

				return packageDetails;
			}
		}

		public ZString RegulatedAgentID
		{
			get
			{
				return parentConsol?.AWBAgentApprovalNumber ?? ZString.Empty;
			}
		}

		public ZString RegulatedAgentCountry
		{
			get
			{
				return parentConsol?.AWBAgentApprovalCountryCode ?? ZString.Empty;
			}
		}

		public ZString SecurityStatusCode
		{
			get
			{
				return parentConsol?.SecurityStatusCode ?? ZString.Empty;
			}
		}

		IEnumerable<IFFMPackageDetails> GetPackageDetailsForContainer(PkgPackage container)
		{
			foreach (var package in container.Packages)
			{
				yield return GetPackageDetailForContainerSubPackage(container.KP_PackageID, package);
			}
		}

		IFFMPackageDetails GetPackageDetailForContainerSubPackage(ZString containerID, PkgPackage subPackage)
		{
			return new DtbBookingConfirmationDetails()
			{
				ContainerNumber = containerID,
				WayBillNumber = confirmation.Booking.WayBillNumber,
				GoodsDescription = GetGoodsDescription(),
				Quantity = subPackage.KP_PackageQty,
				Volume = subPackage.KP_Volume,
				VolumeMetric = subPackage.KP_VolumeUQ,
				Weight = subPackage.KP_Weight,
				WeightMetric = subPackage.KP_WeightUQ,
				PortOfDestination = confirmation.Booking.Schedule?.Discharge,
				PortOfLoading = confirmation.Booking.Schedule?.Load,
			};
		}

		DtbBookingConfirmationDetails GetPackageDetailsForLoose(DtbBookingInstructionPkgDivot packageDivot)
		{
			var package = packageDivot.Package;

			return new DtbBookingConfirmationDetails()
			{
				ContainerNumber = ZString.Empty,
				WayBillNumber = confirmation.Booking.WayBillNumber,
				GoodsDescription = GetGoodsDescription(),
				Quantity = packageDivot.KD_Quantity,
				Volume = package.KP_Volume,
				VolumeMetric = package.KP_VolumeUQ,
				Weight = package.KP_Weight,
				WeightMetric = package.KP_WeightUQ,
				PortOfDestination = confirmation.Booking.Schedule?.Discharge,
				PortOfLoading = confirmation.Booking.Schedule?.Load,
			};
		}

		ZString GetGoodsDescription()
		{
			if (confirmation.Booking.ConsolidationSingleJob == null)
			{
				return ZString.Empty;
			}
			else if (confirmation.Booking.ConsolidationSingleJob.KB_GoodsDescription != ZString.Empty)
			{
				return confirmation.Booking.ConsolidationSingleJob.KB_GoodsDescription;
			}
			else if (confirmation.Booking.ParentJob == null)
			{
				return ZString.Empty;
			}
			else
			{
				return confirmation.Booking.ParentJob.JobType == Core.Constants.TransportParentTypes.Consol ? nameof(Core.Constants.TransportParentTypes.Consol).ToUpper() : NonConsolParentDefaultGoodsDescription;
			}
		}
	}

	public class DtbBookingConfirmationDetails : IFFMPackageDetails
	{
		public ZString ContainerNumber { get; set; }

		public ZString WayBillNumber { get; set; }

		public IRefUNLOCO PortOfDestination { get; set; }

		public IRefUNLOCO PortOfLoading { get; set; }

		public ZString GoodsDescription { get; set; }

		public ZInt Quantity { get; set; }

		public ZDecimal Weight { get; set; }

		public ZDecimal Volume { get; set; }

		public ZString WeightMetric { get; set; }

		public ZString VolumeMetric { get; set; }
	}
}
