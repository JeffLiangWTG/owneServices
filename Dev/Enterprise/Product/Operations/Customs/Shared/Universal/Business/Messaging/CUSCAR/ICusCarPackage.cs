using CargoWise.Types;

namespace Enterprise.Customs.Universal.Messaging.CUSCAR
{
	public interface ICusCarPackage
	{
		ZInt NumberOfPacks { get; }
		ZString PackUQ { get; }
		ZString Description { get; }
		CargoStatusIndicator CargoStatusIndicator { get; }
		ZDecimal GrossVolumeInM3 { get; }
		ZDecimal GrossVolume { get; }
		ZString GrossVolumeUnitCode { get; }
		ZDecimal GrossMassInKilos { get; }
		ZString UNDGClass { get; }
		ZString UNDGNumber { get; }
		ZString MarksAndNumbers { get; }
		ZString CommodityCode { get; }
		ZString ContainerNumber { get; }
		ZString VINNumber { get; }
	}

	public enum CargoStatusIndicator
	{
		/// <summary>
		/// DO NOT USE THIS, it's just to make the stupid Code Analyser STFU (CA1008)
		/// </summary>
		None = 0,
		PartShipment = 5,
		Fullshipment = 9,
		LastPartShipment = 10
	}
}
