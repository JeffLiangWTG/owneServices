namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.AES.Input
{
	using CargoWise.Types;

	[ApplicationIdentifier("XP", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.USCustomsExport)]
	public partial class AESCommShipCL2XP : AESCommShipCL2XPBase
	{
	}

	[InputBlock("CL2")]
	public abstract class AESCommShipCL2XPBase : MessageBlock
	{
		protected AESCommShipCL2XPBase()
			: base("CL2")
		{
		}

		/// <summary>
		/// The identification of the commodity as classified by the U.S. Census Bureau (Schedule B number) or the U.S. International Trade Commission (HTS number).
		///  
		/// See 'Appendix Z - Additional Information Sources' regarding where to find the Schedule B and HTS. See 'Appendix V - HTS Numbers That Cannot be Reported in AES'. See 'Appendix U - HTS/Schedule B Classifications Requiring Used Vehicle Reporting (Input EV1 Record)'.
		///  
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(10, 4, "C")]
		public ZString ScheduleBHTSNumber;

		/// <summary>
		/// A unit of measure code that corresponds to the first quantity (Quantity 1) as prescribed by the Schedule B/HTS number reported.
		/// 
		/// See 'Appendix K - Units of Measure Codes'. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(3, 14, "C")]
		public ZString UnitOfMeasure1;

		/// <summary>
		/// Total number of units that correspond to the first unit (Unit of Measure 1).
		/// 
		/// Zero fill if NOT required.
		/// </summary>
		[MessageBlockDecimal(10, 17, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal Quantity1;

		/// <summary>
		/// Value of the commodity reported in whole U.S. dollars.
		/// 
		/// Zero fill if NOT required.
		/// </summary>
		[MessageBlockDecimal(10, 27, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal ValueOfGoods;

		/// <summary>
		/// A unit of measure code that corresponds to the second quantity (Quantity 2) as prescribed by the Schedule B/HTS number reported.
		/// 
		/// See 'Appendix K - Units of Measure Codes'. 
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(3, 37, "C")]
		public ZString UnitOfMeasure2;

		/// <summary>
		/// Total number of units that correspond to the second unit (Unit of Measure 2).
		/// 
		/// Zero fill if NOT required.
		/// </summary>
		[MessageBlockDecimal(10, 40, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal Quantity2;

		/// <summary>
		/// Gross shipping weight in kilograms. Weight to include container, but exclude carrier equipment.
		/// 
		/// Zero fill if NOT required.
		/// </summary>
		[MessageBlockDecimal(10, 50, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal ShippingWeight;

		/// <summary>
		/// An Export Control Classification Number (ECCN) as issued by the Bureau of Industry and Security (BIS) (formerly the Bureau of Export Administration [BXA]).
		/// 
		/// Space fill if NOT required.
		/// </summary>
		[MessageBlockString(5, 60, "C")]
		public ZString ExportControlClassificationNumberECCN;

		/// <summary>
		/// License number, CFR citation, authorization symbol or Kimberley Process Certificate Number assigned by the licensing agency. 
		/// 
		/// See 'Appendix F - License and License Exemption Type Codes and Reporting Guidelines'.
		/// 
		/// The Approved Community Member # for the United Kingdom must begin with UK followed by nine numbers.
		/// The Approved Community Member # for Australia must begin with DTT followed by eight numbers.
		/// 
		/// Left justify and leave unused positions blank.
		/// </summary>
		[MessageBlockString(14, 65, "C")]
		public ZString ExportLicenseNumberCFRCitationAuthorizationSymbolKCPACM;
	}
}
