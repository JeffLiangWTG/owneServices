namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AMS.Common
{
	using CargoWise.Types;

	[InputBlock("N00")]
	[OutputBlock("N00")]
	public partial class INPN00 : MessageBlock
	{
		public INPN00()
			: base("N00")
		{
		}

		/// <summary>
		/// A code identifying the type of entity.
		/// </summary>
		[MessageBlockString(3, 4, "C")]
		public ZString EntityIDCode;

		/// <summary>
		/// A free form text for name of entity.
		/// </summary>
		[MessageBlockString(35, 7, "C")] // Typo in Spec
		public ZString Name;

		/// <summary>
		/// If positions 4-6, Entity ID Code = CB, then Code 17 is used in positions 42-43 and the ABI Routing Code is placed in the ID code field starting in position 44. Other qualifiers in positions 42-43 are Code 2 = SCAC or FIRMS of SNP (NOTE: The leading numeric 2 must be followed by 1 column of space fill). Future use: Code 9 = DUNS+4, Code F1 = Federal Taxpayers Identification Number.
		/// </summary>
		[MessageBlockString(2, 42, "C")]
		public ZString CodeQualifier;

		/// <summary>
		/// A code related to preceding Code Qualifier data element. This code is required if the Code Qualifier = DUNS +4 or the ABI Office Routing code or the SCAC or FIRMS code if the qualifier is equal to “2”.
		/// </summary>
		[MessageBlockString(17, 44, "C")]
		public ZString IDCode;

		/// <summary>
		/// Not Used. Leave this data element blank.
		/// </summary>
		[MessageBlockString(2, 61, "C")]
		public ZString EntityRelationshipCode;

		/// <summary>
		/// Not Used. Leave this data element blank.
		/// </summary>
		[MessageBlockString(2, 63, "C")]
		public ZString EntityIDCode1;
	}
}
