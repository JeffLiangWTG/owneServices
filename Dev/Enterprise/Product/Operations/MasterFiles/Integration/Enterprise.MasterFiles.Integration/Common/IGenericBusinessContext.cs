using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Integration
{
	public interface IGenericBusinessContext<T>
	{
		IDisposable SetTempContext(BusinessObjectFactory factory, params T[] contexts);
	}
}
