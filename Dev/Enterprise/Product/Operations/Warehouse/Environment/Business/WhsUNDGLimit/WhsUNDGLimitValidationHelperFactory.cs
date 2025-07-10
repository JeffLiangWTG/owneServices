using System.Collections;
using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	class WhsUNDGLimitValidationHelperFactory : IWhsUNDGLimitValidationHelperFactory
	{
		public IWhsUNDGLimitValidationHelper GetWhsUNDGLimitValidationHelper(ZString warehouseType)
		{
			var providers = ObjectFactory.Get<Hashtable>(WhsUNDGLimitValidationHelpers);
			var providerHandle = (ObjectHandle)providers[warehouseType.ToString()];
			return (IWhsUNDGLimitValidationHelper)providerHandle?.GetObject();
		}

		const string WhsUNDGLimitValidationHelpers = nameof(WhsUNDGLimitValidationHelpers);
	}
}
