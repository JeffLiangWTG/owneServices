using System;
using Enterprise.Packing.Business;
using Enterprise.Packing.GUI.Common;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Packing.GUI
{
	public partial class PackingForm : ZTemplateForm
	{
		public PackingForm()
		{
			InitializeComponent();
		}

		public PackingForm(PkgPackageJob packing)
			: base(packing)
		{
			InitializeComponent();
			AddPlugins();
			HookEvents();
		}

		void HookEvents()
		{
			PackingUserControl.PackageJobDelete += (sender, e) => DisablePlugin();
		}

		void AddPlugins()
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		void DisablePlugin()
		{
			var plugin = PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
			if (plugin != null)
			{
				plugin.Enabled = false;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DisableNewAction();
		}

		#region HandleSaveException

		protected override void HandleSaveException(Exception ex)
		{
			if (!FormExceptionHandler.HandleSaveExceptionForTriggers(ex))
			{
				base.HandleSaveException(ex);
			}
		}

		#endregion
	}
}
