using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentPackingDetailForm : ZChildForm
	{
		#region Ctor

		public ShipmentPackingDetailForm(ForwardingShipment shipment)
			: base(shipment)
		{
			InitializeComponent();

			Init();
		}

		#endregion

		#region Properties

		ForwardingShipment Shipment
		{
			get;
			set;
		}

		#endregion

		#region Implementation

		public override string FormCaption
		{
			get { return Res.GetString("52d8898b-8925-44a3-90f8-bfab69f6c3c8", "Packaging Details for Shipment {0}", Shipment.JS_UniqueConsignRef); }
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			if (dataSource != null)
			{
				Shipment = dataSource as ForwardingShipment;
				Shipment.UpdateShipmentTotalsPackQuantityVariation += new CancelEventHandler(ShipmentUpdateShipmentTotalsPackQuantityVariation);
			}
			else if (Shipment != null)
			{
				Shipment.UpdateShipmentTotalsPackQuantityVariation -= new CancelEventHandler(ShipmentUpdateShipmentTotalsPackQuantityVariation);
				Shipment = null;
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (Shipment == null)
			{
				return;
			}

			Shipment.OuterPackLines.RunPreSaveValidation();

			if ((Shipment.OuterPackLines as INotificationProvider).HasNotifications(NotificationType.Error))
			{
				e.Cancel = true;
				Globals.Message.ShowError(Res.GetString("7e88c76b-7a12-452d-9400-7cb8c0996103", "Please fix errors before closing this dialog."));
			}
			else if (matchShipmentTotalCheckBox.Checked)
			{
				Shipment.CheckTotalsDiffer();
			}
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			if (Shipment != null && Shipment.CoLoadMasterShipment != null)
			{
				Shipment.CoLoadMasterShipment.OuterPackLines.Load();
			}
		}

		void ShipmentUpdateShipmentTotalsPackQuantityVariation(object sender, CancelEventArgs e)
		{
			if (!e.Cancel)
			{
				var result = Globals.Message.Show(Res.GetString("b9f306fb-4a64-430d-8f23-28928d89adbe", "Total packs, weight and volume do not match the shipment total. Would you like to update the shipment to match the packline totals?"), Res.GetString("6345ca52-bf13-4eea-85ba-6c7503d4e6e6", "Totals do not match"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				e.Cancel = result != DialogResult.Yes;
			}
		}

		void Init()
		{
			okButton.Click += (s, e) => Close();

			if (!DesignModeFinder.IsDesigning)
			{
				new CustomFieldColumnCreator().Set(packLinesContol.Grid, new PackLineCustomFieldsDescriptor());
				new UNDGDataItemFormManager(packLinesContol.Grid, "", UNDGDataItemFormManagerConfig.ShowSubstanceProperties()).Initialize();
				new PackProductFormManager(packLinesContol.Grid).Initialize();
				new HarmonisedCodeFormManager(packLinesContol.Grid).Initialize();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		public static void Show(ForwardingShipment shipment, Form parentForm)
		{
			if (shipment != null && shipment.ReadOnly || shipment.IsPropertyReadOnlyDueToPhase("OuterPackLines")) // Literal constant for phases
			{
				Globals.Message.Show(Res.GetString("5b6cc75f-0ae1-4660-8176-422d990768b0", "You're not allowed to edit packages of a read-only shipment."));
			}
			else if (shipment != null && shipment.IsMasterShipmentRepresentingAllChildShipments)
			{
				Globals.Message.Show(Res.GetString("68991bc6-f0b8-415b-a83c-1e3412509e60", "You cannot edit pack lines of master shipments."));
			}
			else if (shipment != null && parentForm != null)
			{
				shipment.OuterPackLines.SuspendReadOnly = true;

				ShipmentPackingDetailForm form = new ShipmentPackingDetailForm(shipment);
				form.FormClosed += (s, e) =>
					{
						shipment.OuterPackLines.SuspendReadOnly = false;
					};

				ZFormModaliser.Show(form, parentForm);
				Application.DoEvents();
			}
		}

		#endregion
	}
}
