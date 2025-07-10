namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FC02")]
	public abstract partial class OGAFC02 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFC02()
			: base("FC02")
		{
		}

		/// <summary>
		/// The total number of items (not the number of containers) right justified.
		/// </summary>
		[MessageBlockDecimal(12, 5, "M", 0)]
		public ZDecimal FCCQuantity;

		/// <summary>
		/// A code of W (withhold) if the importer requests FCC 740 data be withheld from public inspection.
		/// </summary>
		[MessageBlockString(1, 17, "O")]
		public ZString WithholdFromPublicInspectionRequested;
	}
}
