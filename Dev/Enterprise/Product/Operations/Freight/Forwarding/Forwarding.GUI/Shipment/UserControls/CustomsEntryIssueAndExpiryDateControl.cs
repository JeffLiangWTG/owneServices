using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class CustomsEntryIssueAndExpiryDateControl : ZUserControl, IExtendedControl
	{
		public CustomsEntryIssueAndExpiryDateControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		ForwardingShipment GetShipment()
		{
			return (ForwardingShipment)CurrentDataItem;
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			ForwardingShipment shipment = GetShipment();

			if (shipment != null)
			{
				shipment.CusEntryNumbers.CountChanged -= CusEntryNumbersCountChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			ForwardingShipment shipment = GetShipment();

			if (shipment != null)
			{
				shipment.CusEntryNumbers.CountChanged += CusEntryNumbersCountChanged;
				SetCustomEntriesControl();
			}
		}

		void CusEntryNumbersCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			SetCustomEntriesControl();
		}

		void SetCustomEntriesControl()
		{
			var shipment = GetShipment();

			if (shipment == null)
			{
				return;
			}

			if (shipment.CusEntryNumbers.Where(x => x.CE_EntryIsSystemGenerated).Count() <= 1)
			{
				customsEntryNameIssueDateEdit.Visible = customsEntryNameExpiryDateEdit.Visible = true;

				this.GetExtension<LabelCaptionRenderer>().Caption = Res.GetString("f63bb61b-5996-45c1-98c8-18c7f4b8039f", "Issue Date");
			}
			else
			{
				customsEntryNameIssueDateEdit.Visible = customsEntryNameExpiryDateEdit.Visible = false;

				this.GetExtension<LabelCaptionRenderer>().Caption = string.Empty;
				this.GetExtension<LabelCaptionRenderer>().LabelSeparator = string.Empty;
			}
		}

		#region IExtendedControl Members

		[Browsable(false)]
		public IControlExtensionCollection Extensions { get; private set; }

		System.Windows.Forms.Control IExtendedControl.Host
		{
			get { return this; }
		}

		#endregion
	}
}
