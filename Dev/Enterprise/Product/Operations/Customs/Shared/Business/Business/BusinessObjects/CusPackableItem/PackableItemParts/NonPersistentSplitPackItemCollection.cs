using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class NonPersistentSplitPackItemCollection : NonPersistentBusinessObjectCollection<NonPersistentSplitPackItem>
	{
		public NonPersistentSplitPackItemCollection(CusPackageCusPackableItemRelation packageCusPackableItemRelation, PackableItemsSplitter splitter)
			: base(packageCusPackableItemRelation.Factory)
		{
			this.packageCusPackableItemRelation = packageCusPackableItemRelation;
			this.splitter = splitter;
		}
		readonly CusPackageCusPackableItemRelation packageCusPackableItemRelation;
		readonly PackableItemsSplitter splitter;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var newItem = new NonPersistentSplitPackItem(packageCusPackableItemRelation, splitter);
			SetDefaultUQ(newItem);
			splitter.PackableItemPartSequenceNumberGenerator.RecalculateWhenAdded(newItem);
			return newItem;
		}

		void SetDefaultUQ(NonPersistentSplitPackItem newItem)
		{
			if (this.Any())
			{
				var firstItem = this.Cast<NonPersistentSplitPackItem>().OrderByDescending(item => item.Sequence).First();
				newItem.PackableUQ = firstItem.PackableUQ;
				newItem.NetWeightUQ = firstItem.NetWeightUQ;
			}
			else
			{
				newItem.PackableUQ = packageCusPackableItemRelation.PackableUQ;
				newItem.NetWeightUQ = packageCusPackableItemRelation.InvoiceLineNetWeightUQ;
			}
		}
	}
}
