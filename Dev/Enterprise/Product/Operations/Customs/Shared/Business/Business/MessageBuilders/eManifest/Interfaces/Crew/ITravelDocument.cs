namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface ITravelDocument
	{
		/// <summary>
		/// Party Travel Doc Type.
		/// US,CA: (M/3). 
		/// </summary>
		ZString TravelDocumentType { get; }

		/// <summary>
		/// Party Travel Doc No.
		/// US: (M/50).
		/// CA: (M/25).
		/// </summary>
		ZString TravelDocumentNumber { get; }

		/// <summary>
		/// Expiry Date of Travel Document.
		/// US: Not required.
		/// CA: (C/YYYYMMDD).
		/// </summary>
		ZDate ExpiryDate { get; }

		/// <summary>
		/// Party Travel Doc Country of Issuance.
		/// US,CA: (C/2), Condition: if applicable for Travel Doc type.
		/// </summary>
		ZString CountryOfIssuance { get; }

		/// <summary>
		/// Party Travel Doc State/Province of Issuance.
		/// US,CA: (C/2), Condition: if applicable for Travel Doc type.
		/// </summary>
		ZString StateOrProvinceOfIssuance { get; }
	}
}
