using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	[TestFixture]
	public class RefCusRateTest
	{
		[Test]
		public void TestZZ2_ZZS_NKPreference()
		{
			var refCusRate = new RefCusRate();
			refCusRate.ZZ2_ZZS_NKPreference = "ABC";
			Assert.AreEqual("ABC", refCusRate.ZZ2_ZZS_NKPreference);
		}

		[Test]
		public void TestRefCusApplicability()
		{
			var refCusRate = new RefCusRate();
			var app = refCusRate.AddNewRefCusApplicability();
			Assert.AreEqual(app.Parent, refCusRate);

			app.ZZT_OrderNumber = "ABC";
			app.ZZT_ZZA_NKTradeGroup = "TG";
			refCusRate.ZZ2_StartDate = new DateTime(2019, 01, 01, 0, 0, 0);
			refCusRate.ZZ2_EndDate = new DateTime(2020, 01, 01, 0, 0, 0);
			app.RefCusExcludedTradeGroup = new List<string>
			{
				"TE"
			};

			Assert.AreEqual("ABC", app.ZZT_OrderNumber);
			Assert.AreEqual("TG", app.ZZT_ZZA_NKTradeGroup);
			Assert.AreEqual(new DateTime(2019, 01, 01, 0, 0, 0), app.ZZT_StartDate);
			Assert.AreEqual(new DateTime(2020, 01, 01, 0, 0, 0), app.ZZT_EndDate);
			Assert.IsTrue(app.RefCusExcludedTradeGroup.Contains("TE"));

			refCusRate.ZZ2_StartDate = new DateTime(2019, 01, 01, 23, 59, 59);
			refCusRate.ZZ2_EndDate = new DateTime(2020, 01, 01, 23, 59, 59);
			Assert.AreEqual(new DateTime(2019, 01, 01, 23, 59, 59), app.ZZT_StartDate);
			Assert.AreEqual(new DateTime(2020, 01, 01, 23, 59, 0), app.ZZT_EndDate);
		}
	}
}
