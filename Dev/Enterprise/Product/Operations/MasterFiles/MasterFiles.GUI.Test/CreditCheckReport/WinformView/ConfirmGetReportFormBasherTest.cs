using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ConfirmGetReportForm))]

	public class ConfirmGetReportFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			confirmGetReportModel.Identifiers.ToList().Add(new Identifier()
			{
				Type = IdentifierType.ACN,
				ID = new string('a', 300)
			});
			var getSpecifiedReportForm = new ConfirmGetReportForm(confirmGetReportModel);
			return getSpecifiedReportForm;
		}

		public void TestSuitableFont()
		{
			using (var getSpecifiedReportForm = new ConfirmGetReportForm(confirmGetReportModel))
			{
				AssertEquals("Segoe UI", getSpecifiedReportForm.Controls.Find("companyNameLabel", true).Cast<ZLabel>().First().Font.FontFamily.Name);
				AssertEquals(10, (int)getSpecifiedReportForm.Controls.Find("companyNameLabel", true).Cast<ZLabel>().First().Font.Size);
				AssertEquals("Segoe UI", getSpecifiedReportForm.Controls.Find("operationDetailLabel", true).Cast<ZLabel>().First().Font.FontFamily.Name);
				AssertEquals(10, (int)getSpecifiedReportForm.Controls.Find("operationDetailLabel", true).Cast<ZLabel>().First().Font.Size);
			}
		}

		public void TestAddIdentityItemControl()
		{
			using (var getSpecifiedReportForm = new ConfirmGetReportForm(confirmGetReportModel))
			{
				var identityItemsLayoutPanel = getSpecifiedReportForm.Controls.Find("identityItemsLayoutPanel", true).Cast<KFlowLayoutPanel>().First();
				AssertEquals(2, identityItemsLayoutPanel.Controls.Count);

				var typeLabel1 = identityItemsLayoutPanel.Controls[0].Controls.Find("typeLabel", true).Cast<ZLabel>().Single();
				var iDLabel1 = identityItemsLayoutPanel.Controls[0].Controls.Find("IDLabel", true).Cast<ZLabel>().Single();
				var typeLabel2 = identityItemsLayoutPanel.Controls[1].Controls.Find("typeLabel", true).Cast<ZLabel>().Single();
				var iDLabel2 = identityItemsLayoutPanel.Controls[1].Controls.Find("IDLabel", true).Cast<ZLabel>().Single();

				AssertEquals("ACN", typeLabel1.Text);
				AssertEquals("2210223311", iDLabel1.Text);
				AssertEquals("DUNS", typeLabel2.Text);
				AssertEquals("123456789", iDLabel2.Text);
			}
		}

		public void TestGetReportButtonClick()
		{
			using (var getSpecifiedReportForm = new ConfirmGetReportForm(confirmGetReportModel))
			{
				Assert(!getSpecifiedReportForm.ConfirmButtonClicked);
				getSpecifiedReportForm.GetReportButton_Click(null, null);
				Assert(getSpecifiedReportForm.ConfirmButtonClicked);
			}
		}

		public void TestCancelButtonClick()
		{
			var getSpecifiedReportForm = new ConfirmGetReportForm(confirmGetReportModel);
			getSpecifiedReportForm.CancelButton_Click(null, null);
			Assert(!getSpecifiedReportForm.ConfirmButtonClicked);
		}

		protected override void SetUp()
		{
			base.SetUp();
			confirmGetReportModel = new ConfirmGetReportModel()
			{
				City = "Sydney",
				CompanyAddress = "42 FRIENDSHIP ROAD",
				CountryState = "NSW",
				CompanyName = "WISETECH GLOBAL666",
				PostCode = "2036",
				Identifiers = new List<Identifier>()
				{
					new Identifier()
					{
						ID = "2210223311",
						Type = IdentifierType.ACN
					},
					new Identifier()
					{
						ID = "123456789",
						Type = IdentifierType.DUNS
					}
				},
			};
		}

		protected override bool ShouldTestFormIsFullyTranslatable
		{
			get { return false; }
		}

		ConfirmGetReportModel confirmGetReportModel;
	}
}
