using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public interface INonPersistentBusinessObject
	{
		ICollection<INonPersistentBusinessObjectFlatten> TopLevelNonPersistentObjects { get; }
		IEnumerable<object> Unlink();
	}
}
