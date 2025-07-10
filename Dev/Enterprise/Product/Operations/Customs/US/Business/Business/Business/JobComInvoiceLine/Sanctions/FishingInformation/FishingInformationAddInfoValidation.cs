using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class FishingInformationAddInfoValidation : USNMFSHarvestingDetailAddInfoValidation
	{
		public FishingInformationAddInfoValidation(USNMFSHarvestingDetailAddInfo parent)
			: base(parent)
		{
		}

		public void ValidateFishingInformation()
		{
			ValidateUS_MethodOfHarvest();
			ValidateUS_VesselName();
			ValidateUS_VesselCountry();
			ValidateUS_VesselIMO();
			ValidateUS_HarvestedCountry();
		}

		public void ValidateWhenUS_MethodOfHarvestChanged()
		{
			ValidateUS_VesselName();
			ValidateUS_VesselCountry();
			ValidateUS_VesselIMO();
		}

		protected override void CheckUS_MethodOfHarvest()
		{
			base.CheckUS_MethodOfHarvest();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_MethodOfHarvestInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.US_MethodOfHarvestInfo, Parent.Lookups.HarvestedMethods);
		}

		protected override void CheckUS_VesselName()
		{
			base.CheckUS_VesselName();
			if (Fishing.IsVesselMethod)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VesselNameInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_VesselNameInfo, Parent.Lookups.RefVessels);
		}

		protected override void CheckUS_VesselCountry()
		{
			base.CheckUS_VesselCountry();
			if (Fishing.IsVesselMethod)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VesselCountryInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.US_VesselCountryInfo, Parent.Lookups.Countries);
		}

		protected override void CheckUS_VesselIMO()
		{
			base.CheckUS_VesselIMO();
			if (Fishing.IsVesselMethod)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_VesselIMOInfo);
			}
		}

		protected override void CheckUS_HarvestedCountry()
		{
			base.CheckUS_HarvestedCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_HarvestedCountryInfo, Parent.Lookups.Countries);
		}

		protected new USNMFSHarvestingDetailAddInfo Parent
		{
			get { return (USNMFSHarvestingDetailAddInfo)base.Parent; }
		}

		protected FishingInformation Fishing
		{
			get { return (FishingInformation)Parent.Parent; }
		}
	}
}
