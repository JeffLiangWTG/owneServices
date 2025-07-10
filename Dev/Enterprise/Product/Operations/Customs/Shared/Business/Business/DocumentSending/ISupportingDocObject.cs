using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface ISupportingDocObject : IDocManagerSupport
	{
		BusinessObjectFactory Factory { get; }
		ZString CountryCode { get; }
		SupportingDocSendingObject GetSupportingDocSendingObject();
	}
}
