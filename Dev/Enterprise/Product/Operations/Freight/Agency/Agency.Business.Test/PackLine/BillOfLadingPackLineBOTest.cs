using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(BillOfLadingPackLine))]
	internal class BillOfLadingPackLineBOTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<BillOfLading>().OuterPackLines.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Factory.New<BillOfLading>().OuterPackLines.AddNew();
		}
		#endregion
	}
}
