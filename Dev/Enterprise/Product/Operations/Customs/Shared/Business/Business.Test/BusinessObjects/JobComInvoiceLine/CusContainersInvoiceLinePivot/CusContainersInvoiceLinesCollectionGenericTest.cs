using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusContainersInvoiceLinesCollection<CusContainerInvoiceLinePivot, BaseJobComInvoiceLine, BaseCusContainer>))]
	sealed class CusContainersInvoiceLinesCollectionGenericTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CusContainersInvoiceLinesCollection<CusContainerInvoiceLinePivot, BaseJobComInvoiceLine, BaseCusContainer>(Factory.New<BaseJobComInvoiceLine>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CusContainerInvoiceLinePivot>();
		}
	}
}
