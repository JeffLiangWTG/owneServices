using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.ForwarderManifest.GUI
{
	public abstract partial class CusEntryNumbersUserControl : ZUserControl
	{
		public CusEntryNumbersUserControl()
		{
			InitializeComponent();
			InitControl();
		}

		protected abstract string CaptionString { get; }

		protected virtual bool IsITN => false;

		void MoreNumbersButton_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is Business.USExportAsycudaBill bill)
			{
				var entryNumCollection = IsITN ? bill.AESITNNumberCollection : bill.InBondNumberCollection;
				var codesForm = new AdditionalCodesForm(entryNumCollection, CaptionString);
				_ = ZFormModaliser.ShowDialogAndDispose(codesForm);
				if (codesForm.DialogResult == System.Windows.Forms.DialogResult.OK)
				{
					var entryNumString = entryNumCollection.GetCodesAsCommaSeparatedString();
					if (IsITN)
					{
						bill.AESITNNumbers = entryNumString;
					}
					else
					{
						bill.InBondNumbers = entryNumString;
					}
				}
				else
				{
					entryNumCollection.RefreshCollection(IsITN ? bill.AESITNNumbers : bill.InBondNumbers);
				}
			}
		}

		protected abstract void InitControl();
	}
}
