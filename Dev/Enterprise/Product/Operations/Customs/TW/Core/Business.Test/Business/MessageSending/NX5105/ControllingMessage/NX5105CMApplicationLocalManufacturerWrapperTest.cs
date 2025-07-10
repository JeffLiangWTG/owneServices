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
	sealed class NX5105CMApplicationLocalManufacturerWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NX5105CMApplicationLocalManufacturerWrapper(null));
			AssertNoExceptionThrown(() => new NX5105CMApplicationLocalManufacturerWrapper(jobDocAddress));
		}

		[ExpectNoExceptions]
		public void TestChineseName()
		{
			NUnit.Framework.Assert.That(declarer.ChineseName, NUnit.Framework.Is.EqualTo("公司名稱X1").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarerWithOverride.ChineseName, NUnit.Framework.Is.EqualTo("override name").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestAddressChineseLine()
		{
			NUnit.Framework.Assert.That(declarer.Address.ChineseLine, NUnit.Framework.Is.EqualTo("忠孝東路三段232號").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarerWithOverride.Address.ChineseLine, NUnit.Framework.Is.EqualTo("override address1321XX").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCommunications()
		{
			address.OA_Phone = ZString.Empty;
			NUnit.Framework.Assert.That(declarer.Communications.Count(), NUnit.Framework.Is.EqualTo(0));
			address.OA_Phone = "03-333-6666";
			NUnit.Framework.Assert.That(declarer.Communications.Count(), NUnit.Framework.Is.EqualTo(1));
			var communicationTE = declarer.Communications.First();
			NUnit.Framework.Assert.That(communicationTE.ID, NUnit.Framework.Is.EqualTo("03-333-6666").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(communicationTE.TypeID, NUnit.Framework.Is.EqualTo("TE").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(declarerWithOverride.Communications.First().ID, NUnit.Framework.Is.EqualTo("13925568211").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = new TestTWCreator(Factory).CreateOrganizationForImporter();
			address = header.MainAddress;
			jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_OA_Address = address.PK;
			declarer = new NX5105CMApplicationLocalManufacturerWrapper(jobDocAddress);
			jobDocAddressWithOverride = Factory.New<JobDocAddress>();
			jobDocAddressWithOverride.E2_OA_Address = address.PK;
			jobDocAddressWithOverride.E2_AddressOverride = ZBool.True;
			jobDocAddressWithOverride.E2_Phone = "13925568211";
			jobDocAddressWithOverride.E2_CompanyName = "override name";
			jobDocAddressWithOverride.E2_Address1 = "override address1";
			declarerWithOverride = new NX5105CMApplicationLocalManufacturerWrapper(jobDocAddressWithOverride);
		}

		IPartyDetails declarer;
		IPartyDetails declarerWithOverride;
		OrgHeader header;
		OrgAddress address;
		JobDocAddress jobDocAddress;
		JobDocAddress jobDocAddressWithOverride;
	}
}
