using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ConsolDetailsControl : ZUserControl
	{
		public ConsolDetailsControl()
		{
			InitializeComponent();
			SetDataSourceBinding(JK_JX_JV_NKVesselTextBox.GetExtension<LabelCaptionRenderer>(), "Caption", "Consols.JK_Calc_VesselLabel", true); // Hard-coded binding constant
			SetDataSourceBinding(JK_JX_JV_VoyageFlightTextBox.GetExtension<LabelCaptionRenderer>(), "Caption", "Consols.JK_Calc_VoyageLabel", true); // Constant used for binding
		}

		#region Consol Position Changing

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			CurrencyManager listManager = ConsolModuleButtonGrid.InnerGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged -= HandleSelectedConsol_CurrentChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			listManager = ConsolModuleButtonGrid.InnerGrid.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged += HandleSelectedConsol_CurrentChanged;
			}
		}

		void HandleSelectedConsol_CurrentChanged(object sender, EventArgs e)
		{
			HandleSelectedConsol_CurrentChanged((CurrencyManager)sender);
		}

		void HandleSelectedConsol_CurrentChanged(CurrencyManager listManager)
		{
			if (listManager.Position > -1)
			{
				ForwardingConsol currentConsol = (ForwardingConsol)listManager.GetCurrent();

				JK_JX_JV_NKVesselTextBox.Visible = !currentConsol.IsAir;
				JK_JX_JA_E_DEPBoundDateEdit.DateTimeFormat = currentConsol.IsSea ? ZDateTimePickerFormat.Short : ZDateTimePickerFormat.Long;
				JK_JX_JB_E_ARVBoundDateEdit.DateTimeFormat = JK_JX_JA_E_DEPBoundDateEdit.DateTimeFormat;
			}
			else
			{
				JK_JX_JV_NKVesselTextBox.Visible = true;
				JK_JX_JA_E_DEPBoundDateEdit.DateTimeFormat = ZDateTimePickerFormat.Short;
				JK_JX_JB_E_ARVBoundDateEdit.DateTimeFormat = JK_JX_JA_E_DEPBoundDateEdit.DateTimeFormat;
			}
		}

		#endregion
	}
}
