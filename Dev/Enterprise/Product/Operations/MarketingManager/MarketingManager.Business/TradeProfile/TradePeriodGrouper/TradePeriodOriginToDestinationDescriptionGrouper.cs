using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodOriginToDestinationDescriptionGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_OriginID);
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_DestinationID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var originToDestinationGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.Origin,
				x.TradeDetail.Parent.Destination
			});

			foreach (var grouping in originToDestinationGrouped)
			{
				var description =
					FallbackToUnknownStringIfEmpty(GetGroupDescription(grouping.Key.Origin))
						+ " -> "
						+ FallbackToUnknownStringIfEmpty(GetGroupDescription(grouping.Key.Destination));
				yield return new Grouping(description, grouping);
			}
		}

		static ZString GetGroupDescription(ViewLocation location)
		{
			if (location == null || location.VLO_Code.IsEmpty)
			{
				return ZString.Empty;
			}

			if (location.IsState)
			{
				return GetStateGroupDescription(location.VLO_Description, location.VLO_CountryCode);
			}
			else if (location.IsUNLOCO || location.IsCityTown)
			{
				if (!location.VLO_StateCode.IsEmpty)
				{
					return ZString.Format("{0}, {1}", location.VLO_Description, location.VLO_StateCode);
				}
				else
				{
					return location.VLO_Description;
				}
			}
			else
			{
				return location.VLO_Description;
			}
		}
	}
}
