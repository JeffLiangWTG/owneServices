using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MarketingManager.GUI.Testing
{
	public class GenerateSpotQuoteFromTradeDetailControllerForTest : GenerateSpotQuoteFromTradeDetailController
	{
		public ZForm LastFormShown { get; private set; }

		protected override void ShowFormForNewEntity(ZController controller, IBusiness spotQuote)
		{
			LastFormShown = (ZForm)controller.ShowFormForNewEntity(spotQuote);
		}
	}
}
