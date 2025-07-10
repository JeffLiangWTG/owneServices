namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("PG19")]
	public abstract partial class AEPAPG19 : MessageBlock // Need to add interface for BIRD System
	{
		protected AEPAPG19()
			: base("PG19")
		{
		}

		/// <summary>
		/// Identifies the role of the entity. For Example: Grower, producer, manufacturer, I-House, etc. If providing FDA actual manufacturer number, the FDA manufacturer is a site-specific location where the product is manufactured, produced, or grown. See Cargo Security Messaging System CSMS message 00-0824 for further information. For Prior Notice the site-specific manufacturer must be provided for processing products. The grower, when known, must be provided for unprocessed food. The consolidator should be provided when the grower is not known. If the CBP entry level ultimate consignee is foreign based, the FDA Consignee must be provided. See Appendix PGA (Entity Role Code) of this publication for valid codes.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString EntityRoleCode;

		/// <summary>
		/// Identifies the code being used to describe the entity, for example: DUNS, IRS number, FDA Facility Code, Manufacturer ID. If providing a CBP-assigned number for a location, a FIRMS code must be given in the "entity number" field below. See Appendix PGA (Entity Identification Code) of this publication for valid codes.
		/// </summary>
		[MessageBlockString(3, 8, "C")]
		public ZString EntityIdentificationCode;

		/// <summary>
		/// Identifier for the Entity.
		/// </summary>
		[MessageBlockString(15, 11, "C")]
		public ZString EntityNumber;

		/// <summary>
		/// Name of the Entity if no DUNS, FIRMS, or Facility identifications exist.
		/// </summary>
		[MessageBlockString(32, 26, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityName;

		/// <summary>
		/// Address Line 1 for the Entity.
		/// </summary>
		[MessageBlockString(23, 58, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityAddress1;
	}
}
