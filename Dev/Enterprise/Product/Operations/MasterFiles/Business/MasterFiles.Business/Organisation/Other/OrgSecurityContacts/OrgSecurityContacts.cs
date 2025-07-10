using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgSecurityContacts : AutoOrgSecurityContacts
	{
		public OrgSecurityContacts(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoOrgSecurityContacts.Schema
		{
			public const string SecurityItemName = "SecurityItemName";
			public const string HasDifferentSecurityToParent = "HasDifferentSecurityToParent";
		}

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return IsDeleted || HasDifferentSecurityToParent; }
		}

		public override bool SupportsNotes => false;
		internal bool ShouldAddLogsOnFactorySaving { get; set; }

		protected override void BeforeSuccessfulDelete()
		{
		}

		#endregion

		#region Properties

		#region Security Item Name

		public ZString SecurityItemName
		{
			get
			{
				return SecurityItemNameFor(Security);
			}
		}

		protected static ZString SecurityItemNameFor(OrgSecurity security)
		{
			ZString result = ZString.Empty;
			if (security != null)
			{
				result = !security.SecurityItemNameForDisplay.IsEmpty ? security.SecurityItemNameForDisplay : security.OX_SecurityItemName;
			}
			return result;
		}

		public ZPropertyInfo SecurityItemNameInfo => GetZPropertyInfo(Schema.SecurityItemName);

		public override ZBool OZ_Granted
		{
			get { return base.OZ_Granted; }
			set
			{
				base.OZ_Granted = value;
				HasDifferentSecurityToParentInfo.RefreshBinding();
			}
		}

		#endregion

		#region OZ_OC

		[ReadOnly(true)]
		public override ZGuid OZ_OC
		{
			get { return base.OZ_OC; }
			set { base.OZ_OC = value; }
		}

		#endregion

		#endregion

		#region Different Security to Parent

		public ZBool HasDifferentSecurityToParent
		{
			get { return Security != null && OZ_Granted != Security.OX_Granted && Contact != null && Contact.OC_WebAccessEnabled; }
		}

		public ZPropertyInfo HasDifferentSecurityToParentInfo => GetZPropertyInfo(Schema.HasDifferentSecurityToParent);

		#endregion

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool contactReadOnlySecurity = true;
			if (Contact != null && Contact.Header != null && Contact.Header.SecurityProvider != null)
			{
				contactReadOnlySecurity = Contact.Header.IsInDatabase
					? Contact.Header.SecurityProvider.HasModifyDetailsWebSecurity
					: Contact.Header.SecurityProvider.HasNewDetailsWebSecurity;
			}

			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || !contactReadOnlySecurity;
		}

		#endregion
	}
}
