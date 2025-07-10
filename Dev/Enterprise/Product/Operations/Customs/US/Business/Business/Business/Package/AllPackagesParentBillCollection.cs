using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// This contains all packages of child bills of a passed parent bill
	/// </summary>
	public class AllPackagesParentBillCollection : BusinessObjectCollectionView<Package>
	{
		public AllPackagesParentBillCollection(Bill parentBill, JobDeclaration declaration)
			: base(declaration.Packages)
		{
			this.parentBill = parentBill;
			Rebuild();
		}

		readonly Bill parentBill;

		protected override void RebuildOnConstruction()
		{
			//parentBill is null at this point
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			Package package = (Package)element;

			return package.PackingGroup != null &&
				package.PackingGroup.Bill != null &&
				(
					package.PackingGroup.Bill == parentBill ||
					package.PackingGroup.Bill.IsThisAChildOf(parentBill)
				);
		}
	}
}
