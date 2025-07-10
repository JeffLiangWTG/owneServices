using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.ContractManagement.GUI
{
	public partial class ContractAndAllocationsAttachForm : ZChildForm
	{
		public ContractAndAllocationsAttachForm(IContractSimulationFormConfiguration configuration)
			: base(new ViewCarrierContractsManager(new BusinessObjectFactory(), configuration))
		{
			this.configuration = configuration;
			InitializeComponent();

			if (configuration.FormActions != null)
			{
				SelectContractButton.Visible = configuration.FormActions.IsEnabledAllocationToContract;
				SelectAllocationRouteButton.Visible = configuration.FormActions.IsEnabledAllocationToRoute;
			}
		}

		readonly IContractSimulationFormConfiguration configuration;

		void SelectContractButton_Click(object sender, EventArgs e)
		{
			if (ContractFilterStripControl.Grid.GetCurrent() is CarrierContractForUtilizationSimulation contractHeader)
			{
				var contractErrors = contractHeader.Notifications
					.Where(notification => notification.Type == NotificationType.Error)
					.ToArray();

				if (contractErrors.Length > 0)
				{
					var error = contractErrors[0].Message;
					Globals.Message.ShowError(error);
				}
				else
				{
					if (!configuration.FormActions.TryAllocateToContract(contractHeader))
					{
						return;
					}

					SetDialogResultAndClose(System.Windows.Forms.DialogResult.OK);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("1955c618-42d8-5d8b-44e9-d0e5900057f2", "No Contract Selected."));
			}
		}

		void SelectAllocationRouteButton_Click(object sender, EventArgs e)
		{
			var allocationGridProvider = ContractFilterStripControl as IAllocationGridProvider;
			var currentGridSelectedItem = allocationGridProvider?.AllocationRouteGrid?.ListManager?.GetCurrent();

			if (currentGridSelectedItem is AllocationRouteForUtilizationSimulation allocationRoute
				&& ContractFilterStripControl.Grid.GetCurrent() is CarrierContractForUtilizationSimulation contractHeader)
			{
				var contractErrors = contractHeader.Notifications
					.Where(notification => notification.Type == NotificationType.Error)
					.ToArray();

				if (allocationRoute.HasErrors)
				{
					var error = allocationRoute.GetErrors().GetFirstMessage();
					Globals.Message.ShowError(error);
				}
				else if (contractErrors.Length > 0)
				{
					var error = contractErrors[0].Message;
					Globals.Message.ShowError(error);
				}
				else
				{
					if (!configuration.FormActions.TryAllocateToAllocationRoute(allocationRoute))
					{
						return;
					}

					SetDialogResultAndClose(System.Windows.Forms.DialogResult.OK);
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("f82a71eb-d5bb-20aa-49d9-ea5fa089d124", "No Allocation Route Selected."));
			}
		}

		void CancelButton_OnClick(object sender, EventArgs e)
		{
			SetDialogResultAndClose(System.Windows.Forms.DialogResult.Cancel);
		}

		void SetDialogResultAndClose(System.Windows.Forms.DialogResult dialogResult)
		{
			DialogResult = dialogResult;
			Close();
		}

		public override string FormVerb => string.Empty;
		public override string FormCaption => Res.GetString("694341ae-44a9-b081-4069-c6c715313485", "Carrier Contract & Allocations");
	}
}
