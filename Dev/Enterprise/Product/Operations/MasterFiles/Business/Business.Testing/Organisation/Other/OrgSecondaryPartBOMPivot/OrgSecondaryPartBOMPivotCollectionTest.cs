using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSecondaryPartBOMPivotCollection))]
	class OrgSecondaryPartBOMPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgSecondaryPartBOMPivotCollection>
	{
		protected override OrgSecondaryPartBOMPivotCollection GetCollectionToTest()
		{
			var secondaryPart = Factory.New<OrgSecondaryPartBOM>();
			return new OrgSecondaryPartBOMPivotCollection(secondaryPart);
		}
	}
}
