using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.GUI
{
	public partial class ContainerManagerForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ContainerManagerForm()
		{
			InitializeComponent();
		}

		public ContainerManagerForm(RefContainerStock container)
			: base(container)
		{
			InitializeComponent();
			WorkflowTabPage.Initialize(container);
		}

		public void SelectAndShowMovement(ZGuid movementPK)
		{
			ContainerMovement movement = Stock.Factory.Load<ContainerMovement>(movementPK);

			if (movement != null)
			{
				if (!Stock.Filter.Movements.Contains(movement))
				{
					if (!((IBusiness)Stock.Filter.Movements).HasChanges)
					{
						Stock.Filter.SetToShow(movement);
						Stock.Filter.Find();
					}
					else
					{
						Globals.Message.Show(
							Res.GetString("9965ef25-7a56-441c-a16d-2c0ee0097342", "The selected movement does not match the existing filters and the filters could not be adjusted because there are movements with changes that need to be saved first."),
							Res.GetString("f89ea191-eea4-4ed1-8908-207c5878957b", "Cannot show selected movement"),
							MessageBoxButtons.OK,
							DialogResult.OK);
					}
				}

				MainTabControl.SelectedTab = movementsTab;

				CurrencyManager manager = (CurrencyManager)BindingContext[BusinessEntity, "Filter.Movements"];
				int index = manager.List.IndexOf(movement);
				if (index >= 0)
				{
					manager.Position = index;
				}
			}
		}

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return Res.GetString("35fe4bad-99db-429e-9bf1-0bc3dc8ae2d1", "Container {0}", Stock.R6_ContainerNum); }
		}

		protected override bool SupportsEDocs
		{
			get { return true; }
		}

		#endregion

		#region Implementation

		RefContainerStock Stock
		{
			get { return (RefContainerStock)BusinessEntity; }
		}

		#endregion
	}
}


