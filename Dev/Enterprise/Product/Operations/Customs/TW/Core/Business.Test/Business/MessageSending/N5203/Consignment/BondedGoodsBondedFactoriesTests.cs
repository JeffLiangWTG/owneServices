using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.N5203;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedGoodsBondedFactoriesTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestID()
		{
			NUnit.Framework.Assert.That(bondedParty.ID, NUnit.Framework.Is.EqualTo("11111111").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBondedID()
		{
			NUnit.Framework.Assert.That(bondedParty.BondedID, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestTypeCode()
		{
			NUnit.Framework.Assert.That(bondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestCustomsControlID()
		{
			NUnit.Framework.Assert.That(bondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo("999").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testHelper = new TestTWCreator(Factory);
			var entryHeader = testHelper.CreateEntryHeaderForN5203();
			var declaration = entryHeader.Declaration;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AA1";
			orgHeader.OH_RL_NKClosestPort = "TW";
			var orgAddress = orgHeader.Addresses[0];
			var vatCode = orgHeader.CustomsCodes.AddNew();
			vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			vatCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			vatCode.OK_CustomsRegNo = "11111111";
			var ccpCode = orgAddress.CustomsCodes.AddNew();
			ccpCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			ccpCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			ccpCode.OK_CustomsRegNo = "22222222";
			orgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "999", "TW");
			declaration.BondedFactories.DeleteAll();
			var bondedFactory = declaration.BondedFactories.AddNew();
			bondedFactory.E2_AddressType = DocAddressTypes.Codes.BondedFactory;
			bondedFactory.E2_OA_Address = orgAddress.PK;
			Factory.Save();
			bondedParty = new Consignment(entryHeader).BondedGoods.BondedFactories.First();
		}

		IBondedParty bondedParty;
	}
}
