using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class DocketExtensions
	{
		public static IUnitOfMeasure GetWeightMeasure(this WhsDocket docket)
		{
			Argument.NotNull(docket, nameof(docket));
			return new WeightMeasure(docket.WD_TotalWeightInfo, docket.WD_TotalWeightUnit);
		}

		public static IUnitOfMeasure GetVolumeMeasure(this WhsDocket docket)
		{
			Argument.NotNull(docket, nameof(docket));
			return new VolumeMeasure(docket.WD_TotalCubicInfo, docket.WD_TotalCubicUnit);
		}

		internal static bool IsDPSMovementRestricted(this WhsDocket docket)
			=> ObjectFactory.Get<IComplianceRiskStatusSupporter>().IsDPSFreightMovementRestricted(docket.WD_ScreeningStatus, null, docket);

		internal static IList<ScreeningParty> GetScreeningParties(this WhsDocket docket)
		{
			var parties = new List<ScreeningParty>();

			var docAddrScreenParties = docket.DocAddresses
				.Cast<JobDocAddress>()
				.Where(docAddress => !docAddress.IsEmpty)
				.Select(docAddress => new ScreeningParty(docket, docAddress.AddressCaption, docAddress));
			parties.AddRange(docAddrScreenParties);

			parties.Add(new ScreeningParty(docket, Res.GetString("84a68471-ce3b-4fec-a563-9c330458f289", "Client"), docket.Client));

			var warehouse = docket.Warehouse;
			if (warehouse != null)
			{
				var whDocAddrScreenParties = warehouse.DocAddresses
					.Cast<JobDocAddress>()
					.Where(docAddress => !docAddress.IsEmpty)
					.Select(docAddress => new ScreeningParty(docket, docAddress.AddressCaption, docAddress));
				parties.AddRange(whDocAddrScreenParties);
				parties.Add(new ScreeningParty(docket, Res.GetString("41427cab-9041-46aa-8766-4c5f6de55649", "Warehouse"), warehouse.WarehouseAddress.Header));
			}

			var jobHeader = docket.JobHeader;
			if (jobHeader != null)
			{
				parties.Add(new ScreeningParty(docket, Res.GetString("70072e3c-b995-48c4-ad4a-f420f2a25e46", "Local Client"), jobHeader.LocalCharges));
			}

			return parties;
		}

		internal static ZString GetWorstScreeningStatus(this IScreeningPartyProvider provider)
		{
			var statuses = provider.ScreeningParties
				.Select(sp => (IScreeningPartyProvider)sp.DocAddress ?? sp.Header)
				.Where(spp => spp != null)
				.Select(spp => spp.GetWorstScreeningStatus());
			return ScreeningStatusUpdater.GetWorstScreeningStatus(statuses);
		}

		internal static ZString GetWorstScreeningStatusUnlessManuallyCleared(this IScreeningPartyProvider provider)
		{
			return provider.ScreeningStatus == ScreeningStatusesList.Codes.JobCleared
				? (ZString)ScreeningStatusesList.Codes.JobCleared
				: provider.GetWorstScreeningStatus();
		}
	}
}
