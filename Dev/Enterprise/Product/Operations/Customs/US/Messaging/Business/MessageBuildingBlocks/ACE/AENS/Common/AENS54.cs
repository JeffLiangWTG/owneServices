namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("54")]
	[OutputBlock("54")]
	public abstract partial class AENS54 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS54()
			: base("54")
		{
		}

		/// <summary>
		/// A code that identifies the type of additional declaration data submitted:
		/// 
		/// 01 = Softwood Lumber Export Information. Information declared to conform to the Softwood Lumber Importer Declaration Program.
		/// </summary>
		[MessageBlockString(2, 3, "M")]
		public ZString ImportersAdditionalDeclarationTypeCode;

		/// <summary>
		/// Importer's additional declaration information (see Note 1) that corresponds to the declaration type.
		/// </summary>
		[MessageBlockString(76, 5, "M", ShouldTrimBegining = false)]
		public ZString ImportersAdditionalDeclarationInformation;
	}
}
