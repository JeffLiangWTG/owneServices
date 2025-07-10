using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.GUI
{
	public class HVLVCancelableMinimisableProgressForm : ProgressForm
	{
		public HVLVCancelableMinimisableProgressForm(ZForm parentForm, Action cancelAction)
		{
			this.parentForm = parentForm;
			this.cancelAction = cancelAction;

			ControlBox = true;
			MinimizeBox = true;
			ShowCancelButton = true;
			CaptionRenderingEnabled = false;
			CancelProgressButtonText = Res.GetString("b4946595-278f-414f-b8e8-8fd705628912", "Cancel");

			Resize += HVLVCancelableProgressForm_Resize;
			Cancelled += HVLVCancelableProgressForm_Cancelled;
			Disposed += HVLVCancelableProgressForm_Disposed;
		}

		readonly ZForm parentForm;
		readonly Action cancelAction;

		FormWindowState parentFormRestoreState;

		public void ShowProcessFormForAction(string text, Action action)
		{
			RunActionWithProcessBox(text, action);
#if DEBUG
			ProcessFormShown = true;
#endif
		}

#if DEBUG
		public bool ProcessFormShown;
#endif

		void HVLVCancelableProgressForm_Resize(object sender, EventArgs e)
		{
			if (parentForm != null)
			{
				if (WindowState == FormWindowState.Minimized)
				{
					parentFormRestoreState = parentForm.WindowState;
					parentForm.WindowState = FormWindowState.Minimized;
					ShowModalTo(parentForm);
				}
				else if (parentForm.WindowState == FormWindowState.Minimized)
				{
					parentForm.WindowState = parentFormRestoreState;
				}
			}
		}

		void HVLVCancelableProgressForm_Cancelled(object sender, EventArgs e)
		{
			cancelAction?.Invoke();
		}

		void HVLVCancelableProgressForm_Disposed(object sender, EventArgs e)
		{
			if (parentForm != null && parentForm.WindowState == FormWindowState.Minimized)
			{
				parentForm.WindowState = parentFormRestoreState;
			}
		}

#if !WINZOR

		protected override CreateParams CreateParams
		{
			get
			{
				var createParams = base.CreateParams;
				createParams.ClassStyle |= CP_NOCLOSE_BUTTON;
				return createParams;
			}
		}

		const int CP_NOCLOSE_BUTTON = 0x200;

#if DEBUG
		public int ClassStyle => CreateParams.ClassStyle;
#endif

#endif
	}
}
