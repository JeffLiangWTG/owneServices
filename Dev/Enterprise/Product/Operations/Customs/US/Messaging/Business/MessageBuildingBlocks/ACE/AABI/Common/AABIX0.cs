namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("X0")]
	[OutputBlock("X0")]
	public partial class AABIX0 : MessageBlock
	{
		public AABIX0()
			: base("X0")
		{
		}

		/// <summary>
		/// An indication as to the type of reference information returned. 
		/// 
		/// See Table 1 'Returned Reference Data'.
		/// </summary>
		[MessageBlockString(6, 4, "")]
		public ZString ReferenceDataTypeCode;

		/// <summary>
		/// If a repeating group, the relative position of the submitted input detail within the grouping, otherwise zero.
		/// </summary>
		[MessageBlockInt(6, 11, "M")]
		public ZInt OccurrencePosition;

		/// <summary>
		/// Always 'REF ID:'.
		/// </summary>
		[MessageBlockString(7, 18, "M")]
		public ZString ReferenceIDConstant;

		/// <summary>
		/// Identifying data extracted from the submitted input that corresponds to the Reference Data Type Code. 
		/// 
		/// See Table 1 'Returned Reference Data'.
		/// </summary>
		[MessageBlockString(55, 26, "M", ShouldTrimBegining = false, OnLengthViolation = LengthViolationAction.SetInvalidValue)]//Should not trim beginning it changes the meaning of the data
		public ZString ReferenceDataText;
	}
}
