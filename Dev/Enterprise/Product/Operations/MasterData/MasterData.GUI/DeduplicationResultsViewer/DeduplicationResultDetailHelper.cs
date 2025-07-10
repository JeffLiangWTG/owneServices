using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterData.GUI
{
	public static class DeduplicationResultDetailHelper
	{
		public static void LayoutModelDetailUserControls(
			KTableLayoutPanel contentPanel,
			IEnumerable<DuplicationModelDetailGroup> detailGroups,
			Func<DuplicationModelDetailGroup, DuplicationModelDetailUserControl> detailControlCreator)
		{
			contentPanel.Controls.RemoveAndDisposeAll();
			contentPanel.RowStyles.Clear();

			var row = 0;
			detailGroups.ForEach(group =>
			{
				var detailControl = detailControlCreator.Invoke(group);
				detailControl.Dock = DockStyle.Fill;
				contentPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, detailControl.PreferredHeight));
				contentPanel.Controls.Add(detailControl, 0, row);
				row++;
			});

			contentPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			contentPanel.Controls.Add(new ZPanel() { Name = "FixedPanel", Size = ControlDpiScalingHelper.NewScaledSize(1, 0, true) }, 0, row);
		}

		public static void LayoutMergeModeUserControls(
			KTableLayoutPanel contentPanel,
			IEnumerable<RowStyle> rowStyles,
			IEnumerable<DuplicationModelDetailGroup> detailGroups,
			Func<DuplicationModelDetailGroup, DuplicationMergeModeUserControl> mergeModeControlCreator)
		{
			contentPanel.Controls.RemoveAndDisposeAll();
			contentPanel.RowStyles.Clear();
			rowStyles.ForEach(x => contentPanel.RowStyles.Add(x));

			var row = 0;
			detailGroups.ForEach(group =>
			{
				if (group.IsSupportMerge)
				{
					var detailControl = mergeModeControlCreator.Invoke(group);
					detailControl.Dock = DockStyle.Fill;
					contentPanel.Controls.Add(detailControl, 0, row);
				}

				row++;
			});

			contentPanel.Controls.Add(new ZPanel() { Name = "FixedPanel" }, 0, row);
		}

		public static void ShowFormForMaster(this IDeduplicationResultDetail resultDetail, BusinessObject bizO, Type dedupeMasterType)
		{
			if (resultDetail is null)
			{
				return;
			}
			ShowForm(resultDetail, bizO, dedupeMasterType, null);
		}

		public static void ShowFormForCandidate(this IDeduplicationResultDetail resultDetail, BusinessObject bizO, Type dedupeMasterType)
		{
			if (resultDetail is null || resultDetail.SelectedCandidatePK.IsEmpty)
			{
				return;
			}

			ShowForm(resultDetail, bizO, dedupeMasterType, (form, businessObject) =>
			{
				businessObject.UpdatedByDataRefresh += ChangeReloadRequired;

				form.FormClosed += (s, e) =>
				{
					bizO.UpdatedByDataRefresh -= ChangeReloadRequired;
					resultDetail.CandidateBizOFormClosedHandler(businessObject);
				};

				void ChangeReloadRequired(object sender, EventArgs eventArgs)
				{
					resultDetail.ReloadRequired = true;
				}
			});
		}

		public static Color GetConfidenceForeColor(ConfidenceRating confidence)
		{
			switch (confidence)
			{
				case ConfidenceRating.Exact:
				case ConfidenceRating.High:
					return Color.Green;
				case ConfidenceRating.Medium:
					return Color.Orange;
				case ConfidenceRating.Low:
				case ConfidenceRating.None:
					return Color.Red;
				default:
					return Color.Transparent;
			}
		}

		static void ShowForm(IDeduplicationResultDetail resultDetail, BusinessObject bizO, Type dedupeMasterType, Action<ZForm, BusinessObject> formClosedHandler)
		{
			if (bizO is null)
			{
				Globals.Message.ShowError(CommonMessage.DuplicateRecordRemovedPressCtrlG);
				return;
			}

			if (!resultDetail.ViewEntityFormCheckPoint.IsAllowed)
			{
				resultDetail.ViewEntityFormCheckPoint.ShowError();
				return;
			}

			var controller = ZControllerFactory.Instance.GetControllerForBizo(bizO) ?? ZControllerFactory.Instance.GetControllerForType(dedupeMasterType);
			if (controller == null)
			{
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"You do not have appropriate controller for the BusinessObject: Type[{0}] PK[{1}]", bizO.GetType(), bizO.PK);
				ExceptionReporter.Instance.ReportDeveloperException("0c9f5348-de1d-4685-b0c6-a3e9909b89a8", message, new ArgumentNullException(message));
				return;
			}

			var form = controller.ShowEditForm(bizO) as ZForm;
			if (form is null)
			{
				return;
			}

			formClosedHandler?.Invoke(form, bizO);
		}
	}
}
