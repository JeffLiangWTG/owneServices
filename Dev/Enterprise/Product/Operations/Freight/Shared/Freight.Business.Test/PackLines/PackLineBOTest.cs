using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLine))]
	sealed class PackLineBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			CommonShipment shipment = CommonShipment.New(factory);
			PackLine packLine = shipment.OuterPackLines.AddNew();
			return packLine;
		}

		#endregion
	}
}
