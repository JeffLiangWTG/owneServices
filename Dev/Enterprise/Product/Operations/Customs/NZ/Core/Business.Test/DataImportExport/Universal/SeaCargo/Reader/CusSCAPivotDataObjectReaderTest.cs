using System.Linq;
using Enterprise.Customs.NZ.Business.Express;

namespace Enterprise.Customs.NZ.Business.Data.Universal.Testing
{
	partial class CusSCAOceanBillDataObjectReaderTest
	{
		protected override void AssertPivot1AllPropertiesSet(CusSCAPackingLine pivot)
		{
			base.AssertPivot1AllPropertiesSet(pivot);
			AssertEquals("CT", pivot.CV_PackageType);
			AssertEquals(4000m, pivot.CV_Volume);
			AssertEquals("UNDGs", "1001", pivot.UNDGs.Single().Substance?.DG_Code);
		}

		protected override void AssertPivot2AllPropertiesSet(CusSCAPackingLine pivot)
		{
			base.AssertPivot2AllPropertiesSet(pivot);
			AssertEquals("DR", pivot.CV_PackageType);
			AssertEquals(40000m, pivot.CV_Volume);
			AssertEquals("UNDGs", "1002", pivot.UNDGs.Single().Substance?.DG_Code);
		}
	}
}
