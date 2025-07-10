using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(AgencyShipmentPackLine))]
	internal class AgencyShipmentPackLineBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<AgencyShipment>().OuterPackLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<AgencyShipment>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
