using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	class UpdateInventoryHeldCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAutoValidationType()
		{
			AssertEquals(typeof(UpdateInventoryHeldCodeValidation), Applicator.Validation.AutoValidationType);
		}

		public void TestValidateInventoryHeldCode()
		{
			AssertValidateInventoryHeldCode("BBB", v => v.ValidateSelectedInventoryHeldCode(), "Enter a valid selection.");
			AssertValidateInventoryHeldCode(InventoryHoldCodes.Codes.Damaged, v => v.ValidateSelectedInventoryHeldCode());
			AssertValidateInventoryHeldCode(string.Empty, v => v.ValidateSelectedInventoryHeldCode());

			Applicator.SelectedInventoryHeldCode = "";
			Applicator.Validation.ValidateSelectedInventoryHeldCode();
		}

		public void TestValidateAll()
		{
			AssertValidateInventoryHeldCode("BBB", v => v.ValidateAll(), "Enter a valid selection.");
			AssertValidateInventoryHeldCode(InventoryHoldCodes.Codes.Damaged, v => v.ValidateAll());

			Applicator.SelectedInventoryHeldCode = "";
			Applicator.Validation.ValidateAll();
		}

		#region Implementation

		void AssertValidateInventoryHeldCode(string heldCode, Action<UpdateInventoryHeldCodeValidation> runValidate,
			string expectedErrorMessage = "")
		{
			Applicator.SelectedInventoryHeldCode = heldCode;
			runValidate(Applicator.Validation);
			if (!string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertHasError(Applicator.SelectedInventoryHeldCodeInfo, expectedErrorMessage);
			}
			else
			{
				AssertNoErrors(Applicator.SelectedInventoryHeldCodeInfo);
			}
		}

		UpdateInventoryHeldCodeActionMethodApplicator Applicator => applicator ?? (applicator = new UpdateInventoryHeldCodeActionMethodApplicator(Factory));
		UpdateInventoryHeldCodeActionMethodApplicator applicator;

		#endregion
	}
}
