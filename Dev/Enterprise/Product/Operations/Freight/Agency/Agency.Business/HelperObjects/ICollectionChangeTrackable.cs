using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Agency
{
	interface ICollectionChangeTrackable<out T> where T : BusinessObject
	{
		IEnumerable<T> AddedElements { get; }
		IEnumerable<T> UpdatedElements { get; }
		IEnumerable<T> RemovedElements { get; }
		IEnumerable<T> ChangedElements { get; }
	}
}
