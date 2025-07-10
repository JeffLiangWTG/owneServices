using System;
using System.Globalization;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class BreakDownPackageValidation : ZValidation
	{
		public BreakDownPackageValidation(BreakDownPackage breakDownPackageBO)
			: base(breakDownPackageBO)
		{
			Parent = breakDownPackageBO;
		}

		readonly BreakDownPackage Parent;

		public override Type AutoValidationType
		{
			get { return typeof(BreakDownPackageValidation); }
		}

		#region ValidateSplitQuantity

		public void ValidateSplitQuantity()
		{
			ValidateCalculatedProperty(Parent.SplitQuantityInfo);
		}

		protected void CheckSplitQuantity()
		{
			if (Parent.SplitQuantity > Parent.MaxSplit)
			{
				Parent.SplitQuantityInfo.AddError(Res.GetString("1f350617-f66c-4bf1-9fa6-84c9c1bfaf6f",
					"You cannot break down Packages into a quantity greater than or equal to the highest number of items in each Package."));
			}
			else if (Parent.SplitQuantity > Int16.MaxValue)
			{
				Parent.SplitQuantityInfo.AddError(Res.GetString("8EE64A70-2C2A-4677-B1FB-984857E09515",
					"You cannot break down Packages into a quantity greater than {0}.", Int16.MaxValue.ToString(CultureInfo.InvariantCulture)));
			}
			else
			{
				MandatoryValidation.CheckNotNegative(Parent.SplitQuantityInfo);
				MandatoryValidation.CheckNotZero(Parent.SplitQuantityInfo);
			}
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidateSplitQuantity();
		}

		#endregion
	}
}

