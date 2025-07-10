using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AgentTestsTest : TestCaseWithFactory
	{
		#region Properties
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(agent.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(agent.Name, NUnit.Framework.Is.EqualTo("AGENT CO., LTD.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(agent.ChineseName, NUnit.Framework.Is.EqualTo("代理股份有限公司").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(agent.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(agent.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(agent.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(agent.RoleCode, NUnit.Framework.Is.EqualTo("CB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(agent.SubBoxID, NUnit.Framework.Is.EqualTo("BBB").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(agent.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(agent.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		#region IAddress
		[ExpectNoExceptions]
		public void TestAddressChineseLine()
		{
			NUnit.Framework.Assert.That(agent.Address.ChineseLine, NUnit.Framework.Is.EqualTo("10093臺北巿臺北代理出口區園東街6號").Using(CustomComparers.TypeComparison), "Address");
		}

		#endregion
		#region ILPCOAuthorizedParty
		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyName()
		{
			NUnit.Framework.Assert.That(agent.LPCOAuthorizedParty.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyID()
		{
			NUnit.Framework.Assert.That(agent.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("TWAEO-123465789").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedPartyTypeCode()
		{
			NUnit.Framework.Assert.That(agent.LPCOAuthorizedParty.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		#endregion
		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			testHelper.CreateAndSetProxyOrganization();
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var decl = entryHeader.Declaration;
			decl.JE_CustomsProfile = "AAA-BBB";
			entryHeader.EntryInstruction.CEI_BoxNumber = "123";
			agent = new Agent(decl, GlbCompany.CurrentCompany.OrgProxy.MainAddress);
		}

		IPartyDetails agent;
	}
}
