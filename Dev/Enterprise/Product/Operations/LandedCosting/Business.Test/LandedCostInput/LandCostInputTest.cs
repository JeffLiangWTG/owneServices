using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Business.ClusterKey.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandCostInput))]
	sealed class LandCostInputTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			AssertNotNull("Header", costInput.Header);
			AssertEquals("Type", typeof(LandedCostHeader), costInput.Header.GetType());
		}

		public void TestFixCostAmountWhenCurrencyChanges()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandCostInput lCInput = lCHeader.CostInputs.AddNew();
			lCInput.LI_CostAmount = 10.99m;
			lCInput.LI_RX_NKCostCurrency = "JPY";

			AssertEquals("Decimal Places are bound and if not do this, users would not be able to see 0.99 and distribution result would be different to what users see and expect", 11m, lCInput.LI_CostAmount);
		}

		public void TestDefaultExchangeRate()
		{
			DummyLandedCostHeader dummyHeader = Factory.New<DummyLandedCostHeader>();
			dummyHeader.DefaultExchangeRatesExposed = new Dictionary<ZString, ZDecimal>();
			dummyHeader.DefaultExchangeRatesExposed.Add(Core.Constants.CurrencyCodes.KoreaRepublicOf, 1.2345m);

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.LT_ParentID = dummyHeader.PK;
			lcHeader.LT_ParentTableCode = BusinessObjectFactory.GetTableCodeFromType(typeof(DummyLandedCostHeader));

			LandCostInput costInput = lcHeader.CostInputs.AddNew();
			costInput.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
			AssertEquals("ExRate defaulted", 1.2345m, costInput.LI_ServiceExRate);

			costInput.LI_RX_NKCostCurrency = Core.Constants.CurrencyCodes.Jordan;
			AssertEquals("No default ExRate defined", 0m, costInput.LI_ServiceExRate);
		}

		public void TestDefaultDescriptionOnChargeCodeSelected()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandCostInput lCInput = lCHeader.CostInputs.AddNew();

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = "ABC";
			chargeCode.AC_Desc = "AABBCC";

			lCInput.LI_AC_ChargeCode = chargeCode.PK;
			AssertEquals("Description", chargeCode.AC_Desc, lCInput.LI_ChargeDescription);

			lCInput.LI_ChargeDescription = "";
			lCInput.LI_AC_ChargeCode = ZGuid.Empty;
			chargeCode.AC_Desc = "A".PadRight(AccChargeCodeSchema.AC_Desc.MaxLength - 1, 'A');
			lCInput.LI_AC_ChargeCode = chargeCode.PK;
			AssertEquals("Description defaulted after substring", chargeCode.AC_Desc.SubstringSafe(0, LandCostInputSchema.LI_ChargeDescription.MaxLength), lCInput.LI_ChargeDescription);
		}

		public void TestGetParentWithRightTypeOfCommonJobComInvoiceHeader()
		{
			BusinessObject jobDeclaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			BusinessObject groupInvoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceGroupHeader>();
			groupInvoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = jobDeclaration.PK;
			groupInvoice[JobComInvoiceHeaderSchema.Constants.JZ_GroupInvoice] = true;

			BusinessObject invoice = (BusinessObject)Factory.New<Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JE] = jobDeclaration.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_JZ_GroupInvoiceFK] = groupInvoice.PK;
			invoice[JobComInvoiceHeaderSchema.Constants.JZ_GroupInvoice] = false;

			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			lCHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			LandCostInput lCInput = lCHeader.CostInputs.AddNew();
			lCInput.LI_ParentID = groupInvoice.PK;
			lCInput.LI_ParentTableCode = JobComInvoiceHeaderSchema.Constants.Prefix;

			AssertEquals("Parent with this information", groupInvoice, lCInput.Parent);

			lCInput.LI_ParentID = invoice.PK;
			AssertEquals("Parent with this information", invoice, lCInput.Parent);
		}

		public void TestCS00110976_ErrorOnCostAmountWhenDefaultingCost()
		{
			var ultimate1 = Factory.New<DummyIUltimateDistributee>();
			ultimate1.PKExposed = ZGuid.NewZGuid();
			ultimate1.ActualExposed = 10m;
			ultimate1.ActualWeightInKGExposed = 50m;
			ultimate1.CostInLocalCurrencyExposed = 1m;

			var distribute1 = Factory.New<DummyLandedCostDistributeTo>();
			distribute1.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1 };

			var defaultCharge = new DummyLandCostInput();
			defaultCharge.IsValidToImportExposed = true;
			defaultCharge.AmountToDistributeExposed = new Money(10m, GlbCompany.CurrentCompany.LocalCurrency);
			defaultCharge.ExchangeRateExposed = 1m;

			DummyLandCostChargeHolder dummyChargeHolder = new DummyLandCostChargeHolder();
			dummyChargeHolder.ChargesToImportForLandedCostingExposed = new IDefaultLandedCostInput[] { defaultCharge };

			DummyLandedCostHeader dummyHeader = Factory.New<DummyLandedCostHeader>();
			dummyHeader.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { distribute1 };
			dummyHeader.ExchangeRateHoldersExposed = Array.Empty<ILandedCostExchangeRateHolder>();
			dummyHeader.UltimateDistributeesExposed = new IUltimateDistributee[] { ultimate1 };
			dummyHeader.ChargeHoldersExposed = new ILandedCostChargeHolder[] { dummyChargeHolder };

			LandedCostHeader lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.DefaultFromHost(dummyHeader);
			lcHeader.SynchroniseAll();

			AssertEquals(1, lcHeader.CostInputs.Count);
			AssertEquals(false, lcHeader.CostInputs[0].HasErrors);
		}

		public void TestDistributionByDescription()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			LandCostInput lCInput = lCHeader.CostInputs.AddNew();
			lCInput.LI_DistributeCostBy = "";
			AssertEquals("CostDistributionDescription", "", lCInput.DistributionByDescription);

			lCInput.LI_DistributeCostBy = "XXX";
			AssertEquals("CostDistributionDescription", "", lCInput.DistributionByDescription);

			lCInput.LI_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;
			AssertEquals("CostDistributionDescription", CostDistributionMechanismList.Descriptions.Actual, lCInput.DistributionByDescription);

			DummyLandedCostHeader dummyHeader = Factory.New<DummyLandedCostHeader>();
			lCHeader.LT_ParentID = dummyHeader.PK;
			lCHeader.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			dummyHeader.IsAirExposed = true;
			lCInput.LI_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;
			AssertEquals("CostDistributionDescription", CostDistributionMechanismList.Descriptions.ActualWeight, lCInput.DistributionByDescription);

			dummyHeader.IsAirExposed = false;
			lCInput.LI_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;
			AssertEquals("CostDistributionDescription", CostDistributionMechanismList.Descriptions.ActualVolume, lCInput.DistributionByDescription);
		}

		public void TestGroupAmountInLocalCurrency()
		{
			LandedCostHeader lCHeader = Factory.New<LandedCostHeader>();
			AddNewCostInput(lCHeader, 1);
			AddNewCostInput(lCHeader, 2);
			AddNewCostInput(lCHeader, 3);
			AddNewCostInput(lCHeader, 4);
			AddNewCostInput(lCHeader, 5);
			AddNewCostInput(lCHeader, 6);
			AddNewCostInput(lCHeader, 7);

			AssertEquals("Group1 for 1", 1m, lCHeader.CostInputs[0].Group1AmountInLocalCurrency);
			AssertEquals("Group2 for 1", 0m, lCHeader.CostInputs[0].Group2AmountInLocalCurrency);
			AssertEquals("Group3 for 1", 0m, lCHeader.CostInputs[0].Group3AmountInLocalCurrency);
			AssertEquals("Group4 for 1", 0m, lCHeader.CostInputs[0].Group4AmountInLocalCurrency);
			AssertEquals("Group5 for 1", 0m, lCHeader.CostInputs[0].Group5AmountInLocalCurrency);
			AssertEquals("Group6 for 1", 0m, lCHeader.CostInputs[0].Group6AmountInLocalCurrency);
			AssertEquals("GroupMisc for 1", 0m, lCHeader.CostInputs[0].GroupMiscAmountInLocalCurrency);

			AssertEquals("Group2 for 1", 2m, lCHeader.CostInputs[1].Group2AmountInLocalCurrency);
			AssertEquals("Group3 for 1", 3m, lCHeader.CostInputs[2].Group3AmountInLocalCurrency);
			AssertEquals("Group4 for 1", 4m, lCHeader.CostInputs[3].Group4AmountInLocalCurrency);
			AssertEquals("Group5 for 1", 5m, lCHeader.CostInputs[4].Group5AmountInLocalCurrency);
			AssertEquals("Group6 for 1", 6m, lCHeader.CostInputs[5].Group6AmountInLocalCurrency);
			AssertEquals("GroupMisc for 1", 7m, lCHeader.CostInputs[6].GroupMiscAmountInLocalCurrency);
		}

		public void TestDefaultChargeGroupAndDistributeByFromConsignee()
		{
			DummyLandedCostHeader dummy = Factory.New<DummyLandedCostHeader>();
			header.LT_ParentID = dummy.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			AccChargeCode chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			AccChargeCode chargeCode2 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, chargeCode1.PK));

			OrgHeader consignee = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgLandedCostingPrefs preference1 = consignee.LandedCostingPreferences.AddNew();
			preference1.O9_LandedCostGroup = 1;
			preference1.O9_DistributeCostBy = CostDistributionMechanismList.Codes.Actual;
			OrgLandedCostingPrefCharges charge1 = preference1.Charges.AddNew();
			charge1.O0_AC_ChargeCode = chargeCode1.PK;

			OrgLandedCostingPrefs preference2 = consignee.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefCharges charge2 = preference2.Charges.AddNew();
			preference2.O9_LandedCostGroup = 2;
			preference2.O9_DistributeCostBy = CostDistributionMechanismList.Codes.ActualWeight;
			charge2.O0_AC_ChargeCode = chargeCode2.PK;

			dummy.ConsigneeExposed = consignee;
			costInput.LI_AC_ChargeCode = chargeCode1.PK;
			AssertEquals("Landed Cost Group", (ZByte)1, costInput.LI_LandedCostGroup);
			AssertEquals("Distribute BY", CostDistributionMechanismList.Codes.Actual, costInput.LI_DistributeCostBy);

			costInput.LI_AC_ChargeCode = chargeCode2.PK;
			AssertEquals("Landed Cost Group", (ZByte)2, costInput.LI_LandedCostGroup);
			AssertEquals("Distribute BY", CostDistributionMechanismList.Codes.ActualWeight, costInput.LI_DistributeCostBy);
		}

		public void TestDefaultFromRegistryIfConsigneeDoesntHavePreferences()
		{
			DummyLandedCostHeader dummy = Factory.New<DummyLandedCostHeader>();
			header.LT_ParentID = dummy.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			AccChargeCode chargeCode1 = Factory.LoadTop1<AccChargeCode>(new ZQuery());
			AccChargeCode chargeCode2 = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.PK, SQLComparisonOperator.NotEqual, chargeCode1.PK));

			OrgHeader consignee = Factory.New<OrgHeader>();
			dummy.ConsigneeExposed = consignee;

			AssertEquals("Consignee doesnt have any preference", 0, consignee.LandedCostingPreferences.Count);
			LandedCostingGroupCollection collection = new LandedCostingGroupCollection();
			LandedCostingGroup landedCostingGroup = collection.AddNew();
			landedCostingGroup.GroupID = 1;
			landedCostingGroup.GroupName = "ONE";
			landedCostingGroup.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			LandedCostingGroup landedCostingGroup2 = collection.AddNew();
			landedCostingGroup2.GroupID = 2;
			landedCostingGroup2.GroupName = "TWO";
			landedCostingGroup2.CostDistributionCode = landedCostingGroup.CostDistributionList[0].Code;

			ChargeGroupAndChargeCode registryCharge1 = landedCostingGroup.Charges.AddNew();
			registryCharge1.ChargeCodePK = chargeCode1.PK;

			ChargeGroupAndChargeCode registryCharge2 = landedCostingGroup2.Charges.AddNew();
			registryCharge2.ChargeCodePK = chargeCode2.PK;

			FreightDataRegistry.Instance.LandedCostingPreferences.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);

			costInput.LI_AC_ChargeCode = chargeCode1.PK;
			AssertEquals("Landed Cost Group", (ZByte)1, costInput.LI_LandedCostGroup);

			costInput.LI_AC_ChargeCode = chargeCode2.PK;
			AssertEquals("Landed Cost Group", (ZByte)2, costInput.LI_LandedCostGroup);
		}

		public void TestLinkedObjectUniqueCode()
		{
			DummyLandedCostHeader dummy = Factory.New<DummyLandedCostHeader>();
			header.LT_ParentID = dummy.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			DummyLandedCostDistributeTo dummyDistribute = Factory.New<DummyLandedCostDistributeTo>();
			dummy.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { dummyDistribute };
			dummyDistribute.UniqueCodeExposed = "TESTCODE";

			costInput.LinkedObjectUniqueCode = dummyDistribute.UniqueCodeExposed + ":";
			AssertEquals("Unique Code", dummyDistribute.UniqueCodeExposed + ":", costInput.LinkedObjectUniqueCode);
		}

		public void TestParent()
		{
			DummyLandedCostHeader dummy = Factory.New<DummyLandedCostHeader>();
			header.LT_ParentID = dummy.PK;
			header.LT_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			DummyLandedCostDistributeTo dummyDistribute = Factory.New<DummyLandedCostDistributeTo>();
			dummy.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { dummyDistribute };

			costInput.LI_ParentID = dummyDistribute.PK;
			costInput.LI_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertEquals("Parent", dummyDistribute, costInput.Parent);

			DummyLandedCostDistributeTo dummyDistribute2 = Factory.New<DummyLandedCostDistributeTo>();
			dummy.CandidatesToDistributeCostToExposed = new ILandedCostDistributeTo[] { dummyDistribute, dummyDistribute2 };
			costInput.LI_ParentID = dummyDistribute2.PK;
			AssertEquals("Parent is refreshed", dummyDistribute2, costInput.Parent);
		}

		public void TestLCGroup()
		{
			costInput.LCGroupString = "1";
			AssertEquals("LCGroup", (byte)1, costInput.LI_LandedCostGroup);

			costInput.LCGroupString = "A";
			AssertEquals("LCGroup", (byte)0, costInput.LI_LandedCostGroup);
		}

		public void TestSetLI_RXToLocalCurrencySetsExRateTo1()
		{
			TestHelper helper = new TestHelper(Factory);
			LandedCostHeader lCHeader = helper.GetLCHeaderWithDistributionObjectsPluggedIn();
			DummyExchangeRateHolder holder1 = Factory.New<DummyExchangeRateHolder>();
			holder1.LandedCostExchangeRateExposed = 0.76543m;
			holder1.CurrencyCodeExposed = "USD";
			holder1.ReferenceNumberExposed = "1";

			DummyExchangeRateHolder holder2 = Factory.New<DummyExchangeRateHolder>();
			holder2.LandedCostExchangeRateExposed = 88.12345m;
			holder2.CurrencyCodeExposed = "JPY";
			holder2.ReferenceNumberExposed = "2";

			helper.DummyHeader.ExchangeRateHoldersExposed = new DummyExchangeRateHolder[] { holder1, holder2 };
			costInput2 = lCHeader.CostInputs.AddNew();

			costInput2.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("Ex rate is set", 1m, costInput2.LI_ServiceExRate);
			costInput2.LI_RX_NKCostCurrency = "USD";
			AssertEquals("USD Rate should be 0.76543", 0.76543m, costInput2.LI_ServiceExRate);
			costInput2.LI_RX_NKCostCurrency = "JPY";
			AssertEquals("JPY Rate should be 88.12345", 88.12345m, costInput2.LI_ServiceExRate);
			costInput2.LI_RX_NKCostCurrency = "NZD";
			AssertEquals("NZD Rate should be 0", 0m, costInput2.LI_ServiceExRate);
		}

		public void TestDefaultFromHost()
		{
			DummyLandCostInput charge = new DummyLandCostInput();
			charge.AmountToDistributeExposed = new Money(1000, GlbCompany.CurrentCompany.LocalCurrency);
			charge.ChargeDescriptionExposed = "TestChargeDescription";
			charge.ExchangeRateExposed = 1m;
			charge.FKToChargeCodeExposed = Factory.LoadTop1(typeof(AccChargeCode), new ZQuery()).PK;

			IDefaultLandedCostInput[] charges = new IDefaultLandedCostInput[] { charge };
			DummyLandCostChargeHolder chargeHolder = new DummyLandCostChargeHolder();
			chargeHolder.ChargesToImportForLandedCostingExposed = charges;

			costInput.DefaultFromHost(chargeHolder, charge);
			AssertEquals("Amount", charge.AmountToDistribute.Amount, costInput.LI_CostAmount);
			AssertEquals("Currency", charge.AmountToDistribute.Currency.Code, costInput.LI_RX_NKCostCurrency);
			AssertEquals("Charge Description", charge.ChargeDescription, costInput.LI_ChargeDescription);
			AssertEquals("Exchange rate", charge.ExchangeRate, costInput.LI_ServiceExRate);
			AssertEquals("AccChargeCode", charge.FKToChargeCode, costInput.LI_AC_ChargeCode);
		}

		public void TestCostAmountInLocalCurrency()
		{
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;

			GlbCompany company2 = Factory.NewWithValidTestData<GlbCompany>();
			company2.GC_IsReciprocal = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				header.LT_ParentID = declaration.PK;
				header.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
				header.LT_GC = company2.PK;

				costInput.LI_CostAmount = 1000m;
				costInput.LI_RX_NKCostCurrency = "USD";
				costInput.LI_ServiceExRate = 0.7890m;
				Assert("IsReciprocalRates", !((Integration.Customs.IBaseJobDeclaration)declaration).IsReciprocalRates);
				AssertEquals("Cost Amount in local currency", 1267.43m, costInput.CostAmountInLocalCurrency.Round(2));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				BusinessObject declaration = (BusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
				header.LT_ParentID = declaration.PK;

				Assert("IsReciprocalRates", ((Integration.Customs.IBaseJobDeclaration)declaration).IsReciprocalRates);
				AssertEquals("Cost Amount in local currency", 789m, costInput.CostAmountInLocalCurrency.Round(2));
			}
		}

		LandedCostHeader header;
		LandCostInput costInput;
		LandCostInput costInput2;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<LandedCostHeader>();
			header.LT_ParentID = ZGuid.NewZGuid();
			header.LT_ParentTableCode = JobOrderHeaderSchema.Constants.Prefix;

			costInput = header.CostInputs.AddNew();
			costInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			costInput.LI_ParentID = ZGuid.NewZGuid();
			costInput.LI_ParentTableCode = JobOrderHeaderSchema.Constants.Prefix;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => costInput;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => costInput;

		protected override BusinessObject GetNewBusinessObject() => costInput;

		void AddNewCostInput(LandedCostHeader lCHeader, ZByte groupID)
		{
			LandCostInput lCInput = lCHeader.CostInputs.AddNew();
			lCInput.LI_LandedCostGroup = groupID;
			lCInput.LI_CostAmount = Convert.ToDecimal(groupID);
			lCInput.LI_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}
	}

	[TestedType(typeof(LandCostInput))]
	sealed class LandCostInputClusterKeyWorkerMandatoryTest : ClusterKeyWorkerMandatoryTest
	{
		#region Overrides of ClusterKeyEntityTest

		protected override IClusterKeyEntity NewClusterKeyEntity()
		{
			var parent = (LandedCostHeader)NewParentObject();

			var lcInput = parent.CostInputs.AddNew();
			lcInput.LI_ParentID = ZGuid.NewZGuid();
			lcInput.LI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
			return lcInput;
		}

		#endregion

		#region Overrides of ClusterKeyWorkerMandatoryTest

		protected override IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren() => null;

		protected override EnterpriseBusinessObject NewParentObject()
		{
			var jobDec = (EnterpriseBusinessObject)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			var lcHeader = Factory.New<LandedCostHeader>();
			lcHeader.LT_ParentID = jobDec.PK;
			lcHeader.LT_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			return lcHeader;
		}

		#endregion
	}
}
