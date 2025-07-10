using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD.Abstract
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("DT")]
	[OutputBlock("DT")]
	public abstract partial class BRDDT : MessageBlock // Need to add interface for BIRD System
	{
		public BRDDT()
			: base("DT")
		{
		}

		/// <summary>
		/// UL = Arrival at first port of unlading
		/// AR = Arrival at port of entry
		/// EN = Entry
		/// ST = Statement
		/// CR = Customs Release
		/// SR = Steamship Release
		/// AV = Cargo Availability
		/// PK = Pick-up
		/// DL = Delivery
		/// FP = Freight Paid
		/// DP = Duty Paid
		/// LQ = Liquidation
		/// DD = Duty Due Date
		/// </summary>
		[MessageBlockString(3, 3, "C")]
		public ZString Date1Qualifier;

		[MessageBlockDate(6, "C", "yyyyMMdd")]
		public ZDate Date1;

		/// <summary>
		/// LT = Local Time
		/// CD = Central Daylight Time
		/// CS = Central Standard Time
		/// CT = Central Time
		/// ED = Eastern Daylight Time
		/// ES = Eastern Standard Time
		/// ET = Eastern Time
		/// MD = Moutain Daylight Time
		/// MS = Moutain Standard Time
		/// MT = Mountain Time
		/// PD = Pacific Daylight Time
		/// PS = Pacific Standard Time
		/// PT = Pacific Time
		/// GM = Greenwich Mean Time
		/// UT = Universal Time
		/// Pnn = Universal Time plus nn hours (P01-P12)
		/// Mnn = Universal Time minus nn hours (M01-M12)
		/// </summary>
		[MessageBlockString(3, 14, "C")]
		public ZString Time1Qualifier;

		/// <summary>
		/// Optional. In 24 hour format HHMMSS
		/// </summary>
		[MessageBlockString(6, 17, "C")]
		public ZString Time1;

		[MessageBlockString(3, 23, "C")]
		public ZString Date2Qualifier;

		[MessageBlockDate(26, "C", "yyyyMMdd")]
		public ZDate Date2;

		[MessageBlockString(3, 34, "C")]
		public ZString Time2Qualifier;

		/// <summary>
		/// Optional. In 24 hour format HHMMSS
		/// </summary>
		[MessageBlockString(6, 37, "C")]
		public ZString Time2;

		[MessageBlockString(3, 43, "C")]
		public ZString Date3Qualifier;

		[MessageBlockDate(46, "C", "yyyyMMdd")]
		public ZDate Date3;

		[MessageBlockString(3, 54, "C")]
		public ZString Time3Qualifier;

		/// <summary>
		/// Optional. In 24 hour format HHMMSS
		/// </summary>
		[MessageBlockString(6, 57, "C")]
		public ZString Time3;
	}
}
