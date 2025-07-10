using System;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	internal sealed partial class ContainerDetentionControl : ZUserControl
	{
		public ContainerDetentionControl()
		{
			InitializeComponent();
		}

		public event EventHandler Find
		{
			add { findButton.Click += value; }
			remove { findButton.Click -= value; }
		}

		public void PerformFind()
		{
			findButton.PerformClick();
		}

		void containersGrid_Attaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			ContainerDetention detention = CurrentDataItem as ContainerDetention;

			if (detention != null && detention.HasPostedCharges)
			{
				args.Cancel = true;
				Globals.Message.Show(Res.GetString("ae5a6bc4-3162-4d46-9a61-b5867bce6948", "Cannot add containers to a detention job with a posted invoice."));
			}
		}

		void containersGrid_Detaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			ContainerDetention detention = CurrentDataItem as ContainerDetention;

			if (detention != null && detention.HasPostedCharges)
			{
				args.Cancel = true;
				Globals.Message.Show(Res.GetString("ab3da782-bc2a-41e0-b4b4-4696886d2a34", "Cannot remove containers from a detention job with a posted invoice."));
			}
		}
	}
}
