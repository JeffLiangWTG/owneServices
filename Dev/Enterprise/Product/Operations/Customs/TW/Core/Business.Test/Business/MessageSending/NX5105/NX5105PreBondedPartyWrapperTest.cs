using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105PreBondedPartyWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBondedParty()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var orgAddress = organization1.Addresses[0];
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CPW", "CPW123", "TW");
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCP", "CCP123", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "VAT123", "TW");
			Factory.Save();
			IBondedParty preBondedParty = new NX5105PreBondedPartyWrapper(orgAddress, "VAT", "");
			NUnit.Framework.Assert.That(preBondedParty.ID, NUnit.Framework.Is.EqualTo("VAT123").Using(CustomComparers.TypeComparison), "BondedParty.ID should be");
			NUnit.Framework.Assert.That(preBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedParty.TypeCode should be");
			NUnit.Framework.Assert.That(preBondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty), "BondedParty.CustomsControlID should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105PreBondedPartyWrapper(null, "", "");
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new NX5105PreBondedPartyWrapper(Factory.NewWithValidTestData<OrgAddress>(), "", "");
			}

			);
		}
	}
}
