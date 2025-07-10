using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class CartageWorkSheetForm : ZTemplateForm, INotifications
	{
		public CartageWorkSheetForm(CommonWorkSheet workSheet)
			: base(workSheet)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
			PlugIns.Add(ControllerIDs.ApportionmentForCommonWorkSheet);

			PlugIns.GetPlugIn(ControllerIDs.ApportionmentForCommonWorkSheet).TabPage.CaptionResourceString = Res.GetData("10b176cb-515e-4a29-86b0-7ea7062048cd", "Costing");

			HookEvents();

			RunSheetSecurityGUIProvider.Register(workSheet.Factory);
		}

		public override string FormCaption
		{
			get
			{
				ZString caption = Res.GetString("6e7fb4cd-29da-4873-97b0-3d9e13938451", "Run Sheet");
				if (!WorkSheet.EY_RunSheetNumber.IsEmpty)
				{
					caption += " - " + WorkSheet.EY_RunSheetNumber;
				}
				return caption;
			}
		}

		void HookEvents()
		{
			if (WorkSheet != null)
			{
				WorkSheet.OnGetCartageLegsToPrint += new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetCartageLegsToPrint);
			}
		}

		void UnHookEvents()
		{
			if (WorkSheet != null)
			{
				WorkSheet.OnGetCartageLegsToPrint -= new EventHandler<DocumentCartageLegEventArgs>(Cartage_OnGetCartageLegsToPrint);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				UnHookEvents();
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void Cartage_OnGetCartageLegsToPrint(object sender, DocumentCartageLegEventArgs e)
		{
			if (e.DocumentCartageLegOptions.CartageLegs.Count == 0)
			{
				e.ContinueToPrint = false;
				Globals.Message.ShowInformation(Res.GetString("b49e8a16-c616-417b-babc-69877f40bc3e", "There are no Port Transport Legs to print."), Res.GetString("7522bec8-cf9e-4ccc-923c-a5344f7a1b03", "No Port Transport Legs"));
			}
			else
			{
				using (var form = new DocumentCartageLegsForm(e.DocumentCartageLegOptions))
				{
					form.ShowDialog();
					e.ContinueToPrint = form.DialogResult == DialogResult.Yes;
				}
			}
		}

		CommonWorkSheet WorkSheet
		{
			get { return (CommonWorkSheet)BusinessEntity; }
		}

		void INotifications.Add(INotification notification)
		{
			Globals.Message.Show(notification.Message);
		}
	}
}
