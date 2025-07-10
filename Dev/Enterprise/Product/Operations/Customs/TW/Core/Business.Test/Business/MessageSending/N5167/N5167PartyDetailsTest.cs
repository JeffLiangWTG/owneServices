using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class N5167PartyDetailsTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(partyDetails.ID, NUnit.Framework.Is.EqualTo("XX1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(partyDetails.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(partyDetails.ChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(partyDetails.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(partyDetails.CustomsControlID, NUnit.Framework.Is.EqualTo("XX2").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(partyDetails.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(partyDetails.RoleCode, NUnit.Framework.Is.EqualTo("XX4").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(partyDetails.SubBoxID, NUnit.Framework.Is.EqualTo("XX3").Using(CustomComparers.TypeComparison));
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
			NUnit.Framework.Assert.That(partyDetails.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
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
			partyDetails = new N5167.PartyDetails(id: "XX1", customsControlID: "XX2", subBoxID: "XX3", roleCode: "XX4");
		}

		IPartyDetails partyDetails;
	}
}
