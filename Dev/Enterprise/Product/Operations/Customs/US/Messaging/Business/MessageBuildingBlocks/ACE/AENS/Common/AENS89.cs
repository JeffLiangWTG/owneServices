namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common
{
	using CargoWise.Types;

	[InputBlock("89")]
	[OutputBlock("89")]
	public partial class AENS89 : MessageBlock
	{
		public AENS89()
			: base("89")
		{
		}

		/// <summary>
		/// CBP accounting classification code representing a specific fee type previously reported at the Entry Summary Header Fee (34-Record) or Line User Fee (62-Record) level.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString AccountingClassCode1; // String

		/// <summary>
		/// Total estimated fee amount that corresponds to Accounting Class Code (1) in U.S. dollars and cents. Two decimal places are implied.
		/// </summary>
		[MessageBlockString(11, 6, "M", Justification = Justification.Right)]
		public ZString TotalFeeAmount1String;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(3, 17, "C")]
		public ZString AccountingClassCode2;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(11, 20, "C", Justification = Justification.Right)]
		public ZString TotalFeeAmount2String;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(3, 31, "C")]
		public ZString AccountingClassCode3;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(11, 34, "C", Justification = Justification.Right)] // Zero fill
		public ZString TotalFeeAmount3String;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(3, 45, "C")]
		public ZString AccountingClassCode4;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(11, 48, "C", Justification = Justification.Right)]
		public ZString TotalFeeAmount4String;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(3, 59, "C")]
		public ZString AccountingClassCode5;

		/// <summary>
		/// An additional fee total class and total estimated fee amount that corresponds to the Accounting Class Code in U.S. dollars and cents.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(11, 62, "C", Justification = Justification.Right)]
		public ZString TotalFeeAmount5String;
	}
}