using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class MergeOrgContact : MergeOrgElement<OrgContact>
	{
		public MergeOrgContact(BusinessObjectFactory factory, OrgContact oldCnt, OrgHeader newOrg)
			: this(factory, oldCnt, newOrg, null)
		{ }

		public MergeOrgContact(BusinessObjectFactory factory, OrgContact oldCnt, OrgHeader newOrg, BusinessObjectCollection newContactCollection)
			: base(factory, oldCnt, newOrg, newContactCollection)
		{ }

		#region Schema

		public static class Schema
		{
			public const string OldContactPK = "OldContactPK";
			public const string OldContactName = "OldContactName";
			public const string OldContactTitle = "OldContactTitle";
			public const string OldContactPhone = "OldContactPhone";
			public const string OldContactMobile = "OldContactMobile";
			public const string OldContactAddress = "OldContactAddress";
			public const string OldContactEmail = "OldContactEmail";
			public const string NewContactPK = "NewContactPK";
		}

		protected override SchemaColumn FKSchemaColumn
		{
			get
			{
				return OrgContactSchema.OC_OH;
			}
		}

		protected override string SchemaNewObjectPK
		{
			get { return Schema.NewContactPK; }
		}

		#endregion

		#region Properties

		#region OldContactPK

		protected ZGuid oldContactPK;

		public ZGuid OldContactPK
		{
			get
			{
				return OldObjectCore.PK;
			}
		}

		public ZPropertyInfo OldContactPKInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactPK);
			}
		}

		#endregion

		#region OldContactName

		[MaxLength(OrgContact.Schema.OC_ContactNameMaxLength)]
		public ZString OldContactName
		{
			get
			{
				return OldObjectCore.OC_ContactName;
			}
		}

		public ZPropertyInfo OldContactNameInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactName);
			}
		}

		#endregion

		#region OldContactTitle

		[MaxLength(OrgContact.Schema.OC_TitleMaxLength)]
		public ZString OldContactTitle
		{
			get
			{
				return OldObjectCore.OC_Title;
			}
		}

		public ZPropertyInfo OldContactTitleInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactTitle);
			}
		}

		#endregion

		#region OldContactPhone

		[MaxLength(OrgContact.Schema.OC_PhoneMaxLength)]
		public ZString OldContactPhone
		{
			get
			{
				return OldObjectCore.OC_Phone;
			}
		}

		public ZPropertyInfo OldContactPhoneInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactPhone);
			}
		}

		#endregion

		#region OldContactMobile

		[MaxLength(OrgContact.Schema.OC_MobileMaxLength)]
		public ZString OldContactMobile
		{
			get
			{
				return OldObjectCore.OC_Mobile;
			}
		}

		public ZPropertyInfo OldContactMobileInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactMobile);
			}
		}

		#endregion

		#region OldContactAddress

		public ZGuid OldContactAddress
		{
			get
			{
				return OldObjectCore.OC_OA_OrgAddress;
			}
		}

		public ZPropertyInfo OldContactAddressInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactAddress);
			}
		}

		#endregion

		#region OldContactEmail

		[MaxLength(OrgContact.Schema.OC_EmailMaxLength)]
		public ZString OldContactEmail
		{
			get
			{
				return OldObjectCore.OC_Email;
			}
		}

		public ZPropertyInfo OldContactEmailInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.OldContactEmail);
			}
		}

		#endregion

		#region Wrappers for binding

		#region NewContactPK

		[List("NewObjectsCollection")]
		public ZGuid NewContactPK
		{
			get { return NewObjectPK; }
			set { NewObjectPK = value; }
		}

		public ZPropertyInfo NewContactPKInfo
		{
			get
			{
				return NewObjectPKInfo;
			}
		}

		protected bool NewContactPK_ReadOnly
		{
			get
			{
				return NewObjectPK_ReadOnly;
			}
		}

		#endregion

		#endregion

		#endregion

		#region overrides

		public override BusinessObjectCollection GetNewObjectsCollection(BusinessObjectFactory factory, ZQuery query)
		{
			return new OrgContactCollection(factory, query);
		}

		protected override ZGuid FindFuzzyMatch()
		{
			return FindFuzzyMatch(NewObjectsCollection, new string[] { OldObjectCore.OC_Email, OldObjectCore.OC_ContactName });
		}

		public override ZGuid FindFuzzyMatch(IEnumerable<BusinessObject> collection, string[] stringToMatch)
		{
			ZGuid result = ZGuid.Empty;
			foreach (OrgContact contact in collection)
			{
				if (!string.IsNullOrEmpty(stringToMatch[0]) && stringToMatch[0].ToLower().Equals(contact.OC_Email.ToLower()))
				{
					result = contact.PK;
					break;
				}
				else if (stringToMatch[1] != null && stringToMatch[1].ToLower().Equals(contact.OC_ContactName.ToLower()))
				{
					result = contact.PK;
					break;
				}
			}
			return result;
		}

		protected override string ElementNameForAction
		{
			get
			{
				return Res.GetString("ea79cf63-cbb9-4420-8e63-446c1a8eccaa", "contact");
			}
		}

		public override SchemaColumn[] ColumnsForMatching
		{
			get { return new SchemaColumn[] { OrgContactSchema.OC_Email, OrgContactSchema.OC_ContactName }; }
		}

		#endregion
	}
}
