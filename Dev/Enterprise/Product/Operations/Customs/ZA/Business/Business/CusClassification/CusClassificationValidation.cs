using CargoWise.EntityFramework;

namespace Enterprise.Customs.ZA.Business
{
	public class CusClassificationValidation : Customs.Business.CusClassificationValidation
	{
		public CusClassificationValidation(CusClassification parent)
			: base(parent)
		{
		}

		public new CusClassification Parent
		{
			get { return base.Parent as CusClassification; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}
	}
}
