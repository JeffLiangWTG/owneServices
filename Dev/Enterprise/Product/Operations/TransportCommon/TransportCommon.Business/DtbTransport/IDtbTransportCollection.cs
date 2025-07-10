using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.TransportCommon.Business
{
	public interface IDtbTransportCollection : IActiveBusinessObjectCollection
	{
		new DtbTransport this[int index] { get; }
		IEnumerable<DtbTransport> Typed { get; }
	}
}
