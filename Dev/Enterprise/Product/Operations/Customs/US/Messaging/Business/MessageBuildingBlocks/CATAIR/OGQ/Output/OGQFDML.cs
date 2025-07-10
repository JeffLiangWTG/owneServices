namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output.Abstract
{
	using CargoWise.Types;

	public abstract partial class OGQFDML : MessageBlock
	{
		protected OGQFDML(string mandatoryCharacters)
			: base(mandatoryCharacters)
		{
		}

		/// <summary>
		/// FDA record update type. There are ten possible types.
		/// </summary>
		[MessageBlockString(2, 1, "M")]
		public ZString RecordType;

		/// <summary>
		/// FDA product code update information; received from FDA in current format. Filers will use the “Update Only” option for the updates from ABI. Input the new text file name into the “Update File” field and press “Enter”.
		/// </summary>
		[MessageBlockString(78, 3, "M")]
		public ZString ProductCode;
	}
}
