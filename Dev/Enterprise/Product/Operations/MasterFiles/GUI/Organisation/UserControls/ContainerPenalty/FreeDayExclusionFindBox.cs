using System;
using System.Drawing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public class FreeDayExclusionFindBox : ZGridGuidFindBox
	{
		public FreeDayExclusionFindBox()
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
			if (ColumnStyle is FreeDayExclusionColumnStyle freeDayExclusionColumnStyle)
			{
				freeDayExclusionColumnStyle.RefreshCodeBoxText();
			}

			if (CurrentItem is not OrgContainerDetention containerDetention)
			{
				return;
			}

			if (containerDetention.FreeDayExclusion.IsEmpty())
			{
				containerDetention.PD_CEX_FreeDayExclusion = ZGuid.Empty;
			}
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			if (CurrentItem is not OrgContainerDetention containerDetention)
			{
				return null;
			}

			popup = new ContainerPenaltyExclusionFindBoxPopup(containerDetention.FreeDayExclusionForBinding, PopupButton.PointToScreen(new Point(0, PopupButton.Height)));
			return popup;
		}

		ContainerPenaltyExclusionFindBoxPopup popup;

		protected override void OnReadOnlyChanged(EventArgs e)
		{
			// Required to set text box to read-only but not the popup button
		}
	}
}
