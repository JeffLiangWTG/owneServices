using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Agency.GUI
{
	public sealed partial class ContainerDetentionForm : ZTemplateForm
	{
		public ContainerDetentionForm(ContainerDetention bo)
			: base(bo)
		{
			InitializeComponent();
			PlugIns.AddJobInvoicing(bo.InvoicingSupporter);
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}

		#region Implementation

		protected override ContinueWithSave ValidateAndSave()
		{
			if (!Invoice.IsInDatabase && Invoice.Movements.Count == 0)
			{
				Globals.Message.Show(Res.GetString("923c18e7-43ed-4b05-b728-b1bdda954948", "There are no containers attached to this detention job."), Res.GetString("c4bf91a5-df51-4ad2-9722-8e44da4ff7b7", "Error"), MessageBoxButtons.OK, DialogResult.OK);
				return ContinueWithSave.No;
			}
			else
			{
				return base.ValidateAndSave();
			}
		}

		public override string FormCaption
		{
			get
			{
				ContainerDetention invoice = Invoice;
				return invoice == null ? null : invoice.HumanReadableName;
			}
		}

		public override bool IsResizableByTabPageAllowed => true;

		ContainerDetention Invoice
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (ContainerDetention)BusinessEntity; }
		}

		void mainControl_Find(object sender, EventArgs e)
		{
			if (Invoice.IsInDatabase)
			{
				Globals.Message.Show(Res.GetString("ac001bae-5150-48a8-af95-70db7c41cc21", "You cannot attach containers to a detention job once saved."));
			}
			else if (!Invoice.Lookups.DetentionTypes.ContainsCode(Invoice.NC_DetentionType))
			{
				Globals.Message.Show(Res.GetString("662ae754-baf1-43bc-bfe6-ebf907ed2fbf", "The selected detention type is not valid."));
			}
			else if (Invoice.Principal == null)
			{
				Globals.Message.Show(Res.GetString("81a16c1b-bf80-4ea0-9c68-2d9667b8b0f2", "You have not entered a principal yet."));
			}
			else if (Invoice.Client == null)
			{
				Globals.Message.Show(Res.GetString("7ba9d115-9db1-4e8e-9a18-e619f7d06373", "You have not entered a client yet."));
			}
			else
			{
				ContainerMovementCollection movements = Invoice.Movements;

				foreach (ContainerMovement movement in movements.ToArray())
				{
					movements.RemoveFromRelationship(movement);
				}

				ContainerMovement[] movementsToAdd = Invoice.FindRelatedMovements();
				foreach (ContainerMovement movement in movementsToAdd)
				{
					movements.Add(movement);
				}

				string message;

				switch (movementsToAdd.Length)
				{
					case 0:
						message = Res.GetString("9dfd8168-7e27-4131-b6c4-c8c6fd5b28b2", "No matching containers found.");
						break;

					case 1:
						message = Res.GetString("51dce3d1-0b36-4994-bb21-3cdea7b341bf", "Found 1 matching container.");
						break;

					default:
						message = Res.GetString("88e13aca-b03d-410b-8237-5fc465e974a4", "Found {0} matching containers.", movementsToAdd.Length);
						break;
				}

				Globals.Message.Show(message);
			}
		}

		#endregion
	}
}


