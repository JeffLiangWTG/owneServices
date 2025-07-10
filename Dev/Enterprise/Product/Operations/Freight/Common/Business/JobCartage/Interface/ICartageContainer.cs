using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Common.Business
{
	public interface ICartageContainer
	{
		ZGuid JobContainerPK { get; }
		ZString ContainerNumber { get; }
		ZString ContainerMode { get; }
		ZGuid ContainerRC { get; }
		ZDecimal NetWeight { get; }
		ZString Seal { get; }
		IReadOnlyCollection<ICartageLooseCargo> LooseCargo { get; }
	}
}
