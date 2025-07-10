using System.Linq;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMApplicationAgentWrapperTest : PartyDetailsWrapperAbstractTest<NX5105CMApplicationAgentWrapper>
	{
		[ExpectNoExceptions]
		public override void TestID()
		{
			orgHeader.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, "96944490", "TW");
			NUnit.Framework.Assert.That(partyDetails.ID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.ID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestName()
		{
			NUnit.Framework.Assert.That(partyDetails.Name, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Name, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestTypeCode()
		{
			NUnit.Framework.Assert.That(partyDetails.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public override void TestCommunications()
		{
			var communications = partyDetails.Communications;
			NUnit.Framework.Assert.That(communications.Count(), NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(communications.First().ID, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communications.First().TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communications.ElementAt(1).ID, NUnit.Framework.Is.EqualTo("123@456.com").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communications.ElementAt(1).TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Communications.Count(), NUnit.Framework.Is.EqualTo(0));
		}

		[ExpectNoExceptions]
		public override void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(partyDetails.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Messaging.ILPCOAuthorizedParty)));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(Messaging.ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public override void TestRoleCode()
		{
			NUnit.Framework.Assert.That(partyDetails.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public override void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(partyDetails.SubBoxID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.RoleCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override PartyDetailsWrapper GetPartyDetailsWrapper(OrgAddress orgAddress) => new NX5105CMApplicationAgentWrapper(orgAddress);
	}
}
