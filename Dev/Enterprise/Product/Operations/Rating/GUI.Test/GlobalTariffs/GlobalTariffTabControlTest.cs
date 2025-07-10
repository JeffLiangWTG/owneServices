using System.Linq;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.Testing
{
	public class GlobalTariffTabControlTest : RatingTestCase
	{
		public void TestZStmALogFilterControl_ShowForAll()
		{
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT", UnitCalculator.Code);
			globalChargeCode.Factory.Save();

			var companyTariff = Helper.NewGlobalTariff();
			var rateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "US", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();
			var tariffLine = rateEntry.AddRateLine(globalChargeCode, UnitCalculator.Code, QuantityUnit.KG);
			tariffLine.GetCalculator<UnitCalculator>().PerUnit = 5.1612m;
			companyTariff.Factory.Save();

			rateEntry.RateLines[0].GetCalculator<UnitCalculator>().PerUnit = 30m;
			companyTariff.Factory.Save();

			using var module = new ZStmALogModule();
			module.InitData(companyTariff);
			var control = (ZStmALogFilterControl)module.EmbeddedControl;
			(control.FilterBusinessObject as ZStmALogFilterBusinessObject).LogsToShow = LogsToShow.All;
			module.PerformSearch_ForTest();

			var collection = (control.GridCollection as StmALogCollection);
			Assert("Collection contains logs belong to CompanyTariff", collection.Cast<StmALog>().All(x => x.SL_Parent == companyTariff.PK));
			AssertEquals("Should Display All the logs", 2, collection.Count);
		}
	}
}
