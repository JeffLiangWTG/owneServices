using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;
using WTG.ROPE.Model;

namespace Enterprise.MasterFiles.GUI.Test
{
	class OrganizationInfoControlTest : TestCase
	{
		public void TestAfterFirstBinding()
		{
			var companyItem = new ResponseCompanyItem
			{
				Name = "TestCompany",
				Address = "TEST ADDRESS 1",
				City = "SAA1",
				PostCode = "1231234",
				StateCode = "AAA",
				Country = "CCC",
				Score = 50,
				Registered = true,
				Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } }
			};

			var lookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany" } });
			var lookupItemModel = new CompanyLookupItemModel(companyItem) { Selected = true };
			lookupModel.CompanyLookupItemModels.Add(lookupItemModel);

			using (var form = new ConfirmOrganizationForm(lookupModel))
			using (var organizationInfoControl = new OrganizationInfoControlForTest())
			{
				form.Controls.Add(organizationInfoControl);
				organizationInfoControl.SetDataBinding(lookupItemModel, string.Empty);
				form.Show();

				var organizationInfoControls = form.Controls.Find("OrganizationInfoControl", true).Cast<OrganizationInfoControl>();
				var control = organizationInfoControls.FirstOrDefault();

				AssertEquals(2, control.Controls.Count);
				AssertEquals("companyInfoContentPanel", control.Controls[0].Name);
				AssertEquals(12, control.Controls[0].Controls.Count);
				AssertEquals("companyAbnIDLabel", control.Controls[0].Controls[0].Name);
				AssertEquals("companyAcnIDLabel", control.Controls[0].Controls[1].Name);
				AssertEquals("companyDunsIDLabel", control.Controls[0].Controls[2].Name);
				AssertEquals("companyScorePanel", control.Controls[0].Controls[3].Name);
				AssertEquals("companyABNLabel", control.Controls[0].Controls[4].Name);
				AssertEquals("companyACNLabel", control.Controls[0].Controls[5].Name);
				AssertEquals("companyAddressLabel", control.Controls[0].Controls[6].Name);
				AssertEquals("companyDunsLabel", control.Controls[0].Controls[7].Name);
				AssertEquals("companyInfoPanel", control.Controls[0].Controls[8].Name);
				AssertEquals("cityStatePostInfoRadioButton", control.Controls[0].Controls[9].Name);
				AssertEquals("companyNameLabel", control.Controls[0].Controls[10].Name);
				AssertEquals("companyCityStatePostLabel", control.Controls[0].Controls[11].Name);
			}
		}

		public void TestRadioButtonAndPanelClick()
		{
			var companyItem = new ResponseCompanyItem
			{
				Name = "TestCompany",
				Address = "TEST ADDRESS 1",
				City = "SAA1",
				PostCode = "1231234",
				StateCode = "AAA",
				Country = "CCC",
				Score = 50,
				Registered = true,
				Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } }
			};
			var lookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany" } });
			var lookupItemModel = new CompanyLookupItemModel(companyItem) { Selected = false };
			lookupModel.CompanyLookupItemModels.Add(lookupItemModel);

			using (var form = new ConfirmOrganizationForm(lookupModel))
			using (var organizationInfoControl = new OrganizationInfoControlForTest())
			{
				form.Controls.Add(organizationInfoControl);
				organizationInfoControl.SetDataBinding(lookupItemModel, string.Empty);
				form.Show();

				AssertEquals(ImageBitmapHelper.CompanyInfoPanelBackColor, organizationInfoControl.BackColor);

				organizationInfoControl.companyInfoContentPanel_ClickForTest(null, null);
				AssertEquals("this record is selected by clicking panel", true, lookupItemModel.Selected);
				AssertEquals(ImageBitmapHelper.CompanyInfoPanelInnerClickColor, organizationInfoControl.BackColor);

				lookupItemModel.Selected = false;
				organizationInfoControl.cityStatePostInfoRadioButton_ClickForTest(null, null);
				AssertEquals("this record is selected by clicking radio button", true, lookupItemModel.Selected);
				AssertEquals(ImageBitmapHelper.CompanyInfoPanelInnerClickColor, organizationInfoControl.BackColor);

				organizationInfoControl.companyInfoContentPanel_MouseLeaveForTest(null, null);
				AssertEquals("selected record will still be highlighted after mouse leave", true, lookupItemModel.Selected);
				AssertEquals(ImageBitmapHelper.CompanyInfoPanelInnerClickColor, organizationInfoControl.BackColor);
			}
		}

		[RequiresSTA]
		public void TestPanelHoverShowTooltip()
		{
			var companyItem = new ResponseCompanyItem
			{
				Name = "TestCompany",
				Address = "TEST ADDRESS 1",
				City = "SAA1",
				PostCode = "1231234",
				StateCode = "AAA",
				Country = "CCC",
				Score = 50,
				Registered = true,
				Identifiers = new Identifier[] { new Identifier() { ID = "12345689", Type = IdentifierType.DUNS } }
			};

			var lookupModel = new CompanyLookupModel(new[] { new ResponseCompanyItem { Name = "TestCompany" } });
			var lookupItemModel = new CompanyLookupItemModel(companyItem) { Selected = true };
			lookupModel.CompanyLookupItemModels.Add(lookupItemModel);

			using (var form = new ConfirmOrganizationForm(lookupModel))
			using (var organizationInfoControl = new OrganizationInfoControlForTest())
			{
				form.Controls.RemoveAndDisposeAll();
				form.Controls.Add(organizationInfoControl);
				organizationInfoControl.SetDataBinding(lookupItemModel, string.Empty);
				form.Show();

				organizationInfoControl.companyInfoContentPanel_MouseEnterForTest(null, null);

				Thread.Sleep(1100);
				Application.DoEvents();
				AssertEquals("tool tip should show after 1 second", true, form.Controls.Find("companyScorePanel", true)[0].Visible);

				organizationInfoControl.companyInfoContentPanel_MouseLeaveForTest(null, null);
				AssertEquals("no tool tip shows after leave", false, form.Controls.Find("companyScorePanel", true)[0].Visible);
			}
		}

		class OrganizationInfoControlForTest : OrganizationInfoControl
		{
			public void companyInfoContentPanel_MouseLeaveForTest(object sender, EventArgs e)
			{
				CompanyInfoContentPanel_MouseLeave(null, null);
			}

			public void cityStatePostInfoRadioButton_ClickForTest(object sender, EventArgs e)
			{
				CityStatePostInfoRadioButton_Click(null, null);
			}

			public void companyInfoContentPanel_ClickForTest(object sender, EventArgs e)
			{
				CompanyInfoContentPanel_Click(null, null);
			}

			public void companyInfoContentPanel_MouseEnterForTest(object sender, EventArgs e)
			{
				CompanyInfoContentPanel_MouseEnter(null, null);
			}
		}
	}
}
