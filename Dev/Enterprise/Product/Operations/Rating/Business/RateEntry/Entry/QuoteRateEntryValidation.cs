using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class QuoteRateEntryValidation : RateEntryValidation
	{
		public QuoteRateEntryValidation(AutoRateEntry parent)
			: base(parent)
		{
		}

		QuoteEntry ParentEntry
		{
			get { return parentEntry ?? (parentEntry = (QuoteEntry)Parent); }
		}

		QuoteEntry parentEntry;

		#region TI_QuotePageIncoTerm

		protected override void CheckTI_QuotePageIncoTerm()
		{
			base.CheckTI_QuotePageIncoTerm();

			ListValidation.ErrorIfInvalidCode(Parent.TI_QuotePageIncoTermInfo, Parent.Lookups.IncoTerms);
			if (ParentEntry.IsFreightEntry() && IsIncoTermRequiredByRegistry)
			{
				MandatoryValidation.CheckEntered(Parent.TI_QuotePageIncoTermInfo);
			}
		}

		bool IsIncoTermRequiredByRegistry
		{
			get { return Factory.GetCachedValue<bool>("IsIncoTermRequiredByRegistry", () => RatingDataRegistry.Instance.QuotationsRequiredFields.TypedValue.RequireIncoterm); }
		}

		#endregion

		#region Pending Quotations - Same Trade Lane

		protected override void CheckTI_OriginLRC()
		{
			base.CheckTI_OriginLRC();
			WarnIfPendingQuoteForSameTradeLane();
		}

		protected override void CheckTI_DestinationLRC()
		{
			base.CheckTI_DestinationLRC();
			WarnIfPendingQuoteForSameTradeLane();
		}

		protected override void CheckTI_Mode()
		{
			base.CheckTI_Mode();
			WarnIfPendingQuoteForSameTradeLane();
		}

		void WarnIfPendingQuoteForSameTradeLane()
		{
			if (ParentEntry != null &&
				!ParentEntry.TI_Mode.IsEmpty &&
				!ParentEntry.TI_OriginLRC.IsEmpty &&
				!ParentEntry.TI_DestinationLRC.IsEmpty &&
				ParentEntry.Parent != null &&
				ParentEntry.Parent.Header != null)
			{
				var errorPrefix = Res.GetString("fd6cac83-bcd4-4f3d-bedb-6ea71c2f04e8", "Unaccepted quotes for this trade lane were found in quotations") + " ";

				if (ParentEntry.RowWarnings.Any(warning => warning.Message.StartsWith(errorPrefix)))
				{
					return;
				}

				// DocAddress
				var addressCode = DocAddressTypes.GetCode(Factory, DocAddressType.QuotationClientAddress);

				var docAddressesFilter = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
				var orgAddressesFilter = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

				orgAddressesFilter.AddToFilter(OrgAddressSchema.OA_OH, ParentEntry.Parent.Header.PK);
				docAddressesFilter.AddSubQuery(orgAddressesFilter, JoinCondition.And);
				docAddressesFilter.AddToFilter(JobDocAddress.GetFilter(addressCode, RatingHeaderSchema.Constants.Prefix, 0), JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(RatingHeader));
				query.AddSubQuery(RatingHeaderSchema.PK, docAddressesFilter, JoinCondition.Or);
				query.AddToFilter(RatingHeaderSchema.PK, SQLComparisonOperator.NotEqual, ParentEntry.Parent.PK);
				query.AddToFilter(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.Quote);
				query.AddToFilter(RatingHeaderSchema.TH_GC, ParentEntry.Company().PK);
				query.AddToFilter(RatingHeaderSchema.TH_Accepted, null);
				query.AddToFilter(RatingHeaderSchema.TH_QuoteEndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
				query.AddToFilter(RatingHeaderSchema.TH_IsCancelled, ZBool.False);
				query.AddToFilter(RatingHeaderSchema.TH_OneTimeQuote, ZBool.False);

				var rateEntrySubQuery = new ZDBOnlySubQuery(typeof(RateEntry), RateEntrySchema.TI_TH);
				rateEntrySubQuery.AddToFilter(RateEntrySchema.TI_OriginLRC, ParentEntry.TI_OriginLRC);
				rateEntrySubQuery.AddToFilter(RateEntrySchema.TI_DestinationLRC, ParentEntry.TI_DestinationLRC);
				rateEntrySubQuery.AddToFilter(RateEntrySchema.TI_Mode, ParentEntry.TI_Mode);

				query.AddSubQuery(rateEntrySubQuery, JoinCondition.And);

				var foundQuotes = Factory.Load<Quote>(query);
				if (foundQuotes.Any())
				{
					var quoteNumbers = foundQuotes.Select(quote => quote.TH_QuoteNumber).ToArray();
					var numbersString = ZString.Join(", ", quoteNumbers);

					ParentEntry.AddRowWarning(errorPrefix + numbersString);
				}
			}
		}

		#endregion

		#region Factory

		BusinessObjectFactory Factory
		{
			get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory fFactory;

		#endregion
	}
}

