using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ForwardingShipmentCusEntryNumberProxyCollection : NonPersistentBusinessObjectCollection<ForwardingShipmentCusEntryNumberProxy>
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual methods are reviewed. All possible values are expected and handled.")]
		public ForwardingShipmentCusEntryNumberProxyCollection(ForwardingShipment shipment)
			: base(shipment != null ? shipment.Factory : null)
		{
			Argument.NotNull(shipment, "shipment");
			Shipment = shipment;
			foreach (CusEntryNumber cusEntryNumber in shipment.CusEntryNumbers)
			{
				Add(new ForwardingShipmentCusEntryNumberProxy(shipment, cusEntryNumber));
			}
		}

		readonly ForwardingShipment Shipment;

		protected override bool AllowNewCore
		{
			get { return true; }
		}

		protected override bool AllowRemoveCore
		{
			get { return true; }
		}

		#region Remove

		protected override void OnRemoved(BusinessObject elementToDelete)
		{
			base.OnRemoved(elementToDelete);
			RemoveCustomsEntryNumber((ForwardingShipmentCusEntryNumberProxy)elementToDelete);
		}

		void RemoveCustomsEntryNumber(ForwardingShipmentCusEntryNumberProxy entryNumberProxy)
		{
			if (entryNumberProxy != null)
			{
				entryNumberProxy.CusEntryNumber.Delete();

				entryNumberProxy.Shipment.CustomsEntryNumberInfo.RefreshBinding();
				entryNumberProxy.Shipment.CustomsEntryNumberTypeInfo.RefreshBinding();
				entryNumberProxy.Shipment.CustomsEntryNumberIssueDateInfo.RefreshBinding();
				entryNumberProxy.Shipment.CustomsEntryNumberExpiryDateInfo.RefreshBinding();
			}
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var cusEntryNumber = Shipment.CusEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryIsSystemGenerated = false;
			cusEntryNumber.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumber.CE_ParentID = Shipment.PK;
			return new ForwardingShipmentCusEntryNumberProxy(Shipment, cusEntryNumber);
		}
	}
}
