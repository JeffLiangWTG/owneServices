using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class ImportDeclarationDialog : ZChildForm
	{
		public ImportDeclarationDialog(ImportJobDeclaration declarationToImport) : base(declarationToImport)
		{
			this.DeclarationToImport = Argument.NotNull(declarationToImport, nameof(declarationToImport));
			DeclarationGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.JobDeclaration;
			DeclarationGuidFindBox.GetCountryCode = GetCountryCode;
		}

		public override string FormHeading
		{
			get { return Res.GetString("9adc6787-da9c-4665-a954-94b174b78c63", "Import Declaration"); }
		}

		public readonly ImportJobDeclaration DeclarationToImport;

		protected internal void ImportDeclarationButtonButton_Click(object sender, System.EventArgs e)
		{
			DeclarationToImport.Validation.ValidateAll();

			if (DeclarationToImport.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("9a191c16-f951-4466-9750-0b036faad82d", "Please fix the errors before importing a declaration"), Res.GetString("9adc6787-da9c-4665-a954-94b174b78c63", "Import Declaration"));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		string GetCountryCode()
		{
			var result = DeclarationToImport.CountryCode;
			return result.IsEmpty ? GlbCompany.CurrentCompany.GC_RN_NKCountryCode : result;
		}
	}
}
