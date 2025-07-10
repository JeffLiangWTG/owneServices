using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccComplianceSequenceCollection))]
	sealed class AccComplianceSequenceCollectionTest : ActiveBusinessObjectCollectionTestCase<AccComplianceSequenceCollection>
	{
		protected override AccComplianceSequenceCollection GetCollectionToTest()
		{
			return new AccComplianceSequenceCollection(Factory);
		}
	}
}
