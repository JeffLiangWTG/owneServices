using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBViewEditForm : ZChildForm
	{
		public AWBViewEditForm(IAWBParent aWBHost)
			: base((IBusiness)aWBHost)
		{
			try
			{
				if (aWBHost.AWBHeader is not null && aWBHost.AWBHeader.Parent is ForwardingShipment shipment)
				{
					shipment.OnShowMessageOnGUI += HandleShowMessageOnGui;
				}

				aWBHost.PopulateAWB();
			}
			catch (ExportAWBHeaderReplaceMacrosException ex)
			{
				replaceMacrosErrorMessage = ex.Message;
			}

			aWBHost.ResetAddressPickerDropLists();
		}

		static void HandleShowMessageOnGui(object o, ShowMessageOnGUIEventArgs e) => Globals.Message.ShowInformation(e.Message, e.Title);

		#region Bind

		protected override void OnClosing(CancelEventArgs e)
		{
			hawbUserControl1.OverrideValuesCheckBox.Focus();

			if (DataSource is IAWBParent aWBHost && aWBHost.AWBHeader is not null && aWBHost.AWBHeader.Parent is ForwardingShipment shipment)
			{
				shipment.OnShowMessageOnGUI -= HandleShowMessageOnGui;
			}

			base.OnClosing(e);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				hawbUserControl1.OverrideValuesCheckBox.BindTo = ((IAWBParent)dataSource).IsAWBValuesOverriddenPropertyInfo.Name;
			}
			base.SetDataBinding(dataSource, dataMember);
		}

		#endregion

		#region OnShown

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);

			if (!string.IsNullOrEmpty(replaceMacrosErrorMessage))
			{
				Globals.Message.ShowError(replaceMacrosErrorMessage);
				Close();
			}
		}

		readonly string replaceMacrosErrorMessage;

		#endregion

		#region Form Caption

		public override string FormVerb
		{
			get
			{
				return "";
			}
		}

		#endregion
	}
}
