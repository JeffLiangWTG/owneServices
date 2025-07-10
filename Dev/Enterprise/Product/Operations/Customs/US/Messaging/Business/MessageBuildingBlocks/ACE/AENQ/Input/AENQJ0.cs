namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("J0")]
	public partial class AENQJ0 : MessageBlock
	{
		public AENQJ0()
			: base("J0")
		{
		}

		/// <summary>
		/// An indication that the latest entry summary information on file in ACE, in the form of AE submission data records (i.e., the 10- through 90-Records), are to be returned in the output. 
		/// 
		/// Y = Include detail in output for all entry summaries found.
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString ReturnDetailRequestIndicator;
	}
}
