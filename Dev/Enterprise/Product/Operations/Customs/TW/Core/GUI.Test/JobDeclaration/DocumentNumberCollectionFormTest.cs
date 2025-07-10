using System.Windows.Forms;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(DocumentNumberCollectionForm))]
	public sealed class DocumentNumberCollectionFormTest : ZFormBasherTest
	{
		public void TestDocumenttNumbersGridMaximumRows()
		{
			using (var form = new DocumentNumberCollectionForm(cusEntryInstruction.DocumentNumbers))
			{
				AssertEquals(3, form.DocumentNumbersGrid.MaximumRows);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new DocumentNumberCollectionForm(cusEntryInstruction.DocumentNumbers);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryInstruction.AttachedDocumentNumbersAsString = "123";
			Factory.Save();
		}

		CusEntryInstruction cusEntryInstruction;
	}
}
