using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	sealed class DeliveryAgentToSelectFromForPrintingTestClass : DeliveryAgentToSelectFromForPrinting
	{
		public DeliveryAgentToSelectFromForPrintingTestClass(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void SetDefaultValueForPrintDocumentFlagMethod()
		{
			base.SetDefaultValueForPrintDocumentFlag();
		}
	}
}
