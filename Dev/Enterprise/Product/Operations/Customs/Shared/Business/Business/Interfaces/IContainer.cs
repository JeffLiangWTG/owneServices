using CargoWise.Types;

namespace Enterprise.Customs.Business.Interfaces
{
	public interface IContainer
	{
		ZString ContainerNumber { get; }
		ZBool IsForInvoiceLine { get; }
	}
}
