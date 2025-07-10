using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class PersonAssociationsOrganizationWrapper : PersonAssociationsOrgGroupWrapper
	{
		public OrgContact Contact { get; }

		public PersonAssociationsOrganizationWrapper(PersonAssociationsTreeModel treeModel, OrgHeader organization, OrgContact contact)
			: base(treeModel, organization)
		{
			this.Contact = contact;
		}

		public bool ViewAllowed
		{
			get { return Env.Security.OrgContactView.IsAllowed; }
		}

		public bool ViewWorkingAddressAllowed => Env.Security.PersonIntelligenceViewPrimaryWorkplace.IsAllowed;

		#region Overrides

		public override ZString Description => PrimarySource.CompanyName.IsEmpty ? organization.OH_FullName : PrimarySource.CompanyName;

		public override ZString Grouping => organization.OH_Code;

		public override ZString City => ViewWorkingAddressAllowed ? PrimarySource.City : ViewDeniedMessage;

		public override ZString State => ViewWorkingAddressAllowed ? PrimarySource.State : ViewDeniedMessage;

		public override ZString CreatedTime => Contact.OC_SystemCreateTimeUtc.ToBestReadableDateString();

		public override ZString Country => ViewWorkingAddressAllowed ? PrimarySource.Country : ViewDeniedMessage;

		public override ZString Email => ViewAllowed ? Contact.OC_Email : ViewDeniedMessage;

		public override ZBool Active => Contact.OC_IsActive;

		#region Primary Working Address

		public override ZBool IsPrimary
		{
			get => TreeModel.Person.PrimaryRelationship != null && TreeModel.Person.PrimaryRelationship.Primary != null && TreeModel.Person.PrimaryRelationship.Primary.PK == Contact.PK;
			set
			{
				if (value)
				{
					TreeModel.Person.SetPrimaryRelationship(Contact);
				}
				else
				{
					TreeModel.Person.RemovePrimaryRelationship();
				}
			}
		}

		public override ZBool IsPrimary_Visible => true;

		public override ZString WorkingAddressUNLOCO => ViewWorkingAddressAllowed ? PrimarySource.UNLOCO : ViewDeniedMessage;

		public IGlbPersonPrimarySource PrimarySource => Contact;

		#endregion

		#endregion
	}
}
