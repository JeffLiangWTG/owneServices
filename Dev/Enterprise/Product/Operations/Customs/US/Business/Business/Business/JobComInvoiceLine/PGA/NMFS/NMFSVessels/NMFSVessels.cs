using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSNMFSVessels")]
	public class NMFSVessels : AutoUSNMFSVessels, INMFSVessel
	{
		public NMFSVessels(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NMFSHarvestingDetail Parent
		{
			get { return (NMFSHarvestingDetail)base.Parent; }
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_HarvestedCountry", Caption = "Vessel Country/Region", ShortCaption = "Vessel Ctry/Rgn.")]
		public override ZString US_HarvestedCountry
		{
			get { return base.US_HarvestedCountry; }
			set { base.US_HarvestedCountry = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_TranshipmentPlace", Caption = "Transhipment Place", ShortCaption = "Transhipment Place", FullDescription = "Transhipment")]
		public override ZString US_TranshipmentPlace
		{
			get { return base.US_TranshipmentPlace; }
			set { base.US_TranshipmentPlace = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_FirstLandingCountry", Caption = "First Landing Country/Region", ShortCaption = "First Landing")]
		public override ZString US_FirstLandingCountry
		{
			get { return base.US_FirstLandingCountry; }
			set { base.US_FirstLandingCountry = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_HarvestedVessel", Caption = "Harvested Vessel", ShortCaption = "Vessel")]
		public override ZString US_HarvestedVessel
		{
			get { return base.US_HarvestedVessel; }
			set
			{
				base.US_HarvestedVessel = value;
				var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, US_HarvestedVessel);
				if (vessel != null)
				{
					US_HarvestedCountry = vessel.RV_RN_NKCountryOfReg;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_NetWeight", Caption = "Net Weight", ShortCaption = "Weight")]
		public override ZDecimal US_NetWeight
		{
			get { return base.US_NetWeight; }
			set { base.US_NetWeight = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSVessels|US_NetWeightUQ", Caption = "Net Weight Unit", ShortCaption = "Weight Unit")]
		public override ZString US_NetWeightUQ
		{
			get { return base.US_NetWeightUQ; }
			set { base.US_NetWeightUQ = value; }
		}

		#endregion

		#region INMFSVessel Members

		ZString INMFSVessel.HarvestedCountry
		{
			get { return US_HarvestedCountry; }
		}

		ZString INMFSVessel.HarvestedVessel
		{
			get { return US_HarvestedVessel; }
		}

		ZString INMFSVessel.TranshipmentPlace
		{
			get { return US_TranshipmentPlace; }
		}

		ZString INMFSVessel.FirstLandingCountry
		{
			get { return US_FirstLandingCountry; }
		}

		ZString INMFSVessel.NetWeightUQ
		{
			get { return US_NetWeightUQ; }
		}

		ZDecimal INMFSVessel.NetWeight
		{
			get { return US_NetWeight; }
		}

		#endregion

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}
	}
}
