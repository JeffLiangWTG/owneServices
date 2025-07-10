using System.Diagnostics;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class RateLineItemsValidationTest : RatingTestCase
	{
		public void TestValidate_Deleted()
		{
			var supplierTransportZoneSet = Helper.CreateRateTransportZoneSet(NewClient, CountryCodes.Australia, zoneNames: new ZString[] { "Z1" });

			var clientRate = Helper.NewClientRate(NewClient);
			var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AUSYD", "");
			var rateLine = rateEntry.AddRateLine("OCART", CartageZoneDistanceCalculator.Code, Volume.CubicMetres);
			var rateLineItem = rateLine.Calculator.AddRateLineItemWithZone("MIN", 0m, 10m, supplierTransportZoneSet.Zones[0].PK);
			Factory.Save();

			rateLineItem.Delete();

			AssertNoExceptionThrown(() => { rateLineItem.Validation.ValidateAll(); });
		}

		public void TestImportAndValidatePerformance()
		{
			var rate = Helper.NewClientRate(Factory.NewWithValidTestData<OrgHeader>());
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine("FRT", CombinedCalculator.Code, "KG");
			line.RateLineItems.RemoveAndDeleteAll();

			((ISupportDataImporting)line).IsImportingData = true;

			var swImport = new Stopwatch();
			swImport.Start();

			var minusItem = line.RateLineItems.AddNew();
			minusItem.TM_Type = CombinedCalculator.Items.Operator.Minus;
			minusItem.TM_Break = 150;
			minusItem.TM_Value = 0.01;

			for (var i = 0; i < 4000; i++)
			{
				var plusItem = line.RateLineItems.AddNew();
				plusItem.TM_Type = CombinedCalculator.Items.Operator.Plus;
				plusItem.TM_Break = 150 + i * 50;
				plusItem.TM_Value = 0.02 + i / 100;
			}

			swImport.Stop();

			var itemWIthError1 = line.RateLineItems[1000];

			Factory.SuspendValidation();
			itemWIthError1.TM_Break = 500;
			Factory.ResumeValidation();

			var itemWIthError2 = line.RateLineItems[8];

			Assert(!itemWIthError1.HasErrors);
			Assert(!itemWIthError2.HasErrors);

			var swValidation = new Stopwatch();
			swValidation.Start();

			rate.RunPreSaveValidation();

			swValidation.Stop();

			Assert(itemWIthError1.HasErrors);
			Assert(itemWIthError2.HasErrors);

			const int reasonableImportSec = 35; //takes 7 sec on my 3yo machine. Was more than 10 minutes before.
			const int reasonableValidationSec = 45; //takes 9 sec on my 3yo machine. Was more than 10 minutes before.

			Assert(string.Format("Import and validation should take reasonable time. But was import: {0} sec, validation: {1} sec", swImport.ElapsedMilliseconds / 1000, swValidation.ElapsedMilliseconds / 1000),
				swImport.ElapsedMilliseconds < reasonableImportSec * 1000 && swValidation.ElapsedMilliseconds < reasonableValidationSec * 1000);
		}
	}
}
