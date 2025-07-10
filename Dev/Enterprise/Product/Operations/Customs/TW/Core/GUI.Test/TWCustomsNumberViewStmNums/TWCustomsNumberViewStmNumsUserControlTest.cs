using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class TWCustomsNumberViewStmNumsUserControlTest : ZFormBasherTest
	{
		public void TestGridColumns()
		{
			using (var control = new TWCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers))
			{
				control.Show();
				var numberRangesGrid = (ZGrid)control.Controls.Find("NumberRangesGrid", true)[0];
				AssertEquals(true, numberRangesGrid.GetColumnStyle(CustomsNumberViewStmNums.Schema.SN_FountainName).IsUnavailable);
				AssertEquals(false, numberRangesGrid.GetColumnStyle(TWCustomsNumberViewStmNumsWrapper.Schema.MessageType).IsUnavailable);
			}
		}

		#region Implementation
		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Company);
					form.CaptionRenderingEnabled = true;
					form.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 340, true);
					var control = new TWCustomsNumberViewStmNumsUserControl(Provider.CustomsNumberWrappers);
					control.Name = "TWCustomsNumberViewStmNumsUserControl";
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.SetDataBinding(Company, "");
				}

				return form;
			}
		}

		ZChildForm form;
		TWCustomsNumberViewStmNumsCompanyProvider Provider => (TWCustomsNumberViewStmNumsCompanyProvider)Company.CustomsNumberProvider;
		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;

		protected override Form GetFormToBashCore()
		{
			return Form;
		}
		#endregion
	}
}
