//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSFDAAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSFDAAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class USFDAAddInfoValidation : AutoUSFDAAddInfoValidation
	{
		public USFDAAddInfoValidation(AutoUSFDAAddInfo parent)
			: base(parent)
		{
			if (!(parent is USFDAAddInfo))
			{
				throw new ArgumentException("Parent should be FDAAddInfo");
			}
		}

		FDA FDA
		{
			get { return ((USFDAAddInfo)Parent).Parent; }
		}

		protected override void CheckUS_OA_FDAFEI()
		{
			base.CheckUS_OA_FDAFEI();
			var fda = FDA;
			if (!fda.US_OA_FDAFEI.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_OA_FDAFEIInfo, fda.FDAFEIAddress);
			}
		}

		internal static string UnknownFDAProductCode
		{
			get { return Res.GetString("4E6B5DDA-963B-4867-95D0-31F47F670D7D", "FDA Product Code is unable to be found. Please ensure that it is valid."); }
		}

		protected override void CheckUS_FDAManufacturerAddress()
		{
			base.CheckUS_FDAManufacturerAddress();

			var fda = FDA;
			if (!fda.US_FDAManufacturerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_FDAManufacturerAddressInfo, fda.ManufacturerAddress);
			}
		}

		protected override void CheckUS_FDAShipperAddress()
		{
			base.CheckUS_FDAShipperAddress();
			var fda = FDA;
			if (!fda.US_FDAShipperAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(fda.US_FDAShipperAddressInfo, fda.ShipperAddress);
			}
		}
	}
}
