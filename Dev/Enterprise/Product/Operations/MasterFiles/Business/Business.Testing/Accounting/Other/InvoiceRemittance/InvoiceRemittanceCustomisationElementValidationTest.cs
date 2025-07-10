using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using CheckAlgorithm = Enterprise.NumberFountain.CheckDigitAlgorithm;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class InvoiceRemittanceCustomisationElementValidationTest : TestCaseWithFactory
	{
		public void TestValidateOrder()
		{
			var element1 = ElementCollection[0];
			element1.Include = true;
			element1.Order = 0;
			AssertHasError(element1.OrderInfo, "Order must be greater than 0.");

			var element2 = ElementCollection[1];
			element2.Include = true;
			element2.Order = 2;

			element1.Order = 2;
			AssertHasError(element1.OrderInfo, "The Order already exists.");
		}

		public void TestValidateInclude()
		{
			ElementCollection.ParentConfiguration = new InvoiceRemittanceConfiguration(Factory);

			var billerCode = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerCode];
			billerCode.Include = true;
			AssertHasError(billerCode.IncludeInfo, "This data element cannot be selected as a 'Biller Code' has not been entered.");

			var billerAccountNumber = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerAccountNumber];
			billerAccountNumber.Include = true;
			AssertHasError(billerAccountNumber.IncludeInfo, "This data element cannot be selected as a 'Biller Account Number' has not been entered.");
		}

		public void TestValidateDigitCode()
		{
			ElementCollection.ParentConfiguration = new InvoiceRemittanceConfiguration(new FallbackLevel(Environment.Env.CurrentCompanyPK, Guid.Empty, Guid.Empty), Factory);
			foreach (InvoiceRemittanceCustomisationElement element in ElementCollection)
			{
				element.Include = true;
				element.DigitCode = ZString.Empty;

				if (element.IsCustomElement || element.IsInvoiceAmountElement)
				{
					AssertHasError(element.DigitCodeInfo, "Please enter a value.");
				}
				else
				{
					AssertNoErrors(element.DigitCodeInfo);
				}
			}

			var element1 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInInvoiceCurrency];
			element1.Include = true;
			element1.DigitCode = "abc";
			AssertHasError(element1.DigitCodeInfo, "Digit/Code should be a number greater than 0.");

			var element2 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInLocalCurrency];
			element2.Include = true;
			element2.DigitCode = "abc";
			AssertHasError(element2.DigitCodeInfo, "Digit/Code should be a number greater than 0.");

			var element3 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];
			element3.Include = true;
			element3.CheckDigitAlgorithm = CheckAlgorithm.MOD10;

			var element4 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			element4.Include = true;
			element4.CheckDigit = InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1;
			element4.DigitCode = "abc";
			AssertHasError(element4.DigitCodeInfo, "Digit/Code should be numeric value with this check digit.");

			element3.CheckDigitAlgorithm = CheckAlgorithm.MOD97;
			element4.DigitCode = "123<";
			AssertHasError(element4.DigitCodeInfo, "Digit/Code with symbol cannot have check digit.");
		}

		public void TestValidateCheckDigit()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			element.Include = true;
			element.CheckDigit = "XXXX";
			AssertHasError(element.CheckDigitInfo, "Enter a valid selection.");

			var element1 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];
			element1.Include = false;
			var element2 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit2];
			element2.Include = true;

			element.CheckDigit = InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1;
			AssertHasError(element.CheckDigitInfo, "This check digit is not included.");

			element.CheckDigit = InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit2;
			AssertNoErrors(element.CheckDigitInfo);

			element2.CheckDigit = InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit2;
			AssertHasError(element2.CheckDigitInfo, "This check digit cannot select itself.");
		}

		public void TestValidateCheckDigitAlgorithm()
		{
			foreach (InvoiceRemittanceCustomisationElement element in ElementCollection)
			{
				if (element.IsCheckDigitElement)
				{
					element.Include = true;
					element.CheckDigitAlgorithm = "XXXX";
					AssertHasError(element.CheckDigitAlgorithmInfo, "Enter a valid selection.");

					element.CheckDigitAlgorithm = "";
					AssertHasError(element.CheckDigitAlgorithmInfo, "Please enter a value.");

					element.CheckDigitAlgorithm = Enterprise.NumberFountain.CheckDigitAlgorithm.MOD97;
					AssertNoErrors(element.CheckDigitAlgorithmInfo);
				}
				else
				{
					Assert("no validation for non-check digit element", true);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (ElementCollection == null)
			{
				ElementCollection = new InvoiceRemittanceCustomisationElementCollection(Factory);
				ElementCollection.ParentConfiguration = new InvoiceRemittanceConfiguration(Factory);
				ElementCollection.PopulateElements();
			}
		}

		InvoiceRemittanceCustomisationElementCollection ElementCollection;
	}
}
