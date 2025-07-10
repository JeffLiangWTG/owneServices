using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI
{
	public class LinkActivityModuleFilter : ModuleDateFilter
	{
		public delegate ZQuery GetLinkActivityQuery(ZQuery linkActivityFilter);

		public LinkActivityModuleFilter(ZString description, FilterStripBusinessObject filterStripBusinessObject, GlbCompanyCampaign campaign)
			: base(description, true, false)
		{
			this.linkActivityQueryDelegate = (q) => q;

			HideFutureDates = true;
			PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			Campaign = campaign;
		}

		protected readonly GlbCompanyCampaign Campaign;

		#region Properties

		[List("LinkTrackingList")]
		public ZString TypeProperty
		{
			get { return typeProperty; }
			set
			{
				if (typeProperty != value)
				{
					typeProperty = value;
					if (!IsValidationSuspended)
					{
						Validation.ValidateTypeProperty();
					}
					TypePropertyInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}
		ZString typeProperty;

		public ZPropertyInfo TypePropertyInfo
		{
			get { return GetZPropertyInfo(nameof(TypeProperty)); }
		}

		#endregion

		#region Validation

		public new LinkActivityModuleFilterValidation Validation
		{
			get { return (LinkActivityModuleFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new LinkActivityModuleFilterValidation(this);
		}

		#endregion

		#region Clear / IsEmpty

		protected override void ClearCore()
		{
			base.ClearCore();
			PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			TypeProperty = ZString.Empty;
		}

		protected override bool IsEmptyCore => (base.IsEmptyCore && TypeProperty.IsEmpty) || TypePropertyInfo.HasErrors();

		public bool IsDateEmpty => IsEmpty;

		#endregion

		#region Query

		protected virtual SchemaStringColumn QueryColumn
		{
			get { return null; }
		}

		protected override ZQuery GetQuery()
		{
			return IsEmpty ? new ZQuery() : linkActivityQueryDelegate(GetLinkActivityFilterQuery());
		}

		public virtual ZQuery GetLinkActivityFilterQuery()
		{
			return GetLinkActivityFilterQueryCore();
		}

		ZQuery GetLinkActivityFilterQueryCore()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(GlbCompanyCampaignItem));
			ZDBOnlySubQuery campaignClickSubQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignClick), GlbCompanyCampaignClickSchema.GCC_G8_Recipient);

			if (!IsEmpty)
			{
				if (FromDate.IsValid)
				{
					campaignClickSubQuery.AddToFilter(GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
				}

				if (ToDate.IsValid)
				{
					campaignClickSubQuery.AddToFilter(GlbCompanyCampaignClickSchema.GCC_ClickTimeUtc, SQLComparisonOperator.LessThan, ToDate);
				}
			}

			if (!TypeProperty.IsEmpty)
			{
				ZDBOnlySubQuery linkQuery = new ZDBOnlySubQuery(typeof(GlbCompanyCampaignLink), GlbCompanyCampaignLinkSchema.PK);
				linkQuery.AddToFilter(QueryColumn, SQLComparisonOperator.Equal, TypeProperty);

				campaignClickSubQuery.AddSubQuery(GlbCompanyCampaignClickSchema.GCC_GCL, linkQuery, JoinCondition.And);
			}

			query.AddSubQuery(campaignClickSubQuery, JoinCondition.And);
			return query;
		}

		readonly GetLinkActivityQuery linkActivityQueryDelegate;

		#endregion

		#region XML Serialization

		protected virtual string XmlElementName
		{
			get { return ""; }
		}

		protected override void SerializePropertiesToXml(System.Xml.XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(XmlElementName, TypeProperty);
		}

		protected override void DeserializePropertiesFromXml(System.Xml.XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);

			if (reader.Name == XmlElementName)
			{
				TypeProperty = reader.ReadElementString(XmlElementName);
			}
		}

		#endregion

		#region Lookup

		public virtual TrackingLinkList LinkTrackingList
		{
			get { return new TrackingLinkList(new[] { Campaign }); }
		}

		public class TrackingLinkList : CodeDescriptionPairList
		{
			public TrackingLinkList(GlbCompanyCampaign[] campaigns)
			{
				Campaigns = campaigns;
			}

			readonly GlbCompanyCampaign[] Campaigns;

			public TrackingLinkList ListByContext()
			{
				foreach (var link in Campaigns.SelectMany(c => c.TrackedLinks))
				{
					AddPair(link.GCL_Context, link.GCL_URL);
				}

				return this;
			}

			public TrackingLinkList ListByURL()
			{
				foreach (var link in Campaigns.SelectMany(c => c.TrackedLinks))
				{
					AddPair(link.GCL_URL, link.GCL_Context);
				}

				return this;
			}
		}

		#endregion
	}
}
