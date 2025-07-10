using System;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class ReportItemModelTest : TestCase
	{
		public void TestReportItemModel()
		{
			var model = new ReportItemModel(CreditReportType.CommercialBureauEnquiry)
			{
				LastReportDate = new DateTime(2020, 4, 1)
			};

			AssertEquals(0, new DateTime(2020, 4, 1).CompareTo(model.LastReportDate.Value));
			AssertEquals(CreditReportType.CommercialBureauEnquiry, model.CreditReportType);
		}
	}
}
