namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipN03XP : AESCommShipN03XPBase
	{
	}

	[InputBlock("N03")]
	public abstract class AESCommShipN03XPBase : MessageBlock
	{
		protected AESCommShipN03XPBase()
			: base("N03")
		{
		}

		/// <summary>
		/// Mailing address (city).
		/// </summary>
		[MessageBlockString(25, 4, "M")]
		public ZString City;

		/// <summary>
		/// Mailing address (state).
		///  
		/// If country is US, use a valid USPS State code. If country is MX, use a valid Mexican state code. See 'Appendix I - Mexican State Codes'. 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 29, "C")]
		public ZString StateCode;

		/// <summary>
		/// Mailing address (country).
		/// 
		/// See 'Appendix C - ISO Country Codes'.
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(2, 31, "C")]
		public ZString CountryCode;

		/// <summary>
		/// Mailing address (Zip Code).
		/// 
		/// If reported, left justify; NO leading spaces or imbedded dashes.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(9, 33, "C")]
		public ZString PostalCode;

		/// <summary>
		/// IRS identifier of the USPPI.
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(9, 42, "C")]
		public ZString USPPIIRSNumber;

		/// <summary>
		/// Type of IRS USPPI ID reported: 
		/// S = SSN 
		/// E = EIN
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(1, 51, "C")]
		public ZString USPPIIRSIDType;

		/// <summary>
		/// Type of Ultimate Consignee reported:
		/// D = Direct Consumer
		/// G = Government Entity
		/// O = Other/Unknown
		/// R = Reseller
		/// </summary>
		[MessageBlockString(1, 52, "C")]
		public ZString UltimateConsigneeType;
	}
}
