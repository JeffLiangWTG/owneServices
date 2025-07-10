using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ZA.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
	{
		public void TestAddInfoFieldType()
		{
			var job = Factory.New<JobDeclaration>();
			var entryLine = job.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var additionalInfo = entryLine.AdditionalInformationCodes.AddNew();
			var helper = new ZAUniversalReferenceTestDataHelper(Factory, setupBasicTariffData: false);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
			helper.CreateAdditionalInformationCusCodeEntry(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
			Factory.Save();
			using (var testForm = new ZForm())
			{
				var exControl = new EntryLineAdditionalDataUserControl();
				testForm.Controls.Add(exControl);
				Application.DoEvents();
				using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
				{
					var grid = ((ZGrid)testForm.Find(c => c.Name == "zGridAdditionalInformation").FirstOrDefault());
					grid.SetDataBinding(entryLine.AdditionalInformationCodes, "");
					testForm.Show();
					var codeControlStyle = (ZDropEditColumnStyle)grid.Columns[0].ColumnStyle;
					var codeControl = (ZGridDropEdit)codeControlStyle.EditControl;
					codeControl.Focus();
					var dataControlStyle = (ZMultiControlColumnStyle)grid.Columns[1].ColumnStyle;
					var cYData_Text = (ZTextBox.Bare)dataControlStyle.EditControl.Controls[0];
					AutomateKeybord(codeControl, UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate);
					AssertEquals(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, codeControl.Text);
					AssertEquals(UniversalReferenceConstants.AdditionalInformation.RebateCreditCertificate, additionalInfo.CY_Code);
					AssertEquals("", cYData_Text.Text);
					AssertType(typeof(ZTextBox.Bare), cYData_Text);
					KeySender.PostKeyDown(cYData_Text, cYData_Text.Handle, Keys.Shift | Keys.Tab);
					Application.DoEvents();
					AutomateKeybord(codeControl, UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount);
					AssertType(typeof(ZCalcEdit.Bare), dataControlStyle.EditControl.Controls[0]);
					AssertEquals(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount, codeControl.Text);
					AssertEquals(UniversalReferenceConstants.AdditionalInformation.BondSuretyAmount, additionalInfo.CY_Code);
					AssertEquals("0", dataControlStyle.EditControl.Controls[0].Text);
				}
			}
		}

		public void TestControlVisibilityAndCaptions()
		{
			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = "IMP";
			var entryLine = job.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			AsserttProvisionalPaymentsCaption(job, "Provisional Payments");
			job.JE_MessageType = "EXP";
			AsserttProvisionalPaymentsCaption(job, "Diamond Levy");
		}

		public void TestEntryLineDutyAndTaxGrid_Action()
		{
			AssertColumnStyle<ZDropEditColumnStyleInfo>(Customs.Business.AutoCusEntryLineFee.Schema.CF_RateOverrideReasonCode, "Action", 140);
		}

		void AsserttProvisionalPaymentsCaption(JobDeclaration job, string caption)
		{
			using (var testForm = new ZForm(job))
			{
				var exControl = new EntryLineAdditionalDataUserControl();
				exControl.ChangeControlsVisibility(job.IsImport);
				testForm.Controls.Add(exControl);
				Application.DoEvents();
				testForm.Show();
				var tab = exControl.Controls.Find("ExtendInfoTabControl", true).FirstOrDefault() as ZTabControl;
				tab.SelectedIndex = 1;
				var tradeLabel = exControl.Controls.Find("TradeAgreementLabel", true).FirstOrDefault() as ZLabel;
				AssertNotNull("TradeAgreementLabel not found", tradeLabel);
				AssertEquals("Trade Label Visible", job.IsImport, tradeLabel.Visible);
				var grpBox = exControl.Controls.Find("ProvisionalPaymentGroupBox", true).FirstOrDefault() as ZGroupBox;
				AssertNotNull("ProvisionalPaymentGroupBox not found", grpBox);
				AssertEquals("GroupBox Caption", caption, grpBox.CaptionResourceString.Caption);
			}
		}

		void AssertColumnStyle<T>(string columnName, string caption, int width) where T : ZGridColumnInfo
		{
			var columnStyle = entryLineAdditionalDataUserControl.entryLineDutyAndTaxGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == columnName);

			CombineAssertions(() =>
			{
				AssertType<T>(columnStyle);
				AssertEquals("Caption", caption, columnStyle.CaptionResourceString.Caption);
				AssertEquals("Width", width, columnStyle.Width);
			});
		}

		static void AutomateKeybord(ZGridDropEdit control, string input)
		{
			foreach (var key in input)
			{
				KeySender.SendKeyPress(control.CodeBox, control.CodeBox.Handle, key);
			}

			KeySender.PostKeyDown(control.CodeBox, control.CodeBox.Handle, Keys.Tab);
			Application.DoEvents();
		}

		protected override void SetUp()
		{
			base.SetUp();
			entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl();
		}
		EntryLineAdditionalDataUserControl entryLineAdditionalDataUserControl;

		protected override void TearDown()
		{
			entryLineAdditionalDataUserControl?.Dispose();
			base.TearDown();
		}
	}
}
