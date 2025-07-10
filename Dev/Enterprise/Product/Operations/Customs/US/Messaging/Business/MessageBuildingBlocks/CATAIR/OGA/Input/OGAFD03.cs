namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FD03")]
	public abstract partial class OGAFD03 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFD03()
			: base("FD03")
		{
		}

		/// <summary>
		/// The value associated with the FDA line item number in whole dollars.
		/// </summary>
		[MessageBlockDecimal(10, 5, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal FDAValueByFDALine;

		/// <summary>
		/// The FDA Ultimate Consignee or “ship to site” consignee number assigned by FDA. Right justify. If the CBP entry level ultimate consignee is foreign based, this data element is mandatory.
		/// </summary>
		[MessageBlockString(12, 15, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString FDAConsigneeFDAEstablishmentIndicatorFEI;

		/// <summary>
		/// The make of the article by manufacturer or distributor from the label or invoice.
		/// </summary>
		[MessageBlockString(38, 27, "C")]
		public ZString TradeOrBrandName;

		/// <summary>
		/// The first dimension of the container. If the container is rectangular, the dimensions are in width, height, and length order. If the container is cylindrical, the dimensions are in diameter and height order. Container dimension information is restricted to use with Acidified and Low Acid canned foods.
		/// </summary>
		[MessageBlockString(4, 65, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ContainerDimension1;

		/// <summary>
		/// The second dimension of the container. If the container is rectangular, the dimensions are in width, height, and length order. If the container is cylindrical, the dimensions are in diameter and height order.
		/// </summary>
		[MessageBlockString(4, 69, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ContainerDimensions2;

		/// <summary>
		/// The third dimension of the container. If the container is rectangular, the dimensions are in width, height, and length order.
		/// </summary>
		[MessageBlockString(4, 73, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ContainerDimensions3;
	}
}
