namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("62")]
	public abstract partial class ENS62 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS62()
			: base("62")
		{
		}

		/// <summary>
		/// A Harmonized Tariff Schedule of the United States Annotated (HTS) code representing the classification number of the goods being imported. If the class code is 499 or 501, the tariff number is not required.
		/// </summary>
		[MessageBlockString(10, 3, "O")]
		public ZString TariffNumber;

		/// <summary>
		/// A code that identifies the type of fee being collected. Valid codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(3, 13, "M")]
		public ZString ClassCode;

		/// <summary>
		/// A value associated with line-item data that represents the declared fee as imposed by Federal regulation. If no fee is applicable to this line, Record Identifier 62 is not required. If the merchandise processing fee is applicable to this line item but calculates to less than $0.01, enter class code 499 and 0 (zero) as the user fee amount. Two decimal places are implied. If the number is a whole number, the two low-order (cents) positions contain zeros.
		/// </summary>
		[MessageBlockDecimal(8, 16, "M", 2)]
		public ZDecimal UserFeeAmount;
	}
}
