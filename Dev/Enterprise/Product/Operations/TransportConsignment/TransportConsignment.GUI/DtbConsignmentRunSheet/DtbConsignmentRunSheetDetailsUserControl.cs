using System;
using System.Drawing;
using System.Linq;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbConsignmentRunSheetDetailsUserControl : ZUserControl
	{
		public DtbConsignmentRunSheetDetailsUserControl()
		{
			InitializeComponent();

			BackColor = SystemColors.Control; // get rid of the ugly blue but we may have to change this to respect the user-defined colours
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			ActionsTabPage.TabVisible = false;

			if (dataSource != null)
			{
				var runSheet = (DtbConsignmentRunSheet)dataSource;
				if (runSheet.RunSheetInstructions.Any())
				{
					var instruction = runSheet.RunSheetInstructions[0];
					if (instruction.IsConsignmentAction)
					{
						ActionsTabPage.TabVisible = true;
						ConfirmationsTabPage.TabVisible = false;
					}
				}
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (!DesignModeFinder.IsDesigning)
			{
				DomesticTransportGridHelper.HookDoubleClickToOpenConsignmentFromConfirmation(ConfirmationsGrid);
				DomesticTransportGridHelper.HookContextMenuShowSignature(ConfirmationsGrid);
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
