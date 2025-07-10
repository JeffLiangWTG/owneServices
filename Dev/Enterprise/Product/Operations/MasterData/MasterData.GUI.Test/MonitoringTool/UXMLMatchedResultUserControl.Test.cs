using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.MasterData.GUI.UXMLMatchingDiagnosticModel;

namespace Enterprise.MasterData.GUI.Tests
{
	public class UXMLMatchedResultUserControlTest : TestCaseWithFactory
	{
		public void TestControlDisplay()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "GHRTSH";
			orgHeader.OH_FullName = "QWERTY";
			orgHeader.OH_IsActive = false;
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Code = "ABCDE";
			address.OA_PostCode = "POST CODE";
			address.OA_City = "CITY";
			address.OA_Address1 = "ADDRESS 1";
			address.OA_Address2 = "ADDRESS 2";
			address.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL INFO";
			address.OA_Email = "EMAIL";
			address.OA_RN_NKCountryCode = "AU";

			var vm = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "GHRTSH",
				matchedOrgName: "QWERTY",
				true,
				matchedAddressCode: "ABCDE",
				orgScore: 0.9,
				addressScore: 0.8,
				result: UXMLMatchingDiagnosticUtils.Constants.Match,
				orgPK: orgHeader.PK);

			Factory.Save();

			using (var form = new ZForm())
			{
				var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);
				form.Controls.Add(uxmlMatchedResultUserControl);
				form.Show();

				var orgInfoTextBox = form.Controls.Find("OrgMatchInfoTextBox", true)[0] as ZTextBox;

				var orgScoreTextBox = form.Controls.Find("OrgScoreTextBox", true)[0] as ZTextBox;
				var addressScoreTextBox = form.Controls.Find("AddressScoreTextBox", true)[0] as ZTextBox;
				var activeStatusTextBox = form.Controls.Find("OrgMatchActiveStatusTextBox", true)[0] as ZTextBox;
				var resultTextBox = form.Controls.Find("ResultTextBox", true)[0] as ZTextBox;

