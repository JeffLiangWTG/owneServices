using CargoWise.Application;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class ProcessWorkflowExceptionTypeLookups : AutoProcessWorkflowExceptionTypeLookups
	{
		public ProcessWorkflowExceptionTypeLookups(AutoProcessWorkflowExceptionType parent)
			: base(parent)
		{
		}

		public ReadOnlyCodeDescriptionPairList ExceptionCategories => WorkflowDataRegistry.Instance.ExceptionCategories.Value;
		public CodeDescriptionPairList JobTypes => (CodeDescriptionPairList)Factory.GetCachedValue(nameof(IWorkflowDescriptorList), () => ObjectFactory.Get<IWorkflowDescriptorList>());
	}
}
