using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public abstract class ContractPenaltyMatcher : IContainerPenaltyMatcher
	{
		public abstract PenaltyMatcherType MatcherType { get; }

		public IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter) => MatchPenalty(filter, GetDetentionPenaltiesQuery);

		public IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter) => MatchPenalty(filter, GetStoragePenaltiesQuery);

		public IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter) => MatchPenalty(filter, GetMddPenaltiesQuery);

		public IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters)
		{
			var primaryResults = new List<IContainerPenaltyMatchResult>();
			var secondaryResults = new List<IContainerPenaltyMatchResult>();

			foreach (var filter in detentionFilters)
			{
				var result = MatchDetention(filter);
				if (result != null)
				{
					if (IsClientPenaltyWithCarrier(result))
					{
						primaryResults.Add(result);
					}
					else
					{
						secondaryResults.Add(result);
					}
				}
			}

			foreach (var filter in storageFilters)
			{
				filter.CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
				var result = MatchStorage(filter);
				if (result != null)
				{
					if (IsClientPenaltyWithCarrier(result))
					{
						primaryResults.Add(result);
					}
					else
					{
						secondaryResults.Add(result);
					}
				}
			}

			foreach (var filter in mddFilters)
			{
				var result = MatchMDD(filter);
				if (result != null)
				{
					if (IsClientPenaltyWithCarrier(result))
					{
						primaryResults.Add(result);
					}
					else
					{
						secondaryResults.Add(result);
					}
				}
			}

			primaryResults.AddRange(secondaryResults);
			return primaryResults;
		}

		IContainerPenaltyMatchResult MatchPenalty(IContainerPenaltyMatchFilter filter, Func<IContainerPenaltyMatchFilter, ZDateTime, IRatingContract, ZQuery> getQuery)
		{
			if (!IsValidProcessType(filter))
			{
				return null;
			}

			if (filter.Container == null)
			{
				return null;
			}

			var departureDate = GetDepartureDateTime(filter);
			var contract = GetRatingContract(filter, departureDate, getQuery.Method.Name != nameof(GetStoragePenaltiesQuery));
			if (contract == null)
			{
				return null;
			}

			return LoadDetention(filter, departureDate, contract, getQuery) as IContainerPenaltyMatchResult;
		}

		protected virtual IRatingContractContainerDetention LoadDetention(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract, Func<IContainerPenaltyMatchFilter, ZDateTime, IRatingContract, ZQuery> getQuery)
		{
			var factory = (contract as BusinessObject)?.Factory;
			var query = getQuery(filter, departureDate, contract);
			return factory?.LoadTop1<IRatingContractContainerDetention>(query);
		}

		protected abstract bool IsValidProcessType(IContainerPenaltyMatchFilter filter);

		protected abstract ZDateTime GetDepartureDateTime(IContainerPenaltyMatchFilter filter);

		#region Contract Query

		IRatingContract GetRatingContract(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, bool isMatchDetention)
		{
			if (!departureDate.IsValidSqlDateTime || !IsValidCreditorType(isMatchDetention, filter.CreditorType))
			{
				return null;
			}

			var consol = (filter.Container as CommonContainer)?.Consol;
			if (consol == null)
			{
				return null;
			}

			var contractQuery = GetContractQuery(filter, departureDate);
			var matchingContracts = consol.Factory.Load<IRatingContract>(contractQuery);
			return SelectContract(matchingContracts, filter);
		}

		bool IsValidCreditorType(bool isMatchDetention, string creditorType)
			=> isMatchDetention || creditorType == Constants.ContainerPenaltyCreditorType.Codes.Carrier;

		ZQuery GetContractQuery(IContainerPenaltyMatchFilter filter, ZDateTime departureDate)
		{
			var contractQuery = new ZQuery();
			AddContractQueryFilters(contractQuery, filter, departureDate);

			contractQuery.OrderBy = string.Format((NoResString)"{0} DESC", RatingContractSchema.RCT_StartDate.Name); // SQL Expression

			return contractQuery;
		}

		protected virtual void AddContractQueryFilters(ZQuery contractQuery, IContainerPenaltyMatchFilter filter, ZDateTime departureDate)
		{
			contractQuery.AddToFilter(RatingContractSchema.RCT_OH, GetContractOrgPKs(filter));
			contractQuery.AddToFilter(RatingContractSchema.RCT_ContractNumber, GetContractNumber(filter));
			contractQuery.AddToFilter(RatingContractSchema.RCT_IsActive, 1);
			contractQuery.AddToFilter(RatingContractSchema.RCT_ContractType, ContractType);
			contractQuery.AddToFilter(RatingContractSchema.RCT_StartDate, SQLComparisonOperator.LessThanOrEqualTo, departureDate);

			var endDateQuery = new ZQuery(RatingContractSchema.RCT_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, departureDate);
			endDateQuery.AddToFilter(JoinCondition.Or, RatingContractSchema.RCT_EndDate, ZDate.Empty);

			contractQuery.AddToFilter(endDateQuery);
		}

		protected abstract IEnumerable<ZGuid> GetContractOrgPKs(IContainerPenaltyMatchFilter filter);

		protected abstract ZString GetContractNumber(IContainerPenaltyMatchFilter filter);

		protected abstract string ContractType { get; }

		protected abstract IRatingContract SelectContract(IEnumerable<IRatingContract> contracts, IContainerPenaltyMatchFilter filter);

		#endregion

		#region Detention Penalties Query

		ZQuery GetDetentionPenaltiesQuery(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			var query = GetCommonPenaltiesQuery(Constants.ContainerPenaltyPenaltyType.Codes.Detention, filter, departureDate, contract);
			AddLocationEqualsOrIsEmptyFilter(query, RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry, filter.OriginPort);

			query.OrderBy = DetentionPenaltiesQueryOrderBy;

			return query;
		}

		protected abstract string DetentionPenaltiesQueryOrderBy { get; }

		#endregion

		#region Storage Penalties Query

		ZQuery GetStoragePenaltiesQuery(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			var query = GetCommonPenaltiesQuery(Constants.ContainerPenaltyPenaltyType.Codes.Storage, filter, departureDate, contract);

			if (filter.Direction == Constants.ContainerDetentionDirection.Import)
			{
				AddLocationEqualsOrIsEmptyFilter(query, RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry, filter.OriginPort);
				query.OrderBy = StorageImportPenaltiesQueryOrderBy;
			}
			else
			{
				query.OrderBy = StorageExportPenaltiesQueryOrderBy;
			}

			return query;
		}

		protected abstract string StorageImportPenaltiesQueryOrderBy { get; }

		protected abstract string StorageExportPenaltiesQueryOrderBy { get; }

		#endregion

		#region MDD Penalties Query

		ZQuery GetMddPenaltiesQuery(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			var query = GetCommonPenaltiesQuery(Constants.ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention, filter, departureDate, contract);
			AddLocationEqualsOrIsEmptyFilter(query, RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry, filter.OriginPort);

			query.OrderBy = MddPenaltiesQueryOrderBy;

			return query;
		}

		protected abstract string MddPenaltiesQueryOrderBy { get; }

		#endregion

		#region Common Penalties Query

		ZQuery GetCommonPenaltiesQuery(ZString penaltyType, IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			var commonPenaltyQuery = new ZQuery();
			AddCommonPenaltyQueryFilters(commonPenaltyQuery, penaltyType, filter, departureDate, contract);

			return commonPenaltyQuery;
		}

		protected virtual void AddCommonPenaltyQueryFilters(ZQuery commonPenaltyQuery, ZString penaltyType, IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			commonPenaltyQuery.AddToFilter(RatingContractContainerDetentionSchema.RCD_RCT, contract.PK);
			commonPenaltyQuery.AddToFilter(RatingContractContainerDetentionSchema.RCD_Direction, filter.Direction);
			commonPenaltyQuery.AddToFilter(RatingContractContainerDetentionSchema.RCD_PenaltyType, penaltyType);
			AddOperatorOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_StartDateOverride, SQLComparisonOperator.LessThanOrEqualTo, departureDate);
			AddOperatorOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_EndDateOverride, SQLComparisonOperator.GreaterThanOrEqualTo, departureDate);
			AddEqualsOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_ContainerType, filter.ContainerClass);
			AddLocationEqualsOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry, filter.DetentionPort);
		}

		#endregion

		protected void AddEqualsOrIsEmptyFilter(ZQuery query, SchemaColumn column, IZType value)
		{
			AddOperatorOrIsEmptyFilter(query, column, SQLComparisonOperator.Equal, value);
		}

		void AddOperatorOrIsEmptyFilter(ZQuery query, SchemaColumn column, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var equalsOrEmptyQuery = new ZQuery();
			equalsOrEmptyQuery.AddToFilter(column, column is SchemaStringColumn ? (IZType)ZString.Empty : null);
			equalsOrEmptyQuery.AddToFilter(JoinCondition.Or, column, comparisonOperator, value);
			query.AddToFilter(equalsOrEmptyQuery, JoinCondition.And);
		}

		protected void AddEqualsOrIsEmptyFilter(ZQuery query, SchemaColumn column, IList<ZGuid> value)
		{
			var equalsOrEmptyQuery = new ZQuery();
			equalsOrEmptyQuery.AddToFilter(column, column is SchemaGuidColumn ? null : (IZType)ZString.Empty);
			equalsOrEmptyQuery.AddToFilter(JoinCondition.Or, column, value);
			query.AddToFilter(equalsOrEmptyQuery, JoinCondition.And);
		}

		void AddLocationEqualsOrIsEmptyFilter(ZQuery query, SchemaStringColumn column, ZString location)
		{
			var locationQuery = new ZQuery();
			locationQuery.AddToFilter(column, ZString.Empty);
			locationQuery.AddToFilter(JoinCondition.Or, column, location);

			if (location.Length >= 2)
			{
				locationQuery.AddToFilter(JoinCondition.Or, column, location.Substring(0, 2));
			}

			query.AddToFilter(locationQuery, JoinCondition.And);
		}

		bool IsClientPenaltyWithCarrier(IContainerPenaltyMatchResult matchResult)
		{
			var penalty = matchResult as IRatingContractContainerDetention;
			return MatcherType == PenaltyMatcherType.ClientContract && !penalty.RCD_OH_Client.IsEmpty;
		}
	}
}
