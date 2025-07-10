using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class AESEntryCollection : BusinessObjectCollectionView<CusEntryHeader>
	{
		public AESEntryCollection(JobDeclaration declaration)
			: base(declaration.CustomsEntryHeaders)
		{
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var entry = (CusEntryHeader)element;
			return entry.IsActive || (entry.HasBeenLodgedAtCustoms && !entry.HasBeenWithdrawn);
		}
	}
}
