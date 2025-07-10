using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PackingGroup))]
	sealed class PackingGroupTest : Customs.Business.Testing.BasePackingGroupTest
	{
		protected override Customs.Business.BaseJobDeclaration ImportJobDeclaration
		{
			get
			{
				var result = Factory.New<JobDeclaration>();
				result.JE_MessageType = JobMessageTypeList.Codes.Import;
				result.US_EnableENS = true;
				return result;
			}
		}

		protected override void SetUp()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			base.SetUp();
		}
	}
}
