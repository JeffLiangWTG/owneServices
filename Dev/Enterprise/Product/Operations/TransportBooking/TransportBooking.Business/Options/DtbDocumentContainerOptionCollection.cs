using CargoWise.EntityFramework;

namespace Enterprise.TransportBookings.Business.Options
{
	public class DtbDocumentContainerOptionCollection : NonPersistentBusinessObjectCollection<DtbDocumentContainerOption>
	{
		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DtbDocumentContainerOption("", "", "", 0, "");
		}
	}
}
