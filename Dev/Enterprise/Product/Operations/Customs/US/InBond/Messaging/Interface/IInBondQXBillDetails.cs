using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Messaging.Interface
{
	public interface IInBondQXBillDetails : IIMessageAttacheeWithDisposition
	{
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZString PreviousInBondNumber { get; }
	}
}