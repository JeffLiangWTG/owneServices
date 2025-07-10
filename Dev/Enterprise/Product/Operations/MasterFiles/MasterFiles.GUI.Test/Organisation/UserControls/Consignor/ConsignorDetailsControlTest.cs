using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ConsignorDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestRelevantEUCountryTabIsShown()
		{
			foreach (var country in ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers())
			{
				AssertDetailsControl(country, control =>
				{
					AssertEquals("Should only be EU Tab Pages visible", 4, control.DefaultsTabControl.TabPages.Count);
					AssertNotNull("EU consignor plugIn added", control.DefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.OrganisationConsignorPlugIn));
					AssertEquals("First tab should be selected by default", control.DefaultsTabControl.SelectedIndex, 0);
					AssertEquals("First tab should be 'Exporter / Consignor Defaults'", control.DefaultsTabControl.SelectedTab.Name, "ExporterTabPage");
				});
			}
		}

		public void TestRelevantINCountryTabIsShown()
		{
			AssertDetailsControl(Constants.CountryCodes.India, control =>
			{
				AssertNotNull("IN consignor plugIn added", control.DefaultsTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IN.OrganisationConsignorPlugIn));
			});
		}

		public void TestEBOLForDirectConsolCheckBoxCaptionChange()
		{
			var org = Factory.New<OrgHeader>();

			using (var form = new ZForm(org))
			using (var control = new DetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(org, "");

				var exporterTabPage = control.DefaultsTabControl.TabPages["ExporterTabPage"] as ZTabPage;

				AssertNotNull(exporterTabPage);

				var eBOLForDirectConsolCheckBox = exporterTabPage.Controls
					.Find("OM_FWRequiresElectronicBOLForDirectConsolCheckBox", true)
					.OfType<ZCheckBox>()
					.FirstOrDefault();

				AssertNotNull(eBOLForDirectConsolCheckBox);

				AssertEquals("CaptionResourceString should be set correctly", "Require Electronic Bill of Lading", eBOLForDirectConsolCheckBox.Text);
			}
		}

		#region Implementation

		void AssertDetailsControl(string countryCode, AssertDetailsControlCoreDelegate assertDetailsControlCore)
		{
			AssertDetailsControl(countryCode, Factory.New<OrgHeader>(), assertDetailsControlCore);
		}

		void AssertDetailsControl(ZString countryCode, OrgHeader org, AssertDetailsControlCoreDelegate assertDetailsControlCore)
		{
			using (countryCode.IsEmpty ? null : GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				using (var form = new ZForm(org))
				{
					using (var control = new DetailsControlForTest())
					{
						form.Controls.Add(control);
						form.Show();
						control.SetDataBinding(org, "");
						assertDetailsControlCore(control);
					}
				}
			}
		}

		delegate void AssertDetailsControlCoreDelegate(DetailsControlForTest control);

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(oldCountry);
			base.TearDown();
		}

		string oldCountry;

		#endregion

		#region DetailsControlForTest

		public class DetailsControlForTest : ConsignorDetailsUserControl
		{
			public new ZTabControl DefaultsTabControl
			{
				get { return base.DefaultsTabControl; }
			}
		}

		#endregion

		#endregion
	}
}
