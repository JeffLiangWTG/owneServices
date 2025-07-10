using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
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
	public class FishingInformation : AutoNMFSHarvestingDetail
	{
		public FishingInformation(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FishingInformation|US_MethodOfHarvest", Caption = "Method of Harvest")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSHarvestingDetailAddInfoLookups.HarvestedMethods))]
		public override ZString US_MethodOfHarvest
		{
			get { return base.US_MethodOfHarvest; }
			set
			{
				var oldValue = base.US_MethodOfHarvest;
				base.US_MethodOfHarvest = value;
				if (oldValue != value)
				{
					AddInfoValidation.ValidateWhenUS_MethodOfHarvestChanged();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FishingInformation|US_VesselName", Caption = "Vessel Name")]
		public override ZString US_VesselName
		{
			get { return base.US_VesselName; }
			set
			{
				base.US_VesselName = value;
				var vessel = Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, US_VesselName);
				if (vessel != null)
				{
					US_VesselCountry = vessel.RV_RN_NKCountryOfReg;
					US_VesselIMO = vessel.RV_LloydsNumber;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FishingInformation|US_VesselCountry", Caption = "Vessel Flag")]
		public override ZString US_VesselCountry
		{
			get { return base.US_VesselCountry; }
			set { base.US_VesselCountry = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FishingInformation|US_HarvestedCountry", Caption = "Country/Region of Harvest")]
		public override ZString US_HarvestedCountry
		{
			get { return base.US_HarvestedCountry; }
			set { base.US_HarvestedCountry = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.FishingInformation|US_VesselIMO", Caption = "Vessel IMO")]
		public override ZString US_VesselIMO
		{
			get { return base.US_VesselIMO; }
			set { base.US_VesselIMO = value; }
		}

		public bool IsVesselMethod => US_MethodOfHarvest == SourceTypeCodesList.Codes.Vessel;

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsDeleted && AllFieldIsEmpty)
			{
				Delete();
			}
		}

		bool AllFieldIsEmpty => GetUsedFieldsInfos().All(x => x.Value.IsEmpty);

		IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			yield return US_VesselCountryInfo;
			yield return US_HarvestedCountryInfo;
			yield return US_MethodOfHarvestInfo;
			yield return US_VesselIMOInfo;
			yield return US_VesselNameInfo;
		}

		public new FishingInformationAddInfoValidation AddInfoValidation
		{
			get { return (FishingInformationAddInfoValidation)base.AddInfoValidation; }
		}
	}
}
