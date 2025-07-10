using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgCusCode : IOrgCusCode, IDeduplicationGlowObject
	{
		public DeduplicationOrgCusCode() { }

		public DeduplicationOrgCusCode(OrgCusCode cusCode, DeduplicationOrgHeader parentGlowOrgHeader)
		{
			OK_PK = cusCode.PK.ToGuid();
			OK_CodeType = cusCode.OK_CodeType;
			OK_CountryDefault = cusCode.OK_CountryDefault;
			OK_CustomsRegNo = cusCode.OK_CustomsRegNo;
			OK_OA_PremisesAddress = !cusCode.OK_OA_PremisesAddress.IsEmpty && cusCode.OK_OA_PremisesAddress.IsValid ? cusCode.OK_OA_PremisesAddress.ToGuid() : Guid.Empty;
			OK_OH = cusCode.OK_OH.ToGuid();
			OK_RN_NKCodeCountry = cusCode.OK_RN_NKCodeCountry;
			IsInDatabase = cusCode.IsInDatabase;
			OrgHeader = parentGlowOrgHeader;
		}
		public Type BizoType => typeof(OrgCusCode);

		public bool IsInDatabase { get; }

		public Guid PK => OK_PK;

		public string CountryCode => string.Empty;

		public string TablePrefix => OrgCusCodeSchema.Constants.Prefix;

		public Guid OK_PK { get; set; }

		public string OK_CodeType { get; set; }

		public bool OK_CountryDefault { get; set; }

		public string OK_CustomsRegNo { get; set; }

		public bool OK_IsValid { get; set; }

		public Guid? OK_OA_PremisesAddress { get; set; }

		public Guid OK_OH { get; set; }

		public string OK_RN_NKCodeCountry { get; set; }

		public DateTime? OK_SystemCreateTimeUtc { get; set; }

		public string OK_SystemCreateUser { get; set; }

		public DateTime? OK_SystemLastEditTimeUtc { get; set; }

		public string OK_SystemLastEditUser { get; set; }

		public IRefCountryInfo CodeCountry { get; set; }

		public IGlbStaffInfo CreatedByStaff { get; set; }

		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public IOrgAddress PremisesAddress { get; set; }

		public IOrgHeader OrgHeader { get; set; }

		public ICollection<IAcknowledgement<IOrgCusCode>> Acknowledgements { get; }

		public string OK_UnSignedCustomsRegNo { get; set; }
	}
}
