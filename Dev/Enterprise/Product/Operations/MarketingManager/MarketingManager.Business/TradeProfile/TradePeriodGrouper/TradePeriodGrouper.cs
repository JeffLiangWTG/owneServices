using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public abstract class TradePeriodGrouper
	{
		protected TradePeriodGrouper()
		{
		}

		public abstract IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods);

		public class Grouping
		{
			public Grouping(ZString key, IEnumerable<OrgTradePeriod> tradePeriods)
			{
				GroupKey = key;
				GroupedTradePeriods = tradePeriods;
			}

			public ZString GroupKey { get; private set; }
			public IEnumerable<OrgTradePeriod> GroupedTradePeriods { get; private set; }
		}

		#region Fetch Hints

		public void AddFetchHints(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var factory = tradePeriods.First().Factory;
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(OrgTradeDetailSchema.Constants.TableName, period.PAS_PA);
			}
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(OrgSalesSchema.Constants.TableName, period.TradeDetail.PA_OW);
			}

			AddFetchHintsCore(factory, tradePeriods);
		}
		protected abstract void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods);

		#endregion

		#region Helper Methods

		protected static ZString FallbackToUnknownStringIfEmpty(ZString str)
		{
			if (!str.IsEmpty)
			{
				return str;
			}

			return UnknownString;
		}

		protected static string UnknownString
		{
			get { return Res.GetString("9e726149-3b8a-45b7-a5b1-c5bcaa001d62", "(Unknown)"); }
		}

		protected static ZString GetCountryGroupDescription(ZString countryCode, ZString countryDescription)
		{
			if (!countryCode.IsEmpty)
			{
				return ZString.Format("{0} ({1})", countryCode, countryDescription);
			}
			else
			{
				return UnknownString;
			}
		}

		protected static ZString GetStateGroupDescription(ZString stateDescription, ZString countryCode)
		{
			if (!stateDescription.IsEmpty)
			{
				return ZString.Format("{0}, {1}", countryCode, stateDescription);
			}
			else if (!countryCode.IsEmpty)
			{
				return ZString.Format("{0}, {1}", countryCode, UnknownString);
			}
			else
			{
				return UnknownString;
			}
		}

		#endregion
	}
}
