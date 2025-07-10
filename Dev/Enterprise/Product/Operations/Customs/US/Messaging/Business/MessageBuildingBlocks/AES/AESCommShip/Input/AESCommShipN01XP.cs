namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipN01XP : AESCommShipN01XPBase
	{
	}

	[InputBlock("N01")]
	public abstract class AESCommShipN01XPBase : MessageBlock
	{
		protected AESCommShipN01XPBase()
			: base("N01")
		{
		}

		/// <summary>
		/// Identifier of a US Principal Party in Interest, Forwarding Agent, Ultimate Consignee, or Intermediate Consignee.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(11, 4, "C")]
		public ZString PartyID;

		/// <summary>
		/// Type of ID reported: 
		/// D = DUNS 
		/// S = SSN 
		/// E = EIN
		/// T = Foreign Entity
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 15, "C")]
		public ZString PartyIDType;

		/// <summary>
		/// Type of party reported: 
		/// E = US Principal Party in Interest 
		/// F = Forwarding Agent 
		/// C = Ultimate Consignee
		/// I = Intermediate Consignee
		/// </summary>
		[MessageBlockString(1, 16, "M")]
		public ZString PartyType;

		/// <summary>
		/// Company or enterprise name.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(30, 17, "C")]
		public ZString PartyName;

		/// <summary>
		/// Company contact first name/title.
		/// </summary>
		[MessageBlockString(13, 47, "O")]
		public ZString ContactFirstName;

		/// <summary>
		/// Company contact last name.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(20, 60, "C")]
		public ZString ContactLastName;

		/// <summary>
		/// A claim that the consignee is unknown at the time of shipment.
		/// N = Ultimate consignee known and reported. 
		/// Y = Cargo to be sold en route, ultimate consignee name and address to be reported within 4 days of export. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 80, "C")]
		public ZString ToBeSoldEnRouteIndicator;
	}
}
