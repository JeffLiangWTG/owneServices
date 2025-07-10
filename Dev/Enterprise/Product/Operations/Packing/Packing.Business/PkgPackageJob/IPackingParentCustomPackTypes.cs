using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public interface IPackingParentCustomPackTypes
	{
		IEnumerable<ZString> PackTypesToExclude { get; }
	}
}
