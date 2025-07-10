using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class DocumentPickupDeliveryConfirmOptions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentPickupDeliveryConfirmOptions(DocumentPickupDeliveryConfirmCollection confirms)
			: base(confirms.Factory)
		{
			this.confirms = confirms;
		}

		#region CartageLeg

		public DocumentPickupDeliveryConfirmCollection Confirms
		{
			get { return confirms; }
		}
		readonly DocumentPickupDeliveryConfirmCollection confirms;

		#endregion
	}
}
