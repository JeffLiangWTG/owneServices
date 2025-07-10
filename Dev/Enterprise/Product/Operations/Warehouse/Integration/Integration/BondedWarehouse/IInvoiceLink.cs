using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	// implement on JobDec to return IInvoiceLineLink
	public interface IInvoiceLinkProvider : IBusiness
	{
		IInvoiceLink Link { get; }
	}

	// warehouse plug-in to ExWarehouse casts BusinessEntity to Provider, then gets link, calls CreateOrUpdate
	public interface IInvoiceLink
	{
		void CreateOrUpdateInvoices(IWhsBondedWarehouseTransaction entry);

		ZGuid DeclarationPK { get; }
		IOrgHeader Importer { get; }
		ZString OwnersReference { get; }

		ZString EntryKeyTitle { get; }
		ZBool IsExWarehouse { get; }
		event IsExWarehouseChangedEvent IsExWarehouseChanged;

		//void NotifyOfEntryCancellation();
		//void NotifyOfEntryCreation();
		void NotifyOfOrderCancellation();
		void NotifyOfOrderCreation();
	}

	public delegate void IsExWarehouseChangedEvent();
}
