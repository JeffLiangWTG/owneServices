using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgCommunicationPreviewPaneControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestOrgPopulatedOnNewCommunication()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (var form = new ZForm(org))
			using (var control = new OrgCommunicationPreviewPaneControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.CommunicationModule.NewMenuItem.PerformClick();

				var lastShownForm = ((IFilterModuleInternalsForTesting)control.CommunicationModule).LastController.LastShownForm;
				AssertNotNull(lastShownForm);
				using (lastShownForm)
				{
					AssertType(typeof(CommunicationForm), lastShownForm);
					var communciation = (OrgSalesCall)((ZForm)lastShownForm).BusinessEntity;

					AssertEquals(org.PK, communciation.OQ_OH);
				}
			}
		}

		[RequiresSTA]
		public void TestDoNotLoadAllCommunicationsOnLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			int expectedCommunicationsLoaded;

			using (var form = new ZForm(org))
			using (var control = new OrgCommunicationPreviewPaneControl())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				expectedCommunicationsLoaded = ((IBusinessObjectFactoryInternals)control.CommunicationModule.GridCollection.Factory).AllBusinessObjects.OfType<OrgSalesCall>().Count();
			}

			for (var i = 0; i < 10; i++)
			{
				org.SalesCalls.AddNew();
			}
			Factory.Save();

			using (var form = new ZForm(new BusinessObjectFactory().Load<OrgHeader>(org.PK)))
			using (var control = new OrgCommunicationPreviewPaneControl())
			{
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var actualCommunicationsLoaded = ((IBusinessObjectFactoryInternals)control.CommunicationModule.GridCollection.Factory).AllBusinessObjects.OfType<OrgSalesCall>().Count();
				AssertEquals("Should not load all calls", expectedCommunicationsLoaded, actualCommunicationsLoaded);
			}
		}

		[RequiresSTA]
		public void TestPrintCallVisitReport()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var call1 = org.SalesCalls.AddNew();
			var call2 = org.SalesCalls.AddNew();
			var call3 = org.SalesCalls.AddNew();

			Factory.Save();

			using (ZForm form = new ZForm(org))
			using (CommunicationPreviewPaneControlForTesting contr = new CommunicationPreviewPaneControlForTesting())
			{
				form.Controls.Add(contr);
				form.Show();
				contr.SetDataBinding(org, "");

				Application.DoEvents();

				Assert("Should contain \"Communication Report\" MenuItem", contr.FilterStripControl.Grid.ContextMenu.MenuItems.Contains(contr.CommunicationReportMenuItem));

				contr.ContextMenu_Popup_Exposed(null, EventArgs.Empty);
				Assert("MenuItem must be Disabled", !contr.CommunicationReportMenuItem.Enabled);

				contr.FilterStripControl.FirePerformSearch();
				contr.FilterStripControl.Grid.Select(0);

				contr.ContextMenu_Popup_Exposed(null, EventArgs.Empty);
				Assert("MenuItem must be Enabled", contr.CommunicationReportMenuItem.Enabled);

				contr.CommunicationReportMenuItem.PerformClick();
				AssertEquals("Flag should be set to false after report generated", false, call1.ShouldIncludeOnSalesCallDocument);
				AssertEquals("Flag should be set to false after report generated", false, call2.ShouldIncludeOnSalesCallDocument);
				AssertEquals("Flag should be set to false after report generated", false, call3.ShouldIncludeOnSalesCallDocument);

				contr.FilterStripControl.Grid.UnSelectAll();
				contr.FilterStripControl.Grid.Select(1);
				contr.FilterStripControl.Grid.Select(2);
				contr.CommunicationReportMenuItem.PerformClick();
				AssertEquals("Flag should be set to false after report generated", false, call1.ShouldIncludeOnSalesCallDocument);
				AssertEquals("Flag should be set to false after report generated", false, call2.ShouldIncludeOnSalesCallDocument);
				AssertEquals("Flag should be set to false after report generated", false, call3.ShouldIncludeOnSalesCallDocument);
			}
		}

		#region Implementation

		class CommunicationPreviewPaneControlForTesting : OrgCommunicationPreviewPaneControl
		{
			public void ContextMenu_Popup_Exposed(object sender, EventArgs e)
			{
				ContextMenu_Popup(sender, e);
			}
		}

		#endregion
	}
}
