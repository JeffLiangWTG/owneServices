using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RelationshipDetailsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestArrLocationAndExamSite()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var org = Factory.New<OrgHeader>();
				var supplierBuyerLink = org.BuyerLinks.AddNew();
				supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();

				using (var form = new ZForm(supplierBuyerLink))
				using (var userControl = new RelationshipDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();
					AssertNotNull(userControl.Controls.Find("controlledArrivalLocationDropEdit", true));
					AssertNotNull(userControl.Controls.Find("examSiteDropEdit", true));
				}
			}
		}

		public void TestUSPortsVisibility()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			OrgHeader org = Factory.New<OrgHeader>();
			OrgSupplierBuyerLink supplierBuyerLink = org.BuyerLinks.AddNew();
			supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
			supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();

			using (ZForm form = new ZForm(supplierBuyerLink))
			using (RelationshipDetailsUserControl userControl = new RelationshipDetailsUserControl())
			{
				form.Controls.Add(userControl);

				form.SetDataBinding(supplierBuyerLink, "");
				userControl.SetDataBinding(supplierBuyerLink, "");
				form.Show();

				Control uSPortOfLadingCodeFindBox = (Control)typeof(RelationshipDetailsUserControl).InvokeMember("USPortOfLadingCodeFindBox", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, userControl, null);
				Control uSPortOfUnLadingCodeFindBox = (Control)typeof(RelationshipDetailsUserControl).InvokeMember("USPortOfUnLadingCodeFindBox", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, userControl, null);

				AssertEquals("USPortOfLadingCodeFindBox", false, uSPortOfLadingCodeFindBox.Visible);
				AssertEquals("USPortOfUnLadingCodeFindBox", false, uSPortOfUnLadingCodeFindBox.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);

			using (ZForm form = new ZForm(supplierBuyerLink))
			using (RelationshipDetailsUserControl userControl = new RelationshipDetailsUserControl())
			{
				form.Controls.Add(userControl);

				Control uSPortOfLadingCodeFindBox = (Control)typeof(RelationshipDetailsUserControl).InvokeMember("USPortOfLadingCodeFindBox", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, userControl, null);
				Control uSPortOfUnLadingCodeFindBox = (Control)typeof(RelationshipDetailsUserControl).InvokeMember("USPortOfUnLadingCodeFindBox", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, userControl, null);

				form.SetDataBinding(supplierBuyerLink, "");
				form.Show();

				AssertEquals("USPortOfLadingCodeFindBox", true, uSPortOfLadingCodeFindBox.Visible);
				AssertEquals("USPortOfUnLadingCodeFindBox", true, uSPortOfUnLadingCodeFindBox.Visible);
				Assert("Lading.Top > UnLading.Top", uSPortOfLadingCodeFindBox.Top > uSPortOfUnLadingCodeFindBox.Top);

				supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;
				AssertEquals("USPortOfLadingCodeFindBox", true, uSPortOfLadingCodeFindBox.Visible);
				AssertEquals("USPortOfUnLadingCodeFindBox", true, uSPortOfUnLadingCodeFindBox.Visible);
				Assert("Lading.Top < UnLading.Top", uSPortOfLadingCodeFindBox.Top < uSPortOfUnLadingCodeFindBox.Top);
			}
		}

		[RequiresSTA]
		public void TestAdditionalCustomsDefaultPlugInVisibility()
		{
			OrgFormForTest testForm;
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsConsignee = true;
			var link = organisation.SupplierLinks.AddNew();
			link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.UnitedStates;

			using (testForm = new OrgFormForTest(organisation))
			{
				AssertNotNull("Pre-condition", testForm);
				testForm.Show();
				testForm.OrgTabControl.SelectedTab = testForm.ConsigneeTabPage;
				testForm.ConsigneeControl.RelationshipsTabPage.Show();

				foreach (ZPlugIn plugin in testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.PlugIns.Instances)
				{
					if (plugin.Name == "Additional Customs Defaults")
					{
						testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.SelectedTab = plugin.TabPage;
						AssertEquals(true, plugin.Enabled);

						var pluginControl = plugin.UserControl;
						var firstSaleDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "FirstSaleDropEdit");
						var reconIndicatorDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "ReconIndicatorDropEdit");
						var nAFTAReconIndicatorCheckBox = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "NAFTAReconIndicatorCheckBox");
						var aESUltimateConsigneeTypeDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "AESUltimateConsigneeTypeDropEdit");
						AssertNotNull(firstSaleDropEdit);
						Assert(firstSaleDropEdit.Visible);

						AssertNotNull(reconIndicatorDropEdit);
						Assert(reconIndicatorDropEdit.Visible);

						AssertNotNull(nAFTAReconIndicatorCheckBox);
						Assert(nAFTAReconIndicatorCheckBox.Visible);

						AssertNotNull(aESUltimateConsigneeTypeDropEdit);
						Assert("AESUltimateConsigneeTypeDropEdit Visible", aESUltimateConsigneeTypeDropEdit.Visible);

						link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
						pluginControl = plugin.UserControl;
						firstSaleDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "FirstSaleDropEdit");
						Assert(!firstSaleDropEdit.Visible);

						reconIndicatorDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "ReconIndicatorDropEdit");
						Assert(!reconIndicatorDropEdit.Visible);

						nAFTAReconIndicatorCheckBox = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "NAFTAReconIndicatorCheckBox");
						Assert(!nAFTAReconIndicatorCheckBox.Visible);

						aESUltimateConsigneeTypeDropEdit = pluginControl.Controls.Cast<Control>().FirstOrDefault(c => c.Name == "AESUltimateConsigneeTypeDropEdit");
						Assert("AESUltimateConsigneeTypeDropEdit Visible", aESUltimateConsigneeTypeDropEdit.Visible);
						break;
					}
				}

				link.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.China;
				foreach (var plugin in testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.relationshipTabControl.PlugIns.Instances)
				{
					if (plugin.Name == "China Customs Defaults")
					{
						testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.relationshipTabControl.SelectedTab = plugin.TabPage;
						AssertEquals(true, plugin.Enabled);
						var pluginControl = plugin.UserControl;
						Assert("ZO_CustomsOfficeCodeFindBox", pluginControl.Controls.Cast<Control>().Any(c => c.Name == "ZO_CustomsOfficeCodeFindBox"));
						Assert("ZO_OfficeOfEntryExitCodeFindBox", pluginControl.Controls.Cast<Control>().Any(c => c.Name == "ZO_OfficeOfEntryExitCodeFindBox"));
						Assert("ZO_CIQOfficeOfEntryExitCodeFindBox", pluginControl.Controls.Cast<Control>().Any(c => c.Name == "ZO_CIQOfficeOfEntryExitCodeFindBox"));
						Assert("CIQOfficesGrid", pluginControl.Controls.Cast<Control>().Any(c => c.Name == "CIQOfficesGrid"));
					}
				}
			}
		}

		public void TestAdditionalCustomsDefaultPlugInIssueWithDetachedRelationshipRow()
		{
			OrgFormForTest testForm;
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_IsConsignee = true;

			using (testForm = new OrgFormForTest(organisation))
			{
				AssertNotNull("Pre-condition", testForm);
				testForm.Show();
				testForm.OrgTabControl.SelectTab(testForm.ConsigneeTabPage);
				Application.DoEvents();

				testForm.ConsigneeControl.RelationshipsTabPage.Show();

				for (int i = 0; i < testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.PlugIns.Instances.Length; i++)
				{
					string name = testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.PlugIns.Instances[i].Name;
					if (name == "Additional Customs Defaults")
					{
						var plugin = testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.PlugIns.Instances[i];
						testForm.ConsigneeControl.RelationshipsControl.RelationshipDetailsUserControl.modesAndTracking.SelectTab(plugin.TabPage);
						AssertNoExceptionThrown(Application.DoEvents);
						break;
					}
				}
			}
		}
		public void TestIncotermPlaceAndMode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var org = Factory.New<OrgHeader>();
				var supplierBuyerLink = org.BuyerLinks.AddNew();
				supplierBuyerLink.OL_RN_NKImporterCountry = Core.Constants.CountryCodes.Australia;
				supplierBuyerLink.OrgSupBuyLinkTrnModes.AddNew();

				using (var form = new ZForm(supplierBuyerLink))
				using (var userControl = new RelationshipDetailsUserControl())
				{
					form.Controls.Add(userControl);
					form.Show();
					AssertNotNull(userControl.Controls.Find("incoModeDropEdit", true));
					AssertNotNull(userControl.Controls.Find("incoPlaceTextBox", true));

					Assert(userControl.Controls.Find("incoModeDropEdit", true)[0].Visible);
					Assert(userControl.Controls.Find("incoPlaceTextBox", true)[0].Visible);
				}
			}
		}

		#region TestSetDataBinding

		[ExpectNoExceptions]
		public void TestSetDataBinding()
		{
			var org = Factory.New<OrgHeader>();
			var supplierBuyerLink = org.BuyerLinks.AddNew();

			using (var userControl = new RelationshipDetailsUserControl())
			{
				userControl.SetDataBinding(supplierBuyerLink, string.Empty);
				AssertEquals("OrgSupBuyLinkTrnModes.Lookups.PickupOrganisations", userControl.pickupAddressControl.BindToOrgList);
				AssertEquals("OrgSupBuyLinkTrnModes.Lookups.DeliveryOrganisations", userControl.deliverAddressControl.BindToOrgList);
				AssertEquals("OrgSupBuyLinkTrnModes.Lookups.NotifyPartyOrganisations", userControl.notifyPartyAddressControl.BindToOrgList);

				userControl.SetDataBinding(org, nameof(org.BuyerLinks));
				AssertEquals("BuyerLinks.OrgSupBuyLinkTrnModes.Lookups.PickupOrganisations", userControl.pickupAddressControl.BindToOrgList);
				AssertEquals("BuyerLinks.OrgSupBuyLinkTrnModes.Lookups.DeliveryOrganisations", userControl.deliverAddressControl.BindToOrgList);
				AssertEquals("BuyerLinks.OrgSupBuyLinkTrnModes.Lookups.NotifyPartyOrganisations", userControl.notifyPartyAddressControl.BindToOrgList);
			}
		}

		#endregion
	}
}
