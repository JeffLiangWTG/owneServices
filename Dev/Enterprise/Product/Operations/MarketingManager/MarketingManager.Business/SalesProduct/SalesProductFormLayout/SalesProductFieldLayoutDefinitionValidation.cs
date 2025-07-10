using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business
{
	public class SalesProductFieldLayoutDefinitionValidation : AutoSalesProductFieldLayoutDefinitionValidation
	{
		public SalesProductFieldLayoutDefinitionValidation(AutoSalesProductFieldLayoutDefinition parent)
			: base(parent)
		{
		}

		protected override void CheckGenCustomColumnDefinitionFk()
		{
			base.CheckGenCustomColumnDefinitionFk();
			MandatoryValidation.CheckEntered(Parent.GenCustomColumnDefinitionFkInfo);
			ListValidation.ErrorIfInvalidPK(Parent.GenCustomColumnDefinitionFkInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.GenCustomColumnDefinitionFkInfo);
		}

		protected override void CheckOrder()
		{
			base.CheckOrder();
			MandatoryValidation.CheckEntered(Parent.OrderInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.OrderInfo);
		}
	}
}
