using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class MainPageModelTest : TestCase
	{
		public void TestMainPageModel()
		{
			using (var configForTest = new SupportCreditCheckForTest())
			{
				var model = new MainPageModel()
				{
					TopBannerModel = new TopBannerModel(CreditReportStatusType.UpToDate, 0, "WiseTech Global Limited")
				};

				model.ReportsModel = new ReportsModel(new List<ReportItemModel>(), null, null, model);
				model.EventsBannerModel = new EventsBannerModel(configForTest, new CreditCheckServiceForTest(), model);

				CombineAssertions(() =>
				{
					AssertNotNull(model.EventsBannerModel);
					AssertNotNull(model.ReportsModel);
					AssertNotNull(model.TopBannerModel);
				});
			}
		}
	}
}
