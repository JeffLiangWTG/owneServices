using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public abstract class WarehouseDocketDataContextManager<TDocket> : ShipmentDataContextManager<TDocket>
		where TDocket : WhsDocket
	{
		public override sealed ZString DataContextKey => ParentBO.WD_DocketID;

		public override sealed string DefaultOutputDirectory => SystemDataRegistry.Instance.WarehouseExportDirectory.Value;

		public override sealed bool ManagesShipments => true;

		protected override sealed ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery(WhsDocketSchema.WD_DocketID, matchingValues.Key);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketTypeCode);
			return query;
		}

		protected abstract string DocketTypeCode { get; }
	}
}
