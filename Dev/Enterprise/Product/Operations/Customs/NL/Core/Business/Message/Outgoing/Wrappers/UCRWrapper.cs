using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class UCRWrapper : IUCR
{
	public UCRWrapper(ZString jZ_UCR)
	{
		this.jZ_UCR = jZ_UCR;
	}
	readonly ZString jZ_UCR;

	public string TraderAssignedReferenceId => jZ_UCR;
}
