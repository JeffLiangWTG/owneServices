using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class ImportProductsFromCSVForm : DataLoaderForm
	{
		public ImportProductsFromCSVForm()
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
			SetAuditStatusGroupBox.Enabled = Env.Security.CustomsSupplierPartAuditImport.IsAllowed || Env.Security.CustomsSupplierPartAuditExport.IsAllowed;
		}

		public override bool ConfirmLoadData()
		{
			DialogResult result = Globals.Message.Show(GetLoadingDataMessage(), Res.GetString("c6b55c70-dbdb-407a-81a8-de9bb183db97", "Confirm Product Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

			if ((result == DialogResult.OK) && UpdateYesRadioButton.Checked)
			{
				string updatingData = Res.GetString("7d846cfd-102a-4740-acb9-4f4e703323d7", "Please Be Aware: Selecting 'Update Products' will cause details that already exist in {0} to be overridden.", BrandingFactory.Instance.ProductName);
				result = Globals.Message.Show(updatingData, Res.GetString("c6b55c70-dbdb-407a-81a8-de9bb183db97", "Confirm Product Load"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
			}

			return (result == DialogResult.OK);
		}

		string GetLoadingDataMessage()
		{
			string currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnion(currentCountry))
			{
				return Res.GetString("2ed6714c-b829-4340-ba20-9c21a9c590e6", "Please Note: Only Products with a valid Organization link will be loaded.");
			}
			else
			{
				switch (currentCountry)
				{
					case Core.Constants.CountryCodes.UnitedStates:
					case Core.Constants.CountryCodes.Canada:
						return Res.GetString("7a7a17e2-32cd-41e7-bb04-e889bac46796", "Please Note: Only Products with a valid Organization and either a valid Classification Lookup link or no Lookup will be loaded.");
					default:
						return Res.GetString("1ed6714c-b829-4340-ba20-9c21a9c590e6", "Please Note: Only Products with valid Organization and Classification Lookup links will be loaded.");
				}
			}
		}

		protected override DataLoad GetNewDataLoader()
		{
			return OrgSupplierPartDataLoad.New();
		}

		protected override void LoadSpecificDataType(DataLoad dataLoader, string dataToLoad)
		{
			try
			{
				var partDataLoader = dataLoader as OrgSupplierPartDataLoad;
				partDataLoader.AuditMessage = AuditMessageForm?.ReferenceText ?? string.Empty;
				partDataLoader.ImportProductData(dataToLoad, UpdateYesRadioButton.Checked, LegacyCodesYesRadioButton.Checked, SetAuditStatusYesRadioButton.Checked);
			}
			catch (System.NotImplementedException)
			{
				Globals.Message.Show(Res.GetString("e3830dbf-e597-447f-80c0-4d8a983082d2", "Product data import from CSV file has not been implemented for your country/region."));
			}
		}

		AuditMessageForm AuditMessageForm;

		void SetAuditStatusYesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			if (SetAuditStatusYesRadioButton.Checked)
			{
				if (AuditMessageForm == null)
				{
					AuditMessageForm = new AuditMessageForm();
				}

				if (ZFormModaliser.ShowDialogWithoutDispose(AuditMessageForm) == DialogResult.Cancel)
				{
					SetAuditStatusNoRadioButton.Checked = true;
					SetAuditStatusYesRadioButton.Checked = false;
				}
			}
		}

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			base.Dispose(isNotFinalizing);

			if (AuditMessageForm != null)
			{
				AuditMessageForm.Dispose();
				AuditMessageForm = null;
			}
		}

		#endregion
	}
}
