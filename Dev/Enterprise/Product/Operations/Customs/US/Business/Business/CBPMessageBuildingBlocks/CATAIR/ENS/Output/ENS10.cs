using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output
{
	public partial class ENSE0 : Messaging.Business.MessageBuildingBlocks.Output.Abstract.ENSE0, IEntryStatusProvider
	{
		#region IEntryStatusProvider Members

		ZString IEntryStatusProvider.EntryStatus
		{
			get { return ImportEntryStatusList.GetEntryStatusFromErrorCode(ErrorMessageIdentifier); }
		}

		#endregion
	}
}
