using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IStatusesAndErrors
	{
		ZString LineNumber { get; }
		ZString NarrativeMessage { get; }

		ZString Code { get; }

		ZString ReferenceNumber { get; }
	}

	public interface ITariffNumberStatusAndErrors : IStatusesAndErrors
	{
		ZString TariffNumber { get; }
		ZString PGAAgencyCode { get; }
		ZString PGALineNo { get; }
	}

	public interface ICargoReleaseStatus
	{
		ZString Status { get; }
		ZString NarrativeMessage { get; }
		ZString ErrorIdentifierCode { get; }
	}
}
