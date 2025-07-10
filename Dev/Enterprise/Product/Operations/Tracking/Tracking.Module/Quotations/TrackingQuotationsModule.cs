using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business.Quotations;
using Enterprise.Tracking.Web;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Module
{
	public class TrackingQuotationsModule : ZFilterStripGridModule
	{
		public TrackingQuotationsModule(BusinessObjectFactory factory, ZPage page) : base(factory, page) { }

		public override ModuleIdentifier ID => WebModuleIDs.TrackingQuotations;

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			if (Page.SiteUser != null && Page.SiteUser.IsLoggedIn)
			{
				var result = new ZDBOnlyQuery(typeof(Quote));

				result.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, ZBool.True);
				result.AddToFilter(RatingHeaderSchema.TH_IsCancelled, ZBool.False);
				result.AddToFilter(RatingHeaderSchema.TH_IsOneOffQuoteConsumed, ZBool.False);
				if (!filterBizO.Filter.Params.Any(x => x.SchemaColumn.Name == RatingHeaderSchema.TH_QuoteEndDate.Name))
				{
					result.AddToFilter(QuoteEndDateGreaterThanToday, JoinCondition.And);
				}

				var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

				var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				docAddressesFilter.AddToFilter(JoinCondition.And, JobDocAddressSchema.E2_AddressType, SQLComparisonOperator.Equal, addressCode);

				var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
				orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, ((OrgContactWebUser)Page.SiteUser).LoggedInUser.OC_OH);

				docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
				result.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.And);

				return result;
			}

			return ZQuery.NoResultQuery;
		}

		ZQuery QuoteEndDateGreaterThanToday
		{
			get
			{
				var result = new ZQuery(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Today);
				result.AddToFilter(JoinCondition.Or, RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.Equal, null);

				return result;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OneOffQuoteFilterStripBusinessObject { IsInWebQuoteMode = true };

		public const string ViewQuoteCommand = "ViewQuote";

		protected override GridColumnProvider GetColumnProvider() => new TrackingQuotationsColumnProvider();

		public override SchemaColumn GetBusinessObjectPKColumn(FilterBusinessObject filter) => RatingHeaderSchema.PK;

		public override Type GridCollectionType => typeof(TrackingQuoteCollection);

		protected override SchemaPKColumn RelevantPersistantPKColumn => RatingHeaderSchema.PK;

		#region Sorting

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(RatingHeaderSchema.TH_QuoteDate.Name, DefaultSortOrder) };

		protected override FilterLayoutCodePairRegistryItem DefaultLayoutRegistryItem => WebDataRegistry.Instance.DefaultFilterLayoutForwardingQuotes;

		#endregion Sorting
	}
}
