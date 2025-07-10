using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgSecurity : AutoOrgSecurity
	{
		public OrgSecurity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			fOX_OH = OX_OH;
		}

		public new class Schema : AutoOrgSecurity.Schema
		{
			public const string OX_GrantedForWeb = "OX_GrantedForWeb";
			public const string SecurityItemNameForDisplay = "SecurityItemNameForDisplay";
		}

		#region Saving

		public override bool IsSavedByFactory
		{
			get
			{
				return IsDeleted ||
					(base.IsSavedByFactory &&
					(IsDifferentToDefaultRight || ChildSecurityContactsNeedSaving));
			}
		}

		bool ChildSecurityContactsNeedSaving
		{
			get
			{
				var inMemorySecuritiesQuery = new ZQuery(OrgSecurityContactsSchema.OZ_OX, PK) { FetchOnlyFromLocalCache = true };
				foreach (var securityContact in Factory.Load<OrgSecurityContacts>(inMemorySecuritiesQuery))
				{
					if (securityContact.IsSavedByFactory)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region IsGrantedByDefault

		public bool IsDifferentToDefaultRight
		{
			get { return IsGrantedByDefault != OX_Granted || OX_IsCustomerManaged; }
		}

		public bool IsGrantedByDefault
		{
			get
			{
				return WebSecurityRight != null && WebSecurityRight.IsGrantedByDefault;
			}
		}

		#endregion

		#region IsWebWarehouseSecurity

		public bool IsWebWarehouseSecurity
		{
			get
			{
				return WebSecurityRight != null && WebSecurityRight.IsWarehouse;
			}
		}

		#endregion

		#region WebSecurityRights
		IWebSecurityRightProvider WebSecurityRights
		{
			get { return webSecurityRights ?? (webSecurityRights = Factory.GetCachedValue("OrgSecurity.WebSecurityRights", () => AllWebSecurityRights.New(Factory))); }
		}
		IWebSecurityRightProvider webSecurityRights;

		#endregion

		#region WebSecurityRight

		public WebSecurityRight WebSecurityRight
		{
			get
			{
				if (webSecurityRight == null)
				{
					WebSecurityRights.TryGetValue(SecurityKey, out webSecurityRight);
				}
				return webSecurityRight;
			}
		}
		WebSecurityRight webSecurityRight;

		#endregion

		#region Properties

		#region OX_Granted

		public override ZBool OX_Granted
		{
			get { return base.OX_Granted; }
			set
			{
				base.OX_Granted = value;
				if (fContactSecurityRights != null)
				{
					foreach (OrgSecurityContacts securityContact in ContactSecurityRights)
					{
						if (!securityContact.IsInDatabase)
						{
							securityContact.OZ_Granted = value && securityContact.Contact.OC_WebAccessEnabled;
						}
					}
				}
			}
		}

		public ZBool OX_GrantedForWeb
		{
			get
			{
				EnsureContactSecurityRightsIsLoaded();
				return base.OX_Granted;
			}
			set
			{
				EnsureContactSecurityRightsIsLoaded();
				base.OX_Granted = value;
			}
		}

		void EnsureContactSecurityRightsIsLoaded()
		{
			if (!ContactSecurityRights.IsLoaded)
			{
				ContactSecurityRights.Load();
			}
		}

		public ZPropertyInfo OX_GrantedForWebInfo => GetWrappedZPropertyInfo(Schema.OX_GrantedForWeb, x => OX_GrantedInfo);

		#endregion

		#region OX_SecurityItemName

		[ReadOnly(true)]
		public override ZString OX_SecurityItemName
		{
			get { return base.OX_SecurityItemName; }
			set
			{
				if (base.OX_SecurityItemName != value)
				{
					base.OX_SecurityItemName = value;
					webSecurityRight = null;
				}
			}
		}

		#endregion

		[ReadOnly(true)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString SecurityItemNameForDisplay
		{
			get
			{
				return securityItemNameForDisplay;
			}
			set
			{
				SetNonPersistentPropertyValue(SecurityItemNameForDisplayInfo, ref securityItemNameForDisplay, value);
			}
		}
		ZString securityItemNameForDisplay;

		public ZPropertyInfo SecurityItemNameForDisplayInfo => GetZPropertyInfo(Schema.SecurityItemNameForDisplay);

		#endregion

		public ZString SecurityKey
		{
			get { return OX_SU != ZGuid.Empty ? new ZString(OX_SU.ToString()) : OX_SecurityItemName; }
		}

		#region Contact Security Rights

		[ChildEditable(true)]
		public OrgSecurityContactsCollection ContactSecurityRights
		{
			get
			{
				if (fContactSecurityRights == null)
				{
					fContactSecurityRights = new OrgSecurityContactsCollection(this);
					fContactSecurityRights.DontLoadDummyContactRecords = dontLoadDummyContactRecords;
					needsToLoadDummyContactRecords = dontLoadDummyContactRecords;
					fContactSecurityRights.Load();
					RegisterEditableChildObject(fContactSecurityRights);
					if (Header != null)
					{
						fContactSecurityRights.SetReadOnlyIncludingChildren(!(Header.IsInDatabase ? Header.SecurityProvider.HasModifyDetailsWebSecurity : Header.SecurityProvider.HasNewDetailsWebSecurity));
					}
				}
				else if (!dontLoadDummyContactRecords && needsToLoadDummyContactRecords)
				{
					fContactSecurityRights.DontLoadDummyContactRecords = false;
					fContactSecurityRights.Refresh();
					needsToLoadDummyContactRecords = false;
				}

				return fContactSecurityRights;
			}
		}
#if DEBUG
		internal
#endif
		OrgSecurityContactsCollection fContactSecurityRights;

		internal OrgSecurityContactsCollection ContactSecurityRightsNoCreate
		{
			get { return fContactSecurityRights; }
		}

		internal void InvalidateContactSecurityRights()
		{
			fContactSecurityRights = null;
		}

		bool dontLoadDummyContactRecords;
		bool needsToLoadDummyContactRecords;

		public bool NoChildrenExistWithDifferentSecurity
		{
			get
			{
				try
				{
					dontLoadDummyContactRecords = true;
					bool result = true;

					foreach (OrgSecurityContacts securityContact in ContactSecurityRights)
					{
						if (securityContact.HasDifferentSecurityToParent)
						{
							result = false;
							break;
						}
					}

					return result;
				}
				finally
				{
					dontLoadDummyContactRecords = false;
				}
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			var rights = fContactSecurityRights;
			if (rights == null)
			{
				rights = new OrgSecurityContactsCollection(this);
				rights.Load(); // Done to avoid touching ReadOnly on the public collection
			}

			rights.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Parent Organisation

		public override ZGuid OX_OH
		{
			get { return IsDeleted ? fOX_OH : base.OX_OH; }
			set
			{
				base.OX_OH = value;
				fOX_OH = value;
			}
		}
		ZGuid fOX_OH;

		public OrgHeader Organisation
		{
			get { return Header; }
		}

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return (Header != null && !(Header.IsInDatabase ? Header.SecurityProvider.HasModifyDetailsWebSecurity : Header.SecurityProvider.HasNewDetailsWebSecurity)) ||
				MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}
