namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface IConveyance : IEquipment
	{
		/// <summary>
		/// The number assigned to the Transponder (RFI equipment) that is attached to the Conveyance.
		/// US: (M/16), (If pre-registered, only Transponder ID is sufficient to associate pre-registered Conveyance to Manifest.
		///				 Otherwise the other Conveyance data elements need to be provided).
		///				 Condition: if Conveyance not pre-registered in ACE and Transponder exists.
		/// CA: (M/35), !!!Future Use!!!.
		/// </summary>
		ZString TransponderId { get; }
	}
}
