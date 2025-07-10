using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Testing
{
	public class LinesByChargeCodeDictionaryTest : RatingTestCase
	{
		public void TestCannotAddNewLinesWithoutGlobalChargeCode()
		{
			var globalChargeCode1 = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT1");
			var globalChargeCode2 = Helper.ChargeCodes.CreateGlobalCharge("GLBFRT2");
			var globalChargeCode3 = Helper.ChargeCodes.CreateGlobalCharge("GLBDST", chargeGroup: ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			globalChargeCode1.ChildChargeCodes.Single(c => c.AC_GC == GlbCompany.CurrentCompany.PK).Delete();
			globalChargeCode3.ChildChargeCodes.Single(c => c.AC_GC == GlbCompany.CurrentCompany.PK).Delete();
			Factory.Save();

			var criteria = new RatingCriteria(null, Factory);

			var globalClientRate = Helper.NewGlobalClientRate(NewClient);
			var entry1 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "", "AU");
			entry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = entry1.AddRateLine(globalChargeCode1).GetCalculator<FlatCalculator>().BaseRate = 10m;
			var rateLine2 = entry1.AddRateLine(globalChargeCode2).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var entry2 = globalClientRate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.ALL, "", "AU");
			var rateLine3 = entry2.AddRateLine(globalChargeCode3).GetCalculator<FlatCalculator>().BaseRate = 20m;

			var entries = new List<IRateEntry> { entry1, entry2 };
			var logger = new TestLogger();
			var repository = new RateLinesRepository(criteria, entries, null, logger);

			AssertEquals("Should only match the global charge with a local charge left", "1xGLBFRT2", repository.DebuggerDisplay);
		}

		[ExpectNoExceptions]
		public void TestNewLinesByChargeCodeDictionary_SomeEntryIsNull()
		{
			var criteria = new RatingCriteria(null, Factory);
			var entries = new List<IRateEntry> { null };
			var repository = new RateLinesRepository(criteria, entries, null, new TestLogger().WithPrefix("(TEST) "));
		}

		public void TestCorrectSpellingInLogs()
		{
			AssertEquals("line equipment LE doesn't match PCE CE", LogMessages.EquipmentTypeIrrelevant("LE", "PCE", "CE"));
		}
	}
}
