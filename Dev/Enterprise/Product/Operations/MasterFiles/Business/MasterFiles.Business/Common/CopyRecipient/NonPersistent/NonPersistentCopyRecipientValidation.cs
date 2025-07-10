using System;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class NonPersistentCopyRecipientValidation : ZValidation
	{
		public NonPersistentCopyRecipientValidation(NonPersistentCopyRecipient parent) : base(parent)
		{
			this.parent = parent;
		}

		#region EmailAddress

		public void ValidateEmailAddress()
		{
			ValidateCalculatedProperty(parent.EmailAddressInfo);
		}

		protected void CheckEmailAddress()
		{
			MandatoryValidation.CheckEntered(parent.EmailAddressInfo);
			EmailAddressValidation.ValidateEmailAddress(parent.EmailAddressInfo);
		}

		#endregion

		#region Implementations

		public override void ValidateAll()
		{
			ValidateEmailAddress();
		}

		public override Type AutoValidationType
		{
			get { return typeof(NonPersistentCopyRecipientValidation); }
		}

		readonly NonPersistentCopyRecipient parent;

		#endregion
	}
}