using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	class OrgContainerPenaltyMatcher : IContainerPenaltyMatcher
	{
		public PenaltyMatcherType MatcherType => PenaltyMatcherType.Organization;

		#region MatchCTOStorage

		public IContainerPenaltyMatchResult MatchStorage(IContainerPenaltyMatchFilter filter)
		{
			var factory = ((filter.Carrier ?? filter.Client) ?? filter.CTO)?.Factory;

			if (factory != null)
			{
				return LoadDetention(factory, filter, GetCTOStorageQuery);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be SQL expression")]
		ZQuery GetCTOStorageQuery(IContainerPenaltyMatchFilter filter)
		{
			var penaltyType = ContainerDetentionPenaltyType.STO;
			var query = new ZQuery();
			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToArray();

			query.AddToFilter(OrgContainerDetentionSchema.PD_Direction, filter.Direction);
			query.AddToFilter(OrgContainerDetentionSchema.PD_PenaltyType, penaltyType);
			query.AddToFilter(OrgContainerDetentionSchema.PD_CreditorType, filter.CreditorType);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Carrier, (filter.Carrier == null) ? ZGuid.Empty : filter.Carrier.PK);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Client, clientPks);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_CTO, (filter.CTO == null) ? ZGuid.Empty : filter.CTO.PK);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_ContainerType, filter.ContainerClass);
			AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_DetentionPortOrCountry, filter.DetentionPort);

			if (filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Pickup || filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery)
			{
				if (filter.Direction == ContainerDetentionDirection.Import)
				{
					AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OriginPortOrCountry, filter.OriginPort);

					query.OrderBy = string.Format((NoResString)"{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
						OrgContainerDetentionSchema.PD_OH_Client.Name,
						OrgContainerDetentionSchema.PD_OH_Carrier.Name,
						OrgContainerDetentionSchema.PD_OH_CTO.Name,
						OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_ContainerType.Name);
				}
				else
				{
					query.OrderBy = string.Format((NoResString)"{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
						OrgContainerDetentionSchema.PD_OH_Client.Name,
						OrgContainerDetentionSchema.PD_OH_Carrier.Name,
						OrgContainerDetentionSchema.PD_OH_CTO.Name,
						OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_ContainerType.Name);
				}
			}
			else
			{
				if (filter.Direction == ContainerDetentionDirection.Import)
				{
					AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OriginPortOrCountry, filter.OriginPort);

					query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
						OrgContainerDetentionSchema.PD_OH_Carrier.Name,
						OrgContainerDetentionSchema.PD_OH_Client.Name,
						OrgContainerDetentionSchema.PD_OH_CTO.Name,
						OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_ContainerType.Name);
				}
				else
				{
					query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
						OrgContainerDetentionSchema.PD_OH_Carrier.Name,
						OrgContainerDetentionSchema.PD_OH_Client.Name,
						OrgContainerDetentionSchema.PD_OH_CTO.Name,
						OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name,
						OrgContainerDetentionSchema.PD_ContainerType.Name);
				}
			}

			return query;
		}

		#endregion

		#region Match Detention

		public IContainerPenaltyMatchResult MatchDetention(IContainerPenaltyMatchFilter filter)
		{
			var factory = (filter.Carrier ?? filter.Client)?.Factory;

			if (factory != null)
			{
				return LoadDetention(factory, filter, GetDetentionQuery);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be SQL expression")]
		ZQuery GetDetentionQuery(IContainerPenaltyMatchFilter filter)
		{
			var query = new ZQuery();
			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToArray();

			query.AddToFilter(OrgContainerDetentionSchema.PD_Direction, filter.Direction);
			query.AddToFilter(OrgContainerDetentionSchema.PD_PenaltyType, ContainerDetentionPenaltyType.DET);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Carrier, (filter.Carrier == null) ? ZGuid.Empty : filter.Carrier.PK);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Client, clientPks);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_ContainerType, filter.ContainerClass);
			AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OriginPortOrCountry, filter.OriginPort);
			AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_DetentionPortOrCountry, filter.DetentionPort);

			if (filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Pickup || filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery)
			{
				query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
					OrgContainerDetentionSchema.PD_OH_Client.Name,
					OrgContainerDetentionSchema.PD_OH_Carrier.Name,
					OrgContainerDetentionSchema.PD_ContainerType.Name,
					OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
					OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name);
			}
			else
			{
				query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
					OrgContainerDetentionSchema.PD_OH_Carrier.Name,
					OrgContainerDetentionSchema.PD_OH_Client.Name,
					OrgContainerDetentionSchema.PD_ContainerType.Name,
					OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
					OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name);
			}

			return query;
		}

		#endregion

		#region Match MDD

		public IContainerPenaltyMatchResult MatchMDD(IContainerPenaltyMatchFilter filter)
		{
			var factory = (filter.Carrier ?? filter.Client)?.Factory;

			if (factory != null)
			{
				return LoadDetention(factory, filter, GetMDDQuery);
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be SQL expression")]
		ZQuery GetMDDQuery(IContainerPenaltyMatchFilter filter)
		{
			var query = new ZQuery();
			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToArray();

			query.AddToFilter(OrgContainerDetentionSchema.PD_Direction, filter.Direction);
			query.AddToFilter(OrgContainerDetentionSchema.PD_PenaltyType, ContainerDetentionPenaltyType.MDD);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Carrier, (filter.Carrier == null) ? ZGuid.Empty : filter.Carrier.PK);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OH_Client, clientPks);
			AddEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_ContainerType, filter.ContainerClass);
			AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_OriginPortOrCountry, filter.OriginPort);
			AddLocationEqualsOrIsEmptyFilter(query, OrgContainerDetentionSchema.PD_DetentionPortOrCountry, filter.DetentionPort);

			if (filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Pickup || filter.ProcessType == Core.Constants.ContainerPenaltyProcessType.Delivery)
			{
				query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
					OrgContainerDetentionSchema.PD_OH_Client.Name,
					OrgContainerDetentionSchema.PD_OH_Carrier.Name,
					OrgContainerDetentionSchema.PD_ContainerType.Name,
					OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
					OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name);
			}
			else
			{
				query.OrderBy = string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
					OrgContainerDetentionSchema.PD_OH_Carrier.Name,
					OrgContainerDetentionSchema.PD_OH_Client.Name,
					OrgContainerDetentionSchema.PD_ContainerType.Name,
					OrgContainerDetentionSchema.PD_OriginPortOrCountry.Name,
					OrgContainerDetentionSchema.PD_DetentionPortOrCountry.Name);
			}

			return query;
		}

		#endregion

		public IEnumerable<IContainerPenaltyMatchResult> MatchPenalties(IEnumerable<IContainerPenaltyMatchFilter> detentionFilters, IEnumerable<IContainerPenaltyMatchFilter> storageFilters, IEnumerable<IContainerPenaltyMatchFilter> mddFilters)
		{
			var carrierResults = new List<IContainerPenaltyMatchResult>();
			var clientResults = new List<IContainerPenaltyMatchResult>();

			foreach (var filter in detentionFilters)
			{
				if (MatchDetention(filter) is OrgContainerDetention containerDetention)
				{
					if (!containerDetention.PD_OH_Carrier.IsEmpty && containerDetention.PD_OH_Client.IsEmpty)
					{
						carrierResults.Add(containerDetention);
					}
					else
					{
						clientResults.Add(containerDetention);
					}
				}
			}

			foreach (var filter in storageFilters)
			{
				filter.CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier;
				if (MatchStorage(filter) is OrgContainerDetention containerDetention)
				{
					if (!containerDetention.PD_OH_Carrier.IsEmpty && containerDetention.PD_OH_Client.IsEmpty)
					{
						carrierResults.Add(containerDetention);
					}
					else
					{
						clientResults.Add(containerDetention);
					}
				}
			}

			foreach (var filter in storageFilters)
			{
				filter.CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.CTO;
				if (MatchStorage(filter) is OrgContainerDetention containerDetention)
				{
					clientResults.Add(containerDetention);
				}
			}

			foreach (var filter in mddFilters)
			{
				if (MatchMDD(filter) is OrgContainerDetention containerDetention)
				{
					if (!containerDetention.PD_OH_Carrier.IsEmpty && containerDetention.PD_OH_Client.IsEmpty)
					{
						carrierResults.Add(containerDetention);
					}
					else
					{
						clientResults.Add(containerDetention);
					}
				}
			}

			carrierResults.AddRange(clientResults);
			return carrierResults;
		}

		static void AddEqualsOrIsEmptyFilter(ZQuery query, SchemaColumn column, IZType value)
		{
			var equalsOrEmptyQuery = new ZQuery();
			equalsOrEmptyQuery.AddToFilter(column, column is SchemaGuidColumn ? null : (IZType)ZString.Empty);
			equalsOrEmptyQuery.AddToFilter(JoinCondition.Or, column, value);
			query.AddToFilter(equalsOrEmptyQuery, JoinCondition.And);
		}

		static void AddEqualsOrIsEmptyFilter(ZQuery query, SchemaColumn column, IList<ZGuid> value)
		{
			var equalsOrEmptyQuery = new ZQuery();
			equalsOrEmptyQuery.AddToFilter(column, column is SchemaGuidColumn ? null : (IZType)ZString.Empty);
			equalsOrEmptyQuery.AddToFilter(JoinCondition.Or, column, value);
			query.AddToFilter(equalsOrEmptyQuery, JoinCondition.And);
		}

		static void AddLocationEqualsOrIsEmptyFilter(ZQuery query, SchemaStringColumn column, ZString location)
		{
			var locationQuery = new ZQuery();
			locationQuery.AddToFilter(column, ZString.Empty);
			locationQuery.AddToFilter(JoinCondition.Or, column, location);
			locationQuery.AddToFilter(JoinCondition.Or, column, location.SubstringSafe(0, 2));
			query.AddToFilter(locationQuery, JoinCondition.And);
		}

		static OrgContainerDetention LoadDetention(BusinessObjectFactory factory, IContainerPenaltyMatchFilter filter, Func<IContainerPenaltyMatchFilter, ZQuery> getQuery)
		{
			var query = getQuery(filter);
			var detentions = factory.Load<OrgContainerDetention>(query);
			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToList();
			clientPks.Add(ZGuid.Empty);

			foreach (var clientPk in clientPks)
			{
				OrgContainerDetention detention = null;

				if (detentions.Length <= 1)
				{
					detention = detentions.FirstOrDefault();
				}
				else
				{
					detention = detentions.FirstOrDefault(d => d.PD_OH_Client == clientPk);
				}

				if (detention != null && IsDetentionValidForProcessType(detention, filter.ProcessType))
				{
					return detention;
				}
			}

			return null;
		}

		static bool IsDetentionValidForProcessType(OrgContainerDetention detention, ZString processType)
		{
			if (processType == Core.Constants.ContainerPenaltyProcessType.Pickup || processType == Core.Constants.ContainerPenaltyProcessType.Delivery)
			{
				return detention.PD_OH_Carrier == ZGuid.Empty && detention.PD_OH_Client == ZGuid.Empty || detention.PD_OH_Client != ZGuid.Empty;
			}
			else
			{
				return detention.PD_OH_Carrier != ZGuid.Empty && detention.PD_OH_Client != ZGuid.Empty || detention.PD_OH_Client == ZGuid.Empty;
			}
		}
	}
}
