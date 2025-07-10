using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Business
{
	public interface ICargoSafeCreator
	{
		DisposableAction TryCreateOrUpdateCargoAndChildBills(INotifications notifications, out BusinessObject masterBill);
		bool IsSupported(string countryCode);
		IBranchProvider GetExistingCargo();
	}
}
