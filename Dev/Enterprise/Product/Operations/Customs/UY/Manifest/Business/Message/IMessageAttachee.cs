using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.UY.Messaging
{
	public interface IMessageAttachee
	{
		ZGuid GlobalBranchPK { get; }
		ZString JobReference { get; }
		IBusinessObjectCollection Messages { get; }
		ZGuid PK { get; }
		ZString TableName { get; }
	}
}
