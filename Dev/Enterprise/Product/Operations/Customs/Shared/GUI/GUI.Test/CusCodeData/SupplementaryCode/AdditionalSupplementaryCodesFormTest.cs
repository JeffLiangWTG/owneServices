using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(AdditionalSupplementaryCodesForm))]
	sealed class AdditionalSupplementaryCodesFormTest : ZFormBasherTest
	{
		public void TestSupplementaryCodeColumn()
		{
			var declaration = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			using var form = new TestAdditionalSupplementaryCodesForm(declaration);
			form.Show();
			var gridControl = form.AssertContainsControl<ZGrid>("AdditionalSupplementaryCodesGrid");
			var supplementaryCodeColumn = gridControl.GetColumnStyle(BaseSupplementaryCode.Schema.CY_Code);
			AssertType<ZMultiControlColumnStyleInfo>(supplementaryCodeColumn);
		}

		public void TestValidate()
		{
			var declaration = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			var collection = declaration.AdditionalSupplementaryCodes;
			var code = collection.AddNew();

			using var form = new TestAdditionalSupplementaryCodesForm(declaration);
			var cancelEventArgs = new CancelEventArgs(false);
			code.CY_Code = "zzzz";
			form.OnClosing(cancelEventArgs);
			AssertEquals("No validation errors, should not be cancelled", false, cancelEventArgs.Cancel);

			cancelEventArgs = new CancelEventArgs(false);
			var supplementaryCodeProvider = BaseSupplementaryCodeProvider.GetBySupplementaryCodeSupporter(declaration);
			for (var i = 0; i < supplementaryCodeProvider.NumberOfCodes; i++)
			{
				collection.AddNew();
			}
			form.OnClosing(cancelEventArgs);
			AssertEquals("Validation errors exists, should be cancelled", true, cancelEventArgs.Cancel);
		}

		public void TestCancel()
		{
			var declaration = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			using var form = new TestAdditionalSupplementaryCodesForm(declaration);
			var supplementaryCodesCollection = form.SupplementaryCodes;
			var initialItem = supplementaryCodesCollection.AddNew();
			initialItem.CY_Code = "zzz";
			form.Show();
			var newItem = supplementaryCodesCollection.AddNew();
			newItem.CY_Code = "yyy";
			AssertEquals("Should have 2 while editing", 2, supplementaryCodesCollection.Count);

			form.DialogResult = DialogResult.Cancel;
			form.Close();
			AssertEquals("Should have 1 again after cancel", 1, supplementaryCodesCollection.Count);
		}

		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.New<JobDeclarationWithSupplementaryCodeSupport>();
			return new AdditionalSupplementaryCodesForm(declaration);
		}

		class TestAdditionalSupplementaryCodesForm : AdditionalSupplementaryCodesForm
		{
			public TestAdditionalSupplementaryCodesForm(ISupplementaryCodeSupporter supporter)
				: base(supporter)
			{
			}

			public new void OnClosing(CancelEventArgs e)
			{
				base.OnClosing(e);
			}
		}
	}
}


