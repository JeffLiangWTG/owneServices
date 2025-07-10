using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(ImportDeclarationDialog))]
	sealed class ImportDeclarationDialogTest : ZFormBasherTest
	{
		public void TestDeclarationGuidFindBoxWithoutCountryOverride()
		{
			ImportJobDeclaration decToImport = new ImportJobDeclaration(Factory);
			using (ImportDeclarationDialog dialog = new ImportDeclarationDialog(decToImport))
			{
				AssertEquals("Country Code", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, dialog.DeclarationGuidFindBox.GetCountryCode());
			}
		}

		public void TestDeclarationGuidFindBoxWithCountryOverride()
		{
			ImportJobDeclaration decToImport = new ImportJobDeclaration(Factory);
			decToImport.CountryCode = "AU";
			using (ImportDeclarationDialog dialog = new ImportDeclarationDialog(decToImport))
			{
				AssertEquals("Country Code", "AU", dialog.DeclarationGuidFindBox.GetCountryCode());
			}
		}

		public void TestImportDeclarationButtonWithErrors()
		{
			ImportJobDeclaration decToImport = new ImportJobDeclaration(Factory);
			using (ImportDeclarationDialog dialog = new ImportDeclarationDialog(decToImport))
			{
				dialog.ImportDeclarationButtonButton_Click(null, null);
				ZString userNotification = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Notifications", true, userNotification.Contains("Please fix the errors before importing a declaration"));
			}
		}

		public void TestImportDeclarationButtonWithoutErrors()
		{
			GlbCompany aUCompany = Factory.New<GlbCompany>();
			aUCompany.GC_Code = "AU";
			GlbBranch aUBranch = aUCompany.Branches.AddNew();
			aUBranch.GB_RL_NKHomePort = "AUSYD";
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_GB = aUBranch.PK;
			Factory.Save();
			ImportJobDeclaration decToImport = new ImportJobDeclaration(Factory);
			decToImport.CountryCode = "AU";
			decToImport.DeclarationPK = declaration.PK;
			using (ImportDeclarationDialog dialog = new ImportDeclarationDialog(decToImport))
			{
				dialog.ImportDeclarationButtonButton_Click(null, null);
				AssertEquals("DialogResult", DialogResult.OK, dialog.DialogResult);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ImportDeclarationDialog(new ImportJobDeclaration(Factory));
		}
	}
}
