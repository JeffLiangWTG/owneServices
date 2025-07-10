using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class N5301PartyDetailsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(partyDetails.ID, NUnit.Framework.Is.EqualTo("P1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(partyDetails.Name, NUnit.Framework.Is.EqualTo("XXXX1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(partyDetails.ChineseName, NUnit.Framework.Is.EqualTo("測試").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(partyDetails.TypeCode, NUnit.Framework.Is.EqualTo("S").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(partyDetails.CustomsControlID, NUnit.Framework.Is.EqualTo("CC1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(partyDetails.PaymentOnAccountBusinessID.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(partyDetails.RoleCode, NUnit.Framework.Is.EqualTo("RC1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(partyDetails.SubBoxID, NUnit.Framework.Is.EqualTo("SB1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(partyDetails.Address, NUnit.Framework.Is.EqualTo(default(IAddress)));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(partyDetails.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			NUnit.Framework.Assert.That(partyDetails.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(partyDetails.ContactName.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestMainManufacturer()
		{
			NUnit.Framework.Assert.That(partyDetails.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestUndertakeCode()
		{
			NUnit.Framework.Assert.That(partyDetails.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();
			partyDetails = new N5301.PartyDetails("P1", customsControlID: "CC1", subBoxID: "SB1", roleCode: "RC1", name: "XXXX1", chineseName: "測試", typeCode: "S");
		}

		IPartyDetails partyDetails;
	}
}
