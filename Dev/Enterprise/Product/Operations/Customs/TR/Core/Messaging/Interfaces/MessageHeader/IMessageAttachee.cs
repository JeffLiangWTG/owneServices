using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Messaging
{
	public interface IMessageAttachee
	{
		ZGuid PK { get; }
		ZString MessageStatus { get; set; }
		ZString CustomsStatus { get; set; }
		ZString JobReference { get; }
		ZGuid GlobalBranchPK { get; }
		IBusinessObjectCollection Messages { get; }
	}
}
