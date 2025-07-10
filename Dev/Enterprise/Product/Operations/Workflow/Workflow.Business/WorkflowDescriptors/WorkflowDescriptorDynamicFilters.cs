using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	public static class WorkflowDescriptorDynamicFilters
	{
		public static void EnsureProductivitiyWiseAndTestCompatibility(CodeDescriptionPairList list, Predicate<WorkflowDescriptor> predicate)
		{
			foreach (var descriptor in WorkflowDescriptors.Instance.GetDynamicDescriptors())
			{
				if (predicate(descriptor) && !list.ContainsCode(descriptor.Code))
				{
					list.AddPair(descriptor.Code, descriptor.Description.ToString());
				}
			}

			if (DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				foreach (var code in list.GetAllCodes())
				{
					if (!WorkflowDescriptors.IsAllowedForProductivityWise(code))
					{
						list.RemoveCode(code);
					}
				}
			}

			list.Sort();
		}
	}
}
