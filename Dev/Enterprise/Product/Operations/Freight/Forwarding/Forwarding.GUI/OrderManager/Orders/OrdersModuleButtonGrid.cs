using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Orders.GUI
{
	public class OrdersModuleButtonGrid : ZModuleButtonGrid
	{
		protected override void NewButton_Click(object sender, System.EventArgs e)
		{
			if (DataSource != null)
			{
				base.NewButton_Click(sender, e);
			}
		}

		protected override void EditButton_Click(object sender, System.EventArgs e)
		{
			if (DataSource != null)
			{
				base.EditButton_Click(sender, e);
			}
		}

		protected override void AttachButton_Click(object sender, System.EventArgs e)
		{
			if (DataSource != null)
			{
				base.AttachButton_Click(sender, e);
			}
		}

		protected override void DetachButton_Click(object sender, System.EventArgs e)
		{
			if (DataSource != null)
			{
				base.DetachButton_Click(sender, e);
			}
		}
	}
}
