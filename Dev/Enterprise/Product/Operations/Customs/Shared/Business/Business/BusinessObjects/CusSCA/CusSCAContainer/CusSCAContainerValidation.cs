//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusSCAContainerValidation
//
//    This class should be used for overriding validation in AutoCusSCAContainerValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusSCAContainerValidation : AutoCusSCAContainerValidation
	{
		public CusSCAContainerValidation(AutoCusSCAContainer parent) : base(parent)
		{
		}

		protected override void CheckCN_ContainerNumber()
		{
			base.CheckCN_ContainerNumber();

			CheckCFSOrgValidation();
			CheckCTOOrgValidation();
		}

		protected virtual void CheckCFSOrgValidation()
		{
		}

		protected virtual void CheckCTOOrgValidation()
		{
		}
	}
}
