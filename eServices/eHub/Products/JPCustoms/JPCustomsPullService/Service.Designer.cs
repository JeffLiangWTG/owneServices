using System.ComponentModel;

namespace CargoWise.eHub.Products.JPCustoms.PullService
{
	partial class Service
	{
		IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ServiceName = "JPCustoms pull service";
		}

		#endregion
	}
}
