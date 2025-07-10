using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class NX5105Consignment_BondedGoodsTest : TestCaseWithFactory
	{
		#region Add Duty Reason Code
		[ExpectNoExceptions]
		public void TestBondedGoods_AddDutyReasonCode()
		{
			var entryHeader = GetEntryHeaderWithMinimumData();
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.AddDutyReasonCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "BondedGoods.AddDutyReasonCode should be");
			entryHeader.EntryInstruction.CEI_ReasonForDuty = "AA";
			NUnit.Framework.Assert.That(bondedGoods.AddDutyReasonCode, NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison), "BondedGoods.AddDutyReasonCode should be");
		}

		#endregion
		#region Bonded Goods Invoices
		[ExpectNoExceptions]
		public void TestBondedGoods_BondedGoodsInvoices()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsInvoices.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.BondedGoodsInvoices.Count() should be");
			var emptyUniform = declaration.GovernmentUniformInvoices.AddNew();
			emptyUniform.CY_Code = "";
			emptyUniform.CY_Data = "12";
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsInvoices.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.BondedGoodsInvoices.Count() should be");
			var uniform = declaration.GovernmentUniformInvoices.AddNew();
			uniform.CY_Code = "COD";
			uniform.CY_Data = "12";
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsInvoices.Count(), NUnit.Framework.Is.EqualTo(1), "BondedGoods.BondedGoodsInvoices.Count() should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsInvoices.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("COD").Using(CustomComparers.TypeComparison), "BondedGoods.BondedGoodsInvoices[0].ID should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsInvoices.ElementAt(0).ValueAmount, NUnit.Framework.Is.EqualTo(12m).Using(CustomComparers.TypeComparison), "BondedGoods.BondedGoodsInvoices[0].ID should be");
		}

		#endregion
		#region Bonded Goods Monthly Report
		[ExpectNoExceptions]
		public void TestBondedGoods_BondedGoodsMonthlyReport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport, NUnit.Framework.Is.EqualTo(default(IBondedGoodsMonthlyReport)));
			entryInstruction.CEI_WHSMonth = "1";
			entryInstruction.CEI_WHSTradeReferenceNo = "AAA";
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport, NUnit.Framework.Is.Not.EqualTo(default(IBondedGoodsMonthlyReport)));
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.MonthNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.MonthNumeric should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.TraderReferenceID, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.TW_WHSTradeReferenceNo should be");
			entryInstruction.CEI_WHSMonth = "1";
			entryInstruction.CEI_WHSTradeReferenceNo = "";
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport, NUnit.Framework.Is.Not.EqualTo(default(IBondedGoodsMonthlyReport)));
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.MonthNumeric, NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.MonthNumeric should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.TraderReferenceID, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.TW_WHSTradeReferenceNo should be");
			entryInstruction.CEI_WHSMonth = "";
			entryInstruction.CEI_WHSTradeReferenceNo = "AAA";
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport, NUnit.Framework.Is.Not.EqualTo(default(IBondedGoodsMonthlyReport)));
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.MonthNumeric, NUnit.Framework.Is.EqualTo(0).Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.MonthNumeric should be");
			NUnit.Framework.Assert.That(bondedGoods.BondedGoodsMonthlyReport.TraderReferenceID, NUnit.Framework.Is.EqualTo("AAA").Using(CustomComparers.TypeComparison), "BondedGoodsMonthlyReport.TW_WHSTradeReferenceNo should be");
		}

		#endregion
		#region InBonded Party
		[ExpectNoExceptions]
		public void TestBondedGoods_InBondedParty()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var addressWithVAT = organization.Addresses.AddNew();
			addressWithVAT.OA_Address1 = "TEST2";
			var addressWithCPW = organization.Addresses.AddNew();
			addressWithCPW.OA_Address1 = "TEST";
			var registrationCodeCPW = organization.CustomsCodes.AddNew();
			registrationCodeCPW.OK_CodeType = "CPW";
			registrationCodeCPW.OK_RN_NKCodeCountry = "TW";
			registrationCodeCPW.OK_CustomsRegNo = "123";
			registrationCodeCPW.OK_OA_PremisesAddress = addressWithCPW.PK;
			var registrationCodeVAT = organization.CustomsCodes.AddNew();
			registrationCodeVAT.OK_CodeType = "VAT";
			registrationCodeVAT.OK_RN_NKCodeCountry = "TW";
			registrationCodeVAT.OK_CustomsRegNo = "456";
			registrationCodeVAT.OK_OA_PremisesAddress = addressWithVAT.PK;
			var addressNoCPW = organization.Addresses.AddNew();
			addressNoCPW.OA_Address1 = "TEST";
			Factory.Save();
			var entryHeader = GetEntryHeaderWithMinimumData();
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_OA_Warehouse2 = addressNoCPW.PK;
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty, NUnit.Framework.Is.EqualTo(default(IBondedParty)));
			entryInstruction.CEI_OA_Warehouse2 = addressNoCPW.PK;
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty, NUnit.Framework.Is.EqualTo(default(IBondedParty)));
			entryHeader.EntryInstruction.CEI_OA_Warehouse2 = addressWithCPW.PK;
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "BondedGoods.InBondedParty.ID should be");
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty.BondedID, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison), "BondedGoods.InBondedParty.BondedID should be");
			NUnit.Framework.Assert.That(bondedGoods.InBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedGoods.InBondedParty.TypeCode should be");
		}

		#endregion
		#region OutBonded Party
		[ExpectNoExceptions]
		public void TestBondedGoods_OutBondedParty()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var addressWithCPW = organization.Addresses.AddNew();
			addressWithCPW.OA_Address1 = "TEST";
			var registrationCodeCPW = organization.CustomsCodes.AddNew();
			registrationCodeCPW.OK_CodeType = "CPW";
			registrationCodeCPW.OK_RN_NKCodeCountry = "TW";
			registrationCodeCPW.OK_CustomsRegNo = "123";
			registrationCodeCPW.OK_OA_PremisesAddress = addressWithCPW.PK;
			var registrationCodeVAT = organization.CustomsCodes.AddNew();
			registrationCodeVAT.OK_CodeType = "VAT";
			registrationCodeVAT.OK_RN_NKCodeCountry = "TW";
			registrationCodeVAT.OK_CustomsRegNo = "456";
			registrationCodeVAT.OK_OA_PremisesAddress = addressWithCPW.PK;
			var addressNoCPW = organization.Addresses.AddNew();
			addressNoCPW.OA_Address1 = "TEST";
			Factory.Save();
			var entryHeader = GetEntryHeaderWithMinimumData();
			var entryInstruction = entryHeader.EntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			entryInstruction.CEI_OA_Warehouse = addressNoCPW.PK;
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty, NUnit.Framework.Is.EqualTo(default(IBondedParty)));
			entryHeader.EntryInstruction.CEI_OA_Warehouse = addressWithCPW.PK;
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty.ID, NUnit.Framework.Is.EqualTo("123").Using(CustomComparers.TypeComparison), "BondedGoods.OutBondedParty.ID should be");
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty.BondedID, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison), "BondedGoods.OutBondedParty.BondedID should be");
			NUnit.Framework.Assert.That(bondedGoods.OutBondedParty.TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedGoods.OutBondedParty.TypeCode should be");
		}

		#endregion
		#region PreBonded Parties
		[ExpectNoExceptions]
		public void TestBondedGoods_PreBondedParties()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var addressWithCPW = organization.Addresses.AddNew();
			addressWithCPW.OA_Address1 = "TEST";
			var registrationCodeCCP = organization.CustomsCodes.AddNew();
			registrationCodeCCP.OK_CodeType = "CCP";
			registrationCodeCCP.OK_RN_NKCodeCountry = "TW";
			registrationCodeCCP.OK_CustomsRegNo = "123";
			registrationCodeCCP.OK_OA_PremisesAddress = addressWithCPW.PK;
			var registrationCodeVAT = organization.CustomsCodes.AddNew();
			registrationCodeVAT.OK_CodeType = "VAT";
			registrationCodeVAT.OK_RN_NKCodeCountry = "TW";
			registrationCodeVAT.OK_CustomsRegNo = "456";
			registrationCodeVAT.OK_OA_PremisesAddress = addressWithCPW.PK;
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CusEntryInstruction;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var emptyFactory = declaration.BondedFactories.AddNew();
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.PreBondedParties.Count should be");
			var factory = declaration.BondedFactories.AddNew();
			factory.E2_AddressType = "BOF";
			factory.E2_OA_Address = addressWithCPW.PK;
			bondedGoods = new NX5105Consignment_BondedGoods(entryHeader);
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.Count(), NUnit.Framework.Is.EqualTo(0), "BondedGoods.PreBondedParties.Count should be");
			var addressWithCBF = organization.Addresses.AddNew();
			addressWithCBF.OA_Address1 = "TEST";
			addressWithCBF.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "999", "TW");
			registrationCodeVAT.OK_OA_PremisesAddress = addressWithCBF.PK;
			Factory.Save();
			factory.E2_OA_Address = addressWithCBF.PK;
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.Count(), NUnit.Framework.Is.EqualTo(1), "BondedGoods.PreBondedParties.Count should be");
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.ElementAt(0).CustomsControlID, NUnit.Framework.Is.EqualTo("999").Using(CustomComparers.TypeComparison), "BondedGoods.PreBondedParties[0].CustomsControlID should be");
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.ElementAt(0).ID, NUnit.Framework.Is.EqualTo("456").Using(CustomComparers.TypeComparison), "BondedGoods.PreBondedParties[0].ID should be");
			NUnit.Framework.Assert.That(bondedGoods.PreBondedParties.ElementAt(0).TypeCode, NUnit.Framework.Is.EqualTo("58").Using(CustomComparers.TypeComparison), "BondedGoods.PreBondedParties[0].TypeCode should be");
		}

		#endregion
		CusEntryHeader GetEntryHeaderWithMinimumData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		public void TestCheckArgumentsNotNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				new NX5105Consignment_BondedGoods(null);
			}

			);
			AssertNoExceptionThrown(() =>
			{
				new NX5105Consignment_BondedGoods(GetEntryHeaderWithMinimumData());
			}

			);
		}

		[ExpectNoExceptions]
		public void TestCheckNotApplicableProperties()
		{
			IBondedGoods bondedGoods = new NX5105Consignment_BondedGoods(GetEntryHeaderWithMinimumData());
			NUnit.Framework.Assert.That(bondedGoods.DocumentCode, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bondedGoods.Refundable, NUnit.Framework.Is.EqualTo("").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bondedGoods.BondedFactories, NUnit.Framework.Is.EqualTo(default(System.Collections.Generic.IEnumerable<IBondedParty>)));
		}
	}
}
