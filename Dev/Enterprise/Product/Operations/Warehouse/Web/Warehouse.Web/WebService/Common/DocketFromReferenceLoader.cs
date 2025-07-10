using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Common
{
	abstract class DocketFromReferenceLoader<TDocket>
		where TDocket : WhsDocket
	{
		public static ZDBOnlyQuery GetDocketIDQuery(WhsWarehouse warehouse, string docketID, string docketType)
		{
			var query = GetUnfinalisedDocketIDQuery(warehouse, docketID, docketType);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);

			return query;
		}

		public static ZDBOnlyQuery GetUnfinalisedDocketIDQuery(WhsWarehouse warehouse, string docketID, string docketType)
		{
			Argument.NotNull(warehouse, nameof(warehouse));

			var query = new ZDBOnlyQuery(typeof(TDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, docketType);
			query.AddToFilter(WhsDocketSchema.WD_DocketID, SQLComparisonOperator.Equal, docketID);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);

			return query;
		}
	}

	/// <summary>
	/// Used for the WebService to load a docket from various References
	/// This is Tested in WhsSecureService.asmx
	/// </summary>
	abstract class DocketFromReferenceLoader<TDocket, TResult> : DocketFromReferenceLoader<TDocket>
		where TDocket : WhsDocket
		where TResult : class
	{
		public DocketFromReferenceLoader(BusinessObjectFactory factory, WhsWarehouse warehouse, Guid? clientPK = null)
		{
			Factory = Argument.NotNull(factory, "BusinessObjectFactory factory");
			Warehouse = Argument.NotNull(warehouse, "WhsWarehouse warehouse");
			ClientPK = clientPK;
		}

		protected readonly BusinessObjectFactory Factory;
		protected readonly WhsWarehouse Warehouse;
		protected readonly Guid? ClientPK;

		#region LoadWhsDockets

		public TResult LoadWhsDockets(string reference, string docketDescription, string docketType, ZQuery additionalFilter)
		{
			TResult result = null;
			Argument.NotNullOrEmpty(reference, nameof(reference));

			var externalReferenceQuery = GetFullQuery(GetExternalReferenceQuery, Warehouse, reference, docketType, additionalFilter, ClientPK);
			result = GetResultFromExternalReference(externalReferenceQuery);

			if (IsResultEmpty(result))
			{
				var query = GetFullQuery(GetDocketIDQuery, Warehouse, reference, docketType, additionalFilter, ClientPK);
				result = GetResultFromID(query);
			}

			if (IsResultEmpty(result))
			{
				var query = GetFullQuery(GetReferenceQuery, Warehouse, reference, docketType, additionalFilter, ClientPK);
				result = GetResultFromReference(query);
			}

			if (IsResultEmpty(result))
			{
				var query = GetFullQuery(GetContainerNoQuery, Warehouse, reference, docketType, additionalFilter, ClientPK);
				result = GetResultFromContainerNo(query);
			}

			return result;
		}

		delegate ZDBOnlyQuery GetQueryDelegate(WhsWarehouse warehouse, string reference, string docketType);

		static ZDBOnlyQuery GetFullQuery(GetQueryDelegate getMainQuery, WhsWarehouse warehouse, string reference, string docketType, ZQuery additionalFilter, Guid? clientPK)
		{
			var query = getMainQuery(warehouse, reference, docketType);

			if (clientPK.HasValue)
			{
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, clientPK.Value);
			}

			if (additionalFilter != null)
			{
				query.AddToFilter(additionalFilter);
			}

			return query;
		}

		protected abstract bool IsResultEmpty(TResult result);
		protected abstract TResult GetResultFromID(ZDBOnlyQuery query);
		protected abstract TResult GetResultFromExternalReference(ZDBOnlyQuery query);
		protected abstract TResult GetResultFromReference(ZDBOnlyQuery query);
		protected abstract TResult GetResultFromContainerNo(ZDBOnlyQuery query);

		#region GetExternalReferenceQuery

		static ZDBOnlyQuery GetExternalReferenceQuery(WhsWarehouse warehouse, string externalReference, string docketType)
		{
			var query = new ZDBOnlyQuery(typeof(TDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, docketType);
			query.AddToFilter(WhsDocketSchema.WD_ExternalReference, SQLComparisonOperator.Equal, externalReference);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			return query;
		}

		#endregion

		#region GetReferenceQuery

		static ZDBOnlyQuery GetReferenceQuery(WhsWarehouse warehouse, string reference, string docketType)
		{
			var query = new ZDBOnlyQuery(typeof(TDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, docketType);

			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketReference), WhsDocketReferenceSchema.WX_WD);
			subQuery.AddToFilter(WhsDocketReferenceSchema.WX_Reference, SQLComparisonOperator.Equal, reference);
			query.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			query.OrderBy = WhsDocketSchema.WD_DocketID.Name;

			return query;
		}

		#endregion

		#region GetContainerNoQuery

		static ZDBOnlyQuery GetContainerNoQuery(WhsWarehouse warehouse, string containerNo, string docketType)
		{
			var query = new ZDBOnlyQuery(typeof(TDocket));
			query.AddToFilter(WhsDocketSchema.WD_DocketType, docketType);

			var subQuery = new ZDBOnlySubQuery(typeof(WhsDocketContainer), WhsDocketContainerSchema.WC_WD);
			subQuery.AddToFilter(WhsDocketContainerSchema.WC_ContainerNum, SQLComparisonOperator.Equal, containerNo);
			query.AddSubQuery(subQuery, JoinCondition.And);

			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.Equal, warehouse.PK);
			query.AddToFilter(JoinCondition.And, WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			query.OrderBy = WhsDocketSchema.WD_DocketID.Name;

			return query;
		}

		#endregion

		#endregion
	}

	#region class DocketFromReferenceToDocketLoader

	class DocketFromReferenceToDocketLoader<TDocket> : DocketFromReferenceLoader<TDocket, TDocket>
		where TDocket : WhsDocket
	{
		public DocketFromReferenceToDocketLoader(BusinessObjectFactory factory, WhsWarehouse warehouse, Guid? clientPK = null)
			: base(factory, warehouse, clientPK)
		{
		}

		protected override bool IsResultEmpty(TDocket result)
		{
			return result == null;
		}

		protected override TDocket GetResultFromID(ZDBOnlyQuery query)
		{
			return Factory.LoadTop1<TDocket>(query);
		}

		protected override TDocket GetResultFromExternalReference(ZDBOnlyQuery query)
		{
			return Factory.LoadTop1<TDocket>(query);
		}

		protected override TDocket GetResultFromReference(ZDBOnlyQuery query)
		{
			return Factory.LoadTop1<TDocket>(query);
		}

		protected override TDocket GetResultFromContainerNo(ZDBOnlyQuery query)
		{
			return Factory.LoadTop1<TDocket>(query);
		}
	}

	#endregion

	#region class DocketFromReferenceToArrayLoader

	class DocketFromReferenceToArrayLoader<TDocket> : DocketFromReferenceLoader<TDocket, TDocket[]>
		where TDocket : WhsDocket
	{
		public DocketFromReferenceToArrayLoader(BusinessObjectFactory factory, WhsWarehouse warehouse)
			: base(factory, warehouse)
		{
		}

		protected override bool IsResultEmpty(TDocket[] result)
		{
			return result == null || result.Length == 0;
		}

		protected override TDocket[] GetResultFromID(ZDBOnlyQuery query)
		{
			return Factory.Load<TDocket>(query);
		}

		protected override TDocket[] GetResultFromExternalReference(ZDBOnlyQuery query)
		{
			query.OrderBy = WhsDocketSchema.WD_DocketID.Name + OrderByClause.Descending;
			return Factory.Load<TDocket>(query);
		}

		protected override TDocket[] GetResultFromReference(ZDBOnlyQuery query)
		{
			return Factory.Load<TDocket>(query);
		}

		protected override TDocket[] GetResultFromContainerNo(ZDBOnlyQuery query)
		{
			return Factory.Load<TDocket>(query);
		}
	}

	#endregion
}
