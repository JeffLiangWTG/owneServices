using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(SlaughterDate))]
	sealed class SlaughterDateTest : Customs.Business.Testing.CusCodeDataTest<SlaughterDate>
	{
		[ExpectNoExceptions]
		public void TestCY_Date()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(slaughterDate.CY_DateInfo, "Date", "The date of slaughter for animal products.");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(slaughterDate.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.SlaughterDates).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(slaughterDate.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			NUnit.Framework.Assert.That(slaughterDate.Validation, NUnit.Framework.Is.TypeOf<SlaughterDateValidation>());
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return slaughterDate;
		}

		protected override IEnumerable<SlaughterDate> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().SlaughterDateCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceLine.SlaughterDateCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			slaughterDate = invoiceLine.SlaughterDateCollection.AddNew();
		}

		SlaughterDate slaughterDate;
		JobComInvoiceLine invoiceLine;
	}
}
