using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Business
{
	public class RelatableActivityTypeDefinition
	{
		public RelatableActivityTypeDefinition(MultilingualString description, ModuleIdentifier moduleId, ControllerID controllerId, ZString tablePrefix, bool shouldUseElementTypeWhenLoading = false)
		{
			Description = description;
			ModuleId = moduleId;
			ControllerId = controllerId;
			TablePrefix = tablePrefix;

			ShouldUseElementTypeWhenLoading = shouldUseElementTypeWhenLoading;
		}

		public readonly MultilingualString Description;
		public readonly ModuleIdentifier ModuleId;
		public readonly ControllerID ControllerId;
		public readonly ZString TablePrefix;
		public readonly bool ShouldUseElementTypeWhenLoading;

		Type elementType;
		public Type ElementType
		{
			get
			{
				if (elementType == null)
				{
					var controllerFactoryType = Type.GetType("Enterprise.ZArchitecture.Modules.ZControllerFactory, Enterprise.ZArchitecture.GUI");
					var controller = controllerFactoryType.GetMethod("Create", BindingFlags.Static | BindingFlags.Public).Invoke(null, new[] { ControllerId });
					elementType = (Type)controller.GetType().GetProperty("TypeOfTopLevelBusinessObject").GetValue(controller, null);
				}

				return elementType;
			}
		}

		public IBusinessObjectCollection GetNewCollection()
		{
			var moduleFactory = Type.GetType("Enterprise.ZArchitecture.Modules.ZModuleFactory, Enterprise.ZArchitecture.GUI").GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
			using (var module = (IZModule)moduleFactory.GetType().GetMethod("Create", new[] { typeof(ModuleIdentifier) }).Invoke(moduleFactory, new[] { ModuleId }))
			{
				var collection = (IBusinessObjectCollection)module.GetType().GetProperty("GridCollection").GetValue(module, null);

				var filterBizObj = GetFilterBusinessObject(module);
				if (filterBizObj != null)
				{
					ZQuery filter = null;
					try
					{
						filter = (ZQuery)filterBizObj.GetType().GetProperty("Filter").GetValue(filterBizObj, null);
					}
					catch (TargetInvocationException ex)
					{
						if (ex.InnerException is ArgumentException)
						{
							ErrorReporter.ReportOnce("Error trying to get Filter for filterBizObj:" + filterBizObj.GetType().Name, ex.InnerException);
						}
						else
						{
							throw;
						}
					}

					if (filter != null)
					{
						var activeBusinessObjectCollection = collection as IActiveBusinessObjectCollection;
						if (activeBusinessObjectCollection != null)
						{
							activeBusinessObjectCollection.AdditionalFilter = filter;
						}
						else
						{
							var legacyBusinessObjectCollection = collection as ILegacyBusinessObjectCollectionInternals;
							if (legacyBusinessObjectCollection != null)
							{
								legacyBusinessObjectCollection.SetOverriddenAdditionalFilter(filter);
							}
						}
					}
				}

				return collection;
			}
		}

		protected virtual FilterBusinessObject GetFilterBusinessObject(IZModule module)
		{
			var filterBizObjProperty = module.GetType().GetProperty("FilterBusinessObject");
			if (filterBizObjProperty != null)
			{
				return filterBizObjProperty.GetValue(module, null) as FilterBusinessObject;
			}

			return null;
		}
	}
}
