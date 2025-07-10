using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusGoodsCatalogValidation : AutoCusGoodsCatalogValidation
	{
		public CusGoodsCatalogValidation(AutoCusGoodsCatalog parent)
			: base(parent)
		{
		}

		protected override void CheckCGC_Description()
		{
			base.CheckCGC_Description();
			MandatoryValidation.CheckEntered(Parent.CGC_DescriptionInfo);
		}

		protected override void CheckCGC_Type()
		{
			base.CheckCGC_Type();
			MandatoryValidation.CheckEntered(Parent.CGC_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CGC_TypeInfo);
		}
	}
}
