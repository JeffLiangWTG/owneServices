
namespace Enterprise.MasterFiles.Business
{
	public class OrgSecurityContactsValidation : AutoOrgSecurityContactsValidation
	{
		public OrgSecurityContactsValidation(AutoOrgSecurityContacts parent) : base(parent)
		{
			this.SecurityContact = (OrgSecurityContacts)parent;
		}

		readonly OrgSecurityContacts SecurityContact;

		#region OZ_Granted

		protected override void CheckOZ_Granted()
		{
			base.CheckOZ_Granted();
			if (SecurityContact.Contact != null && !SecurityContact.Contact.IsDeleted &&
				SecurityContact.OZ_Granted && !SecurityContact.Contact.OC_WebAccessEnabled)
			{
				SecurityContact.OZ_GrantedInfo.AddError(Res.GetString("07a2c5a4-e0e9-4895-8226-ea3881aabcf6", "You cannot grant this security right unless you enable Web Access for this contact."));
			}
		}

		#endregion
	}
}
