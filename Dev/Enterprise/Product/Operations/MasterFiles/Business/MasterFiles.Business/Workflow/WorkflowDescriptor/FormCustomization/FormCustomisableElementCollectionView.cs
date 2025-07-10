using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class FormCustomisableElementCollectionView : BusinessObjectCollectionView<FormCustomisableElement>
	{
		public FormCustomisableElementCollectionView(FormCustomisableElementCollection collection)
			: base(collection)
		{
			this.collection = collection;
		}
		readonly FormCustomisableElementCollection collection;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			FormCustomisableElement customisableElement = element as FormCustomisableElement;
			return customisableElement != null && customisableElement.IsAvailable && customisableElement.IsInTemplate;
		}

		public override bool ReadOnly => base.ReadOnly || collection.ReadOnly;
	}
}
