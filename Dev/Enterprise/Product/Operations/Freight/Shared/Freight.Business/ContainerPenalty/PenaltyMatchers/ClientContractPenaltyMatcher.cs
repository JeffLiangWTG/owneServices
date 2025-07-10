using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	class ClientContractPenaltyMatcher : ContractPenaltyMatcher
	{
		public override PenaltyMatcherType MatcherType => PenaltyMatcherType.ClientContract;

		protected override bool IsValidProcessType(IContainerPenaltyMatchFilter filter)
		{
			var processType = filter.ProcessType;
			return processType == Constants.ContainerPenaltyProcessType.Pickup
				|| processType == Constants.ContainerPenaltyProcessType.Delivery;
		}

		protected override ZDateTime GetDepartureDateTime(IContainerPenaltyMatchFilter filter) => GetShipment(filter)?.JS_E_DEP ?? ZDateTime.Empty;

		#region Contract Query

		protected override IEnumerable<ZGuid> GetContractOrgPKs(IContainerPenaltyMatchFilter filter)
		{
			return ContainerPenaltyMatchFilterUtilities.GetClients(filter, GetShipment(filter)).Select(client => client.PK);
		}

		protected override ZString GetContractNumber(IContainerPenaltyMatchFilter filter)
			=> GetShipment(filter)?.ShipmentJobHeader?.JH_ClientContractNumber ?? ZString.Empty;

		protected override string ContractType => Constants.RatingContractTypes.Client;

		protected override IRatingContract SelectContract(IEnumerable<IRatingContract> contracts, IContainerPenaltyMatchFilter filter)
		{
			var clients = ContainerPenaltyMatchFilterUtilities.GetClients(filter, GetShipment(filter));
			IRatingContract contract = null;
			foreach (var client in clients)
			{
				contract = contracts.FirstOrDefault(c => c.RCT_OH == client?.PK);
				if (contract != null)
				{
					return contract;
				}
			}

			return null;
		}

		#endregion

		#region Penalty Queries

		protected override void AddCommonPenaltyQueryFilters(ZQuery commonPenaltyQuery, ZString penaltyType, IContainerPenaltyMatchFilter filter, ZDateTime departureDate, IRatingContract contract)
		{
			base.AddCommonPenaltyQueryFilters(commonPenaltyQuery, penaltyType, filter, departureDate, contract);

			AddEqualsOrIsEmptyFilter(commonPenaltyQuery, RatingContractContainerDetentionSchema.RCD_OH_Client, filter.Carrier?.PK ?? ZGuid.Empty);
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

		protected override string MddPenaltiesQueryOrderBy => string.Format("{0} DESC, {1} DESC, {2} DESC, {3} DESC, {4} DESC, {5} DESC",
			RatingContractContainerDetentionSchema.RCD_OH_Client.Name,
				RatingContractContainerDetentionSchema.RCD_ContainerType.Name,
				RatingContractContainerDetentionSchema.RCD_DetentionPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_OriginPortOrCountry.Name,
				RatingContractContainerDetentionSchema.RCD_StartDateOverride.Name,
				RatingContractContainerDetentionSchema.PK.Name);

		#endregion

		#endregion

		CommonShipment GetShipment(IContainerPenaltyMatchFilter filter)
			=> filter.Shipments?.FirstOrDefault() as CommonShipment ?? (filter.Container as CommonContainer)?.GetRelatedShipmentsForPenaltyDefaulting(filter.ProcessType)?.FirstOrDefault();
	}
}
