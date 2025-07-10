using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(ProvisionalPaymentAmountCodeData))]
	sealed class ProvisionalPaymentAmountCodeDataTest : Customs.Business.Testing.CusCodeDataTest<ProvisionalPaymentAmountCodeData>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ProvisionalPaymentAmountCodeData>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.NewWithValidTestData<ProvisionalPaymentAmountCodeData>();
		}

		protected override IEnumerable<ProvisionalPaymentAmountCodeData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var result = factory.NewWithValidTestData<ProvisionalPaymentAmountCodeData>();
			var declaration = factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.ProvisionalPayments.Add(result);
			yield return result;
		}
	}
}
