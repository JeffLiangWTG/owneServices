using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocZADA306LineCollection))]
	public class DocZADA306LineCollectionTest : DocumentWrapperCollectionTest<DocZADA306LineCollection>
	{
		protected override DocZADA306LineCollection GetNewDocumentWrapperCollection()
		{
			return new DocZADA306LineCollection(Factory);
		}

		protected override object GetNewObjectToWrap()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var item = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew().Items.AddNew();
			return item;
		}
	}
}
