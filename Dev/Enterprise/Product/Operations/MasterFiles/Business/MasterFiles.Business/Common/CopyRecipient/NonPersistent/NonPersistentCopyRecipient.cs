using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class NonPersistentCopyRecipient : NonPersistentBusinessObject<NonPersistentCopyRecipientValidation>
	{
		public NonPersistentCopyRecipient(OrgHeader organization, ZString type)
		{
			Organization = organization;
			this.type = type;
		}

		#region EmailAddress

		[EmailAddress]
		[List("AvailableEmailAddressList")]
		public ZString EmailAddress
		{
			get { return emailAddress; }
			set
			{
				SetNonPersistentPropertyValue(EmailAddressInfo, ref emailAddress, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateEmailAddress();
				}
			}
		}

		public ZPropertyInfo EmailAddressInfo
		{
			get { return GetZPropertyInfo(nameof(EmailAddress), "Email Address"); }
		}

		ZString emailAddress;

		#endregion

		#region Organization

		public OrgHeader Organization { get; set; }

		#endregion

		#region Type

		public ZString Type
		{
			get { return type; }
		}

		readonly ZString type;

		#endregion

		#region Lookups

		public CodeDescriptionPairList AvailableEmailAddressList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (Organization != null)
				{
					foreach (var contact in Organization.ContactsActive.Cast<OrgContact>())
					{
						result.Add(new CodeDescriptionPair(contact.Email.ToString(), contact.OC_ContactName.ToString()));
					}
				}
				return result;
			}
		}

		#endregion

		#region Implementations

		public override NonPersistentCopyRecipientValidation GetNewValidation()
		{
			return new NonPersistentCopyRecipientValidation(this);
		}

		#endregion
	}
}