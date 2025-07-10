namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("75")]
	public partial class INBQP75 : MessageBlock
	{
		public INBQP75()
			: base("75")
		{
		}

		/// <summary>
		/// A code representing the identification number assigned to the hazardous material.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString HazardousMaterialCode;

		/// <summary>
		/// A code representing the hazardous class or division designated for the material in the International Maritime Dangerous Goods (IMDG) code.
		/// </summary>
		[MessageBlockString(4, 13, "O")]
		public ZString HazardousMaterialClass;

		/// <summary>
		/// A code which describes the hazardous material class.
		/// </summary>
		[MessageBlockString(1, 17, "O")]
		public ZString HazardousMaterialCodeQualifier;

		/// <summary>
		/// The proper shipping name of the material designated as hazardous.
		/// </summary>
		[MessageBlockString(30, 18, "O")]
		public ZString HazardousMaterialDescription;

		/// <summary>
		/// The name and/or phone number of the person or department to contact in case of an emergency.
		/// </summary>
		[MessageBlockString(24, 48, "O")]
		public ZString HazardousMaterialContact;

		/// <summary>
		/// A code representing the lowest temperature at which the vapor of a hazardous combustible liquid will ignite in the air. When provided, the Flashpoint Temperature must be a whole number. No decimals.
		/// </summary>
		[MessageBlockInt(3, 72, "O")] //whole number is required
		public ZInt FlashpointTemperature;

		/// <summary>
		/// A code representing the basic unit of measurement (UOM) for the flashpoint temperature. This is always CE = Degrees Centigrade/Celsius.
		/// </summary>
		[MessageBlockString(2, 75, "O")]
		public ZString UnitOfMeasureCode;

		/// <summary>
		/// A code of N is used when a flashpoint temperature is negative, that is, below 0 degrees Centigrade/Celsius.
		/// </summary>
		[MessageBlockString(1, 77, "O")]
		public ZString NegativeIndicator;
	}
}
