using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	internal class AllocationUsageUserControlTest : BaseAgencyTest
	{
		#region TestBinding
		[ExpectNoExceptions]
		public void TestBinding()
		{
			AgencyShipment shipment = NewShipment(ImportSailing, null, true, false);
			using (FormForTestingControl form = new FormForTestingControl(shipment))
			{
				form.Show();
			}
		}

		#endregion
		#region FormForTestingControl
		class FormForTestingControl : ZForm
		{
			public FormForTestingControl(AgencyShipment shipment) : base(shipment)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				Control = new ShipmentAllocationUsageControl();
				Control.Dock = DockStyle.Fill;
				this.Controls.Add(Control);
			}

			public ShipmentAllocationUsageControl Control;
		}
		#endregion
	}
}
