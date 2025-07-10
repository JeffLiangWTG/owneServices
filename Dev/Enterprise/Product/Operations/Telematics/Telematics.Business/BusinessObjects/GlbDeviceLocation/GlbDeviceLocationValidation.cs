//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoGlbDeviceLocationValidation
//
//    This class should be used for overriding validation in AutoGlbDeviceLocationValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;

namespace Enterprise.Telematics.Business
{
	public class GlbDeviceLocationValidation : AutoGlbDeviceLocationValidation
	{
		public GlbDeviceLocationValidation(AutoGlbDeviceLocation parent)
			: base(parent)
		{
		}

		protected override void CheckV2_SpeedLimitState()
		{
			base.CheckV2_SpeedLimitState();
			if (!Parent.Lookups.SpeedLimitStateList.ContainsCode(Parent.V2_SpeedLimitState))
			{
				ListValidation.ErrorIfInvalidCode(Parent.V2_SpeedLimitStateInfo);
			}
		}
	}
}
