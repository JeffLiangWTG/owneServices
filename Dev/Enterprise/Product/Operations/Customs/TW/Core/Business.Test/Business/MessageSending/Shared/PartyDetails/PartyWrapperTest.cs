using System.Linq;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class PartyWrapperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(wrapper.ID, NUnit.Framework.Is.EqualTo("ID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(wrapper.Name, NUnit.Framework.Is.EqualTo("Name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(wrapper.ChineseName, NUnit.Framework.Is.EqualTo("ChineseName").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestMainManufacturer()
		{
			NUnit.Framework.Assert.That(wrapper.MainManufacturer, NUnit.Framework.Is.EqualTo("MainManufacturer").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(wrapper.TypeCode, NUnit.Framework.Is.EqualTo("TypeCode").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddress()
		{
			NUnit.Framework.Assert.That(wrapper.Address.Line, NUnit.Framework.Is.EqualTo("line").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			var communications = wrapper.Communications;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(communications.Count(), NUnit.Framework.Is.EqualTo(1));
				var communication = communications.First();
				NUnit.Framework.Assert.That(communication.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(communication.TypeID, NUnit.Framework.Is.EqualTo("typeID").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(wrapper.CustomsControlID, NUnit.Framework.Is.EqualTo("CustomsControlID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPaymentOnAccountBusinessID()
		{
			NUnit.Framework.Assert.That(wrapper.PaymentOnAccountBusinessID, NUnit.Framework.Is.EqualTo("PaymentOnAccountBusinessID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRoleCode()
		{
			NUnit.Framework.Assert.That(wrapper.RoleCode, NUnit.Framework.Is.EqualTo("RoleCode").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestSubBoxID()
		{
			NUnit.Framework.Assert.That(wrapper.SubBoxID, NUnit.Framework.Is.EqualTo("SubBoxID").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestLPCOAuthorizedParty()
		{
			NUnit.Framework.Assert.That(wrapper.LPCOAuthorizedParty.ID, NUnit.Framework.Is.EqualTo("id").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestContactName()
		{
			NUnit.Framework.Assert.That(wrapper.ContactName, NUnit.Framework.Is.EqualTo("ContactName").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestOwnerName()
		{
			NUnit.Framework.Assert.That(wrapper.OwnerName, NUnit.Framework.Is.EqualTo("OwnerName").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestUndertakeCode()
		{
			NUnit.Framework.Assert.That(wrapper.UndertakeCode, NUnit.Framework.Is.EqualTo("UndertakeCode").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformations()
		{
			var additionalInformations = wrapper.AdditionalInformations;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(additionalInformations.Count(), NUnit.Framework.Is.EqualTo(1));
				NUnit.Framework.Assert.That(additionalInformations.First().PackingHouse, NUnit.Framework.Is.EqualTo("packingHouse").Using(CustomComparers.TypeComparison));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			wrapper = new PartyWrapper("ID", "Name", "ChineseName", "MainManufacturer", "TypeCode", new AddressWrapper("line"), new[] { new CommunicationWrapper("id", "typeID") }, "CustomsControlID", "PaymentOnAccountBusinessID", "RoleCode", "SubBoxID", new LPCOAuthorizedPartyWrapper("id"), "ContactName", "OwnerName", "UndertakeCode", new[] { new AdditionalInformationWrapper("packingHouse") });
		}

		PartyWrapper wrapper;
	}
}
