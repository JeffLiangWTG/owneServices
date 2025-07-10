namespace Enterprise.Packing.Business
{
	public class UnpackItemsBusinessObjectValidation : ItemsBusinessObjectValidation
	{
		public UnpackItemsBusinessObjectValidation(UnpackItemsBusinessObject parent)
			: base(parent)
		{
		}

		protected new UnpackItemsBusinessObject Parent
		{
			get { return (UnpackItemsBusinessObject)base.Parent; }
		}

		#region ValidatePackageTypeToCreate

		protected override bool RunValidatePackageTypeToCreate
		{
			get { return false; }
		}

		#endregion
	}
}
