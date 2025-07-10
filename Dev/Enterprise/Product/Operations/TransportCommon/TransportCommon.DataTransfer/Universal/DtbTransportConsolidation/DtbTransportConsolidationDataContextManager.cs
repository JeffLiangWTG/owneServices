using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportConsolidationDataContextManager<T> : ShipmentDataContextManager<T>
		where T : DtbTransportConsolidation
	{
		#region Context

		public sealed override ZString DataContextKey
		{
			get { return ParentBO.KB_JobID; }
		}

		protected sealed override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZQuery();
			query.AddToFilter(DtbBookingConsolidationSchema.KB_JobID, matchingValues.Key);
			query.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, GetConsolidationJobTypes());

			return query;
		}

		protected abstract string[] GetConsolidationJobTypes();

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region Shipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#endregion
	}
}
