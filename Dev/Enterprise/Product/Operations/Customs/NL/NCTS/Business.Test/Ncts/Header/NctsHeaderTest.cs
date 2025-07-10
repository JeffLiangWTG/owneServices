using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(NctsHeader))]
sealed class NctsHeaderTest : NctsHeaderAbstractTest
{
	public void TestGuarantees()
	{
		var nctsHeaderPhase5Departure = Factory.New<NctsHeader>();
		nctsHeaderPhase5Departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeaderPhase5Departure.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeaderPhase5Departure.Guarantees);

		var nctsHeaderPhase4Departure = Factory.New<NctsHeader>();
		nctsHeaderPhase4Departure.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		nctsHeaderPhase4Departure.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsGuaranteeCollection<NctsGuarantee>>(nctsHeaderPhase4Departure.Guarantees);
	}

	public void TestSetDefaultValues_CommunicationLanguage()
	{
		CombineAssertions(() =>
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = "ES";
			var header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is not in the list, so the Communication Language should be blank.", ZString.Empty, header.BH_CommunicationLanguage);
			GlbStaff.CurrentUser.GS_WorkingLanguage = SharedConstants.Languages.Dutch;
			header = Factory.New<NctsHeader>();
			AssertEquals("Staff Working Language is NL-NL, so the Communication Language should be defaulted to NL.", "NL", header.BH_CommunicationLanguage);
		});
	}

	public void TestBills()
	{
		var nctsHeader = GetNewBusinessObject(Factory);
		AssertType<NctsBillCollection<NctsBill>>(nctsHeader.Bills);
	}

	public void TestLookups()
	{
		var nctsHeader = GetNewBusinessObject(Factory);
		AssertType<NctsHeaderLookups>(nctsHeader.Lookups);
	}

	public void TestMovementHeader()
	{
		var nctsHeader = GetNewBusinessObject(Factory);
		AssertType<NctsDepartureMovementHeader>(nctsHeader.MovementHeader);
	}

	public void TestArrivalMovementHeader()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		AssertType<NctsArrivalMovementHeader>(nctsHeader.ArrivalMovementHeader);
	}
	
	public void TestAssignDeclarationGoodsItemNumbers()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		var bill1 = nctsHeader.Bills.AddNew();
		var goodsItem1 = bill1.GoodsItems.AddNew();
		var goodsItem2 = bill1.GoodsItems.AddNew();

		var bill2 = nctsHeader.Bills.AddNew();
		var goodsItem3 = bill2.GoodsItems.AddNew();

		goodsItem3.BY_DeclarationGoodsItemNumber = 2;

		nctsHeader.AssignDeclarationGoodsItemNumbers();
		CombineAssertions("AssignDeclarationGoodsItemNumbers() reassigning numbers", () =>
		{
			AssertEquals("Bill 1 GoodsItem 1, BY_DeclarationGoodsItemNumber", 3, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 1 GoodsItem 2, BY_DeclarationGoodsItemNumber", 4, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 2 GoodsItem 1, BY_DeclarationGoodsItemNumber", 2, goodsItem3.BY_DeclarationGoodsItemNumber);
		});

		goodsItem2.BY_DeclarationGoodsItemNumber = 0;
		nctsHeader.AssignDeclarationGoodsItemNumbers(reassignNumbers: true);
		CombineAssertions("AssignDeclarationGoodsItemNumbers() assigning only empty numbers", () =>
		{
			AssertEquals("Bill 1 GoodsItem 1, BY_DeclarationGoodsItemNumber", 1, goodsItem1.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 1 GoodsItem 2, BY_DeclarationGoodsItemNumber", 2, goodsItem2.BY_DeclarationGoodsItemNumber);
			AssertEquals("Bill 2 GoodsItem 1, BY_DeclarationGoodsItemNumber", 3, goodsItem3.BY_DeclarationGoodsItemNumber);
		});
	}

	public void TestGetGuaranteesNoLongerInDeclaration()
	{
		NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

		var org1 = Factory.NewWithValidTestData<OrgHeader>();
		var guaranteeHeader1 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader1.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader1.CPH_Number = "GUA1";
		guaranteeHeader1.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader1.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader1.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader1.CPH_Type = "TRA";
		guaranteeHeader1.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		guaranteeHeader1.CPH_Balance = 1000.0m;
		guaranteeHeader1.CPH_OH_PermitHolder = org1.PK;
		guaranteeHeader1.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		var guaranteeHeader2 = Factory.NewWithValidTestData<CusGuaranteeHeader>();
		guaranteeHeader2.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
		guaranteeHeader2.CPH_Number = "GUA2";
		guaranteeHeader2.CPH_StartDate = ZDate.Today.AddMonths(-1);
		guaranteeHeader2.CPH_EndDate = ZDate.Today.AddMonths(1);
		guaranteeHeader2.CPH_SystemCreateTimeUtc = ZDate.Today;
		guaranteeHeader2.CPH_Type = "TRA";
		guaranteeHeader2.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		guaranteeHeader2.CPH_Balance = 2000.0m;
		guaranteeHeader2.CPH_OH_PermitHolder = org1.PK;
		guaranteeHeader2.AddTransaction("OPENING2", "OPENING2", ZString.Empty, ZString.Empty, 2000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

		var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType("D");
		nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN123456789";

		var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
		guarantee.PW_BondNumber = "GUA1";
		guarantee.PW_BondAmount = 145.0m;
		guarantee.PW_CPH_Guarantee = guaranteeHeader1.PK;

		guaranteeHeader1.AddTransaction(nctsHeader.MovementHeader.BM_PaperlessInbondNum,
			"NCTS write-off " + nctsHeader.MovementHeader.BM_PaperlessInbondNum + " [" + nctsHeader.MovementReferenceNumber + "]",
			"001",
			ZString.Empty,
			guarantee.PW_BondAmount * -1,
			0,
			status: Customs.Business.PermitTransactionStatusList.Codes.Pending);

		guarantee.PW_BondNumber = "GUA2";
		guarantee.PW_CPH_Guarantee = guaranteeHeader2.PK;

		Factory.Save();

		var result = nctsHeader.GuaranteesNoLongerInDeclaration;

		CombineAssertions("GetGuaranteesNoLongerInDeclaration", () =>
		{
			AssertEquals("Number of removed guarantees", 1, result.Length);
			AssertEquals("CPH Number of removed guarantee", "GUA1", result.First().CPH_Number);
			AssertEquals("CPH Balance of removed guarantee", (ZDecimal)1000.0, result.First().CPH_Balance);
		});
	}

	public override void TestClone_Phase4() => Assert("NL doesn't support Phase4 => no need to test it.", condition: true);

	public void TestRetrieveArrivalGoodsItemPackage()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		var bill = nctsHeader.Bills.AddNew();
		bill.MovementDetail.B9_SeqNo = "1";
		var goodsItem = bill.ArrivalGoodsItems.AddNew();
		goodsItem.BY_DeclarationGoodsItemNumber = 1;
		var package = goodsItem.Packages.AddNew();
		package.B5_SequenceNumber = 1;

		CombineAssertions("RetrieveArrivalGoodsItemPackage", () =>
		{
			AssertEquals("Package should be retrieved correctly", package, nctsHeader.RetrieveArrivalGoodsItemPackage(1, 1, 1));
			AssertNull("Should return null if bill not found", nctsHeader.RetrieveArrivalGoodsItemPackage(2, 1, 1));
			AssertNull("Should return null if goods item not found", nctsHeader.RetrieveArrivalGoodsItemPackage(1, 2, 1));
			AssertNull("Should return null if package not found", nctsHeader.RetrieveArrivalGoodsItemPackage(1, 1, 2));
		});
	}

	public void TestCALCalculationMethod()
	{
		_ = AssertEntity<NctsHeader>()
			.HasProperty(x => x.CALCalculationMethod)
			.WithMaxLength(3)
			.WithList("Lookups.CalculationMethodList")
			.WithCaption("CAL");
	}

	public void TestCALCalculationMethod_DefaultValue()
	{
		var header = (NctsHeader)GetNewBusinessObject();
		AssertEquals("System Default DUT", CalculationMethodList.Codes.DUT, header.CALCalculationMethod);
	}

	public void TestApportionedAmountToGuaranteesLiabilityAmount_CalculationMethodDEF()
	{
		var calculationMethodRegistryCollection = new CalCalculationMethodRegistryCollection()
		{
			new CalCalculationMethodRegistry
			{
				CalculationMethodName = CalculationMethodList.Codes.DUT,
				CalculationMethodValue = 10m,
				CalculationMethodDefault = true
			},
			new CalCalculationMethodRegistry
			{
				CalculationMethodName = CalculationMethodList.Codes.WGT,
				CalculationMethodValue = 20m,
				CalculationMethodDefault = false
			},
			new CalCalculationMethodRegistry
			{
				CalculationMethodName = CalculationMethodList.Codes.DEF,
				CalculationMethodValue = 40m,
				CalculationMethodDefault = false
			}
		};

		using (NLCustomsRegistry.Instance.CalCalculationMethod.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, calculationMethodRegistryCollection))
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.CALCalculationMethod = CalculationMethodList.Codes.DEF;
			var nctsGuarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee1.PW_BondNumber = "1234";
			var nctsGuarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			nctsGuarantee2.PW_BondNumber = "2345";
			nctsHeader.ApportionedAmountToGuaranteesLiabilityAmount();

			AssertEquals("When Calculation Method is DEF, the Liability Amount is completed from Registry", 40M, nctsGuarantee1.PW_BondAmount);
			AssertEquals("When Calculation Method is DEF, the Liability Amount is completed from Registry", 40M, nctsGuarantee2.PW_BondAmount);
		}
	}

	public void TestDocumentSupporter()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertNotNull(nameof(NctsHeader.DocumentSupporter), nctsHeader.DocumentSupporter);
		AssertType<NctsHeaderDocumentSupporter>($"{nameof(NctsHeader.DocumentSupporter)} type", nctsHeader.DocumentSupporter);
	}

	[TestDate(2025, 03, 18, 14, 15, 00)]
	public void TestFallbackInformation_NoInvocation()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		AssertEquals(@"NOODPROCEDURE NCTS (GEGEVENS NIET IN HET SYSTEEM)
Datum/uur: 18-03-2025 14:15
Storingsnr.: 18032025 1415", nctsHeader.FallbackInformation);
	}

	[TestDate(2025, 03, 18, 14, 15, 00)]
	public void TestFallbackInformation_Invocation()
	{
		FallbackConfigurationTestHelper.GetFallbackConfigurationWithDVASetValue(new ZDateTime(2025, 03, 18, 14, 15, 00));
		var nctsHeader = GetNewBusinessObject(Factory);
		nctsHeader.MovementHeader.IsFallbackProcedure = true;
		AssertEquals(@"NOODPROCEDURE NCTS (GEGEVENS NIET IN HET SYSTEEM)
Datum/uur: 18-03-2025 14:15
Storingsnr.: Invocation reason", nctsHeader.FallbackInformation);
	}

	public void TestCusAuthorizationUsages()
	{
		var header = (NctsHeader)GetNewBusinessObject();
		AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, NctsHeader>>(header.CusAuthorizationUsages);
	}

	public static NctsHeader GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var header = factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		return header;
	}
}
