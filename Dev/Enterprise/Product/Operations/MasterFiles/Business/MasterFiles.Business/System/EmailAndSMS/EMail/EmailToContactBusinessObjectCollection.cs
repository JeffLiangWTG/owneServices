using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class EmailToContactBusinessObjectCollection : NonPersistentBusinessObjectCollection<EmailToContactBusinessObject>
	{
		public EmailToContactBusinessObjectCollection(BusinessObject businessObjectSendingEmail) : base(businessObjectSendingEmail.Factory)
		{
			this.BusinessObjectSendingEmail = businessObjectSendingEmail;
		}

		readonly BusinessObject BusinessObjectSendingEmail;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EmailToContactBusinessObject(BusinessObjectSendingEmail);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
