using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class TrackingTransactionHeaderCollection : InvoicingBaseCollection
	{
		public TrackingTransactionHeaderCollection(BusinessObjectFactory factory, ZQuery filter) : base(factory, filter)
		{
		}

		public TrackingTransactionHeaderCollection(BusinessObjectFactory factory) : this(factory, new ZQuery())
		{
		}

		#region Overrides

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery newRelationshipQuery = new ZQuery();

			ZQuery transactionTypeFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.CreditNote);
			transactionTypeFilter.AddToFilter(JoinCondition.Or, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Invoice);

			ZQuery nonCancelledTransactionFilter = new ZQuery(AccTransactionHeaderSchema.AH_IsCancelled, SQLComparisonOperator.Equal, ZBool.False);

			newRelationshipQuery.AddToFilter(transactionTypeFilter);
			newRelationshipQuery.AddToFilter(nonCancelledTransactionFilter);
			newRelationshipQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			return newRelationshipQuery;
		}

		#endregion Overrides
	}
}
