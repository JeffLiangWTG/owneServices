using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefAccessorialForm))]
	class RefAccessorialFormTest : ZFormBasherTest
	{
		public void TestImplementation()
		{
			using (var form = new RefAccessorialFormForTest(Factory.New<RefAccessorial>()))
			{
				AssertEquals(expected: false, form.AllowNewForTest);
				AssertEquals(expected: false, form.SupportsEDocsForTest);
				AssertEquals(expected: false, form.ShowNotesTabForTest);
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ReferenceFiles, form.SectionCode);
			}
		}

		public void TestControlBinding()
		{
			var refAccessorial = Factory.NewWithValidTestData<RefAccessorial>();
			using (var form = new RefAccessorialForm(refAccessorial))
			{
				form.Show();

				AssertEquals($"Accessorial - {refAccessorial.ASI_Code} - {refAccessorial.ASI_Description}", form.FormCaption);
				AssertEquals(refAccessorial.ASI_Code, form.FindSingle<ZTextBox>("ASI_CodeTextBox").Text);
				AssertEquals(refAccessorial.ASI_Description, form.FindSingle<ZTextBox>("ASI_DescriptionTextBox").Text);
			}
		}

		protected override Form GetFormToBashCore() => new RefAccessorialForm(Factory.New<RefAccessorial>())
		{
			ControllerID = ControllerIDs.RefAccessorial
		};
	}

	class RefAccessorialFormForTest : RefAccessorialForm
	{
		public RefAccessorialFormForTest(RefAccessorial refAccessorial)
			: base(refAccessorial)
		{
		}

		public bool AllowNewForTest => base.AllowNew;

		public bool SupportsEDocsForTest => base.SupportsEDocs;

		public bool ShowNotesTabForTest => base.ShowNotesTab;
	}
}
