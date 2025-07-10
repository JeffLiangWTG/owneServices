//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoRateAttachmentSetLookups
//
//    This class should be used for overriding collections in AutoRateAttachmentSetLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateAttachmentSetLookups : AutoRateAttachmentSetLookups
	{
		public RateAttachmentSetLookups(AutoRateAttachmentSet parent)
			: base(parent) { }

		public CodeDescriptionPairList TemplateTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(RatingConstants.DocTemplateTypes.CoverPage, Res.GetString("Rating|DocTemplateTypes|CoverPage", "Cover Page"));
				result.AddPair(RatingConstants.DocTemplateTypes.StandardPricingPage, Res.GetString("Rating|DocTemplateTypes|StandardPricingPage", "Standard Pricing Page"));
				result.AddPair(RatingConstants.DocTemplateTypes.OneOffPricingPage, Res.GetString("Rating|DocTemplateTypes|OneOffPricingPage", "One Off Pricing Page"));
				result.AddPair(RatingConstants.DocTemplateTypes.OneOffMultiCarriersPricingPage, Res.GetString("Rating|DocTemplateTypes|OneOffMultiCarriersPricingPage", "One Off Multi Carriers Pricing Page"));
				result.AddPair(RatingConstants.DocTemplateTypes.TableFormatPricingPage, Res.GetString("Rating|DocTemplateTypes|TableFormatPricingPage", "Table Format Pricing Page"));
				result.AddPair(RatingConstants.DocTemplateTypes.TrailingPage, Res.GetString("Rating|DocTemplateTypes|TrailingPage", "Trailing Page"));

				return result;
			}
		}

		public StmMenuItemBaseCollection Documents
		{
			get
			{
				var filter = new ZQuery();
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, BusinessContext.Quotation);

				var collection = new StmMenuItemBaseCollection(Factory, filter);
				collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Business Context", "Property", (ZString)nameof(BusinessContext.Quotation)));
				return collection;
			}
		}
	}
}

