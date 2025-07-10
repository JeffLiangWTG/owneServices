namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("PG27")]
	public abstract partial class PGAPG27 : MessageBlock // Need to add interface for BIRD System
	{
		public PGAPG27()
			: base("PG27")
		{
		}

		/// <summary>
		/// The number of the shipping container or equipment identification number.
		/// </summary>
		[MessageBlockString(20, 5, "M")]
		public ZString ContainerEquipmentID;

		/// <summary>
		/// The number of the shipping container or equipment identification number.
		/// </summary>
		[MessageBlockString(20, 25, "C")]
		public ZString ContainerEquipmentID1;

		/// <summary>
		/// The number of the shipping container or equipment identification number.
		/// </summary>
		[MessageBlockString(20, 45, "C")]
		public ZString ContainerEquipmentID2;
	}
}
