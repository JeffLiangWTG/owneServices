using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingDocsAndCartage))]
	public class ForwardingDocsAndCartageBOTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ForwardingShipment>().DocsAndCartage;
		}

		protected override BusinessObject GetLogParentForEventDateProperty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var docsAndCartage = (ForwardingDocsAndCartage)BusinessObject;
			docsAndCartage.JP_ParentID = shipment.PK;
			docsAndCartage.JP_ParentTableCode = shipment.TablePrefix;
			var hit = shipment.DocsAndCartage;
			return shipment;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			AssertNotNull("lazy loading", shipment.Consols);
			AssertNotNull("lazy loading", shipment.ConsigneeDocumentaryAddress);
			AssertNotNull("lazy loading", shipment.ConsignorDocumentaryAddress);
			return shipment.DocsAndCartage;
		}
	}
}
