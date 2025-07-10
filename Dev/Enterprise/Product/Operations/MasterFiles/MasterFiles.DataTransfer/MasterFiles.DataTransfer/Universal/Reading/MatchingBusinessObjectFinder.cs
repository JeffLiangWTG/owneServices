using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.MasterFiles.DataTransfer.Universal
{
	public abstract class MatchingBusinessObjectFinder<T1, T2>
		where T1 : IDataObject
		where T2 : BusinessObject
	{
		protected MatchingBusinessObjectFinder(T1 dataObject)
		{
			this.dataObject = Argument.NotNull(dataObject, "dataObject");
		}

		protected readonly T1 dataObject;

		public T2 Find(IEnumerable<T2> businessObjects)
		{
			return businessObjects != null ? FindCore(businessObjects) : null;
		}

		protected abstract T2 FindCore(IEnumerable<T2> businessObjects);
	}
}