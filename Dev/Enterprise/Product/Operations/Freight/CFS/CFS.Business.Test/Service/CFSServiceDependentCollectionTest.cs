using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSServiceDependentCollection))]
	public class CFSServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return CFSServiceDependentCollection;
		}

		CFSServiceDependentCollection CFSServiceDependentCollection
		{
			get
			{
				if (fCFSServiceDependentCollection == null)
				{
					CFSShipment shipment = Factory.New<CFSShipment>();
					fCFSServiceDependentCollection = new CFSServiceDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fCFSServiceDependentCollection;
			}
		}
		CFSServiceDependentCollection fCFSServiceDependentCollection;

		#endregion
	}
}
