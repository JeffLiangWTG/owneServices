using System.Windows.Forms;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	[TestedType(typeof(NZTariffBulkChangeStartForm))]
	class NZTariffBulkChangeStartFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new NZTariffBulkChangeStartForm(new NZTariffBulkChange(Factory));
		}

		public void TestTariffUpdateUserFileZButtonClick()
		{
			using (var tempFile = TempFile.New())
			{
				var bizo = new NZTariffBulkChange(Factory);
				using (var form = new NZTariffBulkChangeStartForm(bizo))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					form.FindSingle<ZButton>("TariffUpdateUserFileZButton").PerformClick();
					Assert(bizo.IsSaveAllowed);
					Assert(!bizo.IsImbeddedConcordance);
				}
			}
		}

		public void TestTCOUpdateUserFileZButtonClick()
		{
			using (var tempFile = TempFile.New())
			{
				var bizo = new NZTariffBulkChange(Factory);
				var bizoHasChanges = false;
				using (var form = new NZTariffBulkChangeStartForm(bizo))
				{
					form.Show();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.FileNameToSelectInShowCommonDialog = tempFile.Filename;
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
					{
						if (form is NZImportTCOBulkChangeForm)
						{
							bizoHasChanges = bizo.HasChanges;
						}
					});
					form.FindSingle<ZButton>("TCOUpdateUserFileZButton").PerformClick();
					Assert(bizo.IsSaveAllowed);
					Assert(!bizo.IsImbeddedConcordance);
					Assert("Should flag bizo HasChanges before display NZImportTCOBulkChangeForm", bizoHasChanges);
				}
			}
		}
	}
}
