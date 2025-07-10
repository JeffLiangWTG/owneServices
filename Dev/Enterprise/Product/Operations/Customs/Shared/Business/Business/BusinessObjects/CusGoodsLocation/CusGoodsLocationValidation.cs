using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusGoodsLocationValidation : AutoCusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(AutoCusGoodsLocation parent)
			: base(parent)
		{
		}

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();
			ListValidation.ErrorIfInvalidCode(Parent.CGL_TypeInfo);
		}

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();
			ListValidation.ErrorIfInvalidCode(Parent.CGL_QualifierInfo);
		}
	}
}
