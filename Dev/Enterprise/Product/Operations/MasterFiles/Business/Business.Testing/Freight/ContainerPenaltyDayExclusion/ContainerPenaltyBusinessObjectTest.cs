using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ContainerPenaltyDayExclusion))]
	sealed class ContainerPenaltyDayExclusionBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			var exclusionBizo = Factory.NewWithValidTestData<ContainerPenaltyDayExclusion>();
			exclusionBizo.CEX_Monday = ZBool.True;
			exclusionBizo.CEX_Tuesday = ZBool.False;
			exclusionBizo.CEX_Wednesday = ZBool.False;
			exclusionBizo.CEX_Thursday = ZBool.False;
			exclusionBizo.CEX_Friday = ZBool.False;
			exclusionBizo.CEX_Saturday = ZBool.False;
			exclusionBizo.CEX_Sunday = ZBool.False;
			exclusionBizo.CEX_Weekend = ZBool.False;
			exclusionBizo.CEX_Holiday = ZBool.False;
			Factory.Save();
			AssertNotNull(Factory.Load<ContainerPenaltyDayExclusion>(exclusionBizo.PK));

			exclusionBizo.Delete();
			Factory.Save();
			AssertNull(Factory.Load<ContainerPenaltyDayExclusion>(exclusionBizo.PK));
		}
	}
}
