using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class USCustomsNumberViewStmNumsUserControlTest : ZFormBasherTest
	{
		public void TestGridColumns()
		{
			using (var control = new USCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers))
			{
				control.Show();
				var numberRangesGrid = (ZGrid)control.Controls.Find("NumberRangesGrid", true)[0];
				AssertEquals(true, numberRangesGrid.GetColumnStyle(CustomsNumberViewStmNums.Schema.SN_FountainName).IsUnavailable);
				AssertEquals(false, numberRangesGrid.GetColumnStyle(USCustomsNumberViewStmNumsWrapper.Schema.CheckDigitAddition).IsUnavailable);
				AssertEquals(false, numberRangesGrid.GetColumnStyle(USCustomsNumberViewStmNumsWrapper.Schema.AppliesTo).IsUnavailable);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new ZChildForm(Company);
			form.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 340, true);
			var control = new USCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers);
			control.Name = "USCustomsNumberViewStmNumsUserControl";
			control.Dock = DockStyle.Fill;
			form.Controls.Add(control);
			form.SetDataBinding(Company, "");
			return form;
		}

		USCustomsNumberViewStmNumsCompanyProvider Provider => (USCustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider;

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
	}
}
