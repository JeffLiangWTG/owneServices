using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public partial class OrdersUserControlDecider : ZUserControl
	{
		public OrdersUserControlDecider()
		{
			InitializeComponent();
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public OrdersUserControl Inner
		{
			get { return fInner; }
			set
			{
				if (fInner != null)
				{
					Controls.Remove(fInner);
					fInner.Dispose();
				}
				Controls.Add(value);
				fInner = value;
				value.Dock = DockStyle.Fill;
			}
		}

		protected OrdersUserControl fInner;
	}
}
