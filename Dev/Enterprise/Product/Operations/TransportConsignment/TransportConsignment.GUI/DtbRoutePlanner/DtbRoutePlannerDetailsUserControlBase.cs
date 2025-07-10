using System;
using System.ComponentModel;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	[ToolboxItem(false)]
	public partial class DtbRoutePlannerDetailsUserControlBase : ZUserControl
	{
		public DtbRoutePlannerDetailsUserControlBase()
		{
			InitializeComponent();
			DomesticTransportGridHelper.HookDoubleClickToOpenConsignmentFromConfirmation(ConfirmationsGrid);
		}

		#region SelectedConfirmations

		public virtual DtbConsignmentConfirmation[] SelectedConfirmations
		{
			get { return Array.ConvertAll(ConfirmationsGrid.SelectedElements, c => (DtbConsignmentConfirmation)c); }
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
