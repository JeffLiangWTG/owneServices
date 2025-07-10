using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefContainerForm : ZForm
	{
		public RefContainerForm(RefContainer container)
			: base(container)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
			this.container = container;
			if (this.container != null)
			{
				this.container.RC_ISOTypeInfo.ValueChanged += RC_ISOTypeInfo_ValueChanged;
			}
		}

		readonly RefContainer container;

		void RC_ISOTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ContainerISOType.DefaultSizing(container);
			container.SetDefaultForISO();
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
				if (container != null)
				{
					container.RC_ISOTypeInfo.ValueChanged -= RC_ISOTypeInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
