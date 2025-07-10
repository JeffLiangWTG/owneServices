using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.Module.Test;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(CYDPeriodicInvoicingFilterBusinessObject))]
	public class CYDPeriodicInvoicingFilterBusinessObjectTest : PeriodicInvoicingFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CYDPeriodicInvoicingFilterBusinessObject();
		}

		protected override PeriodicInvoicingCollection GetPeriodicInvoicingCollection()
		{
			return new PeriodicInvoicingCollection(Factory, PeriodicInvoicingStorageTypes.Codes.ContainerYard);
		}
	}
}
