//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSNMFSVesselsAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSNMFSVesselsAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class USNMFSVesselsAddInfoValidation : AutoUSNMFSVesselsAddInfoValidation
	{
		public USNMFSVesselsAddInfoValidation(AutoUSNMFSVesselsAddInfo parent) : base(parent)
		{
		}

		protected new USNMFSVesselsAddInfo Parent
		{
			get { return (USNMFSVesselsAddInfo)base.Parent; }
		}

		protected override void CheckUS_HarvestedVessel()
		{
			base.CheckUS_HarvestedVessel();

			if (IsHarvestOfCaptureFisheries)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HarvestedVesselInfo);
			}
		}

		protected override void CheckUS_HarvestedCountry()
		{
			base.CheckUS_HarvestedVessel();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_HarvestedCountryInfo, Parent.Lookups.RefCountries);

			if (IsHarvestOfCaptureFisheries)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_HarvestedCountryInfo);
			}
		}

		protected override void CheckUS_TranshipmentPlace()
		{
			base.CheckUS_HarvestedVessel();

			if (Parent.US_TranshipmentPlace != "ZZ")
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_TranshipmentPlaceInfo, Parent.Lookups.RefCountries);
			}

			if (IsHarvestOfCaptureFisheries)
			{
				if (Parent.US_TranshipmentPlace.IsEmpty && Parent.US_FirstLandingCountry.IsEmpty)
				{
					Parent.US_TranshipmentPlaceInfo.AddMessageError(TransshipmentPlaceOrFirstLandingCountryRequired);
				}
			}
		}

		protected override void CheckUS_FirstLandingCountry()
		{
			base.CheckUS_HarvestedVessel();

			if (Parent.US_FirstLandingCountry != "ZZ")
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_FirstLandingCountryInfo, Parent.Lookups.RefCountries);
			}

			if (IsHarvestOfCaptureFisheries)
			{
				if (Parent.US_TranshipmentPlace.IsEmpty && Parent.US_FirstLandingCountry.IsEmpty)
				{
					Parent.US_FirstLandingCountryInfo.AddMessageError(TransshipmentPlaceOrFirstLandingCountryRequired);
				}
			}
		}
		internal const string TransshipmentPlaceOrFirstLandingCountryRequired = "Either Transshipment Place or First Landing Country should be entered when Source Type is 'HCF'.";

		protected override void CheckUS_NetWeight()
		{
			base.CheckUS_NetWeight();

			if (IsHarvestOfCaptureFisheries && Parent.US_NetWeight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightInfo);
			}
			else
			{
				if (Parent.US_NetWeight < 0)
				{
					Parent.US_NetWeightInfo.AddMessageError(EnterNumberGreaterThanZero);
				}
			}
		}
		internal const string EnterNumberGreaterThanZero = "Please enter a number greater than 0.";

		protected override void CheckUS_NetWeightUQ()
		{
			base.CheckUS_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_NetWeightUQInfo, Parent.Lookups.UnitOfMeasureList);

			if (!Parent.US_NetWeight.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_NetWeightUQInfo);
			}
		}

		ZBool IsHarvestOfCaptureFisheries
		{
			get { return HarvestingDetail != null && HarvestingDetail.US_SourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries; }
		}

		NMFSVessels Vessel
		{
			get { return (NMFSVessels)Parent.Parent; }
		}

		NMFSHarvestingDetail HarvestingDetail
		{
			get { return Vessel.Parent; }
		}
	}
}
