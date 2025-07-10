using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	public class RelatedDeclarationForExportValidationTest : BusinessObjectValidationTestCase
	{
		RelatedDeclarationForExport relatedDeclarationForExport;
		public void TestCheckCSI_Procedure()
		{
			relatedDeclarationForExport.CSI_Procedure = ZString.Empty;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_ProcedureInfo, "valid type");
			relatedDeclarationForExport.CSI_Procedure = "ABCDE";
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);
			relatedDeclarationForExport.CSI_Procedure = "OZBY";
			AssertNoMessageErrorContaining(relatedDeclarationForExport.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);
			relatedDeclarationForExport.CSI_Procedure = "NATO";
			AssertNoMessageErrorContaining(relatedDeclarationForExport.CSI_ProcedureInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCSI_ReferenceNumber()
		{
			relatedDeclarationForExport.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_ReferenceNumberInfo, "valid Declaration No");
			relatedDeclarationForExport.CSI_ReferenceNumber = "ABC0123465789";
			AssertNoMessageErrorContaining("After set value, there should not be message error on CSI_ReferenceNumber", relatedDeclarationForExport.CSI_ReferenceNumberInfo, "valid Declaration No");
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestCSI_ReferenceNumberMaxLength()
		{
			try
			{
				relatedDeclarationForExport.CSI_ReferenceNumber = "ABC012346578901234567";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestCheckCSI_SubType()
		{
			relatedDeclarationForExport.CSI_SubType = ZString.Empty;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_SubTypeInfo, "Enter a valid choice");
			relatedDeclarationForExport.CSI_SubType = "YES";
			AssertNoMessageErrorContaining("After set value, there should not be message error on CSI_SubType", relatedDeclarationForExport.CSI_SubTypeInfo, "Enter a valid choice");
		}

		[ExpectException(typeof(MaxLengthExceededException))]
		public void TestCSI_SubTypeMaxLength()
		{
			try
			{
				relatedDeclarationForExport.CSI_SubType = "ABCDE";
			}
			catch (MaxLengthExceededException)
			{
				ErrorReporter.Clear();
				throw;
			}
		}

		public void TestCheckCSI_Quantity()
		{
			relatedDeclarationForExport.CSI_Quantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_QuantityInfo, "Enter a valid Box Quantity.");
			relatedDeclarationForExport.CSI_Quantity = 123456789000;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_QuantityInfo, "Enter a valid Box Quantity.");
			relatedDeclarationForExport.CSI_Quantity = -1200;
			AssertHasErrorContaining(relatedDeclarationForExport.CSI_QuantityInfo, "Please enter a non-negative value.");
			relatedDeclarationForExport.CSI_Quantity = 12000;
			AssertNoMessageErrorContaining("After set value, there should not be message error on CSI_Quantity", relatedDeclarationForExport.CSI_QuantityInfo, "Please enter a non-negative value.");
		}

		public void TestCheckCSI_Quantity2()
		{
			relatedDeclarationForExport.CSI_Quantity2 = ZDecimal.Zero;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_Quantity2Info, "Enter a valid Gross Weight.");
			relatedDeclarationForExport.CSI_Quantity2 = 123456789000.000;
			AssertHasMessageErrorContaining(relatedDeclarationForExport.CSI_Quantity2Info, "Enter a valid Gross Weight.");
			relatedDeclarationForExport.CSI_Quantity2 = -1200.000;
			AssertHasErrorContaining(relatedDeclarationForExport.CSI_Quantity2Info, "Please enter a non-negative value.");
			relatedDeclarationForExport.CSI_Quantity2 = 12000.000;
			AssertNoMessageErrorContaining("After set value, there should not be message error on CSI_Quantity2", relatedDeclarationForExport.CSI_Quantity2Info, "Please enter a non-negative value.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			relatedDeclarationForExport = Factory.NewWithValidTestData<RelatedDeclarationForExport>();
		}
	}
}
