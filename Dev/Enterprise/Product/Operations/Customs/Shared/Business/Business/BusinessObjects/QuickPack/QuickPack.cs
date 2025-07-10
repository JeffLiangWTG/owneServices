using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class QuickPack : NonPersistentBusinessObject
	{
		public QuickPack(CusPackableItemCollection packableItems)
			: base(packableItems.Factory)
		{
			PackableItems = Argument.NotNull(packableItems, nameof(packableItems));
		}

		public readonly CusPackableItemCollection PackableItems;

		[ChildEditable]
		public QuickPackItemCollection QuickPackItems
		{
			get
			{
				if (fQuickPackItems == null)
				{
					fQuickPackItems = new QuickPackItemCollection(PackableItems);
					RegisterEditableChildObject(fQuickPackItems);
					fQuickPackItems.Load();
				}
				return fQuickPackItems;
			}
		}
		QuickPackItemCollection fQuickPackItems;

		public void DoQuickPackAction()
		{
			var newPackageMarksAndNumbers = new HashSet<ZString>();
			QuickPackItems.Cast<QuickPackItem>().ForEach(c => c.DoQuickPackAction(newPackageMarksAndNumbers));
		}
	}
}
