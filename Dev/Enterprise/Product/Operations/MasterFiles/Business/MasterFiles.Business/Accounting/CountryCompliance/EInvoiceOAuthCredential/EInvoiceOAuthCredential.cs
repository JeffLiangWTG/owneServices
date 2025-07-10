using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class EInvoiceOAuthCredential : NonPersistentBusinessObject
	{
		public ZString Status { get; set; }

		public ZDateTime AuthorizationDate { get; set; }

		public ZDateTime IssueDate { get; set; }

		public ZDateTime ExpiryDate { get; set; }

		public ZString Description { get; set; }
	}
}
