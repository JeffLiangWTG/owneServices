using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public class DurationExclusionFindBox : ZGridGuidFindBox
	{
		public DurationExclusionFindBox()
		{
			this.CodeBox.ReadOnly = true;
			Disposed += new EventHandler(DisposePopup);
		}

		void DisposePopup(object sender, EventArgs e)
		{
			if (popup != null)
			{
				Disposed -= new EventHandler(DisposePopup);
				popup.Dispose();
				popup = null;
			}
		}

		protected override void OnPopupFormClosed(IFindBoxPopup popupForm)
		{
			if (ColumnStyle is DurationExclusionColumnStyle durationExclusionColumnStyle)
			{
				durationExclusionColumnStyle.RefreshCodeBoxText();
			}

			if (CurrentItem is not OrgContainerDetention containerDetention)
			{
				return;
			}

			if (containerDetention.DurationExclusion.IsEmpty())
			{
				containerDetention.PD_CEX_DurationExclusion = ZGuid.Empty;
			}
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			if (CurrentItem is not OrgContainerDetention containerDetention)
			{
				return null;
			}

			popup = new ContainerPenaltyExclusionFindBoxPopup(containerDetention.DurationExclusionForBinding, PopupButton.PointToScreen(new Point(0, PopupButton.Height)));
			return popup;
		}

		ContainerPenaltyExclusionFindBoxPopup popup;

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			// Required to set text box to read-only but not the popup button
		}
	}
}
