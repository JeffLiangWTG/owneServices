using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	[TestedType(typeof(PeriodicInvoicingDocManagerInfo))]
	public class PeriodicInvoicingDocManagerInfoTest : DocManagerInfoTestCase
	{
		public override BusinessObject GetEmptyParentBusinessObject()
		{
			var invoicing = Factory.New<PeriodicInvoicing>();
			invoicing.ET_StorageType = PeriodicInvoicingStorageTypes.Codes.ContainerYard;
			return invoicing;
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var invoicing = Factory.New<PeriodicInvoicing>();
			invoicing.ET_StorageType = PeriodicInvoicingStorageTypes.Codes.ContainerYard;
			invoicing.ET_StorageJobNumber = "TestInvoiceNumber";

			return invoicing;
		}
	}
}
