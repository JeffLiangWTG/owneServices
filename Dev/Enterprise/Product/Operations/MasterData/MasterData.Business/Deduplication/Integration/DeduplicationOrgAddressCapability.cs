using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgAddressCapability : IOrgAddressCapability, IDeduplicationGlowObject
	{
		[Newtonsoft.Json.JsonConstructor]
		public DeduplicationOrgAddressCapability() { }

		public DeduplicationOrgAddressCapability(OrgAddressCapability capability, DeduplicationOrgAddress parentGlowOrgAddress)
		{
			PZ_PK = capability.PK.ToGuid();
			PZ_AddressType = capability.PZ_AddressType;
			PZ_IsMainAddress = capability.PZ_IsMainAddress;
			PZ_OA = !capability.PZ_OA.IsEmpty && capability.PZ_OA.IsValid ? capability.PZ_OA.ToGuid() : Guid.Empty;
			IsInDatabase = capability.IsInDatabase;
			OrgAddress = parentGlowOrgAddress;
		}

		public Type BizoType => typeof(OrgAddressCapability);

		public bool IsInDatabase { get; }

		public Guid PK => PZ_PK;

		public string CountryCode => string.Empty;

		public string TablePrefix => OrgAddressCapabilitySchema.Constants.Prefix;

		public Guid PZ_PK { get; set; }

		public string PZ_AddressType { get; set; }

		public bool PZ_IsMainAddress { get; set; }

		public bool PZ_IsValid { get; set; }

		public Guid PZ_OA { get; set; }

		public DateTime? PZ_SystemCreateTimeUtc { get; set; }

		public string PZ_SystemCreateUser { get; set; }

		public DateTime? PZ_SystemLastEditTimeUtc { get; set; }

		public string PZ_SystemLastEditUser { get; set; }

		public IGlbStaffInfo CreatedByStaff { get; set; }

		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public IOrgAddress OrgAddress { get; set; }

		public ICollection<IAcknowledgement<IOrgAddressCapability>> Acknowledgements { get; }
	}
}
