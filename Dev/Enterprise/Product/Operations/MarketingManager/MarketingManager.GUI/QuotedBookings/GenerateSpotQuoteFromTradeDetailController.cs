using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI
{
	public interface IGenerateSpotQuoteFromTradeDetailController
	{
		void ShowNewForm(OrgTradeDetail tradeDetail, IRelatableActivity parentActivity);
	}

	public class GenerateSpotQuoteFromTradeDetailController : IGenerateSpotQuoteFromTradeDetailController
	{
		#region New / Constructors

		public static IGenerateSpotQuoteFromTradeDetailController New()
		{
			return ObjectFactory.Get<IGenerateSpotQuoteFromTradeDetailController>();
		}

		protected GenerateSpotQuoteFromTradeDetailController()
		{
		}

		#endregion

		public void ShowNewForm(OrgTradeDetail tradeDetail, IRelatableActivity parentActivity)
		{
			Argument.NotNull(tradeDetail, "tradeDetail");

			var controller = ZControllerFactory.Create(ControllerIDs.QuotedBookings);
			((IQuotedBookingController)controller).SetQuotedBookingState(QuotedBookingState.QuoteOnly);
			ZControllerInternals controllerInternals = controller;

			var spotQuote = controllerInternals.GetNewBusinessEntityInLocalFactory();
			((ISupportTradeDetailImporting)spotQuote).ImportTradeDetailData(tradeDetail);
			if (parentActivity != null)
			{
				var salesRelationActivity = (ISalesRelationActivity)spotQuote;
				salesRelationActivity.RelatedParentActivityPivotCollection.AddActivity(parentActivity);

				var importDeciderFactory = new ImportRelatedActivityPromptUserDeciderFactory(null, string.Empty);
				importDeciderFactory.AddDecider(typeof(IImportRelatedActivityTradeDetailDecider), null);
				SalesRelationTree.DoImportParentRelatedActivityInfoOnNewActions(parentActivity, salesRelationActivity, importDeciderFactory);
			}

			ShowFormForNewEntity(controller, spotQuote);
		}

		protected virtual void ShowFormForNewEntity(ZController controller, IBusiness spotQuote)
		{
			controller.ShowFormForNewEntity(spotQuote);
		}
	}
}
