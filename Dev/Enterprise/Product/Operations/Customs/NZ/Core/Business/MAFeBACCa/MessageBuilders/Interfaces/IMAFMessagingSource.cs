namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;

	public interface IMAFMessagingSource
	{
		BusinessObject Master { get; }
		IMAFOrganisation Broker { get; } // O
		ZString OriginCountry { get; } // M 2
		IEnumerable<ZString> DischargePorts { get; } // M 5
		IEnumerable<ZString> Destinations { get; } // M 50 - may be multiple apparently.

		// Voyage Details OR Flight Details required.
		ZString ShipName { get; } // M 30
		ZString VoyageNumber { get; } // M 8
		ZString ShippingCompany { get; } // M 50
		ZDateTime VoyageArrivalDate { get; } // O

		ZString FlightNumber { get; } // M 7
		ZDateTime FlightArrivalDate { get; } // M

		IEnumerable<ZString> BillOfLadingNumbers { get; } // C 17 - At least one BillOfLadingNumber, SubBillOfLadingNumber or ContainerNumber must be provided per application.
		IEnumerable<ZString> SubBillOfLadingNumbers { get; } // C 17 - At least one BillOfLadingNumber, SubBillOfLadingNumber or ContainerNumber must be provided per application.
		IEnumerable<IMAFContainer> Containers { get; }

		ZString ConsignmentDescription { get; } // M 100
		IEnumerable<IMAFCommodity> Commodities { get; } // M 1+

		ZInt CustomsEntryNumber { get; } // C - Required if CargoType = FCL.
		bool IsECIWriteoff { get; } // C - Required if CustomsEntryNumber provided.
		ZString ClientReferenceCoverSheet { get; } // Cover Sheet

		IMAFOrganisation TransitionalFacility { get; } // O - Code is MPI Transitional Facility Code, Contact Names/Numbers not used.
		IMAFOrganisation TreatmentProvider { get; } // O - Code not used, Contact Names/Numbers not used.
	}
}
