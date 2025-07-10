//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoPkgPackageContainerValidation
//
//    This class should be used for overriding validation in AutoPkgPackageContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Packing.Business
{
	public class PkgPackageContainerValidation : AutoPkgPackageContainerValidation
	{
		public PkgPackageContainerValidation(AutoPkgPackageContainer parent)
			: base(parent)
		{
		}

		public new PkgPackageContainer Parent
		{
			get { return (PkgPackageContainer)base.Parent; }
		}

		#region ValidateGoodsWeight

		public void ValidateGoodsWeight()
		{
			ValidateCalculatedProperty(Parent.GoodsWeightInfo);
		}

		protected virtual void CheckGoodsWeight()
		{
		}

		#endregion
	}
}
