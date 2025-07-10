using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.PGARecapPrinting
{
	public interface IDataSerialiser
	{
		void Clear();
		IEnumerable<ZString> Serialise();
	}
}
