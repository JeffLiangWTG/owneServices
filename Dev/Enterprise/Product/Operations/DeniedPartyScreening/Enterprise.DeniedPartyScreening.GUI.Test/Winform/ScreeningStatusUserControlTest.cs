using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ScreeningStatusUserControlTest : TestCaseWithFactory
	{
		public void TestInitScreeningStatusUserControl()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "C01", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem { Code = "C02", Title = "Title2", ClearingReason = "", IsMandatory = false, }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection);
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty);
				using (var form = new ZForm())
				using (var userControl = new ScreeningStatusUserControl())
				{
					form.Controls.Add(userControl);
					userControl.SetDataBinding(model, "");
					form.Show();

					var matchDecisionDropEdit = userControl.FindSingle<ZDropEdit>("MatchDecisionDropEdit");
					var clearingReasonDropEdit = userControl.FindSingle<ZDropEdit>("ClearingReasonDropEdit");
					var clearingReasonTextBox = userControl.FindSingle<ZTextBox>("ClearingReasonTextBox");

					AssertEquals(model.LearnDeniedPartyScreeningText, userControl.FindSingle<ZLinkLabel>("DpsLinkLabel").Text);
					AssertEquals(true, userControl.FindSingle<ZPanel>("DropEditAndTextBoxPanel").Visible);

					AssertEquals(model.ChangeStatusWaterMark.Caption, matchDecisionDropEdit.CaptionResourceString.Caption);
					AssertEquals(2, matchDecisionDropEdit.List.Count);

					AssertEquals(model.ClearingReasonWaterMark.Caption, clearingReasonDropEdit.CaptionResourceString.Caption);
					AssertEquals(3, clearingReasonDropEdit.List.Count);
					AssertEquals(false, clearingReasonDropEdit.Visible);

					AssertEquals(false, clearingReasonTextBox.Visible);
					AssertEquals(false, clearingReasonTextBox.Enabled);

					model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty, standAlone: true);
					userControl.SetDataBinding(model, "");
					AssertEquals(false, userControl.FindSingle<ZPanel>("DropEditAndTextBoxPanel").Visible);
				}
			}
		}

		public void TestClearingReasonDropEditAndClearingReasonTextBox()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "C01", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem { Code = "C02", Title = "Title2", ClearingReason = "", IsMandatory = false, }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection);
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty);
				using (var form = new ZForm())
				using (var userControl = new ScreeningStatusUserControl())
				{
					form.Controls.Add(userControl);
					userControl.SetDataBinding(model, "");
					form.Show();

					var clearingReasonDropEdit = userControl.FindSingle<ZDropEdit>("ClearingReasonDropEdit");
					var clearingReasonTextBox = userControl.FindSingle<ZTextBox>("ClearingReasonTextBox");

					AssertEquals(false, clearingReasonDropEdit.Visible);
					AssertEquals(false, clearingReasonTextBox.Visible);
					AssertEquals(false, clearingReasonTextBox.Enabled);

					model.ScreeningStatus = ScreeningStatusesList.Codes.Clear;
					AssertEquals(true, clearingReasonDropEdit.Visible);
					AssertEquals(false, clearingReasonTextBox.Visible);

					model.ClearingReason = "C02";
					CombineAssertions("Not mandatory cleared reason", () =>
					{
						AssertEquals(true, clearingReasonDropEdit.Visible);
						AssertEquals(true, clearingReasonTextBox.Visible);
						AssertEquals(false, clearingReasonTextBox.Enabled);
						AssertEquals(StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString(), clearingReasonTextBox.Text);
					});

					model.ClearingReason = "C01";
					CombineAssertions("Mandatory cleared reason", () =>
					{
						AssertEquals(true, clearingReasonDropEdit.Visible);
						AssertEquals(true, clearingReasonTextBox.Visible);
						AssertEquals(true, clearingReasonTextBox.Enabled);
						AssertEquals(string.Empty, clearingReasonTextBox.Text);
					});

					model.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					AssertEquals(false, clearingReasonDropEdit.Visible);
					AssertEquals(false, clearingReasonTextBox.Visible);
				}
			}
		}

		public void TestNotifySavingButtonStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var notifyTriggered = 0;
			var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty, () => { notifyTriggered++; });
			using (var form = new ZForm())
			using (var userControl = new ScreeningStatusUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(model, "");
				form.Show();

				AssertEquals(0, notifyTriggered);

				model.ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals(1, notifyTriggered);
			}
		}

		public void TestClearingReasonTextBox_TextChanged()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var screeningParty = new ScreeningParty(orgHeader, string.Empty, orgHeader);
			var notifyTriggered = 0;
			var model = new ScreeningStatusWinModel(ScoreGrades.Medium, screeningParty, () => { notifyTriggered++; });
			model.ScreeningStatus = "CLR";
			model.ClearingReason = "OTH";
			using (var form = new ZForm())
			using (var userControl = new ScreeningStatusUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(model, "");
				form.Show();

				AssertEquals(string.Empty, model.ClearingReasonText);
				AssertEquals(2, notifyTriggered);
				var clearingReasonTextBox = userControl.FindSingle<ZTextBox>("ClearingReasonTextBox");
				clearingReasonTextBox.Text = "New Reason";
				AssertEquals("Use isSettingClearingText to prevent ClearingReasonText set back to empty", "New Reason", model.ClearingReasonText);
				AssertEquals(3, notifyTriggered);
			}
		}
	}
}
