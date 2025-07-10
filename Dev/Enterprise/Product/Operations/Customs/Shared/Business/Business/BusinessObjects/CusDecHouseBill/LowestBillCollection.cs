using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface ILowestBillCollection<out TBill, out TDeclaration> : IBusinessObjectCollection
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		new TBill this[int index] { get; }
		new TBill AddNew();

		void Rebuild();
	}

	public class LowestBillCollection<TBill, TDeclaration> : BusinessObjectCollectionView<TBill>, ILowestBillCollection<TBill, TDeclaration>
		where TBill : Bill
		where TDeclaration : BaseJobDeclaration
	{
		public LowestBillCollection(TDeclaration declaration)
			: base((BusinessObjectCollection)declaration.Bills)
		{
			this.declaration = declaration;
		}

		protected readonly TDeclaration declaration;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			TBill bill = (TBill)element;
			return bill.IsLowestBill;
		}
	}
}
