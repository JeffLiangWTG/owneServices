using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class NMFSHarvestingDetailWrapper : INMFSHarvestingDetail
	{
		public NMFSHarvestingDetailWrapper(bool is370ProgramType, IEnumerable<NMFSHarvestingDetail> details)
		{
			this.is370ProgramType = is370ProgramType;
			firstDetail = Argument.NotNull(details.FirstOrDefault(), "details should have at least one element");
			this.details = details;
		}
		readonly bool is370ProgramType;
		readonly NMFSHarvestingDetail firstDetail;
		readonly IEnumerable<NMFSHarvestingDetail> details;

		#region INMFSHarvestingDetail Members

		ZString INMFSHarvestingDetail.CountryCode
		{
			get { return firstDetail.US_HarvestedCountry; }
		}

		ZString INMFSHarvestingDetail.GeographicLocation
		{
			get { return firstDetail.US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture ? firstDetail.US_GeographicLocation : (is370ProgramType || firstDetail.US_OceanAreaOfCatchDesc_ReadOnly) ? firstDetail.US_OceanAreaOfCatch : firstDetail.US_OceanAreaOfCatchDesc; }
		}

		ZString INMFSHarvestingDetail.ProcessingTypeCode
		{
			get { return firstDetail.US_GearType; }
		}

		ZBool INMFSHarvestingDetail.ContainsYellowfinTuna
		{
			get { return firstDetail.US_ContainsYellowfinTuna; }
		}

		ZString INMFSHarvestingDetail.SourceTypeCode
		{
			get { return firstDetail.US_SourceType; }
		}

		ZDate INMFSHarvestingDetail.ProcessingStartDate
		{
			get { return firstDetail.US_GearStartDate.Date; }
		}

		ZString INMFSHarvestingDetail.ProcessingDescription
		{
			get { return firstDetail.US_GearDescription; }
		}

		ZString INMFSHarvestingDetail.ContactPartyType
		{
			get { return firstDetail.US_ContactPartyType; }
		}

		ZString INMFSHarvestingDetail.FirstLandingCountry
		{
			get { return firstDetail.US_FirstLandingCountry; }
		}

		ZInt INMFSHarvestingDetail.NumberOfVessels
		{
			get { return firstDetail.US_NoSmallVessels; }
		}

		IEnumerable<ZString> INMFSHarvestingDetail.HarvestingVessels
		{
			get { return details.Select(x => x.US_VesselCountry).OrderBy(x => x); }
		}

		IEnumerable<INMFSVessel> INMFSHarvestingDetail.Vessels
		{
			get { return firstDetail.HarvestingVessles.OfType<NMFSVessels>(); }
		}

		IPGAContactDetails INMFSHarvestingDetail.ContactPartyDetails
		{
			get { return firstDetail.ContactParty != null ? OrgHeaderWrapper.New(firstDetail.ContactParty) : null; }
		}

		#endregion
	}
}
