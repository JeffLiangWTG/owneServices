using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSNMFSHarvestingDetail")]
	public class NMFSHarvestingDetail : AutoNMFSHarvestingDetail, ICusAddInfoTypeSupporter
	{
		public NMFSHarvestingDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoNMFSHarvestingDetail.Schema
		{
			public const string US_GearTypeDesc = "US_GearTypeDesc";
			public const string US_FirstLandingCountry = "US_FirstLandingCountry";
			public const string ContactPartyOrgPK = "ContactPartyOrgPK";
		}

		public new NMFSLine Parent
		{
			get { return (NMFSLine)base.Parent; }
		}

		JobComInvoiceLine InvoiceLine
		{
			get { return (Parent.Parent as JobComInvoiceLine); }
		}

		JobComInvoiceHeader InvoiceHeader
		{
			get { return InvoiceLine?.InvoiceHeader; }
		}

		#region Flags

		public bool Is370ProgramType
		{
			get
			{
				var parent = Parent;
				return parent != null && parent.Is370ProgramType;
			}
		}

		public bool IsSIMPProgramType
		{
			get
			{
				var parent = Parent;
				return parent != null && parent.IsSIMProgramType;
			}
		}

		public bool IsCOAProgramType
		{
			get
			{
				var parent = Parent;
				return parent != null && parent.IsCOAProgramType;
			}
		}

		#endregion

		#region New Properties

		[ChildEditable(true)]
		public NMFSVesselsCollection HarvestingVessles
		{
			get
			{
				if (nmfsHarvestingVessels == null)
				{
					nmfsHarvestingVessels = new NMFSVesselsCollection(this);
					nmfsHarvestingVessels.Load();
					RegisterEditableChildObject(nmfsHarvestingVessels);
				}
				return nmfsHarvestingVessels;
			}
		}
		NMFSVesselsCollection nmfsHarvestingVessels;

		public ZString US_SourceType
		{
			get { return Parent?.US_SourceType ?? ZString.Empty; }
		}

		#endregion

		#region Override Properties

		protected override ZString HumanReadableNameCore
		{
			get { return "Harvesting Detail"; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_GearType", Caption = "Harvesting Gear Type", MediumCaption = "Gear Type", ShortCaption = "Gear", FullDescription = "The fishing vessel's harvesting gear type.")]
		public override ZString US_GearType
		{
			get { return base.US_GearType; }
			set { base.US_GearType = value; }
		}

		#region US_GearTypeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_GearTypeDesc", Caption = "Harvesting Gear Type Description", ShortCaption = "Gear Type Desc.")]
		public ZString US_GearTypeDesc
		{
			get { return AddInfoLookups.GearTypeList.GetDescriptionFromCode(US_GearType); }
		}

		public ZPropertyInfo US_GearTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_GearTypeDesc); }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_HarvestedCountry", Caption = "Harvested Country/Region", ShortCaption = "Harvested", FullDescription = "The Country/Region’s territorial waters the harvest took place in.")]
		public override ZString US_HarvestedCountry
		{
			get { return base.US_HarvestedCountry; }
			set { base.US_HarvestedCountry = value; }
		}

		[ReadOnlyMember(nameof(US_ContainsYellowfinTuna_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_ContainsYellowfinTuna", Caption = "Contains Yellowfin Tuna", MediumCaption = "Yellowfin Tuna", ShortCaption = "Yellowfin")]
		public override ZBool US_ContainsYellowfinTuna
		{
			get { return base.US_ContainsYellowfinTuna; }
			set { base.US_ContainsYellowfinTuna = value; }
		}

		bool US_ContainsYellowfinTuna_ReadOnly
		{
			get { return !Is370ProgramType; }
		}

		[ReadOnlyMember(nameof(US_OceanAreaOfCatch_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_OceanAreaOfCatch", Caption = "Ocean Area Of Catch", MediumCaption = "Catch Area", ShortCaption = "Area")]
		public override ZString US_OceanAreaOfCatch
		{
			get { return base.US_OceanAreaOfCatch; }
			set
			{
				var oldValue = US_OceanAreaOfCatch;
				base.US_OceanAreaOfCatch = value;
				if (oldValue != value && !IsCopying)
				{
					if (US_OceanAreaOfCatchDesc_ReadOnly)
					{
						US_OceanAreaOfCatchDesc = ZString.Empty;
					}
					US_OceanAreaOfCatchDescInfo.RefreshBinding();
				}
			}
		}

		bool US_OceanAreaOfCatch_ReadOnly
		{
			get { return US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture; }
		}

		#region US_OceanAreaOfCatchDesc

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_OceanAreaOfCatchDesc", Caption = "Ocean Area Of Catch Description", MediumCaption = "Catch Area Desc.", ShortCaption = "Area Desc.")]
		public override ZString US_OceanAreaOfCatchDesc
		{
			get
			{
				var result = AddInfoLookups.OceanAreaCodeList.GetDescriptionFromCode(US_OceanAreaOfCatch);
				if (!US_OceanAreaOfCatchDesc_ReadOnly)
				{
					result = base.US_OceanAreaOfCatchDesc;
				}
				return result;
			}
			set { base.US_OceanAreaOfCatchDesc = value; }
		}

		public bool US_OceanAreaOfCatchDesc_ReadOnly
		{
			get { return US_OceanAreaOfCatch != OceanGeographicAreaCodeList.Codes.OTH; }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_VesselCountry", Caption = "Vessel Country/Region", MediumCaption = "Vessel Flag", ShortCaption = "Flag", FullDescription = "Flag (Country/Region) of Registry of the Harvesting Vessel")]
		public override ZString US_VesselCountry
		{
			get { return base.US_VesselCountry; }
			set { base.US_VesselCountry = value; }
		}

		[ReadOnlyMember(nameof(US_GearStartDate_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_GearStartDate", Caption = "Process Start Date", MediumCaption = "Start Date", ShortCaption = "Date")]
		public override ZDateTime US_GearStartDate
		{
			get { return base.US_GearStartDate; }
			set { base.US_GearStartDate = value; }
		}

		bool US_GearStartDate_ReadOnly
		{
			get { return !(IsSIMPProgramType || IsCOAProgramType); }
		}

		[ReadOnlyMember(nameof(US_ContactPartyType_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_ContactPartyType", Caption = "Contact Party Role", ShortCaption = "Role")]
		public override ZString US_ContactPartyType
		{
			get { return base.US_ContactPartyType; }
			set
			{
				base.US_ContactPartyType = value;
				if (InvoiceLine != null)
				{
					if (value == EntityRoleCodeList.Codes.AquacultureFacility || value == EntityRoleCodeList.Codes.Producer)
					{
						US_OA_ContactParty = InvoiceLine.JI_OA_ManufacturerAddress;
					}
					else if (value == EntityRoleCodeList.Codes.Buyer)
					{
						US_OA_ContactParty = Factory.Load<OrgHeader>(InvoiceHeader.BuyerOrgPK)?.MainAddress.PK ?? ZGuid.Empty;
					}
					else if (value == EntityRoleCodeList.Codes.Consignee)
					{
						US_OA_ContactParty = InvoiceLine.JI_OA_ConsigneeAddress;
					}
					else if (value == EntityRoleCodeList.Codes.Exporter)
					{
						US_OA_ContactParty = InvoiceLine.JI_OA_ExporterAddress;
					}
					else if (value == EntityRoleCodeList.Codes.Consignor)
					{
						US_OA_ContactParty = InvoiceHeader.JZ_OA_SupplierAddress;
					}
				}

				ContactPartyOrgPK = ContactParty?.OA_OH ?? ZGuid.Empty;
			}
		}

		bool US_ContactPartyType_ReadOnly
		{
			get { return !(IsSIMPProgramType || IsCOAProgramType); }
		}

		[ReadOnlyMember(nameof(US_GearDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_GearDescription", Caption = "Processing Description", MediumCaption = "Processing Description", ShortCaption = "Processing Desc.")]
		public override ZString US_GearDescription
		{
			get { return base.US_GearDescription; }
			set { base.US_GearDescription = value; }
		}

		bool US_GearDescription_ReadOnly
		{
			get { return !(IsSIMPProgramType || IsCOAProgramType); }
		}

		#region US_OA_ContactParty_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ContactParty_ZAddress
		{
			get
			{
				if (fUS_OA_ContactParty_ZAddress == null)
				{
					fUS_OA_ContactParty_ZAddress = GetNewUS_OA_ContactParty_ZAddress();
					fUS_OA_ContactParty_ZAddress.IsOrgVisible = true;
					fUS_OA_ContactParty_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
					fUS_OA_ContactParty_ZAddress.AddresssListOverride = AddressListOverrider.ShowManufacturerIDInAddressList;
				}
				return fUS_OA_ContactParty_ZAddress;
			}
		}
		ZAddress fUS_OA_ContactParty_ZAddress;

		public void RefreshUS_OA_ContactParty_ZAddress()
		{
			fUS_OA_ContactParty_ZAddress = null;
		}

		protected virtual ZAddress GetNewUS_OA_ContactParty_ZAddress()
		{
			return new ZAddress(US_OA_ContactPartyInfo);
		}

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingVessel, typeof(NMFSVessels));
			return result;
		}

		#endregion

		[List(nameof(US_OA_ContactParty_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ReadOnlyMember(nameof(US_OA_ContactParty_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_OA_ContactParty", Caption = "Party Address", ShortCaption = "Address")]
		public override ZGuid US_OA_ContactParty
		{
			get { return base.US_OA_ContactParty; }
			set
			{
				base.US_OA_ContactParty = value;
			}
		}

		bool US_OA_ContactParty_ReadOnly
		{
			get { return !(IsSIMPProgramType || IsCOAProgramType); }
		}

		#region ContactPartyOrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSHarvestingDetailAddInfoLookups.Organizations))]
		[ReadOnlyMember(nameof(ContactPartyOrgPK_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|ContactPartyOrgPK", Caption = "Contact Party", ShortCaption = "Party")]
		public ZGuid ContactPartyOrgPK
		{
			get { return US_OA_ContactParty_ZAddress.OrgPK; }
			set { US_OA_ContactParty_ZAddress.OrgPK = value; }
		}

		bool ContactPartyOrgPK_ReadOnly
		{
			get { return !(IsSIMPProgramType || IsCOAProgramType); }
		}

		public ZPropertyInfo ContactPartyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ContactPartyOrgPK, x => US_OA_ContactParty_ZAddress.OrgPKInfo); }
		}

		public OrgAddress ContactParty
		{
			get { return Factory.Load<OrgAddress>(US_OA_ContactParty); }
		}
		#endregion

		#endregion

		#region US_NoSmallVessels

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_RemarksText", Caption = "Number of Small Vessels", ShortCaption = "No. Small Vessels")]
		public override ZInt US_NoSmallVessels
		{
			get { return base.US_NoSmallVessels; }
			set { base.US_NoSmallVessels = value; }
		}

		internal bool US_NoSmallVessels_ReadOnly
		{
			get { return !IsSmallVesselHarvest; }
		}

		bool IsSmallVesselHarvest
		{
			get { return US_SourceType == SourceTypeCodesList.Codes.SmallVesselHarvest; }
		}

		#endregion

		#region US_FirstLandingCountry

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_FirstLandingCountry", Caption = "First Landing Country/Region", ShortCaption = "First Landing")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSHarvestingDetailAddInfoLookups.Countries))]
		[MaxLength(2)]
		public ZString US_FirstLandingCountry
		{
			get { return FirstHarvestingVessel.US_FirstLandingCountry; }
			set
			{
				FirstHarvestingVessel.US_FirstLandingCountry = value;
				Validation.ValidateUS_FirstLandingCountry();
				US_FirstLandingCountryInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_FirstLandingCountryInfo
		{
			get { return GetZPropertyInfo(Schema.US_FirstLandingCountry); }
		}

		internal bool US_FirstLandingCountry_ReadOnly
		{
			get { return !IsSmallVesselHarvest; }
		}

		NMFSVessels FirstHarvestingVessel
		{
			get
			{
				if (firstHarvestingVessel == null || firstHarvestingVessel.IsDeleted)
				{
					firstHarvestingVessel = HarvestingVessles.OfType<NMFSVessels>().FirstOrDefault();
				}

				if (firstHarvestingVessel == null)
				{
					firstHarvestingVessel = HarvestingVessles.AddNew();
				}

				return firstHarvestingVessel;
			}
		}
		NMFSVessels firstHarvestingVessel;

		[ReadOnlyMember(nameof(US_GeographicLocation_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSHarvestingDetail|US_GeographicLocation", Caption = "Geographic Location")]
		public override ZString US_GeographicLocation
		{
			get { return base.US_GeographicLocation; }
			set { base.US_GeographicLocation = value; }
		}

		bool US_GeographicLocation_ReadOnly
		{
			get { return US_SourceType != SourceTypeCodesList.Codes.HatcheryBasedAquaculture; }
		}

		#endregion

		#endregion

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		public new NMFSHarvestingDetailValidation Validation
		{
			get { return (NMFSHarvestingDetailValidation)base.Validation; }
		}

		protected override CusAddInfoValidation GetNewValidation()
		{
			return new NMFSHarvestingDetailValidation(this);
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (NMFSHarvestingDetail)base.CloneInternal(args);
			result.HarvestingVessles.RemoveAndDeleteAll();

			foreach (NMFSVessels harvestingVessel in HarvestingVessles)
			{
				result.HarvestingVessles.Add((NMFSVessels)harvestingVessel.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NMFSVessels), false)));
			}

			if (HarvestingVessles.Count > 0)
			{
				result.US_VesselCountry = HarvestingVessles[0].US_HarvestedCountry;
			}

			return result;
		}

		protected override bool IsDataEmpty => base.IsDataEmpty && HarvestingVessles.Count == 0;
	}
}
