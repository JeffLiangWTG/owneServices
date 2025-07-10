using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105BondedPartyWrapperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestBondedParty()
		{
			var organization1 = Factory.New<OrgHeader>();
			organization1.Addresses.RemoveAndDeleteAll();
			organization1.OH_Code = "Org1";
			organization1.OH_RL_NKClosestPort = "TW";
			var orgAddress = organization1.Addresses[0];
			var cusCodeCPW = orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CPW", "CPW123", "TW");
			orgAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("CCP", "CCP123", "TW");
			organization1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "VAT123", "TW");
			Factory.Save();
			IBondedParty inOutBondedParty = new NX5105BondedPartyWrapper(orgAddress, "");
			NUnit.Framework.Assert.That(inOutBondedParty.ID, NUnit.Framework.Is.EqualTo("CPW123").Using(CustomComparers.TypeComparison), "BondedParty.ID should be");
			NUnit.Framework.Assert.That(inOutBondedParty.BondedID, NUnit.Framework.Is.EqualTo("VAT123").Using(CustomComparers.TypeComparison), "BondedParty.BondedID should be");
			NUnit.Framework.Assert.That(inOutBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedParty.TypeCode should be");
			NUnit.Framework.Assert.That(inOutBondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "BondedParty.CustomsControlID should be");
			orgAddress.CustomsCodes.Delete(cusCodeCPW);
			NUnit.Framework.Assert.That(inOutBondedParty.BondedID, NUnit.Framework.Is.EqualTo("VAT123").Using(CustomComparers.TypeComparison), "BondedParty.BondedID should be");
			IBondedParty preBondedParty = new NX5105BondedPartyWrapper(orgAddress, "CCP");
			NUnit.Framework.Assert.That(preBondedParty.ID, NUnit.Framework.Is.EqualTo("CCP123").Using(CustomComparers.TypeComparison), "BondedParty.ID should be");
			NUnit.Framework.Assert.That(preBondedParty.BondedID, NUnit.Framework.Is.EqualTo("VAT123").Using(CustomComparers.TypeComparison), "BondedParty.BondedID should be");
			NUnit.Framework.Assert.That(preBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedParty.TypeCode should be");
			NUnit.Framework.Assert.That(preBondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo("CCP123").Using(CustomComparers.TypeComparison), "BondedParty.CustomsControlID should be");
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105BondedPartyWrapper(null, "");
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new NX5105BondedPartyWrapper(Factory.NewWithValidTestData<OrgAddress>(), "");
			}

			);
		}
	}
}
