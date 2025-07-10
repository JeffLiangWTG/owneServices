using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output
{
	public partial class CMQR0 : Messaging.Business.MessageBuildingBlocks.Output.Abstract.CMQR0, IEntryStatusProvider
	{
		#region IEntryStatusProvider Members

		ZString IEntryStatusProvider.EntryStatus
		{
			get { return ImportEntryStatusList.GetEntryStatusFromErrorCode(ErrorMessageIdentifier); }
		}

		#endregion
	}
}
