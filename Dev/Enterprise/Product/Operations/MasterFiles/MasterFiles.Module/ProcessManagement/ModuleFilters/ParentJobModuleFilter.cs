using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class ParentJobModuleFilter : ModuleGuidModuleSpecifiedFilter
	{
		public ParentJobModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
			: base(category, parentCollection)
		{
		}

		public ParentJobModuleFilter(ZString description, SchemaGuidColumn filterColumn, BusinessObjectFactory factory, MultilingualString multilingualDescription = null)
			: base(description, filterColumn, () => GetWorkflowProviderModules(factory))
		{
			MultilingualDescription = multilingualDescription ?? DefaultMultilingualString;
		}

		public static IEnumerable<ModuleIdentifier> GetWorkflowProviderModules(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(WorkflowProviderModulesCacheKey, () => GetWorkflowProviderModulesList(factory));
		}

		internal const string WorkflowProviderModulesCacheKey = "MasterFiles|Module|ParentJobModuleFilter|WorkflowProviderModules";

		public static MultilingualString DefaultMultilingualString => ResString.GetMultilingualString("MasterFiles.Module|ParentJobModuleFilter", "Parent Job");

		static IEnumerable<ModuleIdentifier> GetWorkflowProviderModulesList(BusinessObjectFactory factory)
		{
			var list = new HashSet<ModuleIdentifier>();
			var types = WorkflowTypeList(factory);

			foreach (var type in types.GetAllCodes())
			{
				ZController controller = null;

				try
				{
					controller = WorkflowProviderHelper.GetControllerForWorkflowType(type);
				}
				catch (ModuleGuiNotSupportedException)
				{
				}

				if (controller != null && controller.ModuleID != null)
				{
					list.Add(controller.ModuleID);
				}
			}

			return list;
		}

		static CodeDescriptionPairList WorkflowTypeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("IWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>());
		}
	}
}
