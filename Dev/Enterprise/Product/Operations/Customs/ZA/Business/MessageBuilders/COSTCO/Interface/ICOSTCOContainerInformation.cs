using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.MessageBuilders
{
	public interface ICOSTCOContainerInformation : IEQD_EquipmentDetails
		, IDTM_DateTimeUnpakcedDeconsolidated
		, IDTM_DateTimeFullyUnloaded
		, ISEL_SealNumber
	{
	}

	public interface IEQD_EquipmentDetails
	{
		ZString EquipmentType { get; }
		ZString ContainerNumber { get; }
		ZString ContainerSize { get; }
		ZString ContainerStatusLandedPurpose { get; }
		ZString ServiceType { get; }
	}

	public interface IDTM_DateTimeUnpakcedDeconsolidated
	{
		ZDateTime DateUnpacked { get; }
	}

	public interface IDTM_DateTimeFullyUnloaded
	{
		ZDateTime DateTimeFullyUnloaded { get; }
	}

	public interface ISEL_SealNumber
	{
		ZString SealNumber { get; }
		ZString SealingParty { get; }
		ZString SealStatus { get; }
	}
}
