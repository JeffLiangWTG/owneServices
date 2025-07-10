using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business
{
	public class ExternalValidationResultMessageCollection : NonPersistentBusinessObjectCollection<ExternalValidationResultMessage>
	{
		public ExternalValidationResultMessageCollection() : base() { }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ExternalValidationResultMessage("", "");
		}
	}
}
