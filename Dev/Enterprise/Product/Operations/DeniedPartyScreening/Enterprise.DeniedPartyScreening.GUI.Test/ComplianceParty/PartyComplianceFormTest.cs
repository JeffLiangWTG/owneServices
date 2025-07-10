using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ComplianceRisk.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	[TestedType(typeof(PartyComplianceForm))]
	public class PartyComplianceFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>()));
		}

		public void TestGridColumnCaptions()
		{
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>())))
			{
				AssertTestGridColumnCaptions((ZGrid)form.Controls.Find("UnprocessedWrappersGrid", true)[0]);
				AssertTestGridColumnCaptions((ZGrid)form.Controls.Find("ProcessedWrappersGrid", true)[0]);
			}

			void AssertTestGridColumnCaptions(ZGrid wrappersGrid)
			{
				CombineAssertions(() =>
				{
					var index = 0;
					AssertEquals("Status Code", (wrappersGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Screening Status", (wrappersGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Party Code", (wrappersGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Party Name", (wrappersGrid.ColumnStyles[index++] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
					AssertEquals("Description", (wrappersGrid.ColumnStyles[index] as ZTextBoxColumnStyleInfo).CaptionResourceString.Caption);
				});
			}
		}

		public void TestGridDisableImportDataMenuItem()
		{
			using var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>()));
			AssertGridDisableImportDataMenuItem((ZGrid)form.Controls.Find("UnprocessedWrappersGrid", true)[0]);
			AssertGridDisableImportDataMenuItem((ZGrid)form.Controls.Find("ProcessedWrappersGrid", true)[0]);

			void AssertGridDisableImportDataMenuItem(ZGrid wrappersGrid)
			{
				wrappersGrid.ContextMenu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals(true, wrappersGrid.DisableImportDataMenuItem);
					AssertEquals(false, wrappersGrid.ContextMenu.MenuItems.FindByText("Import Data...").Enabled);
					AssertEquals(false, wrappersGrid.ContextMenu.MenuItems.FindByText("Import Data...").Visible);
				});
			}
		}

		public void TestGridDisableImportDataMenuItem_ADAW()
		{
			using (GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>())))
			{
				AssertGridDisableImportDataMenuItem((ZGrid)form.Controls.Find("UnprocessedWrappersGrid", true)[0]);
				AssertGridDisableImportDataMenuItem((ZGrid)form.Controls.Find("ProcessedWrappersGrid", true)[0]);
			}

			void AssertGridDisableImportDataMenuItem(ZGrid wrappersGrid)
			{
				wrappersGrid.ContextMenu.ShowPopupMenu();
				CombineAssertions(() =>
				{
					AssertEquals(true, wrappersGrid.DisableImportDataMenuItem);
					AssertEquals(false, wrappersGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Enabled);
					AssertEquals(false, wrappersGrid.ContextMenu.MenuItems.FindByText("Advanced Data Automation Wizard").Visible);
				});
			}
		}

		public void TestButtonsVisibilityAsPerUnmatchedOrg()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader.UnmatchOrg(Factory).OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var unmatchedParty = new ScreeningParty(parent, string.Empty, OrgHeader.UnmatchOrg(Factory));
			var filteredCollection = new PartyComplianceWrapperFilteredCollection(new[] { unmatchedParty });
			using (var form = new PartyComplianceForm(filteredCollection))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals(false, filteredCollection.ProcessedParties.Any());
					AssertEquals(1, filteredCollection.UnprocessedParties.Count);
					AssertEquals(false, form.ScreenButton.Visible);
					AssertEquals(false, form.ForceRescreenButton.Visible);
				});
			}

			AssertButtonsVisibilityAsPerUnmatchedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.NotScreened, 2, 1, false, false);
			AssertButtonsVisibilityAsPerUnmatchedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, 0, 3, false, true);
			AssertButtonsVisibilityAsPerUnmatchedOrg(ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.NotScreened, ScreeningStatusesList.Codes.Clear, 1, 2, true, true);
			AssertButtonsVisibilityAsPerUnmatchedOrg(ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, ScreeningStatusesList.Codes.Clear, 3, 0, true, false);
		}

		void AssertButtonsVisibilityAsPerUnmatchedOrg(string party1Status, string party2Status, string party3Status, int processedCollectionCount, int unprocessedCollectionCount, bool unmatchedOrgProcessed, bool expectedScreenButtonVisible)
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_ScreeningStatus = party1Status;
			var headerParty = new ScreeningParty(parent, string.Empty, header);

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_ScreeningStatus = party2Status;
			var vesselParty = new ScreeningParty(parent, string.Empty, vessel);

			OrgHeader.UnmatchOrg(Factory).OH_ScreeningStatus = party3Status;
			var unmatchedParty = new ScreeningParty(parent, string.Empty, OrgHeader.UnmatchOrg(Factory));
			var filteredCollection = new PartyComplianceWrapperFilteredCollection(new[] { headerParty, vesselParty, unmatchedParty });
			using (var form = new PartyComplianceFormForTest(filteredCollection))
			{
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals(processedCollectionCount, filteredCollection.ProcessedParties.Count);
					AssertEquals(unprocessedCollectionCount, filteredCollection.UnprocessedParties.Count);
					AssertEquals(unmatchedOrgProcessed, filteredCollection.ProcessedParties.Any(p => ((PartyComplianceWrapper)p).WrappedScreeningParty.Header == OrgHeader.UnmatchOrg(Factory)));
					AssertEquals(!unmatchedOrgProcessed, filteredCollection.UnprocessedParties.Any(p => ((PartyComplianceWrapper)p).WrappedScreeningParty.Header == OrgHeader.UnmatchOrg(Factory)));
					AssertEquals(expectedScreenButtonVisible, form.ScreenButton.Visible);
					AssertEquals(true, form.ForceRescreenButton.Visible);
				});
			}
		}

		public void TestHasRowWarningsOnUnmatchedOrg()
		{
			AssertHasRowWarningsOnUnmatchedOrg(ScreeningStatusesList.Codes.Clear);
			AssertHasRowWarningsOnUnmatchedOrg(ScreeningStatusesList.Codes.NotScreened);
		}

		void AssertHasRowWarningsOnUnmatchedOrg(string status)
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader.UnmatchOrg(Factory).OH_ScreeningStatus = status;
			var unmatchedParty = new ScreeningParty(parent, string.Empty, OrgHeader.UnmatchOrg(Factory));
			var filteredCollection = new PartyComplianceWrapperFilteredCollection(new[] { unmatchedParty });
			using (var form = new PartyComplianceForm(filteredCollection))
			{
				form.Show();
				if (status == ScreeningStatusesList.Codes.Clear)
				{
					AssertEquals(1, filteredCollection.ProcessedParties.Count);
					AssertEquals(false, filteredCollection.UnprocessedParties.Any());
					AssertEquals(true, filteredCollection.ProcessedParties.First().HasRowWarnings);
				}

				if (status == ScreeningStatusesList.Codes.NotScreened)
				{
					AssertEquals(false, filteredCollection.ProcessedParties.Any());
					AssertEquals(1, filteredCollection.UnprocessedParties.Count);
					AssertEquals(true, filteredCollection.UnprocessedParties.First().HasRowWarnings);
				}
			}
		}

		public void TestSetExcludeScreenMessageLabel_DoNotHaveSelectionOfPermanentClearSecurity()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;
			tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;

			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var clearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			clearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var clearedParty = new ScreeningParty(parent, string.Empty, clearedHeader) { IsCurrentScreeningStatusValid = true };

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { clearedParty })))
			{
				form.Show();
				CombineAssertions("MessagePanel should be hidden as no Permanent Clear parties were skipped", () =>
				{
					AssertEquals(false, (form.Controls.Find("MessagePanel", true)[0] as ZPanel).Visible);
					AssertEquals(DockStyle.Fill, (form.Controls.Find("MainPanel", true)[0] as KTableLayoutPanel).Dock);
				});
			}

			var permanentClearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearedHeader.OH_Code = "OrgCode";
			permanentClearedHeader.OH_FullName = "OrgFullName";
			permanentClearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var permanentClearedParty = new ScreeningParty(parent, string.Empty, permanentClearedHeader) { IsCurrentScreeningStatusValid = true };

			tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { clearedParty, permanentClearedParty })))
			{
				form.Show();

				CombineAssertions("MessagePanel should be shown as Permanently Clear parties were skipped", () =>
				{
					AssertEquals(true, (form.Controls.Find("MessagePanel", true)[0] as ZPanel).Visible);
					AssertEquals(DockStyle.None, (form.Controls.Find("MainPanel", true)[0] as KTableLayoutPanel).Dock);
					AssertEquals("The following records have been skipped because they are permanently clear", (form.Controls.Find("MessageLabel", true)[0] as ZLabel).Text);
					AssertEquals("	* OrgCode    OrgFullName\r\n", (form.Controls.Find("MessageDetailLabel", true)[0] as ZLabel).Text);
				});
			}
		}

		public void TestSetExcludeScreenMessageLabel_HaveSelectionOfPermanentClearSecurity()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;
			tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = true;

			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var clearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			clearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var clearedParty = new ScreeningParty(parent, string.Empty, clearedHeader) { IsCurrentScreeningStatusValid = true };

			var permanentClearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearedHeader.OH_Code = "OrgCode";
			permanentClearedHeader.OH_FullName = "OrgFullName";
			permanentClearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var permanentClearedParty = new ScreeningParty(parent, string.Empty, permanentClearedHeader) { IsCurrentScreeningStatusValid = true };

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { clearedParty, permanentClearedParty })))
			{
				form.Show();
				CombineAssertions("MessagePanel should be hidden when selection of Permanent Clear security is granted", () =>
				{
					AssertEquals(false, (form.Controls.Find("MessagePanel", true)[0] as ZPanel).Visible);
					AssertEquals(DockStyle.Fill, (form.Controls.Find("MainPanel", true)[0] as KTableLayoutPanel).Dock);
				});
			}
		}

		public void TestGridOrgCodeColumn()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			unknownHeader.OH_Code = "UnknownCode";
			var unknownParty = new ScreeningParty(parent, string.Empty, unknownHeader);
			var unknownWrapper = new PartyComplianceWrapper(unknownParty);

			var clearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			clearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			clearedHeader.OH_Code = "ClearCode";
			var clearedParty = new ScreeningParty(parent, string.Empty, clearedHeader);
			var clearedWrapper = new PartyComplianceWrapper(clearedParty);

			var clearedJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var clearedAddress = Factory.NewWithValidTestData<OrgAddress>();
			clearedAddress.OA_OH = clearedHeader.PK;
			clearedJobDocAddress.E2_OA_Address = clearedAddress.PK;
			var clearedJobDocAddressParty = new ScreeningParty(parent, string.Empty, clearedJobDocAddress);
			var clearedJobDocAddressWrapper = new PartyComplianceWrapper(clearedJobDocAddressParty);

			var unknownJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = unknownHeader.PK;
			unknownJobDocAddress.E2_OA_Address = address.PK;
			var unknownJobDocAddressParty = new ScreeningParty(parent, string.Empty, unknownJobDocAddress);
			var unknownJobDocAddressWrapper = new PartyComplianceWrapper(unknownJobDocAddressParty);

			var clearedRefVessel = Factory.NewWithValidTestData<RefVessel>();
			clearedRefVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var clearedRefVesselParty = new ScreeningParty(parent, string.Empty, clearedRefVessel);
			var clearedRefVesselWrapper = new PartyComplianceWrapper(clearedRefVesselParty);

			var unknownRefVessel = Factory.NewWithValidTestData<RefVessel>();
			unknownRefVessel.RV_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownRefVesselParty = new ScreeningParty(parent, string.Empty, unknownRefVessel);
			var unknownRefVesselWrapper = new PartyComplianceWrapper(unknownRefVesselParty);

			Action<ZGrid, string> assertOrgCodeColumn = (targetGrid, value) =>
			{
				CombineAssertions(() =>
				{
					AssertEquals(true, targetGrid.Columns.Contains("OrgCode"));
					var orgCodeCol = targetGrid.Columns["OrgCode"];
					AssertEquals(true, orgCodeCol.IsVisible);
					AssertEquals(100, orgCodeCol.ColumnStyle.Width);
					AssertEquals(value, targetGrid[0, 1].ToString());
					AssertEquals(string.Empty, targetGrid[1, 1].ToString());
					AssertEquals(value, targetGrid[2, 1].ToString());
				});
			};

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[]
			{
				unknownParty,
				unknownRefVesselParty,
				unknownJobDocAddressParty,
				clearedParty,
				clearedRefVesselParty,
				clearedJobDocAddressParty
			})))
			{
				form.Show();

				var unprocessedWrappersGrid = form.Controls.Find("UnprocessedWrappersGrid", true)[0] as ZGrid;
				assertOrgCodeColumn(unprocessedWrappersGrid, "UnknownCode");

				var processedWrappersGrid = form.Controls.Find("ProcessedWrappersGrid", true)[0] as ZGrid;
				assertOrgCodeColumn(processedWrappersGrid, "ClearCode");
			}
		}

		public void TestButtons()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty })))
			{
				form.Show();
				AssertEquals(PartyComplianceButton.Cancel, form.PerformButtonClick);
				var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				screenButton.PerformClick();
				AssertEquals(PartyComplianceButton.Screen, form.PerformButtonClick);
			}

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty })))
			{
				form.Show();
				var forceReScreenButton = form.Controls.Find("ForceRescreenButton", true)[0] as ZButton;
				forceReScreenButton.PerformClick();
				AssertEquals(PartyComplianceButton.ForceRescreen, form.PerformButtonClick);
			}

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>())))
			{
				form.Show();
				form.CancelButton.PerformClick();
				AssertEquals(PartyComplianceButton.Cancel, form.PerformButtonClick);
			}
		}

		public void TestForceReScreenButtonSecurity()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty })))
			{
				form.Show();
				AssertEquals("Precondition", PartyComplianceButton.Cancel, form.PerformButtonClick);

				Env.Security.DpsAllowForceReScreen.IsAllowed = false;
				var forceReScreenButton = form.Controls.Find("ForceRescreenButton", true)[0] as ZButton;
				forceReScreenButton.PerformClick();
				AssertEquals("Still Cancel", PartyComplianceButton.Cancel, form.PerformButtonClick);
				AssertContains(Env.Security.DpsAllowForceReScreen.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.DpsAllowForceReScreen.IsAllowed = true;
				forceReScreenButton.PerformClick();
				AssertEquals(PartyComplianceButton.ForceRescreen, form.PerformButtonClick);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestButtonVisibility()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);

			var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty = new ScreeningParty(parent, "", matchedHeader);

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty, matchedParty })))
			{
				form.Show();
				var forceReScreenButton = form.Controls.Find("ForceRescreenButton", true)[0] as ZButton;
				var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals(true, screenButton.Visible);
				AssertEquals(true, forceReScreenButton.Visible);
			}

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { matchedParty })))
			{
				form.Show();
				var forceReScreenButton = form.Controls.Find("ForceRescreenButton", true)[0] as ZButton;
				var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals(false, screenButton.Visible);
				AssertEquals(true, forceReScreenButton.Visible);
			}

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>())))
			{
				form.Show();
				var forceReScreenButton = form.Controls.Find("ForceRescreenButton", true)[0] as ZButton;
				var screenButton = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals(false, screenButton.Visible);
				AssertEquals(false, forceReScreenButton.Visible);
			}
		}

		public void TestGridRowColor()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);
			var unknownWrapper = new PartyComplianceWrapper(unknownParty);

			var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty = new ScreeningParty(parent, "", matchedHeader);
			var matchedWrapper = new PartyComplianceWrapper(matchedParty);

			var clearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			clearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var clearedParty = new ScreeningParty(parent, "", clearedHeader);
			var clearedWrapper = new PartyComplianceWrapper(clearedParty);

			var permanentClearedHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var permanentClearedParty = new ScreeningParty(parent, "", permanentClearedHeader);
			var permanentClearedWrapper = new PartyComplianceWrapper(permanentClearedParty);

			using (var form = new PartyComplianceFormForTest(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty, matchedParty, clearedParty, permanentClearedParty })))
			{
				form.Show();

				var args = new ColourDecidingEventArgs(unknownWrapper);
				form.Grid_ColorDeciding_Exposed(this, args);
				AssertEquals(DeniedPartyConstants.GridColor.Unknown, args.Colour);

				args = new ColourDecidingEventArgs(matchedWrapper);
				form.Grid_ColorDeciding_Exposed(this, args);
				AssertEquals(DeniedPartyConstants.GridColor.Matched, args.Colour);

				args = new ColourDecidingEventArgs(clearedWrapper);
				form.Grid_ColorDeciding_Exposed(this, args);
				AssertEquals(DeniedPartyConstants.GridColor.Clear, args.Colour);

				args = new ColourDecidingEventArgs(permanentClearedWrapper);
				form.Grid_ColorDeciding_Exposed(this, args);
				AssertEquals(DeniedPartyConstants.GridColor.PermanentClear, args.Colour);
			}
		}

		public void TestShowOrganizationFormByUsingShortcutWithSecurityRightAndOrganizationUnchanged()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;
			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;

				var parent = Factory.NewWithValidTestData<OrgHeader>();
				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);

				Factory.Save();

				AssertSpecifiedFormsAreOpenedAndClosed(new[] { matchedParty }, true);
				AssertSpecifiedFormsAreOpenedAndClosed(new[] { unknownParty }, false);

				void AssertSpecifiedFormsAreOpenedAndClosed(ScreeningParty[] parties, ZBool isProcessed)
				{
					using (var form = new PartyComplianceFormForTest(new PartyComplianceWrapperFilteredCollection(parties)))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						AssertEquals(1, grid.ListManager.Count);
						KeySender.PostKeyDown(grid, Keys.F3);

						CombineAssertions(() =>
						{
							Application.DoEvents();

							AssertNotNull(form.OpenedOrganizationForm);
							form.OpenedOrganizationForm.Close();
							AssertNotNull(form.OpenedPartyComplianceForm);
						});
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestShowOrganizationFormByUsingShortcutWithSecurityRightAndOrganizationChanged()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;

				var parent = Factory.NewWithValidTestData<OrgHeader>();
				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);
				Factory.Save();

				AssertSpecifiedFormsAreOpenedAndClosed(new[] { matchedParty }, true);
				AssertSpecifiedFormsAreOpenedAndClosed(new[] { unknownParty }, false);

				void AssertSpecifiedFormsAreOpenedAndClosed(ScreeningParty[] parties, ZBool isProcessed)
				{
					UnitTestUserNotification.Instance.ClearMessages();

					using (var form = new PartyComplianceFormForTest(new PartyComplianceWrapperFilteredCollection(parties)))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						AssertEquals(1, grid.ListManager.Count);
						KeySender.PostKeyDown(grid, Keys.F3);

						CombineAssertions(() =>
						{
							Application.DoEvents();

							AssertNotNull(form.OpenedOrganizationForm);
							form.OpenedOrganizationForm.BusinessEntity.HasChanges = true;
							form.OpenedOrganizationForm.BusinessEntity.Factory.Save();
							form.OpenedOrganizationForm.Close();

							AssertEquals("The organization data has been modified, please re-open Party Compliance form.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertNull(form.OpenedPartyComplianceForm);
						});
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestShowOrganizationFormByUsingContextMenuWithSecurityRightAndOrganizationUnchanged()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;
			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;

				var parent = Factory.NewWithValidTestData<OrgHeader>();
				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);
				Factory.Save();

				AssertSpecifiedFormsAreOpenedAndClosed(new[] { matchedParty }, true);
				AssertSpecifiedFormsAreOpenedAndClosed(new[] { unknownParty }, false);

				void AssertSpecifiedFormsAreOpenedAndClosed(ScreeningParty[] parties, ZBool isProcessed)
				{
					using (var form = new PartyComplianceFormForTest(new PartyComplianceWrapperFilteredCollection(parties)))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						AssertEquals(1, grid.ListManager.Count);
						grid.ContextMenu.Popup += delegate
						{
							var editMenuItem = grid.ContextMenu.MenuItems.FindByName("Edit Organization", false);
							if (editMenuItem != null && editMenuItem.Enabled)
							{
								editMenuItem.PerformClick();
							}
							grid.ContextMenu.Dispose();
						};
						grid.ContextMenu.Show(grid, grid.Location);

						CombineAssertions(() =>
						{
							AssertNotNull(form.OpenedOrganizationForm);
							form.OpenedOrganizationForm.Close();
							AssertNotNull(form.OpenedPartyComplianceForm);
						});
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestShowOrganizationFormByUsingContextMenuWithSecurityRightAndOrganizationChanged()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;

				var parent = Factory.NewWithValidTestData<OrgHeader>();
				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);
				Factory.Save();

				AssertSpecifiedFormsAreOpenedAndClosed(new[] { matchedParty }, true);
				AssertSpecifiedFormsAreOpenedAndClosed(new[] { unknownParty }, false);

				void AssertSpecifiedFormsAreOpenedAndClosed(ScreeningParty[] parties, ZBool isProcessed)
				{
					UnitTestUserNotification.Instance.ClearMessages();

					using (var form = new PartyComplianceFormForTest(new PartyComplianceWrapperFilteredCollection(parties)))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						AssertEquals(1, grid.ListManager.Count);
						grid.ContextMenu.Popup += delegate
						{
							var editMenuItem = grid.ContextMenu.MenuItems.FindByName("Edit Organization", false);
							if (editMenuItem != null && editMenuItem.Enabled)
							{
								editMenuItem.PerformClick();
							}
							grid.ContextMenu.Dispose();
						};
						grid.ContextMenu.Show(grid, grid.Location);

						CombineAssertions(() =>
						{
							AssertNotNull(form.OpenedOrganizationForm);
							form.OpenedOrganizationForm.BusinessEntity.HasChanges = true;
							form.OpenedOrganizationForm.BusinessEntity.Factory.Save();
							form.OpenedOrganizationForm.Close();

							AssertEquals("The organization data has been modified, please re-open Party Compliance form.", UnitTestUserNotification.Instance.LastMessage.Text);
							AssertNull(form.OpenedPartyComplianceForm);
						});
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestShowOrganizationFormWithoutSecurityRight()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = false;

				var parent = Factory.NewWithValidTestData<OrgHeader>();
				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);
				Factory.Save();

				AssertIsSpecifiedMessageShown(true);
				AssertIsSpecifiedMessageShown(false);

				void AssertIsSpecifiedMessageShown(ZBool isProcessed)
				{
					using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(new[] { unknownParty, matchedParty })))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						AssertEquals(1, grid.ListManager.Count);

						UnitTestUserNotification.Instance.ClearMessages();
						KeySender.PostKeyDown(grid, Keys.F3);
						Application.DoEvents();

						AssertEquals(Env.Security.OrganisationModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestEditOrganizationMenuItemIsEnabled()
		{
			var oldSecurityValue = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = true;

				var parent = Factory.NewWithValidTestData<OrgHeader>();

				var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
				unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty = new ScreeningParty(parent, "", unknownHeader);

				var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
				matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				var matchedParty = new ScreeningParty(parent, "", matchedHeader);

				var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
				docAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				var unknownParty2 = new ScreeningParty(parent, "", docAddress);

				var vessel = Factory.New<RefVessel>();
				var unknownParty3 = new ScreeningParty(parent, "", vessel);
				Factory.Save();

				AssertEditOrganizationMenuItemIsEnabled(new[] { matchedParty }, true, true);
				AssertEditOrganizationMenuItemIsEnabled(new[] { unknownParty }, false, true);
				AssertEditOrganizationMenuItemIsEnabled(new[] { unknownParty2 }, false, false);
				AssertEditOrganizationMenuItemIsEnabled(new[] { unknownParty3 }, false, false);

				void AssertEditOrganizationMenuItemIsEnabled(ScreeningParty[] parties, ZBool isProcessed, ZBool expectedIsEnabled)
				{
					using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(parties)))
					{
						form.Show();

						var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
						var editOrgMenuItemIsEnabled = true;

						grid.ContextMenu.Popup += delegate
						{
							var editMenuItem = grid.ContextMenu.MenuItems.FindByName("Edit Organization", false);
							if (editMenuItem != null)
							{
								editOrgMenuItemIsEnabled = editMenuItem.Enabled;
							}

							grid.ContextMenu.Dispose();
						};

						grid.ContextMenu.Show(grid, grid.Location);
						AssertEquals(expectedIsEnabled, editOrgMenuItemIsEnabled);
					}
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = oldSecurityValue;
			}
		}

		public void TestPartyComplianceFormCaption()
		{
			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>())))
			{
				form.Show();
				AssertEquals("Party Compliance", form.FormCaption);
			}

			using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(Array.Empty<ScreeningParty>()), true))
			{
				form.Show();
				AssertEquals("Party Risk", form.FormCaption);
			}
		}

		public void TestScreenMenuItemIsEnabled()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);

			var permanentClearHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var permanentClearParty = new ScreeningParty(parent, "", permanentClearHeader);

			Factory.Save();

			AssertScreenMenuItemIsEnabled(new[] { permanentClearParty }, true, true);
			AssertScreenMenuItemIsEnabled(new[] { unknownParty }, false, true);
			AssertScreenMenuItemIsEnabled(Array.Empty<ScreeningParty>(), false, false);
			AssertScreenMenuItemIsEnabled(Array.Empty<ScreeningParty>(), true, false);

			void AssertScreenMenuItemIsEnabled(ScreeningParty[] parties, ZBool isProcessed, ZBool expectedIsEnabled)
			{
				using (var form = new PartyComplianceForm(new PartyComplianceWrapperFilteredCollection(parties)))
				{
					form.Show();

					var grid = form.Controls.Find(isProcessed ? "ProcessedWrappersGrid" : "UnprocessedWrappersGrid", true)[0] as ZGrid;
					var screenMenuItemIsEnabled = true;

					grid.ContextMenu.Popup += delegate
					{
						var screenMenuItem = grid.ContextMenu.MenuItems.FindByName("Screen", false);
						if (screenMenuItem != null)
						{
							screenMenuItemIsEnabled = screenMenuItem.Enabled;
						}

						grid.ContextMenu.Dispose();
					};

					grid.SelectAllElements();
					grid.ContextMenu.Show(grid, grid.Location);
					AssertEquals(expectedIsEnabled, screenMenuItemIsEnabled);
				}
			}
		}

		public void TestScreenMenuItemClick_ProcessedGrid()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty = new ScreeningParty(parent, "", matchedHeader);

			var matchedHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty2 = new ScreeningParty(parent, "", matchedHeader2);

			var permanentClearHeader = Factory.NewWithValidTestData<OrgHeader>();
			permanentClearHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var permanentClearParty = new ScreeningParty(parent, "", permanentClearHeader);

			Factory.Save();

			AssertScreenMenuItemClickResult(false, true);
			AssertScreenMenuItemClickResult(false, false);
			AssertScreenMenuItemClickResult(true, false);

			void AssertScreenMenuItemClickResult(bool hasSecurity, bool onlySelectCLPItem)
			{
				var collection = new PartyComplianceWrapperFilteredCollection(new[] { permanentClearParty, matchedParty, matchedParty2 });
				using (var form = new PartyComplianceFormForTest(collection, true))
				{
					Env.Security.DpsAllowForceReScreen.IsAllowed = hasSecurity;

					form.Show();

					var grid = form.Controls.Find("ProcessedWrappersGrid", true)[0] as ZGrid;
					grid.ContextMenu.Popup += delegate
					{
						var screenMenuItem = grid.ContextMenu.MenuItems.FindByName("Screen", false);
						if (screenMenuItem != null)
						{
							screenMenuItem.PerformClick();
						}

						grid.ContextMenu.Dispose();
					};

					AssertEquals("Precondition", PartyComplianceButton.Cancel, form.PerformButtonClick);
					AssertNull("Precondition", form.SelectedParties);

					UnitTestUserNotification.Instance.ClearMessages();

					if (onlySelectCLPItem)
					{
						grid.SelectSingleElement(collection.ProcessedParties[0]);
						grid.ContextMenu.Show(grid, grid.Location);
						AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
						AssertEquals("Unable to screen parties that are in Permanent Clear status", UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(PartyComplianceButton.Cancel, form.PerformButtonClick);
						AssertNull(form.SelectedParties);
						Assert(!form.ClosingCalled);
					}
					else
					{
						grid.Select(0);
						grid.Select(2);
						grid.ContextMenu.Show(grid, grid.Location);

						if (hasSecurity)
						{
							AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals(PartyComplianceButton.ForceRescreen, form.PerformButtonClick);
							AssertContainsExactElementsInAnyOrder(new[] { permanentClearParty, matchedParty2 }, form.SelectedParties);
							Assert(form.ClosingCalled);
						}
						else
						{
							AssertEquals(Env.Security.DpsAllowForceReScreen.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals(PartyComplianceButton.Cancel, form.PerformButtonClick);
							AssertNull(form.SelectedParties);
							Assert(!form.ClosingCalled);
						}
					}
				}
			}
		}

		public void TestScreenMenuItemClick_UnprocessedGrid()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();

			var notScreenHeader = Factory.NewWithValidTestData<OrgHeader>();
			notScreenHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var notScreenParty = new ScreeningParty(parent, "", notScreenHeader);

			var unknownHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty1 = new ScreeningParty(parent, "", unknownHeader1);

			var unknownHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader2.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty2 = new ScreeningParty(parent, "", unknownHeader2);

			Factory.Save();

			var collection = new PartyComplianceWrapperFilteredCollection(new[] { notScreenParty, unknownParty1, unknownParty2 });
			using (var form = new PartyComplianceFormForTest(collection, true))
			{
				form.Show();

				var grid = form.Controls.Find("UnprocessedWrappersGrid", true)[0] as ZGrid;

				grid.ContextMenu.Popup += delegate
				{
					var screenMenuItem = grid.ContextMenu.MenuItems.FindByName("Screen", false);
					if (screenMenuItem != null)
					{
						screenMenuItem.PerformClick();
					}

					grid.ContextMenu.Dispose();
				};

				AssertEquals("Precondition", PartyComplianceButton.Cancel, form.PerformButtonClick);
				AssertNull("Precondition", form.SelectedParties);

				UnitTestUserNotification.Instance.ClearMessages();

				grid.Select(0);
				grid.Select(2);
				grid.ContextMenu.Show(grid, grid.Location);
				AssertEquals(PartyComplianceButton.Screen, form.PerformButtonClick);
				AssertContainsExactElementsInAnyOrder(new[] { notScreenParty, unknownParty2 }, form.SelectedParties);
				Assert(form.ClosingCalled);
			}
		}

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}
	}

	#region Implementation

	internal class PartyComplianceFormForTest : PartyComplianceForm
	{
		public PartyComplianceFormForTest(PartyComplianceWrapperFilteredCollection collection, bool cancelClosing = false) : base(collection)
		{
			CancelClosingOnce = cancelClosing;
		}

		bool CancelClosingOnce { get; set; }

		public bool ClosingCalled { get; set; }

		public void Grid_ColorDeciding_Exposed(object sender, ColourDecidingEventArgs e) => Grid_ColorDeciding(sender, e);

		protected override void OnClosing(CancelEventArgs e)
		{
			if (CancelClosingOnce)
			{
				e.Cancel = true;
				CancelClosingOnce = false;
				ClosingCalled = true;
			}
			else
			{
				base.OnClosing(e);
			}
		}

		public ZForm OpenedOrganizationForm => (ZForm)ZApplication.GetOpenForms().Where(f => f.Name == "ZOrganisationsForm").FirstOrDefault();

		public ZForm OpenedPartyComplianceForm => (ZForm)ZApplication.GetOpenForms().Where(f => f.Name == "PartyComplianceForm").FirstOrDefault();
	}

	#endregion

}
