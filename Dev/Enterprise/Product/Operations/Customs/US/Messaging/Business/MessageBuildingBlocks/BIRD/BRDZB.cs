using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.BIRD
{
	[ApplicationIdentifier(ApplicationIdentifierCodeList.Codes.BIRDTransaction)]
	[InputBlock("ZB")]
	[OutputBlock("ZB")]
	public sealed partial class BRDZB : MessageBlock
	{
		public BRDZB()
			: base("ZB")
		{
		}

		[MessageBlockString(30, 3, "C")]
		public ZString ConsigneeCity;

		[MessageBlockString(2, 33, "C")]
		public ZString ConsigneeState;

		[MessageBlockString(9, 35, "C")]
		public ZString ConsigneePostalCode;

		/// <summary>
		/// If City/State/Zip Code has not been broken out, use this field.
		/// If another line of address is needed, City field should be used, but state and postal code should be blank
		/// </summary>
		[MessageBlockString(35, 44, "C")]
		public ZString ConsigneeAddress2;
	}
}
