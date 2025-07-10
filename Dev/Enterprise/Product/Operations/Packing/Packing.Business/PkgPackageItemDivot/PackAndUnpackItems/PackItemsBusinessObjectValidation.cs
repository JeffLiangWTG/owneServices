namespace Enterprise.Packing.Business
{
	public class PackItemsBusinessObjectValidation : ItemsBusinessObjectValidation
	{
		public PackItemsBusinessObjectValidation(PackItemsBusinessObject parent)
			: base(parent)
		{
		}

		protected new PackItemsBusinessObject Parent
		{
			get { return (PackItemsBusinessObject)base.Parent; }
		}

		#region ValidatePackageQtyToCreate

		protected override void CheckPackageQtyToCreateCore()
		{
			base.CheckPackageQtyToCreateCore();

			if (Parent.PackageQtyToCreate > PackItemsBusinessObject.MaxPackagesToCreate)
			{
				Parent.PackageQtyToCreateInfo.AddError(Res.GetString("4cf6cea8-0c8d-45bd-84ee-3bec7bcd6e77",
					"Cannot create more than {0} Packages at a time.", PackItemsBusinessObject.MaxPackagesToCreate));
			}
			else
			{
				ValidatePackageQtyForEachWrapperToUpdatePotentialWarning(); // eg. "There are 5 Boxes but only 4 TV(s). 1 Box will not contain a TV."
			}
		}

		#endregion

		#region ValidatePackageTypeToCreate

		protected override void CheckPackageTypeToCreateCore()
		{
			base.CheckPackageTypeToCreateCore();
			ValidatePackageQtyForEachWrapperToUpdatePotentialWarning();
		}

		#endregion

		#region ValidatePackageQtyForEachWrapperToUpdatePotentialWarning

		void ValidatePackageQtyForEachWrapperToUpdatePotentialWarning()
		{
			var packageDesc = Parent.PackageTypes.GetDescriptionFromCode(Parent.PackageTypeToCreate);

			foreach (var wrapper in Parent.PackableItemParentsForBinding)
			{
				wrapper.PackageTypeToCreateDescription = packageDesc;
				wrapper.PackageQtyToCreate = Parent.PackageQtyToCreate;
				wrapper.Validation.ValidateProposedPackQty();
			}
		}

		#endregion
	}
}
