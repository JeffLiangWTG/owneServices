namespace Enterprise.Customs.NZ.Business.MAFeBACCa.Interfaces
{
	using CargoWise.Types;

	interface IMAFMessagingMetaData : IMAFMessagingFallback
	{
		bool? IsMAFAuditRequiredByCustoms { get; } // O
		bool? IsCustomsXRayRequired { get; } // O
		bool? IsCustomsCashClient { get; } // O

		ZString MAFConsignmentNumber { get; } // C 13 - Used to identify updated Applications. (MPI Consignment Reference Number)
		ZString MAFReceiptNumber { get; } // C 13 - Used to identify updated Applications. (Receipt Number from last MPI Response)

		ZString ClientReferenceNumber { get; } // M 14 - Job Number

		ZString Comments { get; } // O 255

		ZString AlternativePaymentMethod { get; } // C - Either submit this or AccountHolderName and AccountNumber.

		IMAFMessagingSource Source { get; }
	}
}
