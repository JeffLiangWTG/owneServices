using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class AllocateWeightValidation : AutoAllocateWeightValidation
	{
		public AllocateWeightValidation(AutoAllocateWeight parent) : base(parent)
		{
		}

		public new AllocateWeight Parent => (AllocateWeight)base.Parent;

		protected override void CheckNetWeight()
		{
			base.CheckNetWeight();
			MandatoryValidation.CheckNotNegative(Parent.NetWeightInfo);
		}

		protected override void CheckNetWeightUnit()
		{
			base.CheckNetWeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.NetWeightUnitInfo);
		}

		protected override void CheckGrossWeight()
		{
			base.CheckGrossWeight();
			MandatoryValidation.CheckNotNegative(Parent.GrossWeightInfo);
		}

		protected override void CheckGrossWeightUnit()
		{
			base.CheckGrossWeightUnit();
			ListValidation.ErrorIfInvalidCode(Parent.GrossWeightUnitInfo);
		}

		protected override void CheckAllocateWeightMethod()
		{
			base.CheckAllocateWeightMethod();
			ListValidation.ErrorIfInvalidCode(Parent.AllocateWeightMethodInfo);
			MandatoryValidation.CheckEntered(Parent.AllocateWeightMethodInfo);
		}
	}
}
