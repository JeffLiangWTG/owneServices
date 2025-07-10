using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public interface IResetToOriginal
	{
		BusinessObjectFactory Factory { get; }
		ZString InBondNumber { get; }
		ZString MovementDescription { get; }
		ZString CustomsStatus { get; }
		ZString BillNumber { get; }
		ZString ContainerNumber { get; }
		ZString Level { get; }
		void ResetStatus(ZString reason);
		StatusLogManager LogManager { get; }
		EDIMessageCollection Messages { get; }
	}
}
