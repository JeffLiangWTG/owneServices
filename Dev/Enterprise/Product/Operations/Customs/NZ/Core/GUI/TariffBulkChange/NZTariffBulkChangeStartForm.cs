using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class NZTariffBulkChangeStartForm : ZChildForm
	{
		public NZTariffBulkChangeStartForm(NZTariffBulkChange businessEntity)
			: base(businessEntity)
		{
		}

		public new NZTariffBulkChange BusinessEntity
		{
			get { return base.BusinessEntity as NZTariffBulkChange; }
		}

		public override string FormCaption
		{
			get { return Enterprise.Customs.NZ.GUI.Res.GetString("CA82C01B-B816-44CF-9255-8088FBF0711C", "Tariff Bulk Change"); }
		}

		void TariffBulkChangeFile(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = "Select Tariff Bulk Change Concordance File";
				dialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt|All files (*.*)|*.*";
				//Dialog.FileName;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;
				dialog.DefaultExt = "csv";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = dialog.OpenFile())
						{
							BusinessEntity.LoadConcordance(stream, AutomaticConvertZCheckBox.Checked, isSaveAllowed: true);
							ZFormModaliser.ShowDialogAndDispose(new NZTariffBulkChangeForm(BusinessEntity));
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
			ClearHasChangesAndClose();
		}

		void ClearHasChangesAndClose()
		{
			if (BusinessEntityForHasChanges != null)
			{
				BusinessEntityForHasChanges.ClearHasChangesIncludingChildren();
			}

			Dispose();
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ApplyTariffChanges(object sender, EventArgs e)
		{
			if (Globals.Message.Show(BusinessEntity.ApplyTariffChangesMessage, "Final Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.ApplyPendingTariffChanges();
				Close();
			}
		}

		void TCOUpdateUserFileZButton_Click(object sender, EventArgs e)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = "Select Concession Bulk Change Concordance File";
				dialog.Filter = "CSV files (*.csv)|*.csv|TXT files (*.txt)|*.txt|All files (*.*)|*.*";
				//Dialog.FileName;
				dialog.CheckFileExists = true;
				dialog.CheckPathExists = true;
				dialog.DefaultExt = "csv";
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					try
					{
						using (var stream = dialog.OpenFile())
						{
							BusinessEntity.ChangeTCO(stream, isSaveAllowed: true);
							BusinessEntity.HasChanges = true;
							ZFormModaliser.ShowDialogAndDispose(new NZImportTCOBulkChangeForm(BusinessEntity));
						}
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						Globals.Message.ShowError(ex.Message);
					}
				}
			}
			ClearHasChangesAndClose();
		}
	}
}
