using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IILElectronicMessageProvider
	{
		EnterpriseBusinessObject BusinessObject { get; }
		BusinessObjectFactory Factory { get; }
		EDIMessageCollection Messages { get; }
		ZString MessageReferenceNumber { get; set; }
	}
}
