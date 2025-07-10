using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecondaryPartBOM))]
	class OrgSecondaryPartBOMTest : EnterpriseBusinessObjectTestCase
	{
		public void TestComponentUsages()
		{
			var secondaryPart = Factory.New<OrgSecondaryPartBOM>();
			AssertType<OrgSecondaryPartBOMPivotCollection>(secondaryPart.ComponentUsages);
			AssertEquals(true, secondaryPart.IsRegisteredEditableChildObject(secondaryPart.ComponentUsages));

			var newPivot = secondaryPart.ComponentUsages.AddNew();
			AssertEquals(secondaryPart.PK, newPivot.OPP_OSB_SecondaryPart);
		}
	}
}
