using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(TallyServiceDependentCollection))]
	public class TallyServiceDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return TallyServiceDependentCollection;
		}

		TallyServiceDependentCollection TallyServiceDependentCollection
		{
			get
			{
				if (fTallyServiceDependentCollection == null)
				{
					PackUnpackShipment shipment = Factory.New<PackUnpackShipment>();
					fTallyServiceDependentCollection = new TallyServiceDependentCollection(shipment.DocsAndCartage, shipment.Factory);
				}

				return fTallyServiceDependentCollection;
			}
		}
		TallyServiceDependentCollection fTallyServiceDependentCollection;

		#endregion
	}
}
