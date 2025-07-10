using System;
using System.Windows.Forms;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.GUI
{
	public partial class SelectAllocationRouteForm : ZChildForm
	{
		public SelectAllocationRouteForm(IAllocationRouteAssignable routeAssignable, ViewMultiAllocationSelectionManager viewManager)
			: base(viewManager)
		{
			this.routeAssignable = routeAssignable;
			InitializeComponent();
		}

		readonly IAllocationRouteAssignable routeAssignable;

		public static void ShowDialog(IAllocationRouteAssignable routeAssignable, ViewMultiAllocationSelectionManager viewManager)
		{
			ZFormModaliser.ShowDialogAndDispose(new SelectAllocationRouteForm(routeAssignable, viewManager));
		}

		public override string FormCaption
		{
			get { return Res.GetString("4dd52422-e668-6e84-4101-a4841f26c769", "Select Allocation Route linked with Sailing Schedule(s)"); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			var selectedRoutes = selectionGrid.SelectedElements;
			if (selectedRoutes.Length != 1)
			{
				Globals.Message.ShowError(Res.GetString("b2271308-1428-7da4-4998-e205084f3911", "Select one Allocation Route."));
				return;
			}

			var route = selectedRoutes[0] as IRatingContractAllocationLine;
			routeAssignable.UpdateCarrierContractAndAllocationDetails(route);

			SetDialogResultAndClose(DialogResult.OK);
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			SetDialogResultAndClose(DialogResult.Cancel);
		}

		void SetDialogResultAndClose(DialogResult dialogResult)
		{
			DialogResult = dialogResult;
			Close();
		}
	}
}
