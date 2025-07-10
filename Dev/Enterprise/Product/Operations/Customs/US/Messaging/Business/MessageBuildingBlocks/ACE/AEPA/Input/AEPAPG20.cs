namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("PG20")]
	public abstract partial class AEPAPG20 : MessageBlock // Need to add interface for BIRD System
	{
		protected AEPAPG20()
			: base("PG20")
		{
		}

		/// <summary>
		/// Address Line 2 for the Entity.
		/// </summary>
		[MessageBlockString(32, 5, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityAddress2;

		/// <summary>
		/// Apartment/Suite number of the entity.
		/// </summary>
		[MessageBlockString(5, 37, "C")]
		public ZString EntityApartmentNumberSuiteNumber;

		/// <summary>
		/// City of the entity.
		/// </summary>
		[MessageBlockString(21, 42, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityCity;

		/// <summary>
		/// State/Province of the entity. See Appendix B in the ACS ABI CATAIR for valid codes.
		/// </summary>
		[MessageBlockString(3, 63, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityStateProvince;

		/// <summary>
		/// ISO Country Code. See Appendix B in the ACS ABI CATAIR for valid codes.
		/// </summary>
		[MessageBlockString(2, 66, "C")]
		public ZString EntityCountry;

		/// <summary>
		/// Zip/Postal Code of the entity.
		/// </summary>
		[MessageBlockString(9, 68, "C", OnLengthViolation = LengthViolationAction.Substring)]
		public ZString EntityZipPostalCode;
	}
}
