namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO30")]
	public class ACEQWO30 : ASESSO30Base
	{
		public ACEQWO30()
			: base("WO30")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO30")]
	public class ASESSO30 : ASESSO30Base
	{
		public ASESSO30()
			: base("SO30")
		{
		}
	}

	public abstract partial class ASESSO30Base : MessageBlock
	{
		public ASESSO30Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// This number is the same as the Line Item Number transmitted on the input Record SE40.
		/// </summary>
		[MessageBlockInt(3, 5, "M")]
		public ZInt LineItemIdentifier;

		/// <summary>
		/// The International Organization for Standardization (ISO) country code representing the country of origin. Valid ISO codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 8, "M")]
		public ZString CountryOfOrigin;

		/// <summary>
		/// A code located in the Harmonized Tariff Schedule of the United States Annotated (HTS) representing the tariff number.
		/// </summary>
		[MessageBlockString(10, 10, "M")]
		public ZString HTSNumber;
	}
}
