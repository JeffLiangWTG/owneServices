using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefDocSource))]
	sealed class RefDocSourceTest : EnterpriseBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override void SetUp()
		{
			base.SetUp();
		}
	}
}
