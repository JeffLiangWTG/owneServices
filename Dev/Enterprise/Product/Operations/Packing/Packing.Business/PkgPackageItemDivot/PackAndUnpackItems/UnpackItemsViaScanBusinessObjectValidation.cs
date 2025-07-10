using CargoWise.ComponentModel;
using CargoWise.ResourceStrings.Grammar;

namespace Enterprise.Packing.Business
{
	public class UnpackItemsViaScanBusinessObjectValidation : UnpackItemsBusinessObjectValidation
	{
		public UnpackItemsViaScanBusinessObjectValidation(UnpackItemsViaScanBusinessObject parent)
			: base(parent)
		{
		}

		protected new UnpackItemsViaScanBusinessObject Parent
		{
			get { return (UnpackItemsViaScanBusinessObject)base.Parent; }
		}

		#region ValidatePackageQtyToCreate

		protected override void CheckPackageQtyToCreateCore()
		{
			base.CheckPackageQtyToCreateCore();

			if (!Parent.PackageQtyToCreateInfo.HasErrors() && Parent.IsTUN && Parent.PackageQtyToCreate > Parent.TUNCodeAvailableCount)
			{
				var packageDesc = (Parent.TUNCodeAvailableCount == 1)
										? Res.GetString("75f46a99-9d2b-43a8-91a6-e2b50c009052", "{0} is", Parent.PackageTypeToCreate)
										: Res.GetString("c918bf7e-d125-4b22-90d9-a07bd742d204", "{0} are", Grammar.Instance.Pluralize(Parent.PackageTypeToCreate));

				Parent.PackageQtyToCreateInfo.AddError(Res.GetString("f67ca1d6-0a6a-4e92-a747-d24393712c05", "Only {0} {1} available for Unpack.",
					Parent.TUNCodeAvailableCount, packageDesc));
			}
		}

		#endregion

		#region ValidatePackageTypeToCreate

		protected override bool RunValidatePackageTypeToCreate
		{
			get { return Parent.IsTUN; }
		}

		#endregion
	}
}