				CombineAssertions("The displayed properties should be same with the VS", () =>
				{
					AssertEquals(vm.OrgMatchInfo, orgInfoTextBox.Text);

					AssertEquals(vm.OrgScore, orgScoreTextBox.Text);
					AssertEquals(vm.AddressScore, addressScoreTextBox.Text);
					AssertEquals("Y", activeStatusTextBox.Text);

					AssertEquals(vm.Result, resultTextBox.Text);
				});
			}

			vm = new UXMLMatchingDiagnosticModel(
				matchedOrgCode: "GHRTSH",
				matchedOrgName: "QWERTY",
				false,
				matchedAddressCode: "ABCDE",
				orgScore: 0.9,
				addressScore: 0.8,
				result: UXMLMatchingDiagnosticUtils.Constants.Match,
				orgPK: orgHeader.PK);

			Factory.Save();

			using (var form = new ZForm())
			{
				var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);
				form.Controls.Add(uxmlMatchedResultUserControl);
				form.Show();

				var orgInfoTextBox = form.Controls.Find("OrgMatchInfoTextBox", true)[0] as ZTextBox;

				var orgScoreTextBox = form.Controls.Find("OrgScoreTextBox", true)[0] as ZTextBox;
				var addressScoreTextBox = form.Controls.Find("AddressScoreTextBox", true)[0] as ZTextBox;
				var activeStatusTextBox = form.Controls.Find("OrgMatchActiveStatusTextBox", true)[0] as ZTextBox;
				var resultTextBox = form.Controls.Find("ResultTextBox", true)[0] as ZTextBox;

				CombineAssertions("The displayed properties should be same with the VS", () =>
				{
					AssertEquals(vm.OrgMatchInfo, orgInfoTextBox.Text);

					AssertEquals(vm.OrgScore, orgScoreTextBox.Text);
					AssertEquals(vm.AddressScore, addressScoreTextBox.Text);
					AssertEquals("N", activeStatusTextBox.Text);

					AssertEquals(vm.Result, resultTextBox.Text);
				});
			}

			vm = new UXMLMatchingDiagnosticModel(
				orgHeader,
				address,
				orgScore: 0.9,
				addressScore: 0.8,
				result: UXMLMatchingDiagnosticUtils.Constants.Match
				);

			Factory.Save();

			using (var form = new ZForm())
			{
				var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);
				form.Controls.Add(uxmlMatchedResultUserControl);
				form.Show();

				var orgInfoTextBox = form.Controls.Find("OrgMatchInfoTextBox", true)[0] as ZTextBox;
				var address1TextBox = form.Controls.Find("Address1TextBox", true)[0] as ZTextBox;
				var address2TextBox = form.Controls.Find("Address2TextBox", true)[0] as ZTextBox;
				var additionalAddressTextBox = form.Controls.Find("AdditionalAddressTextBox", true)[0] as ZTextBox;
				var postCodeTextBox = form.Controls.Find("PostCodeTextBox", true)[0] as ZTextBox;
				var countryTextBox = form.Controls.Find("CountryTextBox", true)[0] as ZTextBox;
				var stateTextBox = form.Controls.Find("StateTextBox", true)[0] as ZTextBox;
				var cityTextBox = form.Controls.Find("CityTextBox", true)[0] as ZTextBox;
				var emailTextBox = form.Controls.Find("EmailTextBox", true)[0] as ZTextBox;
				var addressShortCodeTextBox = form.Controls.Find("AddressShortCodeTextBox", true)[0] as ZTextBox;
				var orgScoreTextBox = form.Controls.Find("OrgScoreTextBox", true)[0] as ZTextBox;
				var addressScoreTextBox = form.Controls.Find("AddressScoreTextBox", true)[0] as ZTextBox;
				var activeStatusTextBox = form.Controls.Find("OrgMatchActiveStatusTextBox", true)[0] as ZTextBox;
				var resultTextBox = form.Controls.Find("ResultTextBox", true)[0] as ZTextBox;

				CombineAssertions("The displayed properties should be same with the VS", () =>
				{
					AssertEquals(vm.OrgMatchInfo, orgInfoTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.Address1], address1TextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.Address2], address2TextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.AdditionalAddress], additionalAddressTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.PostCode], postCodeTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.Country], countryTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.State], stateTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.City], cityTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.Email], emailTextBox.Text);
					AssertEquals(vm.MatchedAddressDict[DataTypes.AddressShortCode], addressShortCodeTextBox.Text);

					AssertEquals(vm.OrgScore, orgScoreTextBox.Text);
					AssertEquals(vm.AddressScore, addressScoreTextBox.Text);
					AssertEquals("N", activeStatusTextBox.Text);

					AssertEquals(vm.Result, resultTextBox.Text);
				});
			}
		}

		#region ShowEditForm

		public void TestShowEditFormWithoutSecurityRight()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var vm = new UXMLMatchingDiagnosticModel();
			var originalValue = Env.Security.OrganisationView.IsAllowed;
			using (var form = new ZForm())
			{
				try
				{
					Env.Security.OrganisationView.IsAllowed = false;
					var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);
					form.Controls.Add(uxmlMatchedResultUserControl);
					form.Show();
					var linkLabel = form.Controls.Find("detailsLinkLabel", true)[0] as ZLinkLabel;
					linkLabel.OnLinkClicked_Exposed(null);
					AssertContains("You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					Env.Security.OrganisationView.IsAllowed = originalValue;
				}
			}
		}

		public void TestShowEditForm()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var vm = new UXMLMatchingDiagnosticModel { OrgPK = orgHeader.PK };
			Factory.Save();
			var originalValue = Env.Security.OrganisationView.IsAllowed;
			using (var form = new ZForm())
			{
				try
				{
					Env.Security.OrganisationView.IsAllowed = true;
					var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);
					form.Controls.Add(uxmlMatchedResultUserControl);
					form.Show();
					var linkLabel = form.Controls.Find("detailsLinkLabel", true)[0] as ZLinkLabel;
					linkLabel.OnLinkClicked_Exposed(null);
					Assert(string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
				}
				finally
				{
					ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().First().Close();
					Env.Security.OrganisationView.IsAllowed = originalValue;
				}
			}
		}

		[RequiresSTA]
		public void TestShowEditFormWithDeletedItems()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var vm = new UXMLMatchingDiagnosticModel { OrgPK = new ZGuid() };
			var originalValue = Env.Security.OrganisationView.IsAllowed;
			using (var form = new ZForm())
			{
				try
				{
					Env.Security.OrganisationView.IsAllowed = true;
					var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);

					form.Controls.Add(uxmlMatchedResultUserControl);
					form.Show();
					var linkLabel = uxmlMatchedResultUserControl.Controls.Find("detailsLinkLabel", true)[0] as ZLinkLabel;
					linkLabel.OnLinkClicked_Exposed(null);
					AssertContains("This item have been removed from the system, please click button to re-calculate matched items.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					Env.Security.OrganisationView.IsAllowed = originalValue;
				}
			}
		}

		public void TestShowEditFormOnOtherClient()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var vm = new UXMLMatchingDiagnosticModel { OrgPK = orgHeader.PK };
			var uxmlMatchedResultUserControl = new UXMLMatchedResultUserControl(vm);

			var originalValue = Env.Security.OrganisationView.IsAllowed;
			Env.Security.OrganisationView.IsAllowed = true;

			using (var form = new ZForm())
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.UPE))
			{
				try
				{
					form.Controls.Add(uxmlMatchedResultUserControl);
					form.Show();

					AssertNullOrEmpty("PreCondition", UnitTestUserNotification.Instance.LastMessage.Text);

					var linkLabel = uxmlMatchedResultUserControl.Controls.Find("detailsLinkLabel", true)[0] as ZLinkLabel;
					linkLabel?.OnLinkClicked_Exposed(null);

					AssertNotContains("You do not have appropriate controller for the Business Object", UnitTestUserNotification.Instance.LastMessage.Text);
				}
				finally
				{
					var forms = ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().ToList();
					forms.ForEach(f => f.Dispose());
					Env.Security.OrganisationView.IsAllowed = originalValue;
				}
			}
		}

		public void TestShowEditForm_UsingEditFormUrlHandler()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var controller = ZControllerFactory.Instance.GetControllerForBizo(orgHeader) ?? ZControllerFactory.Instance.GetControllerForType(typeof(OrgHeader));
			AssertNotNull("PreCondition: Controller should not be null", controller);

			var url = ShowEditFormUrlHandler.Instance.Create(controller.ID, orgHeader.PK);
			var successful = EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, false);

			Assert("Successful showing edit form", successful);

			var orgForms = ZApplication.GetOpenForms().OfType<ZOrganisationsForm>().ToList();
			AssertEquals("Org form opened", 1, orgForms.Count);

			orgForms.ForEach(f => f.Dispose());
		}

		#endregion
	}
}
