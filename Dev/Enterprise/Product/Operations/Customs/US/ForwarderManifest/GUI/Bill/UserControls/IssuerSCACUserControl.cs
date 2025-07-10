using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.ForwarderManifest.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public partial class IssuerSCACUserControl : ZUserControl
	{
		public IssuerSCACUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			var header = (USExportAsycudaManifestHeader)CurrentDataItem;
			if (header != null)
			{
				header.MasterBOLInfo.ValueChanged -= ChangeBolTypeWhenMasterBOLChanged;
				if (header.MasterBill is USExportAsycudaBill bill)
				{
					bill.ABL_BillIssuerInfo.ValueChanged -= ChangeBolTypeWhenBillIssuerChanged;
				}
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var header = (USExportAsycudaManifestHeader)CurrentDataItem;
			if (header != null)
			{
				header.MasterBOLInfo.ValueChanged += ChangeBolTypeWhenMasterBOLChanged;
				if (header.MasterBill is USExportAsycudaBill bill)
				{
					bill.ABL_BillIssuerInfo.ValueChanged += ChangeBolTypeWhenBillIssuerChanged;
				}
			}
		}

		void ChangeBolTypeWhenMasterBOLChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem is USExportAsycudaManifestHeader header)
			{
				ChangeBolType(header);
			}
		}

		void ChangeBolTypeWhenBillIssuerChanged(object sender, EventArgs e)
		{
			if (CurrentDataItem is USExportAsycudaManifestHeader header)
			{
				ChangeBolType(header);
			}
		}

		void ChangeBolType(USExportAsycudaManifestHeader header)
		{
			if (header.IsConsolidator && header.MasterBill is USExportAsycudaBill masterBill)
			{
				var billIssuer = masterBill.ABL_BillIssuer;
				var billIssuerOldValue = masterBill.ABL_BillIssuerOldValue;
				var masterBOL = header.MasterBOL;
				var masterBolOldValue = header.MasterBOLOldValue;

				if ((billIssuerOldValue.IsEmpty || masterBolOldValue.IsEmpty) && !billIssuer.IsEmpty && !masterBOL.IsEmpty && header.Bills.OfType<USExportAsycudaBill>().Any(x => x.IsChildMasterBill))
				{
					ShowDialog(header, ShipmentTypes.StandardHouse, EnterIssuerCodeAndMasterBillResourceString);
				}
				else if (!billIssuerOldValue.IsEmpty && !masterBolOldValue.IsEmpty && billIssuer.IsEmpty && masterBOL.IsEmpty && header.Bills.OfType<USExportAsycudaBill>().Any(x => x.ABL_BolType == ShipmentTypes.StandardHouse))
				{
					ShowDialog(header, AsycudaBill.ChildBolCode, RemoveIssuerCodeAndMasterBillResourceString, masterBolOldValue, billIssuerOldValue);
				}
			}
		}

		void ShowDialog(USExportAsycudaManifestHeader header, string bolType, string resourceKey, string masterBolOldValue = "", string billIssuerOldValue = "")
		{
			var result = Globals.Message.Show(resourceKey, Res.GetString("3685AFA8-D0F0-4409-9EF6-F01AD771D7F0", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
			if (result == DialogResult.OK)
			{
				foreach (var bill in header.Bills)
				{
					bill.ABL_BolType = bolType;
				}
			}
			else
			{
				header.MasterBOL = masterBolOldValue;
				header.MasterBill.ABL_BillIssuer = billIssuerOldValue;
			}
		}

		internal string EnterIssuerCodeAndMasterBillResourceString
		{
			get { return Res.GetString("7913D761-0111-4C21-909C-5D0FB70D6891", "If you enter the Issuer Code and Master Bill of Lading Number, you will convert the Direct/Simple Bills already entered on the Bills tab for this Export Manifest to House Bills. Click OK to confirm this change or Cancel to remove the Issuer Code and Master Bill Number."); }
		}

		internal string RemoveIssuerCodeAndMasterBillResourceString
		{
			get { return Res.GetString("4A370175-4914-451C-821A-43ADBE3B294D", "If you remove the Issuer Code and Master Bill of Lading Number, you will convert the House Bills already entered on the Bills tab for this Export Manifest to Direct/Simple Bills. Click OK to confirm this change or Cancel to keep the Issuer Code and Master Bill Number."); }
		}
	}
}
