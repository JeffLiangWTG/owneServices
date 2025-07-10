using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class CustomsNumberViewStmNumsWrapperCollection : NonPersistentBusinessObjectCollection<CustomsNumberViewStmNumsWrapper>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CustomsNumberViewStmNumsWrapperCollection(CustomsNumberViewStmNumsCollection collection)
			: base(collection.Factory)
		{
			this.collection = collection;
			collection.CountChanged += Collection_CountChanged;
			collection.CollectionCountChange += Collection_CollectionCountChange;
			foreach (var stmNums in collection)
			{
				Add(stmNums);
			}
		}

		readonly CustomsNumberViewStmNumsCollection collection;

		public CustomsNumberViewStmNumsBusinessProvider Provider => collection.Provider;

		public override void Add(BusinessObject bizObj)
		{
			var stmNums = bizObj as CustomsNumberViewStmNums;
			if (stmNums != null)
			{
				bizObj = collection.Provider.GetOrCreateWrapper(stmNums);
			}
			base.Add(bizObj);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotImplementedException();
		}

		protected override bool AllowNewCore => false;
		protected override bool AllowRemoveCore => false;

		void Collection_CountChanged(object sender, EventArgs e)
		{
			var existingWrappers = this.OfType<CustomsNumberViewStmNumsWrapper>().ToList();
			foreach (var stmNum in collection)
			{
				var wrapper = existingWrappers.FirstOrDefault(x => x.StmNums == stmNum);
				if (wrapper == null)
				{
					Add(collection.Provider.GetOrCreateWrapper(stmNum));
				}
				else
				{
					existingWrappers.Remove(wrapper);
				}
			}
			existingWrappers.ForEach(x => Remove(x));
		}

		void Collection_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			var stmNum = (CustomsNumberViewStmNums)e.BizObject;
			if (stmNum != null)
			{
				if (e.ItemAdded)
				{
					Add(stmNum);
				}
				else if (e.ItemRemoved)
				{
					Remove(stmNum.PK);
				}
			}
		}
	}
}