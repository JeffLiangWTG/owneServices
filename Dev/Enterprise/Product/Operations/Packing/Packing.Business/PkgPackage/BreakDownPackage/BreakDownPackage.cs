using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Packing.Business
{
	public class BreakDownPackage : NonPersistentBusinessObject
	{
		public BreakDownPackage(IEnumerable<PkgPackage> selectedPackages)
		{
			SelectedPackages = selectedPackages;
			splitQuantity = 1;
		}

		public static string BreakDownPackageDescription
		{
			get { return Res.GetString("85d20ca5-e379-4909-88a3-c471e3983b78", "Break Down Package"); }
		}

		#region MaxSplit

		public int? MaxSplit
		{
			get
			{
				if (!maxSplit.HasValue)
				{
					maxSplit = SelectedPackages.Any() ? SelectedPackages.Max(pkg => pkg.KP_PackageQty) - 1 : 0;
				}

				return maxSplit;
			}
		}

		IEnumerable<PkgPackage> SelectedPackages
		{
			get;
			set;
		}

		int? maxSplit;

		#endregion

		#region SplitQuantity

		public ZInt SplitQuantity
		{
			get { return splitQuantity; }
			set
			{
				SetNonPersistentPropertyValue(SplitQuantityInfo, ref splitQuantity, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSplitQuantity();
				}
			}
		}

		ZInt splitQuantity;
		public ZPropertyInfo SplitQuantityInfo { get { return GetZPropertyInfo(nameof(SplitQuantity)); } }

		#endregion

		#region Validation

		public BreakDownPackageValidation Validation
		{
			get { return GetNewValidation(); }
		}

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		protected virtual BreakDownPackageValidation GetNewValidation()
		{
			return new BreakDownPackageValidation(this);
		}

		#endregion
	}
}

