using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USHFCDetail))]
	internal class USHFCDetailTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<USHFCDetail>
	{
		public void TestProperties()
		{
			var detail = (USHFCDetail)GetNewBusinessObjectForDeleteTest(Factory);
			AssertEquals("AddInfoLookups: Type", typeof(USHFCDetailAddInfoLookups), detail.AddInfoLookups.GetType());
			AssertEquals("AddInfoValidation: Type", typeof(USHFCDetailAddInfoValidation), detail.AddInfoValidation.GetType());
		}

		public void TestResourceStringDataAttribute()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCDetail), nameof(USHFCDetail.US_LPCONumber), false, x => x.Caption == "Product Code Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCDetail), nameof(USHFCDetail.US_NameOfActiveIngredient), false, x => x.Caption == "Constituent Element");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(USHFCDetail), nameof(USHFCDetail.US_ActiveIngredientPercentage), false, x => x.Caption == "%");
		}

		public void TestIHFCDetail()
		{
			var detail = (USHFCDetail)GetNewBusinessObjectForDeleteTest(Factory);
			detail.US_LPCONumber = "123";
			detail.US_NameOfActiveIngredient = "ABC";
			detail.US_ActiveIngredientPercentage = 56m;

			var iDetail = (IHFCDetail)detail;
			AssertEquals("123", iDetail.ProductCode);
			AssertEquals("ABC", iDetail.NameOfActiveIngredient);
			AssertEquals(56m, iDetail.ActiveIngredientPercentage);
		}

		protected override IEnumerable<USHFCDetail> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (USHFCDetail)GetNewBusinessObjectForDeleteTest(factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var head = invoiceLine.USHFCHeaders.AddNew();
			head.US_ASHRAENumber = "123";
			var deta = head.USHFCDetails.AddNew();
			deta.US_LPCONumber = "456";
			return deta;
		}
	}
}
