using System.Windows.Forms;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.GUI.Testing
{
	[TestedType(typeof(PalletTransactionForm))]
	sealed class PalletTransactionFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var transaction = Factory.New<PkgPalletTransaction>();
			transaction.KTR_PaperDocketID = "TEST12";
			using (var form = new PalletTransactionForm(transaction))
			{
				AssertEquals("Pallet Transaction TEST12", form.FormCaption);
			}
		}

		public void TestIsResizableByTabPageAllowed()
		{
			using (var form = new PalletTransactionForm(null))
			{
				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var palletTransaction = Factory.New<PkgPalletTransaction>();
			return new PalletTransactionForm(palletTransaction);
		}
	}
}
