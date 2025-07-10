using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefZonePivot))]
	public class RefZonePivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLocation()
		{
			RefUNLOCO loco = Factory.New<RefUNLOCO>();
			RefCountry country = Factory.New<RefCountry>();

			RefZonePivot pivot = Factory.New<RefZonePivot>();
			pivot.F2_ParentID = loco.PK;
			pivot.F2_ParentTableCode = RefUNLOCOSchema.Constants.Prefix;

			RefZonePivot pivot2 = Factory.New<RefZonePivot>();
			pivot2.F2_ParentID = country.PK;
			pivot2.F2_ParentTableCode = RefCountrySchema.Constants.Prefix;

			AssertEquals(loco, pivot.Location);
			AssertEquals(country, pivot2.Location);
		}
	}
}
