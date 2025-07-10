using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IBillTypeViewCollection<out TBill, out TDeclaration> : IBusinessObjectCollection
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		new TBill this[int index] { get; }
		new TBill AddNew();

		ZString FilterBy { get; set; }
	}

	public class BillTypeViewCollection<TBill, TDeclaration> : BusinessObjectCollectionView<TBill>, IBillTypeViewCollection<TBill, TDeclaration>
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		public BillTypeViewCollection(TDeclaration declaration)
			: base((BusinessObjectCollection)declaration.Bills)
		{
			this.declaration = declaration;
			fFilterBy = BillFilterByList.Codes.All;
			Rebuild();
		}

		protected readonly TDeclaration declaration;

		public ZString FilterBy
		{
			get => fFilterBy;
			set
			{
				bool hasChanged = FilterBy != value;
				fFilterBy = value;
				if (hasChanged)
				{
					Rebuild();
				}
			}
		}
		ZString fFilterBy;

		protected override void RebuildOnConstruction()
		{
			// do not want to do this at this point
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			TBill bill = (TBill)element;

			if (FilterBy == BillFilterByList.Codes.LowestBills)
			{
				return bill.IsLowestBill;
			}
			return FilterBy == BillFilterByList.Codes.All || bill.CU_BillType == FilterBy;
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			TBill newChild = (TBill)child;

			if (FilterBy != BillFilterByList.Codes.LowestBills && FilterBy != BillFilterByList.Codes.All)
			{
				newChild.CU_BillType = FilterBy;
			}
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			TBill newChild = (TBill)child;
			newChild.CU_JE = declaration.PK;
		}
	}
}
