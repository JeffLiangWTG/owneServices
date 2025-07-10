using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using CheckAlgorithm = Enterprise.NumberFountain.CheckDigitAlgorithm;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(InvoiceRemittanceCustomisationElement))]
	sealed class InvoiceRemittanceCustomisationElementTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestOrderReadOnly()
		{
			var element = ElementCollection[0];

			element.Include = true;
			AssertEquals(false, element.Order_ReadOnly);

			element.Include = false;
			AssertEquals(true, element.Order_ReadOnly);
		}

		public void TestElementNameReadOnly()
		{
			var element = ElementCollection[0];

			AssertEquals(true, element.ElementName_ReadOnly);
		}

		public void TestInclude()
		{
			var configuration = new InvoiceRemittanceConfiguration(Factory);
			configuration.Code = "AAA";
			configuration.BillerCode = "XX";
			configuration.BillerAccountNumber = "123456";
			ElementCollection.ParentConfiguration = configuration;

			ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerCode].Include = true;
			AssertEquals("2", ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerCode].DigitCode);

			ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerAccountNumber].Include = true;
			AssertEquals("6", ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.BillerAccountNumber].DigitCode);

			ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInInvoiceCurrency].Include = true;
			AssertEquals("", ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInInvoiceCurrency].DigitCode);

			ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInLocalCurrency].Include = true;
			AssertEquals("", ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInLocalCurrency].DigitCode);

			var element = ElementCollection[0];
			element.Include = false;
			AssertEquals(0, (ZInt)element.Order);
			AssertEquals(ZString.Empty, element.DigitCode);
			AssertEquals(ZString.Empty, element.CheckDigit);
			AssertEquals(ZString.Empty, element.CheckDigitAlgorithm);
		}

		public void TestDigitCodeReadOnly()
		{
			var element1 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			element1.Include = true;
			AssertEquals(false, element1.DigitCode_ReadOnly);
			element1.Include = false;
			AssertEquals(true, element1.DigitCode_ReadOnly);

			var element2 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.DebtorClientNumber];
			element2.Include = true;
			AssertEquals(true, element2.DigitCode_ReadOnly);
			element2.Include = false;
			AssertEquals(true, element2.DigitCode_ReadOnly);

			var element3 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTotalInInvoiceCurrency];
			element3.Include = true;
			AssertEquals(false, element3.DigitCode_ReadOnly);
			element3.Include = false;
			AssertEquals(true, element3.DigitCode_ReadOnly);
		}

		public void TestIsCheckDigitElement()
		{
			var element1 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];
			AssertEquals(true, element1.IsCheckDigitElement);

			var element2 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference];
			AssertEquals(false, element2.IsCheckDigitElement);
		}

		public void TestIsCustomElement()
		{
			var element1 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			AssertEquals(true, element1.IsCustomElement);

			var element2 = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.InvoiceTransactionReference];
			AssertEquals(false, element2.IsCustomElement);
		}

		public void TestCheckDigitReadOnly()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			element.Include = true;
			AssertEquals(false, element.CheckDigit_ReadOnly);
			element.Include = false;
			AssertEquals(true, element.CheckDigit_ReadOnly);
		}

		public void TestCheckDigitList()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CustomCode1];
			AssertEquals(2, element.CheckDigitList.Count);

			var allCodes = element.CheckDigitList.GetAllCodes();
			AssertCollectionContains(InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1, allCodes);
			AssertCollectionContains(InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit2, allCodes);
		}

		public void TestCheckDigitAlgorithm()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];
			element.Include = true;

			element.CheckDigitAlgorithm = CheckAlgorithm.MOD97;
			AssertEquals("2", element.DigitCode);

			element.CheckDigitAlgorithm = CheckAlgorithm.MOD10;
			AssertEquals("1", element.DigitCode);

			element.CheckDigitAlgorithm = ZString.Empty;
			AssertEquals(ZString.Empty, element.DigitCode);
		}

		public void TestCheckDigitAlgorithmReadOnly()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];

			element.Include = true;
			AssertEquals(false, element.CheckDigitAlgorithm_ReadOnly);

			element.Include = false;
			AssertEquals(true, element.CheckDigitAlgorithm_ReadOnly);
		}

		public void TestCheckDigitAlgorithmList()
		{
			var element = ElementCollection[InvoiceRemittanceCustomisationElement.ElementNames.CheckDigit1];
			AssertEquals(5, element.CheckDigitAlgorithmList.Count);

			var allCodes = element.CheckDigitAlgorithmList.GetAllCodes();
			AssertCollectionContains(CheckAlgorithm.RecursiveMOD10, allCodes);
			AssertCollectionContains(CheckAlgorithm.MOD10V05, allCodes);
			AssertCollectionContains(CheckAlgorithm.Algorithm731, allCodes);
			AssertCollectionContains(CheckAlgorithm.MOD97, allCodes);
			AssertCollectionContains(CheckAlgorithm.MOD10, allCodes);
		}

		#region Implementation

		InvoiceRemittanceCustomisationElementCollection ElementCollection;

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

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new InvoiceRemittanceCustomisationElement();
		}

		#endregion
	}
}
