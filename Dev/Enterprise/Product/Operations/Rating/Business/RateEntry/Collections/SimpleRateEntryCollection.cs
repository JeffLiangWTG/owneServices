using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class SimpleRateEntryCollection : BusinessObjectCollection<RateEntry>
	{
		public SimpleRateEntryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}

	/// <summary>
	///  This is the data behind the Possible Matches form. That's the form that
	///  sometimes pops up and says:
	///  'Autorating was unable to find any matching rates. However similar rates exist in your system.'
	///  etc.
	/// </summary>
	public class PossibleMatchesWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public PossibleMatchesWrapper(CostSell costOrSell)
		{
			this.costOrSell = costOrSell;
		}

		readonly CostSell costOrSell;

		public RatingCriteria Criteria
		{
			get { return criteria; }
			set { criteria = value; }
		}
		RatingCriteria criteria;

		#region Schema

		public abstract class Schema
		{
			public const string Origin = "Origin";
			public const string Destination = "Destination";
			public const string Via = "Via";
			public const string Carrier = "Carrier";
			public const string ServiceLevel = "ServiceLevel";
			public const string Commodity = "Commodity";
		}

		#endregion

		public ZString Origin
		{
			get { return Criteria != null ? Criteria.OriginCode : ZString.Empty; }
		}

		public ZPropertyInfo OriginInfo
		{
			get { return GetZPropertyInfo(Schema.Origin); }
		}

		public ZString Destination
		{
			get { return Criteria != null ? Criteria.DestinationCode : ZString.Empty; }
		}

		public ZPropertyInfo DestinationInfo
		{
			get { return GetZPropertyInfo(Schema.Destination); }
		}

		public ZString Via =>
			Criteria?.GetViaCode(costOrSell) ?? ZString.Empty;

		public ZPropertyInfo ViaInfo
		{
			get { return GetZPropertyInfo(Schema.Via); }
		}

		public ZString Carrier
		{
			get { return Criteria != null && Criteria.Carrier != null ? Criteria.Carrier.OH_Code : ZString.Empty; }
		}

		public ZPropertyInfo CarrierInfo
		{
			get { return GetZPropertyInfo(Schema.Carrier); }
		}

		public ZString ServiceLevel
		{
			get
			{
				if (Criteria != null)
				{
					var builder = new ZStringBuilder();
					var seviceLevels = new[]
						{
							Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Client),
							Criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier)
						}.Distinct();

					foreach (var seviceLevel in seviceLevels)
					{
						builder.AppendIfNotEmpty(seviceLevel);
					}

					return builder.ToStringWithDelimiterBetweenAppends(", ");
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo ServiceLevelInfo
		{
			get { return GetZPropertyInfo(Schema.ServiceLevel); }
		}

		public ZString Commodity
		{
			get
			{
				if (Criteria != null)
				{
					var builder = new ZStringBuilder();
					var commodities = Criteria.JobMeasures.GetCommodities();
					foreach (var commodity in commodities)
					{
						builder.AppendIfNotEmpty(commodity);
					}

					return builder.ToStringWithDelimiterBetweenAppends(", ");
				}

				return ZString.Empty;
			}
		}

		public ZPropertyInfo CommodityInfo
		{
			get { return GetZPropertyInfo(Schema.Commodity); }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public SimpleRateEntryCollection PossibleMatches
		{
			get { return possibleMatches ?? (possibleMatches = new SimpleRateEntryCollection(new BusinessObjectFactory())); }
		}
		SimpleRateEntryCollection possibleMatches;
	}
}

