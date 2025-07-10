namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface ILicensePlate
	{
		/// <summary>
		/// License plate of equipment.
		/// US:(M/17).
		/// CA:(M/25).
		/// </summary>
		ZString LicensePlateNumber { get; }

		/// <summary>
		/// State/Province of registration of License plate.
		/// US:(M/3).
		/// CA:(M/2).
		/// </summary>
		ZString StateOrProvinceOfRegistration { get; }

		/// <summary>
		/// Country of registration of License plate.
		/// US: (C/3), Condition: if not from US/CA/MX.
		/// CA: (M/2).
		/// </summary>
		ZString CountryOfRegistration { get; }
	}
}
