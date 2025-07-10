#if DEBUG
using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	public static class WhsTransactionTestHelperCreator
	{
		public static IWhsTransactionTestHelper GetNewHelper(BusinessObjectFactory factory)
		{
			return (IWhsTransactionTestHelper)Activator.CreateInstance(ObjectFactory.GetType<IWhsTransactionTestHelper>(), factory);
		}
	}
}
#endif
