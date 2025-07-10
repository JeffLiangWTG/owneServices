using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI
{
	public abstract class ImportFromCSVFormBaseTest : ZFormBasherTest
	{
		public ImportFromCSVForm GetNewImportFromCSVForm()
		{
			return GetNewImportFromCSVFormCore();
		}

		protected abstract ImportFromCSVForm GetNewImportFromCSVFormCore();

		protected sealed override Form GetFormToBashCore()
		{
			return GetNewImportFromCSVForm();
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestLoadForm()
		{
			using (var testForm = GetNewImportFromCSVForm())
			{
				testForm.Show();
				Application.DoEvents();
			}
		}
	}
}
