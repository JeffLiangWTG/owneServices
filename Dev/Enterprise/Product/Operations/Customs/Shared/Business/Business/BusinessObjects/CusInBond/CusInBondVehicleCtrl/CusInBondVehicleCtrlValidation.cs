//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusInBondVehicleCtrlValidation
//
//    This class should be used for overriding validation in AutoCusInBondVehicleCtrlValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.Business
{
	public class CusInBondVehicleCtrlValidation : AutoCusInBondVehicleCtrlValidation
	{
		public CusInBondVehicleCtrlValidation(AutoCusInBondVehicleCtrl parent)
			: base(parent)
		{
		}
	}
}
