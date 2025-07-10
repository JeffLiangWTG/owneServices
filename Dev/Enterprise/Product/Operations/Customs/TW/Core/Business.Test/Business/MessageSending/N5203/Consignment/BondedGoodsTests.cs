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
	sealed class BondedGoodsTests : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAddDutyReasonCode()
		{
			NUnit.Framework.Assert.That(BondedGoods.AddDutyReasonCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestBondedGoodsInvoices()
		{
			declaration.GovernmentUniformInvoices.RemoveAndDeleteAll();
			var gui1 = declaration.GovernmentUniformInvoices.AddNew();
			gui1.CY_Code = "XXXX1";
			gui1.Amount = 123456;
			var gui2 = declaration.GovernmentUniformInvoices.AddNew();
			gui2.CY_Code = "XXXX2";
			gui2.Amount = 123457;
			var gui3 = declaration.GovernmentUniformInvoices.AddNew();
			gui3.CY_Code = "XXXX3";
			gui3.Amount = 123458;
			NUnit.Framework.Assert.That(BondedGoods.BondedGoodsInvoices.Count(), NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestBondedGoodsMonthlyReport()
		{
			NUnit.Framework.Assert.That(BondedGoods.BondedGoodsMonthlyReport.GetType(), NUnit.Framework.Is.EqualTo(typeof(BondedGoodsMonthlyReport)));
		}

		[ExpectNoExceptions]
		public void TestInBondedParty()
		{
			this.entryHeader.EntryInstruction.CEI_OA_Warehouse2 = testHelper.CreateOrganizationForWarehouse2().MainAddress.PK;
			var inBondedParty = BondedGoods.InBondedParty;
			NUnit.Framework.Assert.That(BondedGoods.InBondedParty.GetType(), NUnit.Framework.Is.EqualTo(typeof(BondedParty)));
			NUnit.Framework.Assert.That(inBondedParty.ID, NUnit.Framework.Is.EqualTo("00612444").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(inBondedParty.BondedID, NUnit.Framework.Is.EqualTo("00612349").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(inBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(inBondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VGMRegistrationNumber, "BB111", Core.Constants.CountryCodes.Taiwan);
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "789", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = testOrg.MainAddress.PK;
			Factory.Save();
			IBondedGoods bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
			testOrg.MainAddress.CustomsCodes.DeleteAll();
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
		}

		[ExpectNoExceptions]
		public void TestOutBondedParty()
		{
			this.entryHeader.EntryInstruction.CEI_OA_Warehouse = testHelper.CreateOrganizationForWarehouse().MainAddress.PK;
			var outBondedParty = BondedGoods.OutBondedParty;
			NUnit.Framework.Assert.That(BondedGoods.OutBondedParty.GetType(), NUnit.Framework.Is.EqualTo(typeof(BondedParty)));
			NUnit.Framework.Assert.That(outBondedParty.ID, NUnit.Framework.Is.EqualTo("00612348").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(outBondedParty.BondedID, NUnit.Framework.Is.EqualTo("9800002").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(outBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(outBondedParty.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VGMRegistrationNumber, "BB111", Core.Constants.CountryCodes.Taiwan);
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "789", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse = testOrg.MainAddress.PK;
			Factory.Save();
			IBondedGoods bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "CCP111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
			testOrg.MainAddress.CustomsCodes.DeleteAll();
			testOrg.MainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.WarehouseControlledPremisesID, "CPW111", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IBondedParty)));
		}

		[ExpectNoExceptions]
		public void TestPreBondedParties()
		{
			NUnit.Framework.Assert.That(BondedGoods.PreBondedParties, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<Enterprise.Customs.TW.Messaging.IBondedParty>)));
		}

		[ExpectNoExceptions]
		public void TestDocumentCode()
		{
			declaration.CusEntryInstruction.CEI_BillOfMaterials = true;
			NUnit.Framework.Assert.That(BondedGoods.DocumentCode, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			declaration.CusEntryInstruction.CEI_BillOfMaterials = false;
			NUnit.Framework.Assert.That(BondedGoods.DocumentCode, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRefundable()
		{
			declaration.CusEntryInstruction.CEI_DutyRefund = true;
			NUnit.Framework.Assert.That(BondedGoods.Refundable, NUnit.Framework.Is.EqualTo("Y").Using(CustomComparers.TypeComparison));
			declaration.CusEntryInstruction.CEI_DutyRefund = false;
			NUnit.Framework.Assert.That(BondedGoods.Refundable, NUnit.Framework.Is.EqualTo("N").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestBondedFactories()
		{
			this.declaration.BondedFactories.DeleteAll();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "999", "TW");
			var bondedFactory1 = this.declaration.BondedFactories.AddNew();
			bondedFactory1.E2_AddressType = DocAddressTypes.Codes.BondedFactory;
			var bondedFactory2 = this.declaration.BondedFactories.AddNew();
			bondedFactory2.E2_AddressType = DocAddressTypes.Codes.BondedFactory;
			bondedFactory2.E2_OA_Address = orgAddress.PK;
			Factory.Save();
			NUnit.Framework.Assert.That(BondedGoods.BondedFactories.Count(), NUnit.Framework.Is.EqualTo(1));
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress = testOrg.MainAddress;
			mainAddress.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123", Core.Constants.CountryCodes.Taiwan);
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "456", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var emptyFactory = declaration.BondedFactories.AddNew();
			IBondedGoods bondedGoods = new BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.BondedFactories.Count should be");
			var factory = declaration.BondedFactories.AddNew();
			factory.E2_AddressType = "BOF";
			factory.E2_OA_Address = mainAddress.PK;
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.BondedFactories.Count should be");
			var addressWithCBF = testOrg.Addresses.AddNew();
			addressWithCBF.OA_Address1 = "TEST";
			addressWithCBF.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "999", "TW");
			Factory.Save();
			factory.E2_OA_Address = addressWithCBF.PK;
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.Count(), NUnit.Framework.Is.EqualTo(1), "BondedGoods.BondedFactories.Count should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.ElementAt(0).CustomsControlID, NUnit.Framework.Is.EqualTo("999").Using(CustomComparers.TypeComparison));
			mainAddress.CustomsCodes.DeleteAll();
			mainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.FTZ, "777", "TW");
			Factory.Save();
			factory.E2_OA_Address = mainAddress.PK;
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.ElementAt(0).CustomsControlID, NUnit.Framework.Is.EqualTo("777").Using(CustomComparers.TypeComparison));
			mainAddress.CustomsCodes.DeleteAll();
			mainAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "555", "TW");
			Factory.Save();
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories.ElementAt(0).CustomsControlID, NUnit.Framework.Is.EqualTo("555").Using(CustomComparers.TypeComparison));
		}

		protected override void SetUp()
		{
			base.SetUp();
			testHelper = new TestTWCreator(Factory);
			entryHeader = testHelper.CreateEntryHeaderForN5203();
			declaration = entryHeader.Declaration;
			Factory.Save();
		}

		TestTWCreator testHelper;
		CusEntryHeader entryHeader;
		JobDeclaration declaration;
		IBondedGoods BondedGoods => new Consignment(entryHeader).BondedGoods;
	}
}
