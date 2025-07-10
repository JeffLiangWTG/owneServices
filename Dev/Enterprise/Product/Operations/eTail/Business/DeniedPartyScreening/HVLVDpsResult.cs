#if NETFRAMEWORK
using Enterprise.Freight.Business;
#endif
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.eTail.Business.DeniedPartyScreening
{
	public class HVLVDpsResult : NonPersistentBusinessObject
	{
		public HVLVDpsResult(List<DpsResponseWithScreeningParty> responseWithScreeningParties, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(responseWithScreeningParties, nameof(responseWithScreeningParties));
			Argument.NotNull(factory, nameof(factory));

			responseWithScreeningPartiesCollection = new HVLVDpsMatchCollection(responseWithScreeningParties, Factory);
			RegisterEditableChildObject(responseWithScreeningPartiesCollection);
			ResponseWithScreeningParties = new HVLVDpsMatchCollectionView(responseWithScreeningPartiesCollection);
			CalculateCount(ResponseWithScreeningParties.OfType<HVLVDpsMatch>());

			notScreenedPartiesCountInfo = GetZPropertyInfo(nameof(NotScreenedPartiesCount));
			clearPartiesCountInfo = GetZPropertyInfo(nameof(ClearPartiesCount));
			matchedPartiesCountInfo = GetZPropertyInfo(nameof(MatchedPartiesCount));
			unknownPartiesCountInfo = GetZPropertyInfo(nameof(UnknownPartiesCount));
		}

		readonly HVLVDpsMatchCollection responseWithScreeningPartiesCollection;

		public HVLVDpsMatchCollectionView ResponseWithScreeningParties { get; }

		public int ScreenedPartiesCount { get; private set; }

		public int NotScreenedPartiesCount { get; private set; }
		readonly ZPropertyInfo notScreenedPartiesCountInfo;

		public int ClearPartiesCount { get; private set; }
		readonly ZPropertyInfo clearPartiesCountInfo;

		public int MatchedPartiesCount { get; private set; }
		readonly ZPropertyInfo matchedPartiesCountInfo;

		public int UnknownPartiesCount { get; private set; }
		readonly ZPropertyInfo unknownPartiesCountInfo;

		void CalculateCount(IEnumerable<HVLVDpsMatch> matchParties)
		{
			var counts = matchParties.GroupBy(x => x.Status).ToDictionary(g => g.Key, g => g.Count());

			ScreenedPartiesCount = matchParties.Count();
			ClearPartiesCount = counts.GetValueOrDefault(ScreeningStatusesList.Codes.Clear);
			MatchedPartiesCount = counts.GetValueOrDefault(ScreeningStatusesList.Codes.Matched);
			NotScreenedPartiesCount = counts.GetValueOrDefault(ScreeningStatusesList.Codes.NotScreened);
			UnknownPartiesCount = counts.GetValueOrDefault(ScreeningStatusesList.Codes.Unknown);
		}

		public void RefreshCountPropertyInfo()
		{
			notScreenedPartiesCountInfo.RefreshBinding();
			clearPartiesCountInfo.RefreshBinding();
			matchedPartiesCountInfo.RefreshBinding();
			unknownPartiesCountInfo.RefreshBinding();
		}

		public void UpdateCount(string oldStatus, string newStatus)
		{
			if (oldStatus == newStatus)
			{
				return;
			}

			UpdateStatusCount(oldStatus, -1);
			UpdateStatusCount(newStatus, 1);
		}

		void UpdateStatusCount(string status, int count)
		{
			switch (status)
			{
				case ScreeningStatusesList.Codes.Clear:
					ClearPartiesCount += count;
					break;
				case ScreeningStatusesList.Codes.NotScreened:
					NotScreenedPartiesCount += count;
					break;
				case ScreeningStatusesList.Codes.Unknown:
					UnknownPartiesCount += count;
					break;
				case ScreeningStatusesList.Codes.Matched:
					MatchedPartiesCount += count;
					break;
			}
		}

		public ZBool ShowHasMatchesOnly
		{
			get => ResponseWithScreeningParties.ShowHasChangesOnly;
			set
			{
				ResponseWithScreeningParties.ShowHasChangesOnly = value;
				ResponseWithScreeningParties.Rebuild();
			}
		}
	}
}
