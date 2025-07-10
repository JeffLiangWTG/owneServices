using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class CombineTranslatedAddressForm : ZChildForm
	{
		OrgAddress AddressA;
		OrgAddress AddressB;
		public CombineTranslatedAddressForm(AddressCombiner businessEntity) : base(businessEntity)
		{
			SetupAddresses(businessEntity);
			InitializeComponent();
			this.SaveAsButton1.CaptionResourceString = this.SaveAsButton1.CaptionResourceString.Format(AddressA.DisplayText);
			this.SaveAsButton2.CaptionResourceString = this.SaveAsButton2.CaptionResourceString.Format(AddressB.DisplayText);
			SaveAsButton1.AllowOverlap(CombineTranslatedAddressAUserControl);
			SaveAsButton2.AllowOverlap(CombineTranslatedAddressBUserControl);
		}

		void SetupAddresses(AddressCombiner businessEntity)
		{
			AddressA = businessEntity.Address1;
			AddressB = businessEntity.Address2;
			AddressA.SuspendMarkingAsNeedingValidation();
			AddressB.SuspendMarkingAsNeedingValidation();
			AddressA.ReadOnly = true;
			AddressB.ReadOnly = true;
			FormClosed += delegate
			{
				AddressA.ReadOnly = false;
				AddressB.ReadOnly = false;
			};
		}

		protected override bool AllowNew => false;
		public override string FormVerb => string.Empty;

		string GetDialogMessage(string address1, string address2)
		{
			return Res.GetString("39CE5B97-D2E6-47DE-8243-B9FD652463A7", "By proceeding, {0} will be changed to a new translated address for {1}. All jobs referencing {0} will be changed to {1} Are you sure you want to proceed?", address1, address2);
		}

		void ShowErrorDialogIfRequired(AddressCombiner.CombineResult result)
		{
			if (result == AddressCombiner.CombineResult.FailedSourceAddressHasExistingTranslatedRecords)
			{
				ShowErrorDialog(Res.GetString("860AF8ED-B42C-4666-8A8A-489810AA96B5", "Unable to combine. The Dissolved address has one or more existing translated addresses."));
			}
			if (result == AddressCombiner.CombineResult.FailedTargetAddressHasSameLanguageTranslatedRecords)
			{
				ShowErrorDialog(Res.GetString("21CBC6DD-9A6E-496F-AECF-4C5441EA9642", "Unable to combine. The Retained address already has a translated address in the same language."));
			}
		}

		void ShowErrorDialog(string message)
		{
			Globals.Message.ShowError(message, Res.GetString("823E8A5D-689D-47E9-820E-0EE68C5B87C8", "Error Combining Addresses."));
		}

		string GetDialogCaption(string address1, string address2)
		{
			return Res.GetString("C695AF14-ED8C-4CA2-A8F8-940D26EAA462", "Save {0} as a translated address for {1}", address1, address2);
		}

		void SaveAsButton1_Click(object sender, EventArgs e)
		{
			CombineAddresses(AddressCombiner.CombineOptions.TranslateAddress1);
		}

		void SaveAsButton2_Click(object sender, EventArgs e)
		{
			CombineAddresses(AddressCombiner.CombineOptions.TranslateAddress2);
		}
		void CombineAddresses(AddressCombiner.CombineOptions combineOption)
		{
			using (var saveAddressMessageBox = (combineOption == AddressCombiner.CombineOptions.TranslateAddress1)
				? new ZMessageBox(GetDialogMessage(AddressA.DisplayText, AddressB.DisplayText), GetDialogCaption(AddressA.DisplayText, AddressB.DisplayText), MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
				: new ZMessageBox(GetDialogMessage(AddressB.DisplayText, AddressA.DisplayText), GetDialogCaption(AddressB.DisplayText, AddressA.DisplayText), MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
			{
				var result = ZFormModaliser.ShowDialogWithoutDispose(saveAddressMessageBox, ParentForm);
				if (result == DialogResult.Yes)
				{
					var combineResult = ((AddressCombiner)BusinessEntity).CombineAsTranslatedAddressInBulkTransaction(combineOption);
					ShowErrorDialogIfRequired(combineResult);
				}
			}
			this.Close();
		}
	}
}
