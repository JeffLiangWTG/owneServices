namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	public interface IMAFMessagingFallback
	{
		IMAFOrganisation Importer { get; } // M
		IMAFOrganisation Exporter { get; } // M
		ZString ProcessingOffice { get; } // M
		ZString ConsignmentType { get; } // M

		ZString CargoType { get; } // M

		ZString MeasurementUQ { get; } // M
		ZInt MeasurementValue { get; } // M - High level (Bill level) Outer Package Count.

		IMAFAccountDetails AccountDetails { get; }
	}
}
