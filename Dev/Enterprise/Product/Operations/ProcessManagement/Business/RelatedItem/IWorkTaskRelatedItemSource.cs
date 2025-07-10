using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Integration;

namespace Enterprise.ProcessManagement.Business
{
	public interface IWorkTaskRelatedItemSource : IBusiness
	{
		WorkTaskRelatedItemCollection RelatedItems { get; }
		IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules { get; }
		void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem);
		ZBool ShowOnlyNonClosedItems { get; set; }
		FilteredWorkTaskRelatedItemCollection FilteredRelatedItems { get; }
		ZBool ShouldAddRelatedItemAsParent { get; set; }
	}
}
