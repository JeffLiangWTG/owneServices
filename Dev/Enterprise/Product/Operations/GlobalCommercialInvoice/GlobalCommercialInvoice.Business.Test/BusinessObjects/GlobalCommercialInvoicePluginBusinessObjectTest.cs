using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.GlobalCommercialInvoice.Business.Test
{
	[TestedType(typeof(GlobalCommercialInvoicePluginBusinessObject))]
	public class GlobalCommercialInvoicePluginBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GlobalCommercialInvoicePluginBusinessObject((BusinessObject)Factory.CreateNewShipment());
		}
	}
}
