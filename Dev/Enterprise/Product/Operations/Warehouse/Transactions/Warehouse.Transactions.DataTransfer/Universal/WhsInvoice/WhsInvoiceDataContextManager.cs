using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsInvoiceDataContextManager : ShipmentDataContextManager<WhsInvoice>
	{
		public WhsInvoiceDataContextManager()
			: base()
		{
		}

		public override ZString DataContextKey => ParentBO.ET_StorageJobNumber;
		public override bool ManagesEvents => false;
		public override bool ManagesShipments => WarehouseDataRegistry.Instance.EnableWhsPeriodicInvoiceXUT.Value;
		public override DataContextType DataContextType => DataContextType.WarehousePeriodicInvoice;
		public override string DefaultOutputDirectory => null;

		protected override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return new ZQuery(JobStorageSchema.ET_StorageJobNumber, matchingValues.Key);
		}

		protected override ITopLevelDataObjectReader GetShipmentDataObjectReader(Shipment shipment, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new WhsInvoiceDataObjectReader(shipment, logger, factory);
		}

		protected override ITopLevelDataObjectWriter GetShipmentDataObjectWriter(IDataWritingManager writeManager)
		{
			return new WhsInvoiceDataObjectWriter(writeManager);
		}

		protected override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		protected override IEnumerable<KeyValuePair<TypeWithDescription, IZType>> GetEventContextValues()
		{
			return Enumerable.Empty<KeyValuePair<TypeWithDescription, IZType>>();
		}

		protected override EventParentFinder GetEventParentFinder(BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			return null;
		}
	}
}
