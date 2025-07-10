using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using CargoWise.Tools.DuplicateDetector.Common;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgBrandOrRelatedName : IOrgBrandOrRelatedName, IDeduplicationGlowObject, IRawNameProvider
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationOrgBrandOrRelatedName() { }

		public DeduplicationOrgBrandOrRelatedName(OrgBrandOrRelatedName brand, DeduplicationOrgHeader parentGlowOrgHeader)
		{
			P1_PK = brand.PK.ToGuid();
			P1_OH = !brand.P1_OH.IsEmpty && brand.P1_OH.IsValid ? brand.P1_OH.ToGuid() : Guid.Empty;
			P1_RelatedName = brand.P1_RelatedName;
			IsInDatabase = brand.IsInDatabase;
			RawName = brand.P1_RelatedName;
			OrgHeader = parentGlowOrgHeader;
		}
		public Type BizoType => typeof(OrgBrandOrRelatedName);

		public bool IsInDatabase { get; }

		public Guid PK => P1_PK;

		public string CountryCode => string.Empty;

		public string TablePrefix => OrgBrandOrRelatedNameSchema.Constants.Prefix;

		public Guid P1_PK { get; set; }

		public bool P1_IsValid { get; set; }

		public Guid P1_OH { get; set; }

		public string P1_RelatedName { get; set; }

		public DateTime? P1_SystemCreateTimeUtc { get; set; }

		public string P1_SystemCreateUser { get; set; }

		public DateTime? P1_SystemLastEditTimeUtc { get; set; }

		public string P1_SystemLastEditUser { get; set; }

		public IGlbStaffInfo CreatedByStaff { get; set; }

		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public IOrgHeader OrgHeader { get; set; }

		public ICollection<IAcknowledgement<IOrgBrandOrRelatedName>> Acknowledgements { get; }

		public ICollection<IOrgProductType> OrgProductTypes { get; }

		public string RawName { get; }
	}
}
