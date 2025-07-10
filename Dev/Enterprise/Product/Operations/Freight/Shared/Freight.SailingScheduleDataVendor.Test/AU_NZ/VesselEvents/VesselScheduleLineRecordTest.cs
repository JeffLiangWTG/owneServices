using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	sealed class VesselScheduleLineRecordTest : TestCaseWithFactory
	{
		public void TestIsValid()
		{
			var csvLines = new string[][]
			{
				new string[] { "AUMEL", "VICTM", "2020-06-21 19:37:00", "2020-06-24 19:37:00", "VICTORIA WEBB", "143", "0123456", "", "", "VIC", "VICT Test Shipping Line", "", "143", "", "", "", "", "", "", "" },
				new string[] { "PECLL", "APMCLLAPMCLL", "2015-01-17 05:00:00", "2015-01-18 05:00:00", "WARNOW DOLPHIN", "GI204N", "9395070", "2015-01-16 23:00:00", "2015-01-17 07:00:00", "CMA", "CMA CGM", "CMA", "GI204N", "", "", "2015-02-11 03:08:00", "", "2015-01-17 22:50:00", "2015-01-18 04:25:00", "" },
				new string[] { "PECLL", "APMCLL", "2020-09-19 19:00:00", "2020-09-25 19:00:00", "MSC ARICA", "NX035A", "96194528", "", "", "MSC", "MEDITERRANEAN SHIPPING COMPANY", "MSC", "NX035A", "", "", "", "", "", "", "" },
				new string[] { "PECLL", "APMCLL", "2020-09-19 07:00:00", "2020-09-20 13:00:00", "BBC REEF", "1293006", "9539365", "", "", "BBC", "BBC CHARTERING", "", "01234567890", "", "", "", "", "", "", "" },
				new string[] { "PECLL", "APMCLL", "2020-09-15 15:00:00", "2020-09-16 11:00:00", "MSC CAPELLA", "FA032AFA032", "9465289", "2020-09-14 11:00:00", "2020-09-14 19:00:00", "MSC", "MEDITERRANEAN SHIPPING COMPANY", "MSC", "FA032A", "2020-09-12 11:00:00", "", "", "", "2020-09-15 16:02:00", "", "" },
				new string[] { "PECLL", "APMCLL", "2020-09-27 23:00:00", "2020-09-29 AB:00:00", "E.R. BERLIN", "039W", "9214214", "", "", "MAE", "MAERSK LINES - MAE", "MAE", "035E", "", "", "", "", "", "", "" },
				new string[] { "AUFRE", "ASLFR", "2020-04 07:00:00", "2020-04-22 20:00:00", "MP THE BROWN", "FC016R", "9403396", "2020-04-21 15:00:00", "2020-04-21 15:00:00", "MSC", "MEDITERRANEAN SHIPPING COMPANY", "", "FC016A", "2020-04-17 07:00:00", "2020-04-23 07:00:00", "2020-04-27 00:00:00", "LOLO", "2020-04-22 06:30:00", "2020-04-22 21:00:00", "" },
				new string[] { "PHMNL", "ICTSI", "2019-02-17 07:20:00", "2019-02-18 16:20:00", "SITC LIAONING", "1906W", "9712369", "1700-01-31 19:00:00", "", "GSL", "GOLD STAR SHIPPING", "", "1905E", "2019-01-24 19:00:00", "", "", "", "", "", "" },
				new string[] { "NZAKL", "NZCCO", "2018-04-30 06:00:00", "2021-12-31 15:30:00", "PLANETA", "TBA", "7811111", "", "2020-03-32 00:00:00", "MSK", "MAERSK", "MSK", "TBAS", "", "", "", "", "", "", "" },
			};

			AssertIsValid("Valid", csvLines[0], true);
			AssertIsValid("Terminal ID too long", csvLines[1], false);
			AssertIsValid("Lloyds ID too long", csvLines[2], false);
			AssertIsValid("ShipOperatorVoyageIn too long", csvLines[3], false);
			AssertIsValid("ShipOperatorVoyageOut too long", csvLines[4], false);
			AssertIsValid("Invalid ETD", csvLines[5], false);
			AssertIsValid("Invalid ETA", csvLines[6], false);
			AssertIsValid("Invalid Cargo Cutoff", csvLines[7], false);
			AssertIsValid("Invalid Reefer Cutoff", csvLines[8], false);
		}

		void AssertIsValid(string message, string[] csvLine, bool expectedIsValid)
		{
			var record = new VesselScheduleLineRecord(csvLine);
			AssertEquals(message, expectedIsValid, record.IsValid());
		}
	}
}
