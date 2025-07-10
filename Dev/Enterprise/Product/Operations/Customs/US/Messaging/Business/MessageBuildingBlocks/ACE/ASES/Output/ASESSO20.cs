namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO20")]
	public class ACEQWO20 : ASESSO20Base
	{
		public ACEQWO20()
			: base("WO20")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO20")]
	public class ASESSO20 : ASESSO20Base
	{
		public ASESSO20()
			: base("SO20")
		{
		}
	}

	public abstract partial class ASESSO20Base : MessageBlock
	{
		public ASESSO20Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// Code that defines the Reference Identifier.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// Reference data.
		/// </summary>
		[MessageBlockString(50, 8, "M")]
		public ZString ReferenceIdentifier;
	}
}
