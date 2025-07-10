using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(GatePassRequiredDocumentDependentCollection))]
	public class GatePassRequiredDocumentDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionIsReadOnly()
		{
			Assert(GatePassRequiredDocumentCollection.ReadOnly);
		}

		public void TestCollectionDoesNotAllowNew()
		{
			AssertEquals(false, GatePassRequiredDocumentCollection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return GatePassRequiredDocumentCollection;
		}

		GatePassRequiredDocumentDependentCollection GatePassRequiredDocumentCollection
		{
			get
			{
				if (fGatePassRequiredDocumentCollection == null)
				{
					GatePassShipment shipment = Factory.New<GatePassShipment>();
					fGatePassRequiredDocumentCollection = new GatePassRequiredDocumentDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fGatePassRequiredDocumentCollection;
			}
		}
		GatePassRequiredDocumentDependentCollection fGatePassRequiredDocumentCollection;

		#endregion
	}
}
