//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSOMCAquacultureFacilityAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSOMCAquacultureFacilityAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;

namespace Enterprise.Customs.US.Business
{
	using CargoWise.ComponentModel;

	public class USOMCAquacultureFacilityAddInfoValidation : AutoUSOMCAquacultureFacilityAddInfoValidation
	{
		public USOMCAquacultureFacilityAddInfoValidation(AutoUSOMCAquacultureFacilityAddInfo parent) : base(parent)
		{
		}

		USOMCAquacultureFacility AquacultureFacility
		{
			get { return (USOMCAquacultureFacility)Parent.Parent; }
		}

		protected override void CheckUS_OA_AquacultureFacility()
		{
			base.CheckUS_OA_AquacultureFacility();
			if (AquacultureFacility != null)
			{
				var header = AquacultureFacility.Parent;
				if (header != null)
				{
					if (header.AquacultureFacilities.Cast<USOMCAquacultureFacility>()
							.Count(x => x.US_OA_AquacultureFacility == Parent.US_OA_AquacultureFacility) > 1)
					{
						Parent.US_OA_AquacultureFacilityInfo.AddMessageError(AquacultureFacilityRepeat);
					}
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_AquacultureFacilityInfo, AquacultureFacility.AquacultureFacilityAddress);
				}
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_AquacultureFacilityInfo, AquacultureFacility.AquacultureFacilityAddress);
			}
		}
		internal const string AquacultureFacilityRepeat = "Same AquacultureFacility cannot be reported.";
	}
}
