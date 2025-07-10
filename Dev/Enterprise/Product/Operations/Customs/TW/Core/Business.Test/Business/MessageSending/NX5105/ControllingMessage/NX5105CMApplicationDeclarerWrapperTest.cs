using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105CMApplicationDeclarerWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NX5105CMApplicationDeclarerWrapper(null));
			AssertNoExceptionThrown(() => new NX5105CMApplicationDeclarerWrapper(address));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(declarer.ChineseName, NUnit.Framework.Is.EqualTo("公司名稱X1").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(declarer.ID, NUnit.Framework.Is.EqualTo("I222222").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestName()
		{
			NUnit.Framework.Assert.That(declarer.Name, NUnit.Framework.Is.EqualTo("Importer company name xxx.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(declarer.TypeCode, NUnit.Framework.Is.EqualTo(PartyIdentifierCodeList.Codes._58).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddressChineseLine()
		{
			NUnit.Framework.Assert.That(declarer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("忠孝東路三段232號").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			address.OA_Phone = ZString.Empty;
			address.OA_Email = ZString.Empty;
			NUnit.Framework.Assert.That(declarer.Communications.Count(), NUnit.Framework.Is.EqualTo(0));
			address.OA_Email = "mary@yahoo.com";
			var communicationMA = declarer.Communications.First();
			NUnit.Framework.Assert.That(communicationMA.ID, NUnit.Framework.Is.EqualTo("mary@yahoo.com").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communicationMA.TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison));
			address.OA_Phone = "03-333-6666";
			var communicationTE = declarer.Communications.First();
			NUnit.Framework.Assert.That(communicationTE.ID, NUnit.Framework.Is.EqualTo("03-333-6666").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communicationTE.TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison));
			communicationMA = declarer.Communications.Skip(1).First();
			NUnit.Framework.Assert.That(communicationMA.ID, NUnit.Framework.Is.EqualTo("mary@yahoo.com").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communicationMA.TypeID, NUnit.Framework.Is.EqualTo("MA").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = new TestTWCreator(Factory).CreateOrganizationForImporter();
			address = header.MainAddress;
			declarer = new NX5105CMApplicationDeclarerWrapper(address);
		}

		IPartyDetails declarer;
		OrgHeader header;
		OrgAddress address;
	}
}
