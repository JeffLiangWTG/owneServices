using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	static class ControllerIdToModuleIdCache_Extensions
	{
		public static ModuleIdentifier GetModuleId(this ControllerID controllerID, BusinessObjectFactory factory)
		{
			if (controllerID == null)
			{
				return null;
			}
			else
			{
				var cache = factory.GetCachedValue(nameof(ControllerIdToModuleIdCache_Extensions), () => new Dictionary<ControllerID, ModuleIdentifier>());

				if (cache.TryGetValue(controllerID, out var result))
				{
					return result;
				}
				else
				{
					var controller = ObjectFactory.Get<IControllerFactory>().Create(controllerID);
					return cache[controllerID] = controller?.ModuleID;
				}
			}
		}
	}
}
