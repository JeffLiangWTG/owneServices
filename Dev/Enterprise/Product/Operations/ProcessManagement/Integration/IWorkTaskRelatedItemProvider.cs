using CargoWise.EntityFramework;

namespace Enterprise.ProcessManagement.Integration
{
	public interface IWorkTaskRelatedItemProvider
	{
		IBusinessObjectCollection RelatedItems { get; }
	}
}
