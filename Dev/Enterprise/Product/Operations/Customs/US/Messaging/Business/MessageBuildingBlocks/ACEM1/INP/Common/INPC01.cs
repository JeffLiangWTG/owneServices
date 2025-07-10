namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("C01")]
	[OutputBlock("C01")]
	public partial class INPC01 : MessageBlock
	{
		public INPC01()
			: base("C01")
		{
		}

		/// <summary>
		/// A valid container/equipment number associated with a bill of lading. This container/equipment number must reflect the number exactly as it physically appears on the container. Indicate NC for non-containerized freight. Neither an identical container/equipment number nor the designation NC should be repeated within the same bill.
		/// </summary>
		[MessageBlockString(14, 4, "M")]
		public ZString ContainerEquipmentNumber;

		/// <summary>
		/// A valid exporter/carrier seal number associated with the container/equipment. When provided, this field must contain at least 2 non-blank alpha/numeric characters.
		/// </summary>
		[MessageBlockString(15, 18, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)] // It should be less than 15 (MaxLength) even user enter 20 characters to avoid an error.
		public ZString SealNumber1;

		/// <summary>
		/// A valid exporter/carrier seal number associated with the container/equipment. When provided, this field must contain at least 2 non-blank alpha/numeric characters.
		/// </summary>
		[MessageBlockString(15, 33, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString SealNumber2;

		/// <summary>
		/// A code for describing the type of container or equipment used for shipment. Refer to CAMIR Appendix I for valid Container/ Equipment Description codes.
		/// </summary>
		[MessageBlockString(2, 48, "M")]
		public ZString ContainerEquipmentDescriptionCode;

		/// <summary>
		/// Length (in feet and inches) of container/ equipment used to transport shipment. The formula is FFFII, where FFF is feet and II is inches. The range for II is 00 through 11. Length is space filled if Container/ Equipment Type is provided.
		/// </summary>
		[MessageBlockString(5, 50, "C")]
		public ZString ContainerEquipmentLength;

		/// <summary>
		/// Vertical dimension of an object when object is in upright position. The formula is FFFFFFII, where FFFFFF is feet and II is inches. The range for II is 00 through 11. Height should be space filled if Container/Equipment Type is provided.
		/// </summary>
		[MessageBlockString(8, 55, "C")]
		public ZString Height;

		/// <summary>
		/// A shorter measurement of the two horizontal dimensions measured with the object in the upright position. The formula is FFFFFFII, where FFFFFF is feet and II is inches. The range for II is 00 through 11. Width should be space filled if Container/Equipment Type is provided.
		/// </summary>
		[MessageBlockString(8, 63, "C")]
		public ZString Width;

		/// <summary>
		/// A code identifying the type of container/ equipment. A container/equipment type code alone may be used in lieu of the container/equipment length, height and width. Refer to CAMIR Appendix M valid codes. The Container/Equipment Length, Height, and Width fields should be space filled when the Container/Equipment Type is given, otherwise they will be ignored.
		/// </summary>
		[MessageBlockString(4, 71, "C")]
		public ZString ContainerEquipmentType;

		/// <summary>
		/// A code which specifies the loaded condition of the transportation equipment. Valid status codes are:
		/// 
		/// E = Empty
		/// L = Loaded
		/// 
		/// This field must equal E when the B01 Bill of Lading Status Indicator equals 2 (Empty Equipment).
		/// </summary>
		[MessageBlockString(1, 75, "M")]
		public ZString LoadEmptyStatusCode;

		/// <summary>
		/// A code specifying the extent of transportation service required. Valid codes are:
		/// 
		/// BB	=	Break Bulk
		/// CS	=	Container Station
		/// CY	=	Container Yard
		/// HH	=	House-to-House
		/// HL	=	Headload or Devanning
		/// HP	=	House-to-Pier
		/// MD	=	Mixed Delivery
		/// NC	=	Non-Containerized
		/// PH	=	Pier-to-House
		/// PP	=	Pier-to-Pier
		/// RR	=	Roll on - Roll Off
		/// </summary>
		[MessageBlockString(2, 76, "C")]
		public ZString TypeOfServiceCode;
	}
}
