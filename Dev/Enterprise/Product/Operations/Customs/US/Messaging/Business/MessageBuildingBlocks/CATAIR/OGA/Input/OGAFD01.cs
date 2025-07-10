namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FD01")]
	public abstract partial class OGAFD01 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFD01()
			: base("FD01")
		{
		}

		/// <summary>
		/// The FDA line item number.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt FDALineNumber;

		/// <summary>
		/// The code identifying the FDA product.
		/// </summary>
		[MessageBlockString(7, 8, "M")]
		public ZString FDAProductCode;

		/// <summary>
		/// A code of A (ambient temperature), F (frozen), or R (refrigerated).
		/// </summary>
		[MessageBlockString(1, 15, "O")]
		public ZString CargoStorageStatus;

		/// <summary>
		/// A valid International Organization for Standardization (ISO) country code representing the country in which the merchandise was produced or grown (for unprocessed products). This code may be different from that required for CBP purposes. Do not use pseudo country codes such as EU or AP.
		/// </summary>
		[MessageBlockString(2, 18, "M")]
		public ZString FDACountryOfProduction;

		/// <summary>
		/// A code used to affirm compliance with FDA requirements. If more than one Affirmation of Compliance applies to a single FDA line, the second and others are sent in separate FD05 records. If more than one Affirmation of Compliance is sent they cannot duplicate any other unless differentiated by an Affirmation of Compliance Qualifier. An Affirmation of Compliance and Qualifier combination must be unique in a single FDA line. If the merchandise is subject to BTA prior notice requirements, this data element is mandatory. If the PNC or PND affirmation of compliance codes are used, they must appear in this position first. To report additional affirmations of compliance, use the FD05 record. The affirmation of compliance codes and qualifiers are listed in Appendix O of this publication.
		/// </summary>
		[MessageBlockString(3, 20, "C")]
		public ZString AffirmationOfComplianceCode;

		/// <summary>
		/// If the Compliance Indicator is Y (yes), this code is mandatory.
		/// </summary>
		[MessageBlockString(25, 23, "C")]
		public ZString AffirmationOfComplianceQualifier;

		/// <summary>
		/// The actual manufacturer number required by FDA. The FDA Manufacturer is the site-specific location where the product is manufactured, produced, or grown. See ABI Administrative Message 00-0824 for further information. For Prior Notice the site-specific manufacturer must be provided for processing products. The grower, when known, must be provided for unprocessed food and consolidator when grower is not known.
		/// </summary>
		[MessageBlockString(15, 48, "M")]
		public ZString FDAActualManufacturerNumber;

		/// <summary>
		/// The actual shipper or supplier number required by FDA.
		/// </summary>
		[MessageBlockString(15, 63, "M")]
		public ZString FDAActualShipperSupplierNumber;
	}
}
