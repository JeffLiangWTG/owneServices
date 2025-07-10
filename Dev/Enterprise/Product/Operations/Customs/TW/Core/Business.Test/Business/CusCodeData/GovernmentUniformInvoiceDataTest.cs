using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(GovernmentUniformInvoiceData))]
	sealed class GovernmentUniformInvoiceDataTest : Customs.Business.Testing.CusCodeDataTest<GovernmentUniformInvoiceData>
	{
		[ExpectNoExceptions]
		public void TestPropertyCaptions()
		{
			var governmentUniformInvoice = Factory.New<GovernmentUniformInvoiceData>();
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(governmentUniformInvoice.CY_CodeInfo, "Government Uniform Invoice Number", "Number", "The Uniform Invoice numbers of the bonded goods included in monthly reporting.");
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(governmentUniformInvoice.AmountInfo, "Amount", "The Uniform Invoices amount of the bonded goods included in monthly reporting.");
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var governmentUniformInvoice = Factory.New<GovernmentUniformInvoiceData>();
			NUnit.Framework.Assert.That(governmentUniformInvoice.Validation.GetType(), NUnit.Framework.Is.EqualTo(typeof(GovernmentUniformInvoiceDataValidation)), "Validation");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			var governmentUniformInvoice = Factory.New<GovernmentUniformInvoiceData>();
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_Type, NUnit.Framework.Is.EqualTo("GUI").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.GOVUniformInvoice).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobDeclarationSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAmount()
		{
			var governmentUniformInvoice = Factory.New<GovernmentUniformInvoiceData>();
			governmentUniformInvoice.CY_Code = "AB123";
			governmentUniformInvoice.Amount = 12345678;
			NUnit.Framework.Assert.That(governmentUniformInvoice.Amount, NUnit.Framework.Is.EqualTo(12345678m).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_Data, NUnit.Framework.Is.EqualTo("12345678").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestParent()
		{
			var governmentUniformInvoice = Factory.New<GovernmentUniformInvoiceData>();
			governmentUniformInvoice.CY_ParentID = Declaration.PK;
			governmentUniformInvoice.CY_ParentTableCode = Declaration.TablePrefix;
			NUnit.Framework.Assert.That(governmentUniformInvoice.Parent, NUnit.Framework.Is.EqualTo(Declaration));
		}

		[ExpectNoExceptions]
		public void TestCY_CodeMaxLength()
		{
			var governmentUniformInvoice = Factory.New<JobDeclaration>().GovernmentUniformInvoices.AddNew();
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_CodeInfo.MaxLength, NUnit.Framework.Is.EqualTo(12));
		}

		[ExpectNoExceptions]
		public void TestCY_DataMaxLength()
		{
			var governmentUniformInvoice = Factory.New<JobDeclaration>().GovernmentUniformInvoices.AddNew();
			NUnit.Framework.Assert.That(governmentUniformInvoice.CY_DataInfo.MaxLength, NUnit.Framework.Is.EqualTo(12));
		}

		protected override IEnumerable<GovernmentUniformInvoiceData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().GovernmentUniformInvoices.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Declaration.GovernmentUniformInvoices.AddNew();
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		JobDeclaration declaration;
	}
}
