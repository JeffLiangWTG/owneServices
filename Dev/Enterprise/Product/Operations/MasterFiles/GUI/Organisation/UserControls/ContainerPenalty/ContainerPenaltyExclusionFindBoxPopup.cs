using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	public class ContainerPenaltyExclusionFindBoxPopup : ZChildForm, IFindBoxPopup
	{
		readonly Point initialPopupLocation;
		public new event EventHandler Closed;

		public ContainerPenaltyExclusionFindBoxPopup(ContainerPenaltyDayExclusion exclusion, Point initialPopupLocation) : base(exclusion)
		{
			this.initialPopupLocation = initialPopupLocation;
			Disposed += new EventHandler(DisposePopup);
			CaptionRenderingEnabled = true;
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

		public SilentSelectResult SelectFromPopupWithoutDisplaying(IFindBox findBox, EmbeddedModulePopup popup) => SilentSelectResult.None;

		public void SelectRowByPK(ZGuid pK)
		{
		}

		public void ShowModal(IFindBox findBox, Form parentForm)
		{
			if (findBox == null || parentForm == null)
			{
				return;
			}

			popup = new ContainerPenaltyExclusionPopup();
			popup.FormClosed += (s, e) => Closed?.Invoke(this, EventArgs.Empty);
			popup.Location = initialPopupLocation;
			popup.ParentWinForm = parentForm;
			BindingSource.SetBindingMember(popup, ".");
			popup.ShowPopup();
		}

		protected ContainerPenaltyExclusionPopup popup;
	}
}
