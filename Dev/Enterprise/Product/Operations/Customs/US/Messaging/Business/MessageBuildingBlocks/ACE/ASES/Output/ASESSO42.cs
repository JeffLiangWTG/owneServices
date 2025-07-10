namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoManifestEntryReleaseStatusQueryResponse)]
	[OutputBlock("WO42")]
	public class ACEQWO42 : ASESSO42Base
	{
		public ACEQWO42()
			: base("WO42")
		{
		}
	}

	[ApplicationIdentifier(ACEApplicationIdentifierCodeList.Codes.CargoReleaseStatus)]
	[OutputBlock("SO42")]
	public class ASESSO42 : ASESSO42Base
	{
		public ASESSO42()
			: base("SO42")
		{
		}
	}

	public abstract class ASESSO42Base : MessageBlock
	{
		public ASESSO42Base(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// The number identifying the in-bond movement.
		/// </summary>
		[MessageBlockString(12, 5, "M")]
		public ZString InbondNumber;

		/// <summary>
		/// The code representing the type of in-bond movement.
		/// </summary>
		[MessageBlockString(2, 17, "M")]
		public ZString InbondEntryType;

		/// <summary>
		/// In-bond port of departure in Schedule D code.
		/// </summary>
		[MessageBlockString(4, 19, "M")]
		public ZString USPortOfInbondDeparture;

		/// <summary>
		/// In-bond port of arrival in Schedule D code.
		/// </summary>
		[MessageBlockString(4, 23, "M")]
		public ZString USPortOfInbondArrival;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of departure of the in-bond.
		/// </summary>
		[MessageBlockDate(27, "M", "MMddyy")]
		public ZDate InbondCreateDate;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of arrival of the in-bond.
		/// </summary>
		[MessageBlockDate(33, "C", "MMddyy")]
		public ZDate DateOfInbondArrival;

		/// <summary>
		/// This field is used when the in-bond quantity is less than the full bill quantity of the Bill of Lading. If not provided, the in-bond quantity will automatically be assumed to be full Bill quantity.
		/// </summary>
		[MessageBlockInt(8, 39, "C")]
		public ZInt InBondQuantity;
	}
}
