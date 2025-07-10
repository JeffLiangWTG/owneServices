using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Types;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgAddress : IOrgAddress, IDeduplicationGlowObject
	{
		public DeduplicationOrgAddress() { }

		public DeduplicationOrgAddress(OrgAddress address, DeduplicationOrgHeader parentGlowOrgHeader)
		{
			OA_PK = address.PK.ToGuid();
			OA_Address1 = address.OA_Address1;
			OA_Address2 = address.OA_Address2;
			OA_City = address.OA_City;
			OA_Code = address.OA_Code;
			OA_Email = address.OA_Email;
			OA_Fax = address.OA_Fax;
			OA_IsActive = address.OA_IsActive;
			OA_Latitude = address.OA_Latitude;
			OA_Longitude = address.OA_Longitude;
			OA_Mobile = address.OA_Mobile;
			OA_OH = !address.OA_OH.IsEmpty && address.OA_OH.IsValid ? address.OA_OH.ToGuid() : Guid.Empty;
			OA_Phone = address.OA_Phone;
			OA_PostCode = address.OA_PostCode;
			OA_RL_NKRelatedPortCode = address.OA_RL_NKRelatedPortCode;
			OA_RN_NKCountryCode = address.OA_RN_NKCountryCode;
			OA_State = address.OA_State;
			OA_ValidationStatus = OrganisationsDataRegistry.Instance.ShouldUseAddressValidation(address.Country?.PK.ToGuid() ?? Guid.Empty, AddressValidationSection.OrganizationAddress) ? address.OA_ValidationStatus.ToString() : "NTC";
			OA_VerifiesContainerGrossWeight = address.OA_VerifiesContainerGrossWeight;
			OA_GeofencePolygon = address.OA_GeofencePolygon.AsText();
			OA_Language = address.OA_Language;
			OA_GeoLocation = address.OA_GeoLocation.AsText();
			IsInDatabase = address.IsInDatabase;
			OrgAddressCapabilities = address.CapabilitiesCollection
				.Where(capability => capability != null)
				.Select(capability => new DeduplicationOrgAddressCapability(capability, this))
				.ToArray();
			OrgHeader = parentGlowOrgHeader;
		}

		public Type BizoType => typeof(OrgAddress);
		public bool IsInDatabase { get; }
		public ZBool IsExcludedFromDeduplication { get; set; }

		public ZString AddressFull
		{
			get
			{
				var result = new ZStringBuilder();

				if (!string.IsNullOrEmpty(OA_Address1))
				{
					result.Append(OA_Address1);
					if (!string.IsNullOrEmpty(OA_Address2))
					{
						result.Append(", ");
					}
				}
				if (!string.IsNullOrEmpty(OA_Address2))
				{
					result.Append(OA_Address2);
				}

				if (!string.IsNullOrEmpty(OA_City))
				{
					result.Append(" " + OA_City);
				}

				if (!string.IsNullOrEmpty(OA_State))
				{
					result.Append(" " + OA_State);
				}

				if (!string.IsNullOrEmpty(OA_PostCode))
				{
					result.Append(" " + OA_PostCode);
				}

				if (!string.IsNullOrEmpty(OA_RL_NKRelatedPortCode))
				{
					result.Append(" " + OA_RL_NKRelatedPortCode);
				}

				return result.ToString().Trim();
			}
		}

		public Guid PK => OA_PK;
		public string CountryCode => string.Empty;
		public string TablePrefix => OrgAddressSchema.Constants.Prefix;
		public Guid OA_PK { get; set; }
		public string OA_AccessPoint { get; set; }
		public string OA_AdditionalAddressInformation { get; set; }
		public string OA_Address1 { get; set; }
		public string OA_Address2 { get; set; }
		public string OA_AddressMap { get; set; }
		public string OA_AIREquipmentNeeded { get; set; }
		public string OA_AuthorityToLeave { get; set; }
		public string OA_City { get; set; }
		public string OA_Code { get; set; }
		public string OA_CommunicationRequired { get; set; }
		public string OA_CompanyNameOverride { get; set; }
		public string OA_ContainerHandling { get; set; }
		public DateTime? OA_DeliverFromTimeOnly { get; set; }
		public DateTime? OA_DeliverToTimeOnly { get; set; }
		public string OA_DeliveryRoute { get; set; }
		public short OA_DeliveryRouteSequence { get; set; }
		public string OA_Dock_Height { get; set; }
		public bool OA_DockLeveler { get; set; }
		public DateTime? OA_DoNotAttendFrom { get; set; }
		public DateTime? OA_DoNotAttendTo { get; set; }
		public string OA_Email { get; set; }
		public string OA_Fax { get; set; }
		public string OA_FCLEquipmentNeeded { get; set; }
		public bool OA_ForkLift { get; set; }
		public short OA_GroupNumber { get; set; }
		public bool OA_IsActive { get; set; }
		public bool OA_IsValid { get; set; }
		public int OA_JobLoadingDuration { get; set; }
		public string OA_LabourRequired { get; set; }
		public string OA_Language { get; set; }
		public decimal OA_Latitude { get; set; }
		public string OA_LCLEquipmentNeeded { get; set; }
		public string OA_LoadingUnloadingConstraints { get; set; }
		public decimal OA_Longitude { get; set; }
		public string OA_Mobile { get; set; }
		public Guid OA_OH { get; set; }
		public bool OA_PalletJack { get; set; }
		public string OA_Phone { get; set; }
		public DateTime? OA_PickupFromTimeOnly { get; set; }
		public DateTime? OA_PickupToTimeOnly { get; set; }
		public string OA_PostCode { get; set; }
		public string OA_RL_NKRelatedPortCode { get; set; }
		public string OA_RN_NKCountryCode { get; set; }
		public string OA_State { get; set; }
		public bool OA_SuppressAddressValidationError { get; set; }
		public bool OA_UseCumulativeFreeWaitingTime { get; set; }
		public string OA_ValidationStatus { get; set; }
		public IRefUNLOCOInfo RelatedPort { get; set; }
		public IRefCountryInfo Country { get; set; }
		public IOrgHeader OrgHeader { get; set; }
		public bool OA_VerifiesContainerGrossWeight { get; set; }
		public string OA_GeofencePolygon { get; set; }
		public string OA_GeoLocation { get; set; }
		public DateTime? OA_SystemCreateTimeUtc { get; set; }
		public string OA_SystemCreateUser { get; set; }
		public DateTime? OA_SystemLastEditTimeUtc { get; set; }
		public string OA_SystemLastEditUser { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public ICollection<IOrgAddressCapability> OrgAddressCapabilities { get; set; }
		public ICollection<IOrgAddressAdditionalInfo> OrgAddressAdditionalInfos { get; set; }

		public ICollection<IAcknowledgement<IOrgAddress>> Acknowledgements { get; }
	}
}
