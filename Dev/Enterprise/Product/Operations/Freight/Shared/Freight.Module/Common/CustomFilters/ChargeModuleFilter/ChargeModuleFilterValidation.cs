using System.Diagnostics;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Module
{
	public class ChargeModuleFilterValidation : AutoChargeModuleFilterValidation
	{
		public ChargeModuleFilterValidation(AutoChargeModuleFilter parent)
			: base(parent) { }

		protected override void CheckChargeGroup()
		{
			base.CheckChargeGroup();
			ListValidation.ErrorIfInvalidCode(Parent.ChargeGroupInfo, Parent.Lookups.ChargeGroups);
		}

		protected override void CheckLowerBound()
		{
			base.CheckLowerBoundIsValidZDecimal();

			if (BoundsOutOfOrder())
			{
				Parent.LowerBoundInfo.AddError(Res.GetString("ed902adf-62ac-4d75-a3d0-e1477f0dc728", "Lower bound cannot be greater than upper bound."));
			}
		}

		protected override void CheckUpperBound()
		{
			base.CheckUpperBound();

			if (BoundsOutOfOrder())
			{
				Parent.UpperBoundInfo.AddError(Res.GetString("18145ed7-0e16-447f-8ccf-8f0886c4da7b", "Upper bound cannot be less than lower bound."));
			}
		}

		#region Implementation

		bool BoundsOutOfOrder()
		{
			ChargeModuleFilter parent = this.Parent;
			return parent.UseUpperBound && parent.UseLowerBound && parent.LowerBound > parent.UpperBound;
		}

		public new ChargeModuleFilter Parent
		{
			[DebuggerStepThrough]
			get { return (ChargeModuleFilter)base.Parent; }
		}

		#endregion
	}
}
