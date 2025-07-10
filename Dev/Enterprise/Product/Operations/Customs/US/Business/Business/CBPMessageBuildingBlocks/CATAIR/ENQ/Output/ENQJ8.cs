using CargoWise.Types;
using Enterprise.Customs.US.Business.BIRD.Common;

namespace Enterprise.Customs.US.Business.MessageBuildingBlocks.Output
{
	public partial class ENQJ8 : Messaging.Business.MessageBuildingBlocks.Output.Abstract.ENQJ8, IBIRDHeaderIDRecord
	{
		#region IBIRDHeaderIDRecord Members

		ZString IBIRDHeaderIDRecord.EntryNumber
		{
			get { return EntryNumber; }
		}

		ZString IBIRDHeaderIDRecord.EntryFilerCode
		{
			get { return BrokerNumberOrEntryFilerCode; }
		}

		#endregion
	}
}
