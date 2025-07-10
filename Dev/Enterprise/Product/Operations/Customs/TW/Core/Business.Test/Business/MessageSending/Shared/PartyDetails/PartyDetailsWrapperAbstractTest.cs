using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class PartyDetailsWrapperAbstractTest<TPartyDetailsWrapper> : TestCaseWithFactory
		where TPartyDetailsWrapper : PartyDetailsWrapper
	{
		[ExpectNoExceptions]
		public virtual void TestID()
		{
			NUnit.Framework.Assert.That(partyDetails.ID, NUnit.Framework.Is.EqualTo(idForTesting));
		}

		[ExpectNoExceptions]
		public virtual void TestName()
		{
			NUnit.Framework.Assert.That(partyDetails.Name, NUnit.Framework.Is.EqualTo("HAPPY CO., LTD.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Name.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(partyDetails.ChineseName, NUnit.Framework.Is.EqualTo("綠晃科技股份有限公司").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.ChineseName.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public virtual void TestTypeCode()
		{
			NUnit.Framework.Assert.That(partyDetails.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(partyDetails.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(partyDetails.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestRoleCode()
		{
			NUnit.Framework.Assert.That(partyDetails.RoleCode, NUnit.Framework.Is.EqualTo(roleCodeForTesting));
		}

		[ExpectNoExceptions]
		public virtual void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(partyDetails.SubBoxID, NUnit.Framework.Is.EqualTo(subBoxIDForTesting));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(partyDetails.Address, NUnit.Framework.Is.TypeOf<AddressWrapper>());
			NUnit.Framework.Assert.That(partyDetails.Address, NUnit.Framework.Is.Not.EqualTo(default(IAddress)));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Address, NUnit.Framework.Is.TypeOf<AddressWrapper>());
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Address, NUnit.Framework.Is.Not.EqualTo(default(IAddress)));
		}

		[ExpectNoExceptions]
		public virtual void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(partyDetails.LPCOAuthorizedParty, NUnit.Framework.Is.TypeOf<LPCOAuthorizedPartyWrapper>());
			NUnit.Framework.Assert.That(partyDetails.LPCOAuthorizedParty, NUnit.Framework.Is.Not.EqualTo(default(ILPCOAuthorizedParty)));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.LPCOAuthorizedParty, NUnit.Framework.Is.EqualTo(default(ILPCOAuthorizedParty)));
		}

		[ExpectNoExceptions]
		public virtual void TestCommunications()
		{
			NUnit.Framework.Assert.That(partyDetails.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));
			NUnit.Framework.Assert.That(partyDetailsWithNullAddress.Communications, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<ICommunication>)));
		}

		[ExpectNoExceptions]
		public virtual void TestContactName()
		{
			NUnit.Framework.Assert.That(partyDetails.ContactName, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public virtual void TestMainManufacturer()
		{
			NUnit.Framework.Assert.That(partyDetails.MainManufacturer.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		[ExpectNoExceptions]
		public virtual void TestUndertakeCodeCore()
		{
			NUnit.Framework.Assert.That(partyDetails.UndertakeCode.ToString(), NUnit.Framework.Is.Null.Or.Empty);
		}

		protected abstract PartyDetailsWrapper GetPartyDetailsWrapper(OrgAddress orgAddress);

		protected override void SetUp()
		{
			base.SetUp();
			var testDataHelper = new TestTWCreator(Factory);
			orgHeader = testDataHelper.CreateOrganization();
			partyDetails = GetPartyDetailsWrapper(orgHeader.MainAddress);
			partyDetailsWithNullAddress = GetPartyDetailsWrapper(null);
			testDataHelper.CreateAndSetProxyOrganization();
		}

		protected IPartyDetails partyDetails;
		protected IPartyDetails partyDetailsWithNullAddress;
		protected OrgHeader orgHeader;
		protected ZString idForTesting = "id";
		protected ZString roleCodeForTesting = "role code";
		protected ZString subBoxIDForTesting = "subbox id";
	}
}
