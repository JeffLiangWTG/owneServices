using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	class CarrierContractPenaltyMatcher : ContractPenaltyMatcher
	{
		public override PenaltyMatcherType MatcherType => PenaltyMatcherType.CarrierContact;

		protected override bool IsValidProcessType(IContainerPenaltyMatchFilter filter)
		{
			var processType = filter.ProcessType;
			return processType == Constants.ContainerPenaltyProcessType.Import
				|| processType == Constants.ContainerPenaltyProcessType.Export
				|| (processType == Constants.ContainerPenaltyProcessType.Delivery
				|| processType == Constants.ContainerPenaltyProcessType.Pickup) && FilterHasClients(filter);
		}

		protected override ZDateTime GetDepartureDateTime(IContainerPenaltyMatchFilter filter)
		{
			if (GetConsol(filter)?.Transports?.MostInterestingTransport is Transport transport)
			{
				return transport.JW_ATD.IsEmpty ? transport.JW_ETD : transport.JW_ATD;
			}

			return ZDateTime.Empty;
		}

		#region Contract Query

		protected override void AddContractQueryFilters(ZQuery contractQuery, IContainerPenaltyMatchFilter filter, ZDateTime departureDate)
		{
			base.AddContractQueryFilters(contractQuery, filter, departureDate);

			var transportMode = GetConsol(filter)?.JK_TransportMode ?? ZString.Empty;
			contractQuery.AddToFilter(RatingContractSchema.RCT_TransportMode, transportMode);
		}

		protected override IEnumerable<ZGuid> GetContractOrgPKs(IContainerPenaltyMatchFilter filter)
			=> new[] { GetConsol(filter)?.ShippingLine?.PK ?? ZGuid.Empty };

		protected override ZString GetContractNumber(IContainerPenaltyMatchFilter filter)
			=> GetConsol(filter)?.JK_CarrierContractNumber ?? ZString.Empty;

		protected override string ContractType => Constants.RatingContractTypes.Provider;

		protected override IRatingContract SelectContract(IEnumerable<IRatingContract> contracts, IContainerPenaltyMatchFilter filter) => contracts.FirstOrDefault();

		#endregion

		#region Penalty Queries

		protected override void AddCommonPenaltyQueryFilters(ZQuery commonPenaltyQuery, ZString penaltyType, IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			base.AddCommonPenaltyQueryFilters(commonPenaltyQuery, penaltyType, filter, departureDate, contract);

			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToArray();

			if (filter.ProcessType == Constants.ContainerPenaltyProcessType.Delivery || filter.ProcessType == Constants.ContainerPenaltyProcessType.Pickup)
			{
				commonPenaltyQuery.AddToFilter(RatingContractContainerDetentionSchema.RCD_OH_Client, clientPks);
			}
			else
			{
				AddEqualsOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_OH_Client, clientPks);
			}
		}

		protected override IRatingContractContainerDetention LoadDetention(IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract, System.Func<IContainerPenaltyMatchFilter, ZDateTime, IRatingContract, ZQuery> getQuery)
		{
			var factory = (contract as BusinessObject)?.Factory;
			var query = getQuery(filter, departureDate, contract);
			var detentions = factory.Load<IRatingContractContainerDetention>(query);
			var clientPks = ContainerPenaltyMatchFilterUtilities.GetClients(filter).Select(client => client.PK).ToList();
			clientPks.Add(ZGuid.Empty);

			foreach (var clientPk in clientPks)
			{
				IRatingContractContainerDetention detention = null;

				if (detentions.Length <= 1)
				{
					detention = detentions.FirstOrDefault();
				}
				else
				{
					detention = detentions.FirstOrDefault(d => d.RCD_OH_Client == clientPk);
				}

				if (detention != null)
				{
					return detention;
				}
			}

			return null;
		}

		bool FilterHasClients(IContainerPenaltyMatchFilter filter)
		{
			if (filter.Client != null)
			{
				return true;
			}

			foreach (CommonShipment shipment in filter.Shipments)
			{
				var clients = ContainerPenaltyMatchFilterUtilities.GetClients(filter, shipment);
				if (clients.Any())
				{
					return true;
				}
			}

			return false;
		}

		#region SuppressResourceStringsCheckRegion

		protected override string DetentionPenaltiesQueryOrderBy
			=> string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
				RatingContractContainerDetentionSchema.RCD_OH_Client.Name,
				RatingContractContainerDetentionSchema.RCD_ContainerType.Name,
				RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_StartDateOverride.Name,
				RatingContractContainerDetentionSchema.PK.Name);

		protected override string StorageImportPenaltiesQueryOrderBy
			=> string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
				RatingContractContainerDetentionSchema.RCD_OH_Client.Name,
				RatingContractContainerDetentionSchema.RCD_ContainerType.Name,
				RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_StartDateOverride.Name,
				RatingContractContainerDetentionSchema.PK.Name);

		protected override string StorageExportPenaltiesQueryOrderBy
			=> string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC",
				RatingContractContainerDetentionSchema.RCD_OH_Client.Name,
				RatingContractContainerDetentionSchema.RCD_ContainerType.Name,
				RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_StartDateOverride.Name,
				RatingContractContainerDetentionSchema.PK.Name);

		protected override string MddPenaltiesQueryOrderBy
			=> string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
				RatingContractContainerDetentionSchema.RCD_OH_Client.Name,
				RatingContractContainerDetentionSchema.RCD_ContainerType.Name,
				RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_StartDateOverride.Name,
				RatingContractContainerDetentionSchema.PK.Name);

		#endregion

		#endregion

		CommonConsol GetConsol(IContainerPenaltyMatchFilter filter) => (filter.Container as CommonContainer)?.Consol;
	}
}
