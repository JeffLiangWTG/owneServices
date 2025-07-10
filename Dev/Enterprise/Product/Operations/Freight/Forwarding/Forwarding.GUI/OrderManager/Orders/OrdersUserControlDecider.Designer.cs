using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersUserControlDecider : ZUserControl
	{
		void InitializeComponent()
		{
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrdersUserControlDecider
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "OrdersUserControlDecider";
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
