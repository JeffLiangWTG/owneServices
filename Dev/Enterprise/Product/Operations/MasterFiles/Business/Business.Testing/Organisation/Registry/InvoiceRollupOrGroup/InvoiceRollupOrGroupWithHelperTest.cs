using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceRollupOrGroupWithHelperTest : RollupOrGroupWithHelperBaseTest
	{
		#region InvoicePostingCurrency

		public void TestInvoicePostingCurrency()
		{
			AssertEquals("Default value", ZString.Empty, InvoiceRollupOrGroup.InvoicePostingCurrency);

			InvoiceRollupOrGroup.InvoicePostingCurrency = "USD";
			AssertEquals("Set value", "USD", InvoiceRollupOrGroup.InvoicePostingCurrency);
		}

		#endregion

		#region Implementation

		protected override IInvoiceRollupOrGroup InvoiceRollupOrGroup
		{
			get { return InvoiceRollOrGroupForTest; }
		}

		protected override IInvoiceRollupOrGroup SecondInvoiceRollupOrGroup
		{
			get { return SecondInvoiceRollupOrGroupForTest; }
		}

		protected override void RunPreSaveValidation()
		{
			InvoiceRollOrGroupForTest.RunPreSaveValidation();
			SecondInvoiceRollupOrGroupForTest.RunPreSaveValidation();
		}

		InvoiceRollupOrGroup InvoiceRollOrGroupForTest;
		InvoiceRollupOrGroup SecondInvoiceRollupOrGroupForTest;

		protected override void SetUp()
		{
			base.SetUp();
			InvoiceRollupOrGroupCollection collection = new InvoiceRollupOrGroupCollection();
			InvoiceRollOrGroupForTest = collection.AddNew();
			SecondInvoiceRollupOrGroupForTest = collection.AddNew();
			AssertEquals("Precondition: collection must have only 2 elements.", 2, collection.Count);
		}

		#endregion
	}
}
