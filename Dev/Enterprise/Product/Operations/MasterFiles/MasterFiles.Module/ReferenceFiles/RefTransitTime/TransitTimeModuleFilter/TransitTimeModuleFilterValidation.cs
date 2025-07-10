using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Module
{
	public class TransitTimeModuleFilterValidation : ModuleFilterValidation
	{
		public TransitTimeModuleFilterValidation(TransitTimeModuleFilter parent)
			: base(parent)
		{
			this.parent = Argument.NotNull(parent, "parent");
		}

		readonly TransitTimeModuleFilter parent;

		public void ValidateComparisonOperator()
		{
			ValidateCalculatedProperty(parent.ComparisonOperatorInfo);
		}

		protected virtual void CheckComparisonOperator()
		{
			MandatoryValidation.CheckEntered(parent.ComparisonOperatorInfo);
			ListValidation.ErrorIfInvalidCode(parent.ComparisonOperatorInfo);
		}

		public void ValidateTransitDays()
		{
			ValidateCalculatedProperty(parent.TransitDaysInfo);
		}

		protected virtual void CheckTransitDays()
		{
			MandatoryValidation.CheckNotNegative(parent.TransitDaysInfo);
		}

		public void ValidateTransitHours()
		{
			ValidateCalculatedProperty(parent.TransitHoursInfo);
		}

		protected virtual void CheckTransitHours()
		{
			MandatoryValidation.CheckNotNegative(parent.TransitHoursInfo);

			if (parent.TransitHours >= 24)
			{
				parent.TransitHoursInfo.AddError(Res.GetString("c4051483-e9bb-4a38-ad3e-ec19c6b7203f", "Hours must be less than 24."));
			}
		}

		public override void ValidateAll()
		{
			ValidateTransitDays();
			ValidateTransitHours();
		}

		public override System.Type AutoValidationType
		{
			get { return GetType(); }
		}
	}
}
