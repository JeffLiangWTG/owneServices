using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public abstract class BaseCusLinkPackageValidation : ZValidation
	{
		protected BaseCusLinkPackageValidation(BaseCusLinkPackage package)
			: base(package)
		{
			Parent = Argument.NotNull(package, nameof(package));
		}

		protected BaseCusLinkPackage Parent { get; }

		#region ValidateIsLinked

		public void ValidateIsLinked()
		{
			ValidateCalculatedProperty(Parent.IsLinkedInfo);
		}

		protected virtual void CheckIsLinked()
		{
		}

		#endregion

		#region ValidatePackQty

		public void ValidatePackQty()
		{
			ValidateCalculatedProperty(Parent.PackQtyInfo);
		}

		protected virtual void CheckPackQty()
		{
		}

		public void ValidateQuantity()
		{
			ValidateCalculatedProperty(Parent.QuantityInfo);
		}

		protected virtual void CheckQuantity()
		{
		}

		#endregion

		#region Override

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateIsLinked();
				ValidatePackQty();
				ValidateQuantity();
			}
		}

		#endregion
	}
}
