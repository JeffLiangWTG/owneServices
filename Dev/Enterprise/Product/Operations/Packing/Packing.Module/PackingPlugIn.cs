using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Packing.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Packing.Module
{
	public class PackingPlugIn : ZPlugIn
	{
		public PackingPlugIn(IBusiness parent)
			: base(parent)
		{
		}

		public override string Name
		{
			get { return Res.GetString("50c09192-d4ec-4fdc-ac23-de880aaff9c2", "Packing"); }
		}

		#region BusinessEntity / UserControl

		protected override IBusiness GetBusinessEntityForPlugIn() => HostBusinessEntity;

		protected override Control GetNewUserControl() => new PackingUserControl();

		protected override ZBool HasUserControl => true;

		public new PackingUserControl UserControl => (PackingUserControl)base.UserControl;

		#endregion

		#region Hook / Unhook Events

		protected override void HookFormEventsCore()
		{
			base.HookFormEventsCore();
			HookSelectedTabChangedEvent();
		}

		protected override void UnHookFormEventsCore()
		{
			base.UnHookFormEventsCore();
			UnhookHookSelectedTabChangedEvent();
		}

		void HookSelectedTabChangedEvent()
		{
			if (TopLevelTabControl != null)
			{
				TopLevelTabControl.SelectedIndexChanging += TabControl_SelectedIndexChanging;
			}
		}

		void UnhookHookSelectedTabChangedEvent()
		{
			if (TopLevelTabControl != null)
			{
				TopLevelTabControl.SelectedIndexChanging -= TabControl_SelectedIndexChanging;
			}
		}

		#endregion

		#region Selected Tab Change

		/// <summary>
		/// This will Force the Packing User Control to bind when the user clicks on the Packing Tab.
		/// This is important to ensure the Control is bound before OnVisible is called.
		/// </summary>
		void TabControl_SelectedIndexChanging(object sender, EventArgs e)
		{
			if (TopLevelTabControl.SelectedTab == TabPage)
			{
				if (!UserControl.IsBound)
				{
					BindPackingUserControl();
				}

				UnhookHookSelectedTabChangedEvent();
			}
		}

		#endregion

		#region Auto-Size Parent Form to fit Packing

		protected override ZTabPagePlugIn GetTabPage()
		{
			return new ZAutoSizedTabPagePlugIn(this);
		}

		public override void UpdateTabPageMinimumAutoSized()
		{
			const int ExtraWidthRequiredToSizeTabPageProperly = 4;
			TabPage.MinimumAutoSizedWidth = UserControl.MinimumSize.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(ExtraWidthRequiredToSizeTabPageProperly);
			TabPage.MinimumAutoSizedHeight = UserControl.MinimumSize.Height;
		}

		#endregion

		#region Current (DataSource) Changed

		protected override void OnCurrentChanged()
		{
			base.OnCurrentChanged();
			BindPackingUserControl();
		}

		#endregion

		#region Bind

		void BindPackingUserControl() => UserControl.SetDataBinding(CurrentPackingParent, "");

		IBusiness CurrentPackingParent => (IsCurrentDependent ? Current : HostBusinessEntity);

		#endregion

		#region License Checkpoint

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Core;

		#endregion
	}
}
