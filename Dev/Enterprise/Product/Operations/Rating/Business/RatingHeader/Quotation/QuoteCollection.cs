using CargoWise.EntityFramework;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Rating.Business
{
	[DescriptionProperty(AutoRatingHeader.Schema.TH_QuoteNumber), CodeProperty(AutoRatingHeader.Schema.TH_QuoteNumber)]
	[ModuleID(ModuleId.Quotations)]
	public class QuoteCollection : RatingHeaderCollection, IQuoteCollection
	{
		public QuoteCollection(BusinessObjectFactory factory)
			: this(factory, GlbCompany.CurrentCompany)
		{
		}

		public QuoteCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, company)
		{
		}

		public QuoteCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery filter)
			: base(factory, company, filter)
		{
		}

		public new Quote this[int index]
		{
			get { return (Quote)Elements[index]; }
		}

		public new Quote AddNew()
		{
			return (Quote)base.AddNew();
		}
	}

	#region Simple One Off Quote Collection Wrapper - used for Matching Form

	public class SimpleOneOffQuoteCollectionWrapper : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SimpleOneOffQuoteCollectionWrapper(QuoteCollection possibleMatches)
		{
			fPossibleMatches = possibleMatches;
			foreach (Quote possibleMatch in possibleMatches)
			{
				possibleMatch.InitialiseOneOffQuotation();
				possibleMatch.OneOffQuote.SetReadOnlyIncludingChildren(true);
			}
			RegisterEditableChildObject(possibleMatches);
			SetReadOnlyIncludingChildren(true);
		}

		readonly QuoteCollection fPossibleMatches;
		public QuoteCollection PossibleMatches
		{
			get { return fPossibleMatches; }
		}

		Quote fSelectedQuote;
		public Quote SelectedQuote
		{
			get { return fSelectedQuote; }
			set { fSelectedQuote = value; }
		}
	}

	#endregion
}

