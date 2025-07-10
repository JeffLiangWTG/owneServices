using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class LoadedContainersUserControl : ZUserControl
	{
		public LoadedContainersUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (!this.DesignMode)
			{
				ContainerCustomColumnAdder.Set(this.containersGrid, ContainerCustomColumnAdder.TargetGridType.Editable);
			}
		}
	}
}
