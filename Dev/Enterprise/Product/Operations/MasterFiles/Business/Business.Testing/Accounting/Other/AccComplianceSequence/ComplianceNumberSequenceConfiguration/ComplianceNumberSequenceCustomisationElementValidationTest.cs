using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.ComplianceNumberSequenceCustomisationElement;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceNumberSequenceCustomisationElementValidationTest : TestCaseWithFactory
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
			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			var element = ElementCollection[ElementNames.InvoiceDateDayOfIssue];
			element.ParentConfiguration.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			element.Include = true;

			AssertHasError(element.IncludeInfo, "'Invoice Date' and 'Post Date' elements cannot be included when the Compliance Document Module is enabled.");

			element = ElementCollection[ElementNames.TaxStatusCode];
			element.ParentConfiguration.CurrentFallbackLevel = new ZArchitecture.Environment.FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			element.Include = true;
			AssertHasError(element.IncludeInfo, "This element is not applicable for your company.");
		}

		public void TestValidateDigitCode()
		{
			var element = ElementCollection[ElementNames.OriginalAmendmentStatus];
			element.Include = true;
			element.DigitCode = "123";

			AssertHasError(element.DigitCodeInfo, "Invalid code. Please enter exactly non-empty codes for Original/Amendment Status separated by '/'");

			element.DigitCode = "_12/34";
			AssertHasError(element.DigitCodeInfo, "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only.");

			element.DigitCode = "12/_34";
			AssertHasError(element.DigitCodeInfo, "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only.");

			element.DigitCode = "a/a";
			AssertHasError(element.DigitCodeInfo, "The codes on either side of the '/' must be different values.");

			element = ElementCollection[ElementNames.TransactionType];
			element.Include = true;
			element.DigitCode = "123";

			AssertHasError(element.DigitCodeInfo, "Invalid code. Please enter exactly non-empty codes for Invoice/Credit Note/Adjustment Note separated by '/'");

			element.DigitCode = "_12/34/56";
			AssertHasError(element.DigitCodeInfo, "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only.");

			element.DigitCode = "12/_34/56";
			AssertHasError(element.DigitCodeInfo, "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only.");

			element.DigitCode = "12/34/_56";
			AssertHasError(element.DigitCodeInfo, "The one of the codes split by '/' is invalid. Please enter alphanumeric characters only.");

			element = ElementCollection[ElementNames.ComplianceDateYearOfIssue];
			element.Include = true;
			element.DigitCode = "123";
			AssertHasError(element.DigitCodeInfo, "The length of year should be either 1, 2 or 4.");
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (ElementCollection == null)
			{
				ElementCollection = new ComplianceNumberSequenceCustomisationElementCollection(Factory);
				ElementCollection.ParentConfiguration = new ComplianceNumberSequenceConfiguration(Factory);
				ElementCollection.PopulateElements();
			}
		}

		ComplianceNumberSequenceCustomisationElementCollection ElementCollection;
	}
}
