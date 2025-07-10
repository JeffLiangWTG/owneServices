using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business
{
	public class DA63AdditionalDutyLookups
		: Customs.Business.CusCodeDataLookups
	{
		public DA63AdditionalDutyLookups(DA63AdditionalDuty parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Factory.GetDA63PartList();

		protected new DA63AdditionalDuty Parent => (DA63AdditionalDuty)base.Parent;
	}
}
