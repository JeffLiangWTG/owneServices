using CargoWise.EntityFramework;
using Enterprise.Customs.Business.DocumentWrappers;

namespace Enterprise.Customs.US.Business
{
	public class NonABI3461Page : NonPersistentBusinessObject
	{
		public NonABI3461Page()
		{
			bills = new BusinessObjectCollectionWrapper<BillDocWrapper>();
			entryLines = new BusinessObjectCollectionWrapper<EntryLineDocWrapper>();
		}
		public BusinessObjectCollectionWrapper<BillDocWrapper> Bills => bills;
		readonly BusinessObjectCollectionWrapper<BillDocWrapper> bills;

		public BusinessObjectCollectionWrapper<EntryLineDocWrapper> EntryLines => entryLines;
		readonly BusinessObjectCollectionWrapper<EntryLineDocWrapper> entryLines;

		public void AddNewBill(BillDocWrapper bill)
		{
			bills.Add(bill);
		}

		public void AddNewEntryLine(EntryLineDocWrapper entryLine)
		{
			entryLines.Add(entryLine);
		}

		public const int BillCountMax = 2;
		public const int EntryLineCountMax = 4;
	}
}
