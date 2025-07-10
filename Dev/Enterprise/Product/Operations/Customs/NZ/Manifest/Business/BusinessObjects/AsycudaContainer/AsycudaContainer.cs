using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Manifest.Business
{
	public class AsycudaContainer : ASYCUDA.Business.AsycudaContainer,
		Integration.Customs.ASYCUDA.NZManifest.IAsycudaContainer, IDocAddresses
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaManifestHeader Header => (AsycudaManifestHeader)base.Header;

		#region Schema

#pragma warning disable IDE0001 // Simplify Names
		public new class Schema : ASYCUDA.Business.AsycudaContainer.Schema
#pragma warning restore IDE0001 // Simplify Names
		{
			public const string SendMCDInformation = "SendMCDInformation";
			public const string HasMPIQD = "HasMPIQD";
			public const string IsContainerClean = "IsContainerClean";
			public const string IsPackingContaminated = "IsPackingContaminated";
			public const string IsWoodPackingUsed = "IsWoodPackingUsed";
			public const string IsWoodPackingTreated = "IsWoodPackingTreated";
			public const string HasWoodPackingTreatmentCert = "HasWoodPackingTreatmentCert";
			public const string DeliveryDestination = "DeliveryDestination";
			public const string DeliveryDestinationOrgPK = "DeliveryDestinationOrgPK";
			public const string PackLocationOrgPK = "PackLocationOrgPK";
		}

		#endregion

		#region IDocAddresses

		#region DocAddresses

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}

				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;

		#endregion

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[]
		{
				DocAddressType.ContainerLegDeliveryAddress
		};

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			// Add DocAddressTypes to this list that DO NOT support overrides because they are displayed using simple address control
			switch (docAddress.DocAddressType)
			{
				case DocAddressType.ContainerLegDeliveryAddress:
					return new CannotOverrideAddressSecurityCheckpoint();
				default:
					return Environment.Env.Security.None;
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress) { }

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress) { }

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress) { }

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress) { }

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress) { }

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		#endregion

		#region Propertes

		#region MPI Quarantine Declaration for Containers

		#region SendMCD

		[ResourceStringData("ADF719E4-F99B-4028-A3BD-DA91415E4C14", Caption = "Send MCD Information")]
		[ReadOnlyMember(nameof(SendMCDInformation_ReadOnly))]
		public ZBool SendMCDInformation
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.SendMCDInformation);
			set
			{
				var oldValue = SendMCDInformation;
				this.SetSystemDefinedValue(Schema.SendMCDInformation, value);
				SendMCDInformationInfo.RefreshBinding(oldValue);
			}
		}
		public ZPropertyInfo SendMCDInformationInfo => GetZPropertyInfo(Schema.SendMCDInformation);

		protected bool SendMCDInformation_ReadOnly => HasMPIQD || IsContainerClean || IsPackingContaminated || IsWoodPackingUsed || IsWoodPackingTreated || HasWoodPackingTreatmentCert;

		#endregion

		#region HasMPIQD

		[ResourceStringData("C465ED0E-4EFE-4272-A59F-C5EFA90D1674", Caption = "Has MPI QD", FullDescription = "If there is no Quarantine Declaration or it is unknown whether there is one, leave this field blank.")]
		public ZBool HasMPIQD
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.HasMPIQD);
			set
			{
				var oldValue = HasMPIQD;
				this.SetSystemDefinedValue(Schema.HasMPIQD, value);
				HasMPIQDInfo.RefreshBinding(oldValue);
				if (oldValue != HasMPIQD)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo HasMPIQDInfo => GetZPropertyInfo(Schema.HasMPIQD);

		#endregion

		#region IsContainerClean

		[ResourceStringData("4CF2B497-827B-4568-AE39-65FB37BBED15", Caption = "Cleanliness", FullDescription = "At the time of packing, was the container inspected internally and externally, and is clean and free from contamination with live organisms, material of plant or animal origin, soil and water?")]
		public ZBool IsContainerClean
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsContainerClean);
			set
			{
				var oldValue = IsContainerClean;
				this.SetSystemDefinedValue(Schema.IsContainerClean, value);
				IsContainerCleanInfo.RefreshBinding(oldValue);
				if (oldValue != IsContainerClean)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo IsContainerCleanInfo => GetZPropertyInfo(Schema.IsContainerClean);

		#endregion

		#region IsPackingContaminated

		[ResourceStringData("D0864CDD-796E-463C-AB0A-25603B6BB276", Caption = "Restricted Packing Materials", FullDescription = "Has any soil, peat, moss, used sacking material, used tires, hay, straw, chaff or any packing material contaminated with live organisms, material of plant or animal origin, soil and water been used?")]
		[BusinessObjectTestExclude]
		public ZBool IsPackingContaminated
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsPackingContaminated);
			set
			{
				var oldValue = IsPackingContaminated;
				this.SetSystemDefinedValue(Schema.IsPackingContaminated, value);
				IsPackingContaminatedInfo.RefreshBinding(oldValue);
				if (oldValue != IsPackingContaminated)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo IsPackingContaminatedInfo => GetZPropertyInfo(Schema.IsPackingContaminated);

		#endregion

		#region WoodPacking

		[ResourceStringData("250632A5-CA09-4292-8AA5-9A2EF4722CA9", Caption = "Wood Packaging", FullDescription = "Has any wood packaging been used within the container such as cases, crates, pallets or wood used to separate, brace, protect or secure cargo in transit?")]
		public ZBool IsWoodPackingUsed
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsWoodPackingUsed);
			set
			{
				var oldValue = IsWoodPackingUsed;
				this.SetSystemDefinedValue(Schema.IsWoodPackingUsed, value);
				IsWoodPackingUsedInfo.RefreshBinding(oldValue);
				if (oldValue != IsWoodPackingUsed)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo IsWoodPackingUsedInfo => GetZPropertyInfo(Schema.IsWoodPackingUsed);

		#endregion

		#region WoodPackingTreated

		[ResourceStringData("F13DF438-DAFA-44D3-9F07-FB22C49B5C77", Caption = "Wood Packaging Treated", FullDescription = "Has the wood been ISPM 15 treated and marked or is the packaging made from material exempt from these requirements (such as Plywood or Medium Density Fibre-board (MDF))? Certification is not required for ISPM 15 treated and marked wood packaging.")]
		public ZBool IsWoodPackingTreated
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.IsWoodPackingTreated);
			set
			{
				var oldValue = IsWoodPackingTreated;
				this.SetSystemDefinedValue(Schema.IsWoodPackingTreated, value);
				IsWoodPackingTreatedInfo.RefreshBinding(oldValue);
				if (oldValue != IsWoodPackingTreated)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo IsWoodPackingTreatedInfo => GetZPropertyInfo(Schema.IsWoodPackingTreated);

		#endregion

		#region HasTreatmentCertificate

		[ResourceStringData("8BA245E7-1FE9-4CF8-B666-28DE41063736", Caption = "Wood Packaging Treatment Cert.", FullDescription = "Has the wood been otherwise treated and certified as per the Import Health Standard?")]
		public ZBool HasWoodPackingTreatmentCert
		{
			get => this.GetSystemDefinedValue<ZBool>(Schema.HasWoodPackingTreatmentCert);
			set
			{
				var oldValue = HasWoodPackingTreatmentCert;
				this.SetSystemDefinedValue(Schema.HasWoodPackingTreatmentCert, value);
				HasWoodPackingTreatmentCertInfo.RefreshBinding(oldValue);
				if (oldValue != HasWoodPackingTreatmentCert)
				{
					UpdateSendMCDInformationIfRequired();
				}
			}
		}

		public ZPropertyInfo HasWoodPackingTreatmentCertInfo => GetZPropertyInfo(Schema.HasWoodPackingTreatmentCert);

		#endregion

		void UpdateSendMCDInformationIfRequired()
		{
			if (!SendMCDInformation && SendMCDInformation_ReadOnly)
			{
				SendMCDInformation = true;
			}
		}

		#endregion

		#region DeliveryDestinationPartyDocAddress

		[List(nameof(DeliveryDestinationPartyDocAddress) + "." + nameof(JobDocAddress.Lookups) + "." + nameof(JobDocAddressLookups.Address_List))]
		[ResourceStringData("6BD03C54-F83C-44F0-BBD6-89E56A408737", Caption = "Delivery Destination Address")]
		public ZGuid DeliveryDestination
		{
			get => DeliveryDestinationPartyDocAddress.E2_OA_Address;
			set => DeliveryDestinationPartyDocAddress.E2_OA_Address = value;
		}
		public ZPropertyInfo DeliveryDestinationInfo => GetWrappedZPropertyInfo(Schema.DeliveryDestination, x => DeliveryDestinationPartyDocAddress.E2_OA_AddressInfo);

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.DeliveryDestinationPartyOrganisations))]
		[ResourceStringData("05B70136-29A8-4E51-A825-AC806B463ABD", Caption = "Delivery Destination")]
		public ZGuid DeliveryDestinationOrgPK
		{
			get => DeliveryDestinationPartyDocAddress.OrganisationPK;
			set => DeliveryDestinationPartyDocAddress.OrganisationPK = value;
		}
		public ZPropertyInfo DeliveryDestinationOrgPKInfo => GetWrappedZPropertyInfo(Schema.DeliveryDestinationOrgPK, x => DeliveryDestinationPartyDocAddress.OrganisationPKInfo);

		public JobDocAddress DeliveryDestinationPartyDocAddress
		{
			get
			{
				if (deliveryDestinationPartyDocAddress == null || deliveryDestinationPartyDocAddress.IsDeleted)
				{
					deliveryDestinationPartyDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ContainerLegDeliveryAddress);
				}

				return deliveryDestinationPartyDocAddress;
			}
		}
		JobDocAddress deliveryDestinationPartyDocAddress;

		#endregion

		#region Pack Location

		[List(nameof(ACN_OA_PackLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		[ResourceStringData("6302ABC3-4096-4136-8831-C7ABC5464A58", Caption = "Packing Location Address")]
		public override ZGuid ACN_OA_PackLocation
		{
			get => base.ACN_OA_PackLocation;
			set => base.ACN_OA_PackLocation = value;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaContainerLookups.PackLocationOrganisations))]
		[ResourceStringData("9D7207ED-F033-4D75-854B-19EE409C24FC", Caption = "Packing Location")]
		public ZGuid PackLocationOrgPK { get => ACN_OA_PackLocation_ZAddress.OrgPK; set => ACN_OA_PackLocation_ZAddress.OrgPK = value; }

		public ZPropertyInfo StuffingLocationOrgPKInfo => GetWrappedZPropertyInfo(Schema.PackLocationOrgPK, x => ACN_OA_PackLocation_ZAddress.OrgPKInfo);

		#endregion

		#endregion

		#region Lookups

		public new AsycudaContainerLookups Lookups => (AsycudaContainerLookups)base.Lookups;
		protected override ManifestBase.AsycudaContainerLookups GetNewLookups() => new AsycudaContainerLookups(this);

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaContainerFetchStrategy(this);
	}
}
