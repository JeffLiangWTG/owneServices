using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedPartyTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "123", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			IBondedParty bondedParty = new BondedParty(testAddress);
			NUnit.Framework.Assert.That(bondedParty.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "AA111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(bondedParty.ID, NUnit.Framework.Is.EqualTo("AA111").Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.DeleteAll();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BB111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(bondedParty.ID, NUnit.Framework.Is.EqualTo("BB111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBondedID()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT111", Core.Constants.CountryCodes.Taiwan);
			var testAddress = testOrg.Addresses.AddNew();
			testAddress.OA_Address1 = "test address1";
			testAddress.OA_Address2 = "test address2";
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierCode, "00612348", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsCPPermitCode, "212233", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "123", Core.Constants.CountryCodes.Taiwan);
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			IBondedParty bondedParty = new BondedParty(testAddress);
			NUnit.Framework.Assert.That(bondedParty.BondedID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "AA111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(bondedParty.BondedID, NUnit.Framework.Is.EqualTo("VAT111").Using(CustomComparers.TypeComparison));
			testAddress.CustomsCodes.DeleteAll();
			testAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "BB111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			NUnit.Framework.Assert.That(bondedParty.BondedID, NUnit.Framework.Is.EqualTo("VAT111").Using(CustomComparers.TypeComparison));
		}
	}
}
