using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Messaging.Business
{
	public interface IMessageAttachee : IControllerIDProvider
	{
		ZString MessageStatus { get; set; }
		CBPEDIMessageCollection Messages { get; }
		BusinessObjectFactory Factory { get; }
		GlbBranch Branch { get; }
		BusinessObject TopLevelBusinessObject { get; }
		string TopLevelBizObjReferenceNumber { get; }
		Logs TopLevelBusinessObjectLogs { get; }
	}
}
