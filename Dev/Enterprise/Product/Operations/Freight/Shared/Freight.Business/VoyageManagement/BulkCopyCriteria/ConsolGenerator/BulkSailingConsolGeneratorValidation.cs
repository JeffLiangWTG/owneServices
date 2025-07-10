using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class BulkSailingConsolGeneratorValidation : AutoBulkSailingConsolGeneratorValidation
	{
		public BulkSailingConsolGeneratorValidation(AutoBulkSailingConsolGenerator parent)
			: base(parent) { }

		protected override void CheckVolumeUnit()
		{
			base.CheckVolumeUnit();
			MandatoryValidation.CheckEntered(Parent.VolumeUnitInfo);
			ListValidation.ErrorIfInvalidCode(Parent.VolumeUnitInfo, Parent.Lookups.VolumeUnitList);
		}

		protected override void CheckWeightUnit()
		{
			base.CheckWeight();
			MandatoryValidation.CheckEntered(Parent.WeightUnitInfo);
			ListValidation.ErrorIfInvalidCode(Parent.WeightUnitInfo, Parent.Lookups.WeightUnitList);
		}

		#region Implementation

		public new BulkSailingConsolGenerator Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (BulkSailingConsolGenerator)base.Parent; }
		}

		#endregion
	}
}
