using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public class DebtorToSelectFromForPrintingCollection : NonPersistentBusinessObjectCollection<DebtorToSelectFromForPrinting>
	{
		public DebtorToSelectFromForPrintingCollection(OrgHeaderCollection collection)
			: base(collection.Factory)
		{
			foreach (OrgHeader debtor in collection)
			{
				Add(new DebtorToSelectFromForPrinting(debtor));
			}
			fCollection = collection;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		readonly OrgHeaderCollection fCollection;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DebtorToSelectFromForPrinting(fCollection.AddNew());
		}
	}
}
