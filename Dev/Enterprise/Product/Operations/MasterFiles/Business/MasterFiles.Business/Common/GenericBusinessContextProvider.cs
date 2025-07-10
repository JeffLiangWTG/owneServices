using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class GenericBusinessContextProvider : IGenericBusinessContextProvider
	{
		public IGenericBusinessContext<T> GetInstance<T>(BusinessObjectFactory factory) where T : struct, IComparable, IConvertible
		{
			return GenericBusinessContext<T>.GetOrCreateBusinessContext(factory);
		}
	}
}
