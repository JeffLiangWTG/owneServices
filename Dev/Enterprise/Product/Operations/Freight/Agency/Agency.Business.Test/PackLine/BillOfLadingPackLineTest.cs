using System;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingPackLineTest : BaseFreightTest
	{
		public void TestSetDefaultValues_UnitOfDimensionIsDefaultedFromRegistry()
		{
			var billOfLading = Factory.New<BillOfLading>();
			AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Inches);
			AssertEquals(Constants.Length.Inches, billOfLading.OuterPackLines.AddNew().JL_UnitOfDimension);
			AgencyRegistry.Instance.DefaultBillDimensionUnit.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Length.Feet);
			AssertEquals(Constants.Length.Feet, billOfLading.OuterPackLines.AddNew().JL_UnitOfDimension);
		}
	}
}
