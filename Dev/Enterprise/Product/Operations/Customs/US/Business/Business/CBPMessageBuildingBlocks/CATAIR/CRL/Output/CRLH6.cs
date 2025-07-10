using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output
{
	public partial class CRLH6 : Messaging.Business.MessageBuildingBlocks.Output.Abstract.CRLH6, IEntryStatusProvider
	{
		#region IEntryStatusProvider Members

		ZString IEntryStatusProvider.EntryStatus
		{
			get { return ImportEntryStatusList.GetEntryStatusFromErrorCode(MessageIdentifierCode); }
		}

		#endregion
	}
}