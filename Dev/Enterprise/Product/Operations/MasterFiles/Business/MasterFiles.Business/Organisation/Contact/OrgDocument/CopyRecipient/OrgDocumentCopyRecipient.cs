using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	public class OrgDocumentCopyRecipient : AutoOrgDocumentCopyRecipient
	{
		public OrgDocumentCopyRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[List("Lookups.ODR_AvailableEmailAddress_List")]
		[EmailAddress]
		public override ZString ODR_EmailAddress
		{
			get { return base.ODR_EmailAddress; }
			set { base.ODR_EmailAddress = value; }
		}

		public OrgHeader Organization
		{
			get
			{
				OrgHeader result = null;
				var orgDocument = Document;
				if (orgDocument != null)
				{
					var orgContact = orgDocument.Contact;
					if (orgContact != null)
					{
						result = orgContact.ParentOrg;
					}
				}
				return result;
			}
		}

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			bool result = Organization != null && !Organization.SecurityProvider.HasModifyContactDocDeliveryDetailsSecurity;
			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#endregion
	}
}