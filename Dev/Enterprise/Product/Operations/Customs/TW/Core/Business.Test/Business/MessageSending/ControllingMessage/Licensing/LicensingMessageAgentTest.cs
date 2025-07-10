using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(LicensingMessageAgent))]
	sealed class LicensingMessageAgentTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			declaration.CusEntryInstruction.CEI_BoxNumber = "123";
			NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "ID");
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(agent.RoleCode, NUnit.Framework.Is.EqualTo("CB").Using(CustomComparers.TypeComparison), "RoleCode");
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			declaration.JE_CustomsProfile = "AAA-BBB";
			NUnit.Framework.Assert.That(agent.SubBoxID, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison), "SubBoxID");
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			agent = new LicensingMessageAgent(header);
		}

		JobDeclaration declaration;
		CusTWControllingMessageHeader header;
		IDeclarationAgent agent;
	}
}
