using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class CusGoodsCatalogProductionInfoValidation : AutoCusGoodsCatalogProductionInfoValidation
	{
		public CusGoodsCatalogProductionInfoValidation(AutoCusGoodsCatalogProductionInfo parent)
			: base(parent)
		{
		}

		protected override void CheckCGI_Reference()
		{
			if (Parent.CGI_BFR_ForeignOperator.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.CGI_ReferenceInfo);
			}
		}
	}
}

