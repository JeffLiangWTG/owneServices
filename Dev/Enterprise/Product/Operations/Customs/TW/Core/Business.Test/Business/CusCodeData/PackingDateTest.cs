using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingDate))]
	sealed class PackingDateTest : Customs.Business.Testing.CusCodeDataTest<PackingDate>
	{
		[ExpectNoExceptions]
		public void TestCY_Date()
		{
			BusinessObjectCaptionTestHelper.AssertCaptionsWithFullDescription(packingDate.CY_DateInfo, "Date", "The date of packing for animal products.");
		}

		[ExpectNoExceptions]
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(packingDate.CY_Type, NUnit.Framework.Is.EqualTo(CusCodeDataTypeList.Codes.PackingDates).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(packingDate.CY_ParentTableCode, NUnit.Framework.Is.EqualTo(JobComInvoiceLineSchema.Constants.Prefix).Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestValidationType()
		{
			NUnit.Framework.Assert.That(packingDate.Validation, NUnit.Framework.Is.TypeOf<PackingDateValidation>());
		}

		protected override IEnumerable<PackingDate> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return factory.New<JobDeclaration>().JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew().PackingDateCollection.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return invoiceLine.PackingDateCollection.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			invoiceLine = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			packingDate = invoiceLine.PackingDateCollection.AddNew();
		}

		PackingDate packingDate;
		JobComInvoiceLine invoiceLine;
	}
}
