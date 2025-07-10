using System;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ComplianceNumberSequenceCustomisationElement))]
	class ComplianceNumberSequenceCustomisationElementTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestElementNameReadOnly()
		{
			var element = ElementCollection[0];

			AssertEquals(true, element.ElementName_ReadOnly);
		}

		public void TestInclude()
		{
			var configuration = new ComplianceNumberSequenceConfiguration(Factory);
			configuration.Code = "AAA";
			configuration.Description = "XX";
			ElementCollection.ParentConfiguration = configuration;

			Assert(ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber].Include);
			Assert(ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber].IncludeInfo.ReadOnly);

			var element = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1];
			element.Include = true;
			element.DigitCode = "XX";
			element.Order = 2;
			AssertEquals(2, (ZInt)element.Order);
			AssertEquals("XX", element.DigitCode);

			element.Include = false;
			AssertEquals(0, (ZInt)element.Order);
			AssertEquals(ZString.Empty, element.DigitCode);

			element = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateYearOfIssue];
			element.Include = true;
			AssertEquals("4", element.DigitCode);
			AssertEquals(4, element.Length);
		}

		public void TestOrderReadOnly()
		{
			var element = ElementCollection[0];

			element.Include = true;
			AssertEquals(false, element.OrderInfo.ReadOnly);

			element.Include = false;
			AssertEquals(true, element.OrderInfo.ReadOnly);
		}

		public void TestMandatoryConfigHaveReadOnlyFields()
		{
			var configuration = new ComplianceNumberSequenceConfiguration(Factory);
			configuration.Code = "OTH";
			ElementCollection = new ComplianceNumberSequenceCustomisationElementCollection(Factory);
			ElementCollection.ParentConfiguration = configuration;
			ElementCollection.PopulateElements();

			ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include = true;
			ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCode = "/";
			ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Length = 1;

			ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Include = true;
			ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Length = 1;

			Assert(!ElementCollection.ParentConfiguration.IsMandatory);
			Assert(!ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].IncludeInfo.ReadOnly);
			Assert(!ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].OrderInfo.ReadOnly);
			Assert(!ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCodeInfo.ReadOnly);
			Assert(!ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].LengthInfo.ReadOnly);

			GlbCompany.CurrentCompany.SetCountry(CountryCodes.Portugal);
			var configCollection = AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var mandatoryElements = configCollection.GetComplianceNumberSequenceConfigurationByCode("MPC").Elements;

			Assert(mandatoryElements.ParentConfiguration.IsMandatory);
			Assert(mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].Include);
			AssertEquals("/", mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCode);
			AssertEquals(1, mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].Length);

			Assert(mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].IncludeInfo.ReadOnly);
			Assert(mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].OrderInfo.ReadOnly);
			Assert(mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1].DigitCodeInfo.ReadOnly);
			Assert(mandatoryElements[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace].LengthInfo.ReadOnly);
		}

		public void TestDigitCodeReadOnly()
		{
			var element1 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1];
			element1.Include = true;
			AssertEquals(false, element1.DigitCodeInfo.ReadOnly);
			element1.Include = false;
			AssertEquals(true, element1.DigitCodeInfo.ReadOnly);

			var element2 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement2];
			element2.Include = true;
			AssertEquals(false, element2.DigitCodeInfo.ReadOnly);
			element2.Include = false;
			AssertEquals(true, element2.DigitCodeInfo.ReadOnly);

			var element3 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.OriginalAmendmentStatus];
			element3.Include = true;
			AssertEquals(false, element3.DigitCodeInfo.ReadOnly);
			element3.Include = false;
			AssertEquals(true, element3.DigitCodeInfo.ReadOnly);

			var element4 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateYearOfIssue];
			element4.Include = true;
			AssertEquals(false, element4.DigitCodeInfo.ReadOnly);
			element4.Include = false;
			AssertEquals(true, element4.DigitCodeInfo.ReadOnly);
		}

		public void TestIsCustomElement()
		{
			var element1 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1];
			AssertEquals(true, element1.IsCustomElement);

			var element2 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.InvoiceDateDayOfIssue];
			AssertEquals(false, element2.IsCustomElement);
		}

		public void TestIsCodePairElement()
		{
			var element1 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.TransactionType];
			AssertEquals(true, element1.IsCodePairElement);

			var element2 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.OriginalAmendmentStatus];
			AssertEquals(true, element2.IsCodePairElement);
		}

		public void TestIsYearElement()
		{
			var element1 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateYearOfIssue];
			AssertEquals(true, element1.IsYearElement);

			var element2 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.InvoiceDateYearOfIssue];
			AssertEquals(true, element2.IsYearElement);

			var element3 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.PostDateYearOfIssue];
			AssertEquals(true, element3.IsYearElement);
		}

		public void TestOriginalAmendmentStatusElement()
		{
			var element = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.OriginalAmendmentStatus];
			element.Include = true;
			element.DigitCode = "11/123";

			AssertEquals("11", element.OriginalStatusCode);
			AssertEquals("123", element.AmendmentStatusCode);
			AssertEquals(3, element.Length);

			element.DigitCode = "1234/123";
			AssertEquals(4, element.Length);

			element.DigitCode = "1234123";
			AssertEquals(0, element.Length);
		}

		public void TestTransactionTypeElement()
		{
			var element = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.TransactionType];
			element.Include = true;
			element.DigitCode = "aa/bb/ccc";

			AssertEquals("aa", element.InvoiceCode);
			AssertEquals("bb", element.CreditNoteCode);
			AssertEquals("ccc", element.AdjustmentNoteCode);
			AssertEquals(3, element.Length);

			element.DigitCode = "aaaa/bb/ccc";
			AssertEquals(4, element.Length);

			element.DigitCode = "aabbcc";
			AssertEquals(0, element.Length);
		}

		public void TestLegth()
		{
			var customElement1 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement1];
			customElement1.Include = true;
			customElement1.DigitCode = "ABC";
			AssertEquals(3, customElement1.Length);

			var customElement2 = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.CustomElement2];
			customElement2.Include = true;
			customElement2.DigitCode = "AB";
			AssertEquals(2, customElement2.Length);

			var complianceDateDayOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateDayOfIssue];
			AssertEquals(2, complianceDateDayOfIssue.Length);

			var complianceDateMonthOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateMonthOfIssue];
			AssertEquals(2, complianceDateMonthOfIssue.Length);

			var complianceDateYearOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.ComplianceDateYearOfIssue];
			complianceDateYearOfIssue.Include = true;
			AssertEquals(4, complianceDateYearOfIssue.Length);
			complianceDateYearOfIssue.DigitCode = "1";
			AssertEquals(1, complianceDateYearOfIssue.Length);

			var invoiceDateDayOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.InvoiceDateDayOfIssue];
			AssertEquals(2, invoiceDateDayOfIssue.Length);

			var invoiceDateMonthOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.InvoiceDateMonthOfIssue];
			AssertEquals(2, invoiceDateMonthOfIssue.Length);

			var invoiceDateYearOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.InvoiceDateYearOfIssue];
			invoiceDateYearOfIssue.Include = true;
			AssertEquals(4, invoiceDateYearOfIssue.Length);
			invoiceDateYearOfIssue.DigitCode = "1";
			AssertEquals(1, invoiceDateYearOfIssue.Length);

			var postDateDayOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.PostDateDayOfIssue];
			AssertEquals(2, postDateDayOfIssue.Length);

			var postDateMonthOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.PostDateMonthOfIssue];
			AssertEquals(2, postDateMonthOfIssue.Length);

			var postDateYearOfIssue = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.PostDateYearOfIssue];
			postDateYearOfIssue.Include = true;
			AssertEquals(4, postDateYearOfIssue.Length);
			postDateYearOfIssue.DigitCode = "2";
			AssertEquals(2, postDateYearOfIssue.Length);

			var originalAmendmentStatus = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.OriginalAmendmentStatus];
			originalAmendmentStatus.Include = true;
			originalAmendmentStatus.DigitCode = "1/123";
			AssertEquals(3, originalAmendmentStatus.Length);

			var transactionType = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.TransactionType];
			transactionType.Include = true;
			transactionType.DigitCode = "1/123/45";
			AssertEquals(3, transactionType.Length);

			var blankSpace = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.BlankSpace];
			blankSpace.Include = true;
			blankSpace.Length = 2;
			AssertEquals(2, blankSpace.Length);

			var sequenceNumber = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.SequenceNumber];
			AssertEquals(0, sequenceNumber.Length);

			var seriesPrefix = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.SeriesPrefix];
			AssertEquals(0, seriesPrefix.Length);

			var taxStatusCode = ElementCollection[ComplianceNumberSequenceCustomisationElement.ElementNames.TaxStatusCode];
			AssertEquals(0, taxStatusCode.Length);
		}

		#region Implementation

		ComplianceNumberSequenceCustomisationElementCollection ElementCollection;

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

		protected new ComplianceNumberSequenceCustomisationElement BizObj
		{
			get { return (ComplianceNumberSequenceCustomisationElement)base.BizObj; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ComplianceNumberSequenceCustomisationElement();
		}

		#endregion
	}
}
