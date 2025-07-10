using System.Windows.Forms;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.GUI.Declaration
{
	public class DeliveryNotificationsTabPage : BaseDeclarationTabPage
	{
		public DeliveryNotificationsTabPage()
		{
			this.LazyCreateControls += DeliveryNotificationsTabPage_LazyCreateControls;
		}

		void DeliveryNotificationsTabPage_LazyCreateControls(object sender, System.EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (Controls.Count == 0 && TabVisible)
				{
					DeliveryNotificationsUserControl = new DeliveryNotificationsUserControl();
					DeliveryNotificationsUserControl.Dock = DockStyle.Fill;
					Controls.Add(DeliveryNotificationsUserControl);
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				this.LazyCreateControls -= DeliveryNotificationsTabPage_LazyCreateControls;
			}

			base.Dispose(disposing);
		}

		public DeliveryNotificationsUserControl DeliveryNotificationsUserControl { get; private set; }
	}
}
