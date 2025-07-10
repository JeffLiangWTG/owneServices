using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgWebURL : IOrgWebURL, IDeduplicationGlowObject
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationOrgWebURL() { }

		public DeduplicationOrgWebURL(string url)
		{
			PU_PK = Guid.Empty;
			PU_Description = string.Empty;
			PU_IsPrimary = true;
			PU_OH = Guid.Empty;
			PU_Type = string.Empty;
			PU_URL = url;
			IsInDatabase = false;
		}

		public DeduplicationOrgWebURL(OrgWebURL webUrl, DeduplicationOrgHeader parentGlowOrgHeader)
		{
			PU_PK = webUrl.PK.ToGuid();
			PU_Description = webUrl.PU_Description;
			PU_IsPrimary = webUrl.PU_IsPrimary;
			PU_OH = !webUrl.PU_OH.IsEmpty && webUrl.PU_OH.IsValid ? webUrl.PU_OH.ToGuid() : Guid.Empty;
			PU_Type = webUrl.PU_Type;
			PU_URL = webUrl.PU_URL;
			IsInDatabase = webUrl.IsInDatabase;
			OrgHeader = parentGlowOrgHeader;
		}

		public Type BizoType => typeof(OrgWebURL);

		public bool IsInDatabase { get; }

		public Guid PK => PU_PK;

		public string CountryCode => string.Empty;

		public string TablePrefix => OrgWebURLSchema.Constants.Prefix;

		public Guid PU_PK { get; set; }

		public string PU_Description { get; set; }

		public bool PU_IsPrimary { get; set; }

		public Guid PU_OH { get; set; }

		public DateTime? PU_SystemCreateTimeUtc { get; set; }

		public string PU_SystemCreateUser { get; set; }

		public DateTime? PU_SystemLastEditTimeUtc { get; set; }

		public string PU_SystemLastEditUser { get; set; }

		public string PU_Type { get; set; }

		public string PU_URL { get; set; }

		public IGlbStaffInfo CreatedByStaff { get; set; }

		public IGlbStaffInfo LastEditedByStaff { get; set; }

		public IOrgHeader OrgHeader { get; set; }

		public ICollection<IAcknowledgement<IOrgWebURL>> Acknowledgements { get; }
	}
}
