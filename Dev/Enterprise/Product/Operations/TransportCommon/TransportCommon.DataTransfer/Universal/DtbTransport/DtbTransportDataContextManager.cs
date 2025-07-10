using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.TransportCommon.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.DataTransfer.Universal
{
	public abstract class DtbTransportDataContextManager<T> : ShipmentDataContextManager<T>, IShipmentDataContextManager
		where T : DtbTransport
	{
		#region Context

		public override ZString DataContextKey
		{
			get { return ParentBO.KM_JobID; }
		}

		protected sealed override ZQuery GetDataContextKeyMatchingQuery(IDataContextMatchingKey matchingValues, BusinessObjectFactory factory, IXmlImportLogger logger)
		{
			var query = new ZDBOnlyQuery(typeof(T));
			query.AddToFilter(DtbBookingSchema.KM_JobID, matchingValues.Key);

			var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbTransportConsolidation), DtbBookingSchema.KM_KB_Booking);
			consolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, GetConsolidationJobTypes());
			query.AddSubQuery(consolidationSubQuery, JoinCondition.And);

			return query;
		}

		protected abstract string[] GetConsolidationJobTypes();

		public override string DefaultOutputDirectory
		{
			get { return null; }
		}

		#endregion

		#region ManagesShipments

		public override bool ManagesShipments
		{
			get { return true; }
		}

		#endregion

		#region Recipient Roles

		protected sealed override bool RecipientRoleTargettedToThisModule(IEnumerable<IRecipientRoleDataObject> recipientRoles, IEnumerable<IDataSourceDataObject> dataSources, IXmlSessionTracker importSessionLogger)
		{
			return false;
		}

		#endregion
	}
}
