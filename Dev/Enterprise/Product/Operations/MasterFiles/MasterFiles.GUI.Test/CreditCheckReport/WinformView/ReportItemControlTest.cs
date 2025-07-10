using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Test
{
	class ReportItemControlTest : TestCase
	{
		public void TestResetEventHandlerWhenSetDataBinding()
		{
			using (var control = new ReportItemControl())
			{
				var model = new ReportItemModel(CreditReportType.CommercialBureauEnquiry);
				var reportItemInfoModel = new ReportItemInfoModel(model, null, null, null);
				control.SetDataBinding(reportItemInfoModel, string.Empty);

				AssertEquals(1, reportItemInfoModel.GetEventHandlerCount());
				control.SetDataBinding(null, string.Empty);
				AssertEquals(0, reportItemInfoModel.GetEventHandlerCount());
			}
		}

		public void TestResetEventHandler_WhenControlDispose()
		{
			var control = new ReportItemControl();
			var model = new ReportItemModel(CreditReportType.CommercialBureauEnquiry);
			var reportItemInfoModel = new ReportItemInfoModel(model, null, null, null);
			control.SetDataBinding(reportItemInfoModel, string.Empty);

			AssertEquals(1, reportItemInfoModel.GetEventHandlerCount());
			control.Dispose();
			AssertEquals(0, reportItemInfoModel.GetEventHandlerCount());
		}

		public void TestGetReportButton_Click()
		{
			using (var form = new ZForm())
			{
				var reportItemControl = new ReportItemControl();
				var isClicked = false;
				reportItemControl.getReportButton.Click += (sender, args) =>
				{
					isClicked = true;
				};
				form.Controls.Add(reportItemControl);
				form.Show();

				var getReportButton = form.Controls.Find("getReportButton", true)[0] as ZButton;
				getReportButton.PerformClick();
				Assert(isClicked);
			}
		}
	}
}
