namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	// duplication in Spec
	[InputBlock("B04")]
	[OutputBlock("B04")]
	public partial class INPB04 : MessageBlock
	{
		public INPB04()
			: base("B04")
		{
		}

		/// <summary>
		/// This data element identifies the type of reference identifier being reported for a manifest at the manifest grouping level.
		/// 
		/// Always set to 'V3' for Unique Voyage Identifier (UVI). Left justify code qualifier values that are less than 3 characters in length.
		/// </summary>
		[MessageBlockString(3, 4, "M")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// This field is used to provide the Unique Voyage Identifier for the manifest. The field must be at least 5 characters in length where the first 4 characters must equal the SCAC for the vessel operator.
		/// 
		/// To ensure uniqueness the unique voyage identifier cannot be re-used for another voyage for a period of 5 years.
		/// </summary>
		[MessageBlockString(30, 7, "M")]
		public ZString ReferenceIdentifier;
	}
}
