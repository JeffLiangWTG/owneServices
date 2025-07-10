using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI
{
	public partial class CreateInBondForConsolForm : ZChildForm
	{
		public CreateInBondForConsolForm(CreateInBondForConsolWraper dataSource)
			: base(dataSource)
		{
			InitializeComponent();
			this.Text = string.Format("Create In-Bond For {0}", dataSource.ConsolNumber);
			NewMovementHeadersGrid.AfterBind += NewMovementHeadersGrid_AfterBind;
		}

		void CreateButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		void GaveUpButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		void AddButton_Click(object sender, System.EventArgs e)
		{
			var shipmentsWithoutInBonds = ShipmentsForInBondGrid.SelectedElements;
			var movementHeader = NewMovementHeadersGrid.GetCurrent();
			if (movementHeader is MovementHeaderWrapper header)
			{
				if (shipmentsWithoutInBonds.Length > 0)
				{
					foreach (var shipmentsWithoutInBond in shipmentsWithoutInBonds)
					{
						if (shipmentsWithoutInBond is CusInBondShipmentWrapper cusInBondWrapper)
						{
							((CreateInBondForConsolWraper)DataSource).ShipmentsWithoutInBond.MoveBOToTargetCollection(header.AllocatedShipmentsForMovement, cusInBondWrapper);
						}
					}
				}
				else
				{
					var shipmentsWithoutInBond = ShipmentsForInBondGrid.GetCurrent();
					if (shipmentsWithoutInBond is CusInBondShipmentWrapper cusInBondWrapper)
					{
						((CreateInBondForConsolWraper)DataSource).ShipmentsWithoutInBond.MoveBOToTargetCollection(header.AllocatedShipmentsForMovement, cusInBondWrapper);
					}
				}
			}
		}

		void RemoveButton_Click(object sender, System.EventArgs e)
		{
			var allocatedShipmentsForMovements = AllocatedShipmentsForMovementGrid.SelectedElements;
			var movementHeader = NewMovementHeadersGrid.GetCurrent();
			if (movementHeader is MovementHeaderWrapper header)
			{
				if (allocatedShipmentsForMovements.Length > 0)
				{
					foreach (var allocatedShipmentsForMovement in allocatedShipmentsForMovements)
					{
						if (allocatedShipmentsForMovement is CusInBondShipmentWrapper cusInBondWrapper)
						{
							header.AllocatedShipmentsForMovement.MoveBOToTargetCollection(((CreateInBondForConsolWraper)DataSource).ShipmentsWithoutInBond, cusInBondWrapper);
						}
					}
				}
				else
				{
					var allocatedShipmentsForMovement = AllocatedShipmentsForMovementGrid.GetCurrent();
					if (allocatedShipmentsForMovement is CusInBondShipmentWrapper cusInBondWrapper)
					{
						header.AllocatedShipmentsForMovement.MoveBOToTargetCollection(((CreateInBondForConsolWraper)DataSource).ShipmentsWithoutInBond, cusInBondWrapper);
					}
				}
			}
		}

		void NewMovementHeadersGrid_AfterBind(object sender, System.EventArgs e)
		{
			NewMovementHeadersGrid.ListManager.CurrentChanged -= NewMovementHeadersGrid_ListManager_CurrentChanged;
			NewMovementHeadersGrid.ListManager.CurrentChanged += NewMovementHeadersGrid_ListManager_CurrentChanged;
			NewMovementHeadersGrid_ListManager_CurrentChanged(null, null);

			NewMovementHeadersGrid.ListManager.CurrentItemChanged -= NewMovementHeadersGrid_ListManager_CurrentChanged;
			NewMovementHeadersGrid.ListManager.CurrentItemChanged += NewMovementHeadersGrid_ListManager_CurrentChanged;
			NewMovementHeadersGrid_ListManager_CurrentChanged(null, null);
		}

		void NewMovementHeadersGrid_ListManager_CurrentChanged(object sender, System.EventArgs e)
		{
			var movementHeader = NewMovementHeadersGrid.ListManager.GetCurrent();
			if (movementHeader is MovementHeaderWrapper header)
			{
				AllocatedShipmentsForMovementGroupBox.Text = string.Format("Allocated Shipments for Movement ({0})", header.InBondNumber);
			}
		}
	}
}
