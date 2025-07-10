using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class QuickPackItemCollection : NonPersistentBusinessObjectCollection<QuickPackItem>
	{
		public QuickPackItemCollection(CusPackableItemCollection packableItems)
			: base(packableItems.Factory)
		{
			this.packableItems = packableItems;
		}
		readonly CusPackableItemCollection packableItems;

		#region override
		public override void Load()
		{
			RemoveAll();

			var packableItems = this.packableItems.Cast<CusPackableItem>().Where(c => c.NotPackedQty > 0);
			foreach (var packableItem in packableItems)
			{
				AddPackableItems(packableItem);
			}
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new QuickPackItem(packableItems.First());
		}

		#endregion

		void AddPackableItems(CusPackableItem packableItem)
		{
			var quickPack = new QuickPackItem(packableItem);
			quickPack.PackedQty = packableItem.NotPackedQty;
			Add(quickPack);
		}
	}
}
