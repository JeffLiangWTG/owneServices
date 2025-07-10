using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class IntercompanyTariffTabControlTest : RatingTestCase
	{
		public void TestZStmALogFilterControl_ShowForAll()
		{
			Helper.ChargeCodes.CreateGlobalCharge("FRT");

			var intercompanyTariff = Helper.NewIntercompanyTariff(Helper.NewOrgHeader());
			var rateEntry = intercompanyTariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "NZAKL", "AUSYD", "FRT", 50m);
			Factory.Save();

			rateEntry.TI_OriginLRC = "USLAX";
			Factory.Save();

			using var module = new ZStmALogModule();
			module.InitData(intercompanyTariff);
			var control = (ZStmALogFilterControl)module.EmbeddedControl;
			(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
			module.PerformSearch_ForTest();

			var collection = (control.GridCollection as StmALogCollection);
			Assert("Collection contains logs belong to Intercompany Tariff", collection.Cast<StmALog>().All(x => x.SL_Parent == intercompanyTariff.PK));
			AssertEquals("Should Display All the logs", 2, collection.Count);
		}
	}
}
