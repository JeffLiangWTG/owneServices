using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public interface IAllowAttachEmailsToEDocs : IBusiness, IDocManagerSupport
	{
		string ReferenceNumber { get; }
	}
}
