using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.MasterFiles.Business
{
	public class CreateItemsFromTemplateResult
	{
		public CreateItemsFromTemplateResult(bool shouldPreventFallback)
		{
			ShouldPreventFallback = shouldPreventFallback;
		}

		public CreateItemsFromTemplateResult(ICollection<TemplateItemApplication> createdItems)
		{
			this.createdItems = Argument.NotNull(createdItems, nameof(createdItems));
			ShouldPreventFallback = createdItems.Any();
		}

		readonly ICollection<TemplateItemApplication> createdItems;

		public bool ShouldPreventFallback { get; }
		public IEnumerable<TemplateItemApplication> CreatedItems => createdItems ?? Enumerable.Empty<TemplateItemApplication>();
	}
}
