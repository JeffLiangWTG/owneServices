using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackRequiredDocumentDependentCollection))]
	public class PackUnpackRequiredDocumentDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return PackUnpackRequiredDocumentDependentCollection;
		}

		PackUnpackRequiredDocumentDependentCollection PackUnpackRequiredDocumentDependentCollection
		{
			get
			{
				if (fPackUnpackRequiredDocumentDependentCollection == null)
				{
					GatePassShipment shipment = Factory.New<GatePassShipment>();
					fPackUnpackRequiredDocumentDependentCollection = new PackUnpackRequiredDocumentDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fPackUnpackRequiredDocumentDependentCollection;
			}
		}
		PackUnpackRequiredDocumentDependentCollection fPackUnpackRequiredDocumentDependentCollection;

		#endregion
	}
}
