using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output
{
	public partial class CMQR5 : Messaging.Business.MessageBuildingBlocks.Output.Abstract.CMQR5, IEntryStatusProvider
	{
		#region IEntryStatusProvider Members

		ZString IEntryStatusProvider.EntryStatus
		{
			get { return DispositionActionCode; }
		}

		#endregion
	}
}
