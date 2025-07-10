using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGenericBusinessContextProvider
	{
		IGenericBusinessContext<T> GetInstance<T>(BusinessObjectFactory factory) where T : struct, IComparable, IConvertible;
	}
}
