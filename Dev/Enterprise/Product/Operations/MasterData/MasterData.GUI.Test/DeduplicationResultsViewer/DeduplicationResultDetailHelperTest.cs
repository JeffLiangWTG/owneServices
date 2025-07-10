using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	public class DeduplicationResultDetailHelperTest : TestCaseWithFactory
	{
		public void TestLayoutModelDetailUserControls()
		{
			var contentPanel = new KTableLayoutPanel();
			var shouldbeRemovedControl = new ZTextBox();
			contentPanel.Controls.Add(shouldbeRemovedControl);

			var group = new DuplicationModelDetailGroup(
				null,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				new[] { "Name" },
				new[] { "Name" });

			var control1 = new DuplicationModelDetailUserControlForTest(group, withConfidence: false);
			var control2 = new DuplicationModelDetailUserControlForTest(group, withConfidence: true);
			var controls = new[] { control1, control2 };

			var index = 0;
			var creator = new Func<DuplicationModelDetailGroup, DuplicationModelDetailUserControl>(_ =>
			{
				var control = controls[index];
				index++;
				return control;
			});

			var groups = new DuplicationModelDetailGroup[] { null, null };
			DeduplicationResultDetailHelper.LayoutModelDetailUserControls(contentPanel, groups, creator);

			using (var form = new ZForm())
			{
				form.Controls.Add(contentPanel);
				form.Size = new Size(500, 500);
				form.Show();

				AssertEquals(2 + 1, contentPanel.Controls.Count);
				AssertEquals(2 + 1, contentPanel.RowStyles.Count);
				AssertEquals(control1, contentPanel.Controls[0]);
				AssertEquals(control2, contentPanel.Controls[1]);
				AssertEquals("FixedPanel", contentPanel.Controls[2].Name);
				AssertEquals(control1.PreferredHeight, (int)contentPanel.RowStyles[0].Height);
				AssertEquals(control2.PreferredHeight, (int)contentPanel.RowStyles[1].Height);
			}
		}

		public void TestLayoutMergeModeUserControls()
		{
			var contentPanel = new KTableLayoutPanel();
			var shouldbeRemovedControl = new ZTextBox();
			contentPanel.Controls.Add(shouldbeRemovedControl);

			var control1 = new DuplicationMergeModeUserControl(new DuplicationModelDetailCollection());
			var controls = new[] { control1 };

			var index = 0;
			var creator = new Func<DuplicationModelDetailGroup, DuplicationMergeModeUserControl>(group =>
			{
				var control = controls[index];
				index++;
				return control;
			});

			var rowStyles = new[]
			{
				new RowStyle(SizeType.Absolute, 50),
				new RowStyle(SizeType.Absolute, 100),
				new RowStyle(SizeType.Percent, 100F)
			};

			var groupCreator = new Func<string, DuplicationModelDetailGroup>(groupType => new DuplicationModelDetailGroup(
				groupType,
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<DuplicationModelDetail>(),
				Enumerable.Empty<string>(),
				Enumerable.Empty<string>()));
			var groups = new DuplicationModelDetailGroup[] { groupCreator.Invoke(null), groupCreator.Invoke(DeduplicationProvider.Constants.Addresses) };

			DeduplicationResultDetailHelper.LayoutMergeModeUserControls(contentPanel, rowStyles, groups, creator);

			using (var form = new ZForm())
			{
				form.Controls.Add(contentPanel);
				form.Size = new Size(500, 500);
				form.Show();

				AssertEquals(1 + 1, contentPanel.Controls.Count);
				AssertEquals(3, contentPanel.RowStyles.Count);
				AssertEquals(control1, contentPanel.Controls[0]);
				AssertEquals("FixedPanel", contentPanel.Controls[1].Name);
				AssertEquals(50, (int)contentPanel.RowStyles[0].Height);
				AssertEquals(100, (int)contentPanel.RowStyles[1].Height);
			}
		}

		public void TestGetConfidenceForeColor()
		{
			AssertEquals(Color.Green, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.Exact));
			AssertEquals(Color.Green, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.High));
			AssertEquals(Color.Orange, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.Medium));
			AssertEquals(Color.Red, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.Low));
			AssertEquals(Color.Red, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.None));
			AssertEquals(Color.Transparent, DeduplicationResultDetailHelper.GetConfidenceForeColor(ConfidenceRating.Undefined));
		}

		[ExpectNoExceptions]
		public void TestShowForm_DoNothing()
		{
			var nullResultDetail = (IDeduplicationResultDetail)null;
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();

			nullResultDetail.ShowFormForMaster(null, null);
			nullResultDetail.ShowFormForCandidate(null, null);
			resultDetail.ShowFormForMaster(null, null);
			resultDetail.ShowFormForCandidate(null, null);
		}

		[ExpectNoExceptions]
		public void TestShowForm_NonSelectedCandidate()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest();

			resultDetail.ShowFormForMaster(null, null);
			resultDetail.ShowFormForCandidate(null, null);
		}

		[ExpectNoExceptions]
		public void TestShowForm_BizoIsNull()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest()
			{
				SelectedCandidatePK = ZGuid.NewZGuid()
			};

			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForMaster(null, null);
			AssertEquals(CommonMessage.DuplicateRecordRemovedPressCtrlG.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForCandidate(null, null);
			AssertEquals(CommonMessage.DuplicateRecordRemovedPressCtrlG.ToString(), UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();
		}

		[ExpectNoExceptions]
		public void TestShowForm_ViewEntityFormCheckPointNotAllowed_OrgHeader()
		{
			var resultDetail = new DeduplicationOrganisationResultDetailForTest()
			{
				SelectedCandidatePK = ZGuid.NewZGuid()
			};
			var bizOGetter = Factory.New<OrgHeader>();

			var organisationView = Env.Security.OrganisationView.IsAllowed;
			Env.Security.OrganisationView.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForMaster(bizOGetter, null);
			AssertEquals(Env.Security.OrganisationView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForCandidate(bizOGetter, null);
			AssertEquals(Env.Security.OrganisationView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			Env.Security.OrganisationView.IsAllowed = organisationView;
		}

		public void TestShowForm_ViewEntityFormCheckPointNotAllowed_GlbPerson()
		{
			var resultDetail = new DeduplicationPersonResultDetailForTest()
			{
				SelectedCandidatePK = ZGuid.NewZGuid()
			};
			var bizOGetter = Factory.New<GlbPerson>();

			var personIntelligenceView = Env.Security.PersonIntelligenceView.IsAllowed;
			Env.Security.PersonIntelligenceView.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForMaster(bizOGetter, null);
			AssertEquals(Env.Security.PersonIntelligenceView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			resultDetail.ShowFormForCandidate(bizOGetter, null);
			AssertEquals(Env.Security.PersonIntelligenceView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			Env.Security.PersonIntelligenceView.IsAllowed = personIntelligenceView;
		}

		protected override void TearDown()
		{
			UnitTestUserNotification.Instance.ClearMessages();
			base.TearDown();
		}
	}
}
