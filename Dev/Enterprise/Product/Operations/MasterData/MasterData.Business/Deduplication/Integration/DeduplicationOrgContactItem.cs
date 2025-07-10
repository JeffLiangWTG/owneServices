using System;
using System.Collections.Generic;
using CargoWise.Glow.Model.Interfaces;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.Business
{
	public class DeduplicationOrgContactItem : IOrgContactItem, IDeduplicationGlowObject
	{
		[Newtonsoft.Json.JsonConstructor]
		DeduplicationOrgContactItem() { }

		public DeduplicationOrgContactItem(OrgContactItem item, DeduplicationOrgContact parentGlowOrgContact)
		{
			OI_PK = item.PK.ToGuid();

			OI_Address = item.OI_Address;
			OI_ContactItemType = item.OI_ContactItemType;
			OI_Description = item.OI_Description;
			OI_OC = item.Contact.PK.IsValid ? item.Contact.PK.ToGuid() : Guid.Empty;
			IsInDatabase = item.IsInDatabase;
			OrgContact = parentGlowOrgContact;
		}

		public Guid PK => OI_PK;

		public string TablePrefix => OrgContactItemSchema.Constants.Prefix;

		public bool IsInDatabase { get; }

		public Type BizoType => typeof(OrgContactItem);

		public string CountryCode => string.Empty;

		public Guid OI_PK { get; }

		public string OI_Address { get; set; }
		public string OI_ContactItemType { get; set; }
		public string OI_Description { get; set; }
		public bool OI_IsPrimary { get; set; }
		public Guid? OI_OC { get; set; }
		public IOrgContact OrgContact { get; set; }
		public ICollection<IAcknowledgement<IOrgContactItem>> Acknowledgements { get; }
		public DateTime? OI_SystemCreateTimeUtc { get; set; }
		public string OI_SystemCreateUser { get; set; }
		public DateTime? OI_SystemLastEditTimeUtc { get; set; }
		public string OI_SystemLastEditUser { get; set; }
		public IGlbStaffInfo CreatedByStaff { get; set; }
		public IGlbStaffInfo LastEditedByStaff { get; set; }
	}
}
