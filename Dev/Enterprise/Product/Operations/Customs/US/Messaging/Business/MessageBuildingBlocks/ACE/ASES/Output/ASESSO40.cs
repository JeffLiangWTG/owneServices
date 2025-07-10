namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO40")]
	public class ACEQWO40 : ASESSO40Base
	{
		public ACEQWO40()
			: base("WO40")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO40")]
	public class ASESSO40 : ASESSO40Base
	{
		public ASESSO40()
			: base("SO40")
		{
		}
	}

	public abstract class ASESSO40Base : MessageBlock
	{
		public ASESSO40Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// Code identifying the type of Bill of Lading Number. Valid codes are:
		/// 
		/// R = Regular / Simple Bill of Lading
		/// M = Master Bill of Lading
		/// H = House Bill of Lading
		/// S = Sub-House Bill of Lading (future use)
		/// T = Express Carrier Tracking Number (Air only)
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString BillTypeIndicator;

		/// <summary>
		/// A code representing the party who issued the bill of lading. Space fill for Air mode of transportation.
		/// </summary>
		[MessageBlockString(4, 6, "C")]
		public ZString IssuerCodeOfBillOfLadingNumber;

		/// <summary>
		/// The bill of lading number as listed on the manifest. If the number is less than 50 positions, it is left justified. Do not include spaces, hyphens, slashes or other special characters.
		/// </summary>
		[MessageBlockString(50, 10, "M")]
		public ZString BillOfLadingNumber;

		/// <summary>
		/// The entered quantity associated with the bill of lading number being reported. It is the smallest exterior packaging unit.
		/// </summary>
		[MessageBlockInt(8, 60, "C")]
		public ZInt Quantity;

		/// <summary>
		/// The unit of measure corresponding to the bill of lading quantity.
		/// </summary>
		[MessageBlockString(5, 68, "C")]
		public ZString UnitOfMeasure;

		/// <summary>
		/// The manifested quantity associated with the bill of lading number being reported. It is the smallest exterior packaging unit.
		/// </summary>
		[MessageBlockString(8, 73, "C")]
		public ZString ManifestedQuantity;
	}
}
