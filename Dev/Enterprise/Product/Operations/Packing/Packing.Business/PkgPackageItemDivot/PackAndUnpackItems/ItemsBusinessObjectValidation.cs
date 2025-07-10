using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Packing.Business
{
	public class ItemsBusinessObjectValidation : ZValidation
	{
		public ItemsBusinessObjectValidation(ItemsBusinessObject parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected readonly ItemsBusinessObject Parent;

		public override Type AutoValidationType
		{
			get { return typeof(ItemsBusinessObjectValidation); }
		}

		#region ValidatePackageQtyToCreate

		public void ValidatePackageQtyToCreate()
		{
			ValidateCalculatedProperty(Parent.PackageQtyToCreateInfo);
		}

		protected void CheckPackageQtyToCreate()
		{
			if (Parent.PackageQtyToCreate < 1)
			{
				Parent.PackageQtyToCreateInfo.AddError(Res.GetString("e2637b07-8381-4b9b-be2d-ddc0bd7d733f", "Package Qty cannot be less than 1."));
			}
			else
			{
				CheckPackageQtyToCreateCore();
			}
		}

		protected virtual void CheckPackageQtyToCreateCore()
		{
		}

		#endregion

		#region ValidatePackageTypeToCreate

		public void ValidatePackageTypeToCreate()
		{
			ValidateCalculatedProperty(Parent.PackageTypeToCreateInfo);
		}

		protected void CheckPackageTypeToCreate()
		{
			if (RunValidatePackageTypeToCreate)
			{
				MandatoryValidation.CheckEntered(Parent.PackageTypeToCreateInfo);
				if (!Parent.PackageTypeToCreateInfo.HasErrors())
				{
					ListValidation.ErrorIfInvalidCode(Parent.PackageTypeToCreateInfo);
				}
				if (!Parent.PackageTypeToCreateInfo.HasErrors())
				{
					CheckPackageTypeToCreateCore();
				}
			}
		}

		protected virtual void CheckPackageTypeToCreateCore()
		{
		}

		protected virtual bool RunValidatePackageTypeToCreate
		{
			get { return true; }
		}

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			ValidatePackageQtyToCreate();
			ValidatePackageTypeToCreate();
			ValidateIsAnythingSelectedToPackOrUnpack();
		}

		void ValidateIsAnythingSelectedToPackOrUnpack()
		{
			// if packing multiple items, it is valid to pack 0 for some items, *but not for all items*.
			var text = Res.GetString("48d1e58d-3d1f-41a5-9f1f-6f727e2b0478", "Nothing is selected to {0}.", Parent.ProposedPackUnpackDescription);
			Parent.RemoveRowError(text);

			if (!Parent.IsAnythingSelectedToPackOrUnpack)
			{
				Parent.AddRowError(text);
			}
		}

		#endregion
	}
}
