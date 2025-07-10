using System;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
#if !WINZOR
	[Designer(typeof(ManifestUserControlDesigner))]
#endif
	public partial class ManifestUserControl : ZUserControl
	{
		public ManifestUserControl()
		{
			InitializeComponent();
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public ZTabControl TabControl
		{
			get { return ManifestTabControl; }
		}

		#region Implementaion

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UserIdleWorker.QueueWorkItem(tripUserControl1, 0, new Action(() => tripUserControl1.Focus()), null);
		}

		#endregion

#if !WINZOR
		#region ManifestUserControlDesigner

		class ManifestUserControlDesigner : ParentControlDesigner
		{
			public override void Initialize(IComponent component)
			{
				base.Initialize(component);
				var container = (ManifestUserControl)component;
				EnableDesignMode(container.TabControl, "TabControl");
			}
		}

		#endregion
#endif
	}
}
