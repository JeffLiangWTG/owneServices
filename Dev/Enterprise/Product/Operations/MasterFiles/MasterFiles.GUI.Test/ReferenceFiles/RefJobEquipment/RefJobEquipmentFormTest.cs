using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefJobEquipmentForm))]
	class RefJobEquipmentFormTest : ZFormBasherTest
	{
		public void TestImplementation()
		{
			using (var form = new RefJobEquipmentFormTestForTest(Factory.New<JobEquipment>()))
			{
				AssertEquals(expected: true, form.AllowNewForTest);
				AssertEquals(expected: false, form.SupportsEDocsForTest);
				AssertEquals(expected: false, form.ShowNotesTabForTest);
			}
		}

		public void TestControlBinding()
		{
			var jobEquipment = Factory.NewWithValidTestData<JobEquipment>();
			jobEquipment.JEQ_IsActive = true;

			using (var form = new RefJobEquipmentForm(jobEquipment))
			{
				form.Show();

				AssertEquals($"Equipment Combination - {jobEquipment.JEQ_Code} - {jobEquipment.JEQ_Description}", form.FormCaption);
				AssertEquals(jobEquipment.JEQ_Code, form.FindSingle<ZTextBox>("JEQ_CodeTextBox").Text);
				AssertEquals(jobEquipment.JEQ_Description, form.FindSingle<ZTextBox>("JEQ_DescriptionTextBox").Text);
				AssertEquals(jobEquipment.JEQ_IsActive, form.FindSingle<ZCheckBox>("JEQ_IsActiveCheckBox").Checked);
			}
		}

		protected override Form GetFormToBashCore() => new RefJobEquipmentForm(Factory.New<JobEquipment>())
		{
			ControllerID = ControllerIDs.RefJobEquipment
		};
	}

	class RefJobEquipmentFormTestForTest : RefJobEquipmentForm
	{
		public RefJobEquipmentFormTestForTest(JobEquipment jobEquipment)
			: base(jobEquipment)
		{
		}

		public bool AllowNewForTest => base.AllowNew;

		public bool SupportsEDocsForTest => base.SupportsEDocs;

		public bool ShowNotesTabForTest => base.ShowNotesTab;
	}
}
