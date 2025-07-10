using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSRequiredDocumentDependentCollection))]
	public class CFSRequiredDocumentDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CFSRequiredDocumentDependentCollection;
		}

		CFSRequiredDocumentDependentCollection CFSRequiredDocumentDependentCollection
		{
			get
			{
				if (fCFSRequiredDocumentDependentCollection == null)
				{
					CFSShipment shipment = Factory.New<CFSShipment>();
					fCFSRequiredDocumentDependentCollection = new CFSRequiredDocumentDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fCFSRequiredDocumentDependentCollection;
			}
		}
		CFSRequiredDocumentDependentCollection fCFSRequiredDocumentDependentCollection;

		#endregion
	}
}
