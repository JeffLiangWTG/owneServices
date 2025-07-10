using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(JobCharge))]
	public abstract class JobChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_JR_AL_APLine()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Accrual;
			line.AL_JH = ZGuid.NewZGuid();
			line.AL_OH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_OH_CostAccount = ZGuid.NewZGuid();

			charge.JR_AL_APLine = line.PK;
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {charge.JR_OH_CostAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge and line have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			charge.JR_AL_APLine = line.PK;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets line with the same org as before", collectedInfo, infoNotCollected);

			using (line.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
			{
				line.AL_ReverseDate = ZDateTime.Now;
				charge.JR_AL_APLine = ZGuid.Empty;
				line.AL_ReverseDate = ZDateTime.Empty;
			}
			charge.JR_OH_CostAccount = line.AL_OH;
			charge.JR_AL_APLine = line.PK;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets line with the same org as on line", collectedInfo, infoNotCollected);
		}

		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_JR_AL_ARLine()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_JH = ZGuid.NewZGuid();
			line.AL_OH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_OH_SellAccount = ZGuid.NewZGuid();

			charge.JR_AL_ARLine = line.PK;
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {charge.JR_OH_SellAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge and line have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			charge.JR_AL_ARLine = line.PK;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets line with the same org as before", collectedInfo, infoNotCollected);

			using (line.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
			{
				line.AL_ReverseDate = ZDateTime.Now;
				charge.JR_AL_ARLine = ZGuid.Empty;
				line.AL_ReverseDate = ZDateTime.Empty;
			}
			charge.JR_OH_SellAccount = line.AL_OH;
			charge.JR_AL_ARLine = line.PK;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets line with the same org as on line", collectedInfo, infoNotCollected);
		}

		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_JR_OH_CostAccount()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Accrual;
			line.AL_JH = ZGuid.NewZGuid();
			line.AL_OH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_AL_APLine = line.PK;

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			charge.JR_OH_CostAccount = ZGuid.NewZGuid();
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Cost Organization: {charge.JR_OH_CostAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge and line have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			charge.JR_OH_CostAccount = charge.JR_OH_CostAccount;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets the same org as before", collectedInfo, infoNotCollected);

			charge.JR_OH_CostAccount = line.AL_OH;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets the same org as on line", collectedInfo, infoNotCollected);
		}

		public void TestWIPACROrganizationNotEqualToRelatedChargeForNewLine_JR_OH_SellAccount()
		{
			var line = Factory.New<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.WIP;
			line.AL_JH = ZGuid.NewZGuid();
			line.AL_OH = ZGuid.NewZGuid();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = line.AL_JH;
			charge.JR_AL_ARLine = line.PK;

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			charge.JR_OH_SellAccount = ZGuid.NewZGuid();
			var expectedInfo =
$@"
WIPACROrganizationNotEqualToRelatedChargeForNewLine:
RelatedCharge PK: {charge.PK}, Charge Sell Organization: {charge.JR_OH_SellAccount}, Line Organization: {line.AL_OH}, Old Organization: {ZGuid.Empty}
   at Enterprise.MasterFiles.Business.Accounting.CriticalValidation.CriticalValidationHelpers";

			var collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge and line have different orgs", collectedInfo, expectedInfo);

			CriticalValidationInfoCollectorService.GetService(Factory).ClearServiceCache();
			var infoNotCollected = "WIPACROrganizationNotEqualToRelatedChargeForNewLine: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.";
			charge.JR_OH_SellAccount = charge.JR_OH_SellAccount;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets the same org as before", collectedInfo, infoNotCollected);

			charge.JR_OH_SellAccount = line.AL_OH;
			collectedInfo = CriticalValidationInfoCollectorService.GetService(Factory).GetInfo(line.PK, CriticalValidationInfoCollectorServiceKeyType.WIPACROrganizationNotEqualToRelatedChargeForNewLine);
			AssertContainsInOrder("charge sets the same org as on line", collectedInfo, infoNotCollected);
		}

		public void TestIsInDatabaseAndReadyForFinancialClosure()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_Desc = "Original Description";
			job.JH_Status = JobHeaderStatus.JobReadyForFinancialClosure.Code;
			Factory.Save();
			var cacheValue = Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed;

			Assert("Pre condition", charge.IsInDatabase);
			Assert("Pre condition", !charge.IsDeleted);
			AssertNotNull("Pre condition", charge.Job);

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = false, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity should be true", charge.IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity);
			}

			using (new DisposableAction(() => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = true, () => Env.Security.ModifyChargesforFinancialClosureJob.IsAllowed = cacheValue))
			{
				Assert("IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity should be false", !charge.IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity);
			}
		}

		public void TestHasChargeChanged()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var chargeInNewFactory = newFactory.Load<JobCharge>(charge.PK);

			AssertEquals(DataRowState.Unchanged, ((IBusinessObjectInternals)chargeInNewFactory).Row.RowState);
			Assert(!charge.HasChargeChanged());

			var originalDisplaySequence = chargeInNewFactory.JR_DisplaySequence;
			chargeInNewFactory.JR_DisplaySequence = 5;
			AssertEquals(DataRowState.Modified, ((IBusinessObjectInternals)chargeInNewFactory).Row.RowState);
			Assert("Row modified, but only display sequence changed.", !chargeInNewFactory.HasChargeChanged());

			var originalDescription = chargeInNewFactory.JR_Desc;
			chargeInNewFactory.JR_Desc = "test description";
			Assert("Row modified, display sequence and description changed.", chargeInNewFactory.HasChargeChanged());

			chargeInNewFactory.JR_DisplaySequence = originalDisplaySequence;
			Assert("Row modified, display sequence reset to original value and description changed.", chargeInNewFactory.HasChargeChanged());

			chargeInNewFactory.JR_Desc = originalDescription;
			Assert("Row modified, display sequence and description reset to original values.", chargeInNewFactory.HasChargeChanged());
		}

		public void TestCostPlacesOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupply);

				charge.JR_CostPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_CostPlaceOfSupplyType);
				AssertEquals("DL", charge.JR_CostPlaceOfSupply);

				var location = charge.CostPlaceOfSupplyLocation;
				AssertNotNull(location);
				AssertEquals("DL", location.Code);
				AssertEquals("DL", location.State.RW_Code);
				Assert(!location.IsLocationRule());

				charge.JR_CostPlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, charge.JR_CostPlaceOfSupplyType);
				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, charge.JR_CostPlaceOfSupply);

				location = charge.CostPlaceOfSupplyLocation;
				AssertNotNull(location);
				AssertEquals("ALX", location.Code);
				AssertNull(location.State);
				Assert(location.IsLocationRule());

				charge.JR_CostPlaceOfSupply = "";
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupply);

				location = charge.CostPlaceOfSupplyLocation;
				AssertNull(location);
			}
		}

		public void TestCostPlacesOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupply);

				charge.JR_CostPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_CostPlaceOfSupplyType);
				AssertEquals("DL", charge.JR_CostPlaceOfSupply);

				charge.JR_CostPlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, charge.JR_CostPlaceOfSupplyType);
				AssertEquals("No changes to Place when we change Type", "DL", charge.JR_CostPlaceOfSupply);

				charge.JR_CostPlaceOfSupplyType = "";
				AssertEquals(string.Empty, charge.JR_CostPlaceOfSupplyType);
				AssertEquals("No changes to Place when we change Type", "DL", charge.JR_CostPlaceOfSupply);
			}
		}

		public void TestSellPlacesOfSupply()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupply);

				charge.JR_SellPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_SellPlaceOfSupplyType);
				AssertEquals("DL", charge.JR_SellPlaceOfSupply);

				var location = charge.SellPlaceOfSupplyLocation;
				AssertNotNull(location);
				AssertEquals("DL", location.Code);
				AssertEquals("DL", location.State.RW_Code);
				Assert(!location.IsLocationRule());

				charge.JR_SellPlaceOfSupply = PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry;
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, charge.JR_SellPlaceOfSupplyType);
				AssertEquals(PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry, charge.JR_SellPlaceOfSupply);

				location = charge.SellPlaceOfSupplyLocation;
				AssertNotNull(location);
				AssertEquals("ALX", location.Code);
				AssertNull(location.State);
				Assert(location.IsLocationRule());

				charge.JR_SellPlaceOfSupply = "";
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupply);

				location = charge.CostPlaceOfSupplyLocation;
				AssertNull(location);
			}
		}

		public void TestSellPlacesOfSupplyType()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var charge = Factory.NewWithValidTestData<JobCharge>();
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupplyType);
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupply);

				charge.JR_SellPlaceOfSupply = "DL";
				AssertEquals(PlaceOfSupplyTypes.State.Code, charge.JR_SellPlaceOfSupplyType);
				AssertEquals("DL", charge.JR_SellPlaceOfSupply);

				charge.JR_SellPlaceOfSupplyType = PlaceOfSupplyTypes.PredefinedRule.Code;
				AssertEquals(PlaceOfSupplyTypes.PredefinedRule.Code, charge.JR_SellPlaceOfSupplyType);
				AssertEquals("No changes to Place when we change Type", "DL", charge.JR_SellPlaceOfSupply);

				charge.JR_SellPlaceOfSupplyType = "";
				AssertEquals(string.Empty, charge.JR_SellPlaceOfSupplyType);
				AssertEquals("No changes to Place when we change Type", "DL", charge.JR_SellPlaceOfSupply);
			}
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesJobCharge()
		{
			var charge = Factory.New<JobCharge>();

			var localList = new List<string>
			{
				nameof(charge.JR_AgentDeclaredSellAmtLocal),
				nameof(charge.JR_AgentDeclaredCostAmtLocal),
				nameof(charge.JR_Calc_LocalSellTaxAmt),
				nameof(charge.JR_LocalSellAmt),
				nameof(charge.JR_LocalCostAmt)
			};

			var osSellList = new List<string>
			{
				nameof(charge.JR_OSSellAmt),
				nameof(charge.JR_AgentDeclaredSellAmt),
				nameof(charge.JR_EstimatedRevenue)
			};

			var osCostList = new List<string>
			{
				nameof(charge.JR_OSCostAmt),
				nameof(charge.JR_DeclaredOSCostAmt),
				nameof(charge.JR_AgentDeclaredCostAmt),
				nameof(charge.JR_EstimatedCost)
			};

			var exList = new List<string>
			{
				nameof(charge.JR_OSCostExRate),
				nameof(charge.JR_OSSellExRate)
			};

			var percentList = new List<string>
			{
				nameof(charge.JR_LineCFX),
				nameof(charge.JR_MarginPercentage)
			};

			var unitList = new List<string>
			{
				nameof(charge.JR_ProductQuantity)
			};

			var tester = new DecimalPlacesAttributeTester(charge);
			tester.CheckLocalCurrency(localList, nameof(charge.LocalCurrencyDecimals));
			tester.CheckNonLocalCurrency(osSellList, nameof(charge.OSSellCurrencyDecimals), nameof(charge.JR_RX_NKSellCurrency), charge);
			tester.CheckNonLocalCurrency(osCostList, nameof(charge.OSCostCurrencyDecimals), nameof(charge.JR_RX_NKCostCurrency), charge);
			tester.CheckExchangeRate(exList, nameof(charge.ExchangeRateDecimalPlaces));
			tester.CheckConstant(percentList, nameof(charge.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
			tester.CheckConstant(unitList, nameof(charge.UnitDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForIndividualUnits);
		}

		public void TestErrorIsReportedWhenDeletedChargeRemainsInBusinessObjectCollection()
		{
			AssertErrorIsReportedWhenDeletedChargeRemainsInBusinessObjectCollection(false);
		}

		public void TestErrorIsReportedWhenDeletedChargeRemainsInBusinessObjectCollection_MasterDeleted()
		{
			AssertErrorIsReportedWhenDeletedChargeRemainsInBusinessObjectCollection(true);
		}

		void AssertErrorIsReportedWhenDeletedChargeRemainsInBusinessObjectCollection(bool mastersAreDeleted)
		{
			var charge = Factory.New<JobCharge>();

			var collection1 = new MyDummyBizObjCollectionWithInternalOverrides(Factory);
			collection1.MastersAreDeleted = mastersAreDeleted;
			collection1.Add(charge);
			AssertEquals(1, collection1.Count);

			var collection2 = new JobChargeCollection(Factory);
			collection2.Add(charge);
			AssertEquals(1, collection2.Count);

			AssertEquals(2, ((IBusinessObjectInternals)charge).ParentCollections.Length);

			var expectedKey = "Business_Object_Collections_With_Deleted_Charge_3";
			var expectedMessage = string.Format(
@"Deleted Charge:
	PK = {0}
	Type = Charge
	Types around row = Charge
	Factory Instance = {1}
	IsDeleted = True
	IsInDb = False
	IsSavedByFactory = True
	HasChanges = True
	HasErrors = False
	IsDeleting = False

Business Contexts = Factory Level : (DeletingJobCharge)

Original property values for deleted bizo are not accessible

ChargeCollectionRemoveMethodInfo: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.
Before Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.MasterFiles.Business.Testing.JobChargeTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject
	Factory Instance = {1}
	Hash Code = {2}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
BusinessObjectCollection Info:
	Collection Type = Enterprise.MasterFiles.Business.JobChargeCollection
	Element Type = Enterprise.MasterFiles.Business.JobCharge
	Factory Instance = {1}
	Hash Code = {3}
	Contains bizo = False
	Has Changes = False
	Number of elements = 0
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True
After Delete:
Parent collections:
BusinessObjectCollection Info:
	Collection Type = Enterprise.MasterFiles.Business.Testing.JobChargeTest+MyDummyBizObjCollectionWithInternalOverrides
	Element Type = CargoWise.EntityFramework.Testing.DummyBusinessObject
	Factory Instance = {1}
	Hash Code = {2}
	Contains bizo = True
	Has Changes = True
	Number of elements = 1
	Is List Changed Suspended = False
	Has Changes From Delete = False
	Masters Are Deleted = False
	Masters Are In Database  = True",
				charge.PK, Factory._Instance, collection1.GetHashCode(), collection2.GetHashCode());

			ErrorReporter.Clear();

			charge.Delete();

			Assert("Postcondition: Collection1 contains deleted charge", collection1.Contains(charge));
			Assert("Postcondition: Collection2 does not contain deleted charge", !collection2.Contains(charge));

			if (mastersAreDeleted)
			{
				AssertEquals("TotalErrorCount", 0, ErrorReporter.TotalErrorCount);
			}
			else
			{
				AssertEquals("TotalErrorCount", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Error must be reported", expectedKey, ErrorReporter.LastKeyReported);
				AssertMultilineASCIIEquals(expectedMessage, ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		class MyDummyBizObjCollectionWithInternalOverrides : DummyBusinessObjectCollection, IBusinessObjectCollectionInternals
		{
			public MyDummyBizObjCollectionWithInternalOverrides(BusinessObjectFactory factory)
			: base(factory) { }

			bool IBusinessObjectCollectionInternals.MastersAreDeleted
			{
				get { return MastersAreDeleted; }
			}

			public override void Remove(BusinessObject elementToRemove)
			{
			}

			public bool MastersAreDeleted { get; set; }
		}

		public void TestSetExchangeRateShouldNotReportDevErrorWhenAlreadyInDatabase()
		{
			ErrorReporter.Clear();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			//Incorrect setup - can't be rates non-equal to 1 for local currency
			((IDbConnected)Factory).Connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, @"
UPDATE dbo.JobCharge
SET
	JR_OSCostExRate = 0.66,
	JR_OSSellExRate = 0.77,
	JR_SystemLastEditTimeUtc = GETUTCDATE(),
	JR_SystemLastEditUser = '~BP'
WHERE
	JR_PK = '{0}'", charge.PK));
			charge.Reload();

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKCostCurrency);
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, charge.JR_RX_NKSellCurrency);
			Assert(!charge.JR_OSCostExRateInfo.HasChanges);
			Assert(charge.IsInDatabase);

			charge.JR_OSCostExRate = 0.66m;
			charge.JR_OSSellExRate = 0.77m;

			Assert("charge cost exchange rate has no change", !charge.JR_OSCostExRateInfo.HasChanges);
			AssertEquals("the error should not be reported because charge already in DB and no change in cost or sell exchange rates", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		public void TestJobChargeShouldNotLinkToInactiveJob()
		{
			ErrorReporter.Clear();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			job.MarkAsInactive();
			charge.JR_JH = job.PK;

			var expectedKey = "ChargeIsLinkedToInactiveJob";
			AssertEquals("ErrorReportedCount should be 1", 1, ErrorReporter.TotalErrorCount);
			AssertEquals("LastMessageReported", expectedKey, ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}

		public void TestDeletingChargeWithBusinessContextReportDeletingCharges()
		{
			ErrorReporter.Clear();
			var charge1 = Factory.New<JobCharge>();
			charge1.SetContext(BusinessContext.ReportDeletingCharges);
			charge1.Delete();
			AssertContains("when we do not expect it to be deleted", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			var charge2 = Factory.New<JobCharge>();
			charge2.SetContext(BusinessContext.ReportDeletingCharges);
			charge2.BeforeSuccessfulDeleting += (object sender, EventArgs args) => (sender as JobCharge)?.RemoveContext(BusinessContext.ReportDeletingCharges);
			charge2.Delete();
			AssertContains("when we do not expect it to be deleted", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestDeletingChargeWithBusinessContextReportDeletingChargesByDataRefresh()
		{
			var charge1 = Factory.NewWithValidTestData<JobCharge>();
			charge1.SetContext(BusinessContext.ReportDeletingCharges);
			Factory.Save();

			var newFactoryForCharge1 = new BusinessObjectFactory();
			var charge1InNewFactory = newFactoryForCharge1.Load<JobCharge>(charge1.PK);
			ErrorReporter.Clear();
			charge1InNewFactory.Delete();
			newFactoryForCharge1.Save();

			AssertContains("when we do not expect it to be deleted", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.SetContext(BusinessContext.ReportDeletingCharges);
			charge2.BeforeSuccessfulDeleting += (object sender, EventArgs args) => (sender as JobCharge)?.RemoveContext(BusinessContext.ReportDeletingCharges);
			Factory.Save();

			var newFactoryForCharge2 = new BusinessObjectFactory();
			var charge2InNewFactory = newFactoryForCharge2.Load<JobCharge>(charge2.PK);
			ErrorReporter.Clear();
			charge2InNewFactory.Delete();
			newFactoryForCharge2.Save();

			AssertEquals("Shouldn't report developer error since the business context is removed", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestIsRevenuePostedWithJobRevenueJournal()
		{
			var charge = Factory.New<JobCharge>();
			var line = Factory.New<AccTransactionLines>();
			var header = Factory.New<AccTransactionHeader>();

			charge.JR_AL_ARLine = line.PK;
			line.AL_AH = header.PK;

			header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			AssertEquals(true, charge.IsRevenuePostedWithManualJobRevenueJournal);
			AssertEquals(false, charge.IsRevenuePostedWithAutoJobRevenueJournal);

			header.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.AutoJobRevenueJournal;
			AssertEquals(false, charge.IsRevenuePostedWithManualJobRevenueJournal);
			AssertEquals(true, charge.IsRevenuePostedWithAutoJobRevenueJournal);
		}

		public void TestChangesToJobChargeDescriptionDoNotCreateAddOrEdtEventForJobHeader()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_Desc = "Original Description";
			Factory.Save();

			var filter = new ZQuery(StmALogSchema.SL_Parent, job.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystemCode);
			var logs = Factory.Load<StmALog>(filter);

			AssertEquals("There should be only one ADD event for JobHeader", 1, logs.Length);
			AssertEquals("ADD event is added by system for the creation of JobHeader, it is not for charge description change.", ZString.Empty, logs[0].SL_Reference);

			charge.JR_Desc = "Original Description Updated";
			Factory.Save();

			filter.Clear();
			filter.AddToFilter(StmALogSchema.SL_Parent, job.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			logs = Factory.Load<StmALog>(filter);

			AssertEquals("There should be no EDT event created for JobHeader", 0, logs.Length);
		}

		public void TestAnyChargeChangeAddsJobEditEvent()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			Factory.Save();

			var expectedNumberOfEvents = 1;
			var filter = new ZQuery(StmALogSchema.SL_Parent, job.PK);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.BillingJobEditCode);
			filter.AddToFilter(StmALogSchema.SL_Reference, string.Empty);
			var logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should have a Log for job charge adding", expectedNumberOfEvents, logs.Length);

			charge.JR_Desc = "ss";
			job.JH_JobNum = "33";
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = job.PK;
			Factory.Save();
			logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should have a Log for any job charge change. Log should be add once per transaction despite amount of changed objects.", ++expectedNumberOfEvents, logs.Length);

			charge.Delete();
			Factory.Save();
			logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should have a Log for job charge deleting", ++expectedNumberOfEvents, logs.Length);

			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_JH = job.PK;
			charge3.Delete();
			Factory.Save();
			logs = Factory.Load<StmALog>(filter);
			AssertEquals("Should not have a Log for job charge deleting if charge was just created and not saved yet.", expectedNumberOfEvents, logs.Length);
		}

		public void TestIsApportioned()
		{
			Assert(!Charge.JR_IsApportioned);
			Charge.JR_E6 = ZGuid.NewZGuid();
			Assert(Charge.JR_IsApportioned);
		}

		#region AgentDeclaredSellAndCost

		public void TestAgentDeclaredSellAndCost()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Charge.JR_JH = jobHeader.PK;
			Charge.JR_RX_NKSellCurrency = USD.RX_Code;
			var jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
			AssertNotNull(jobExRate);
			jobExRate.SetBuyRate_ForTestOnly(0.5m);

			AssertEquals(0.5m, Charge.JR_OSSellExRate);
			Charge.JR_AgentDeclaredSellAmt = 400m;

			Charge.JR_RX_NKCostCurrency = GBP.RX_Code;
			jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == GBP.RX_Code);
			AssertNotNull(jobExRate);
			jobExRate.SetBuyRate_ForTestOnly(0.8m);

			AssertEquals(0.8m, Charge.JR_OSCostExRate);
			Charge.JR_AgentDeclaredCostAmt = 300m;

			AssertEquals("Local Agent Declared Sell Amount correct", 800m, Charge.JR_AgentDeclaredSellAmtLocal);
			AssertEquals("Local Agent Declared Cost Amount correct", 375m, Charge.JR_AgentDeclaredCostAmtLocal);
		}

		public void TestAgentDeclaredCostAmtLocal()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Charge.JR_JH = jobHeader.PK;
			Charge.JR_RX_NKCostCurrency = GBP.RX_Code;
			var jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == GBP.RX_Code);
			AssertNotNull(jobExRate);
			jobExRate.SetBuyRate_ForTestOnly(0.4m);

			AssertEquals(0.4m, Charge.JR_OSCostExRate);
			Charge.JR_AgentDeclaredCostAmt = 100m;
			AssertEquals("Local Agent Declared Cost Amount incorrect", 250m, Charge.JR_AgentDeclaredCostAmtLocal);

			Charge.JR_AgentDeclaredCostAmtLocal = 3000m;
			AssertEquals("Local Agent Declared Cost Amount incorrect", 3000m, Charge.JR_AgentDeclaredCostAmtLocal);
			AssertEquals("Overseas Agent Declared Cost Amount incorrect", 1200m, Charge.JR_AgentDeclaredCostAmt);

			Charge.JR_RX_NKCostCurrency = string.Empty;
			AssertEquals("Overseas Agent Cost Amount should become equal to Declared Cost Amount for local/empty currency", 1200m, Charge.JR_AgentDeclaredCostAmtLocal);

			Charge.JR_AgentDeclaredCostAmtLocal = 100m;
			AssertEquals("Should be the same as local amount if foreign currency unspecified", 100m, Charge.JR_AgentDeclaredCostAmt);
		}

		public void TestAgentDeclaredSellAmtLocal()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Charge.JR_JH = jobHeader.PK;
			Charge.JR_RX_NKSellCurrency = USD.RX_Code;
			var jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
			AssertNotNull(jobExRate);
			jobExRate.SetBuyRate_ForTestOnly(0.75m);

			AssertEquals(0.75m, Charge.JR_OSSellExRate);
			Charge.JR_AgentDeclaredSellAmt = 150m;
			AssertEquals("Local Agent Declared Sell Amount incorrect", 200m, Charge.JR_AgentDeclaredSellAmtLocal);

			Charge.JR_AgentDeclaredSellAmtLocal = 100m;
			AssertEquals("Local Agent Declared Sell Amount incorrect", 100m, Charge.JR_AgentDeclaredSellAmtLocal);
			AssertEquals("Overseas Agent Declared Sell Amount incorrect", 75m, Charge.JR_AgentDeclaredSellAmt);

			Charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
			AssertEquals("Local Amount should become equal to Declared Cost Amount for local/empty currency", 75m, Charge.JR_AgentDeclaredSellAmtLocal);

			Charge.JR_AgentDeclaredSellAmtLocal = 900m;
			AssertEquals("Should be the same as local amount if foreign currency unspecified", 900m, Charge.JR_AgentDeclaredSellAmt);
		}

		RefCurrency GBP
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedKingdom); }
		}

		RefCurrency USD
		{
			get { return RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates); }
		}

		#endregion

		public void TestJR_Calc_LocalSellTaxAmt_UseTaxDate()
		{
			Charge.JR_LocalSellAmt = 100m;
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(10, 1, endTaxDate: ZDate.Today);
			taxRate.SetRate_ForTestOnly(20, 1, ZDate.Today.AddDays(1));
			Charge.JR_AT_SellGSTRate = taxRate.PK;

			Assert("Precondition: JR_SellTaxDate", charge.JR_SellTaxDate.IsEmpty);
			AssertEquals(10m, Charge.JR_Calc_LocalSellTaxAmt);

			charge.JR_SellTaxDate = ZDate.Today.AddDays(1);
			AssertEquals(20m, Charge.JR_Calc_LocalSellTaxAmt);
		}

		public void TestJR_Calc_LocalSellTaxAmt10Percent()
		{
			Charge.JR_LocalSellAmt = 100m;
			AccTaxRate accTaxRate = Factory.New<AccTaxRate>();
			accTaxRate.SetRateNumerator_ForTestOnly(10);
			Charge.JR_AT_SellGSTRate = accTaxRate.PK;

			AssertEquals("Local sell tax amount", 10m, Charge.JR_Calc_LocalSellTaxAmt);
		}

		public void TestJR_Calc_LocalSellTaxAmt10PercentWithRounding()
		{
			Charge.JR_LocalSellAmt = 123.45m;
			AccTaxRate accTaxRate = Factory.New<AccTaxRate>();
			accTaxRate.SetRateNumerator_ForTestOnly(10);
			Charge.JR_AT_SellGSTRate = accTaxRate.PK;

			AssertEquals("Local sell tax amount", 12.35m, Charge.JR_Calc_LocalSellTaxAmt);
		}

		public void TestJR_Calc_LocalSellTaxAmtNoTaxRate()
		{
			Charge.JR_LocalSellAmt = 100m;
			Charge.JR_AT_SellGSTRate = ZGuid.Empty;

			AssertEquals("Local sell tax amount", 0m, Charge.JR_Calc_LocalSellTaxAmt);
		}

		public virtual void TestIsRevenuePosted()
		{
			Charge.JR_AL_ARLine = ZGuid.Empty;
			AssertEquals(false, Charge.IsRevenuePosted);

			AccTransactionLines transactionLine = Factory.New<AccTransactionLines>();
			Charge.JR_AL_ARLine = transactionLine.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(false, Charge.IsRevenuePosted);

			transactionLine.AL_LineType = TransactionLineTypes.Accrual;
			AssertEquals(false, Charge.IsRevenuePosted);

			transactionLine.AL_LineType = TransactionLineTypes.WIP;
			AssertEquals(false, Charge.IsRevenuePosted);

			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			AssertEquals(true, Charge.IsRevenuePosted);

			var header = Factory.New<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionLine.AL_AH = header.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(true, Charge.IsRevenuePosted);// Automatic Job Revenue Journals have AL_LineType = CST and can be linked to the AR Line of a charge
		}

		public void TestIsCostPosted()
		{
			Charge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, Charge.IsCostPosted);

			var transactionLine = Factory.New<AccTransactionLines>();
			Charge.JR_AL_APLine = transactionLine.PK;

			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			AssertEquals(true, Charge.IsCostPosted); // Automatic Job Revenue Journals have AL_LineType = REV and can be linked to the AP Line of a charge
			AssertEquals(false, Charge.IsCostPostedWithAPTransaction);
			AssertEquals(false, Charge.IsCostPostedWithJobRevenueJournal);

			var header = Factory.New<AccTransactionHeader>();
			header.AH_TransactionType = TransactionTypes.JobRevenueJournal;
			transactionLine.AL_AH = header.PK;
			AssertEquals(true, Charge.IsCostPostedWithJobRevenueJournal);
			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(true, Charge.IsCostPostedWithJobRevenueJournal);
			transactionLine.AL_AH = ZGuid.Empty;

			transactionLine.AL_LineType = TransactionLineTypes.Accrual;
			AssertEquals(false, Charge.IsCostPosted);
			AssertEquals(false, Charge.IsCostPostedWithAPTransaction);
			AssertEquals(false, Charge.IsCostPostedWithJobRevenueJournal);

			transactionLine.AL_LineType = TransactionLineTypes.WIP;
			AssertEquals(false, Charge.IsCostPosted);
			AssertEquals(false, Charge.IsCostPostedWithAPTransaction);
			AssertEquals(false, Charge.IsCostPostedWithJobRevenueJournal);

			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(true, Charge.IsCostPosted);
			AssertEquals(true, Charge.IsCostPostedWithAPTransaction);
			AssertEquals(false, Charge.IsCostPostedWithJobRevenueJournal);

			transactionLine.AL_LineType = TransactionLineTypes.UnapprovedCost;
			AssertEquals(true, Charge.IsCostPosted);
			AssertEquals(true, Charge.IsCostPostedWithAPTransaction);
			AssertEquals(false, Charge.IsCostPostedWithJobRevenueJournal);
		}

		public void TestIsReadyForCostPosting()
		{
			Charge.FillWithValidTestData();
			AssertEquals("Precondition: Charge is not ready for Cost Posting", false, Charge.IsInDatabaseAndReadyForCostPosting);
			AssertEquals("Precondition: Charge should not be in Database", false, Charge.IsInDatabase);
			AssertNotNull("Precondition: Charge.Job", Charge.Job);

			Charge.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
			AssertEquals("Charge's Job is ready for Cost Posting", true, Charge.Job.IsReadyForCostPosting);
			AssertEquals("Charge is not ready for Cost Posting until it is saved", false, Charge.IsInDatabaseAndReadyForCostPosting);

			Factory.Save();
			AssertEquals("Charge should be in Database", true, Charge.IsInDatabase);
			AssertEquals("Charge's Job is ready for Cost Posting", true, Charge.Job.IsReadyForCostPosting);
			AssertEquals("Charge is ready for Cost Posting as it is saved", true, Charge.IsInDatabaseAndReadyForCostPosting);

			Charge.JR_JH = ZGuid.Empty;
			AssertNull("Charge.Job", Charge.Job);
			AssertEquals("Charge is not ready for Cost Posting as its Job is null", false, Charge.IsInDatabaseAndReadyForCostPosting);
		}

		public void TestIsReadyForRevenuePosting()
		{
			Charge.FillWithValidTestData();
			AssertEquals("Precondition: Charge is not ready for Revenue Posting", false, Charge.IsInDatabaseAndReadyForRevenuePosting);
			AssertEquals("Precondition: Charge should not be in Database", false, Charge.IsInDatabase);
			AssertNotNull("Precondition: Charge.Job", Charge.Job);

			Charge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
			AssertEquals("Charge's Job is ready for Revenue Posting", true, Charge.Job.IsReadyForRevenuePosting);
			AssertEquals("Charge is not ready for Revenue Posting until it is saved", false, Charge.IsInDatabaseAndReadyForRevenuePosting);

			Factory.Save();
			AssertEquals("Charge should be in Database", true, Charge.IsInDatabase);
			AssertEquals("Charge's Job is ready for Revenue Posting", true, Charge.Job.IsReadyForRevenuePosting);
			AssertEquals("Charge is ready for Cost Posting as it is saved", true, Charge.IsInDatabaseAndReadyForRevenuePosting);

			Charge.JR_JH = ZGuid.Empty;
			AssertNull("Charge.Job", Charge.Job);
			AssertEquals("Charge is not ready for Revenue Posting as its Job is null", false, Charge.IsInDatabaseAndReadyForRevenuePosting);
		}

		public virtual void TestIsApproved()
		{
			Charge.JR_AL_APLine = ZGuid.Empty;
			AssertEquals(false, Charge.IsApproved);

			var transactionLine = Factory.New<AccTransactionLines>();
			Charge.JR_AL_APLine = transactionLine.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;
			AssertEquals(false, Charge.IsApproved);

			transactionLine.AL_LineType = TransactionLineTypes.Accrual;
			AssertEquals(false, Charge.IsApproved);

			transactionLine.AL_LineType = TransactionLineTypes.WIP;
			AssertEquals(false, Charge.IsApproved);

			transactionLine.AL_LineType = TransactionLineTypes.Cost;
			AssertEquals(true, Charge.IsApproved);

			transactionLine.AL_LineType = TransactionLineTypes.UnapprovedCost;
			AssertEquals(false, Charge.IsApproved);
		}

		public void TestRoundingOfInAppropriateLocalSellAmt()
		{
			ZString originalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			ZQuery filter = new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 1);
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(filter);
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = currency.RX_Code;

			Charge.JR_LocalSellAmt = 100.123456789M;

			ZString expectedExceptionMessageKey = "JR_LocalSellAmt had to be rounded at the JobCharge level";
			AssertEquals("Local Sell Amount should be rounded to a whole number", 100.0M, Charge.JR_LocalSellAmt);
		}

		public void TestRoundingOfInAppropriateLocalCostAmt()
		{
			ZString originalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			ZQuery filter = new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 100);
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(filter);

			ZString expectedExceptionMessageKey = "JR_LocalCostAmt had to be rounded at the JobCharge level";

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = currency.RX_Code;
			Charge.JR_LocalCostAmt = 100.123456789M;

			AssertEquals("Local Cost Amount should be rounded to 2 decimal places", 100.12M, Charge.JR_LocalCostAmt);

			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = originalCurrency;
		}

		public void TestRoundingOfInAppropriateOSSellAmt()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 1);
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(filter);
			Charge.JR_RX_NKSellCurrency = currency.RX_Code;
			Charge.JR_OSSellAmt = 100.123456789M;
			AssertEquals("OS Sell Amount should be rounded to a whole number", 100.0M, Charge.JR_OSSellAmt);
		}

		public void TestRoundingOfInAppropriateOSCostAmt()
		{
			ZQuery filter = new ZQuery(RefCurrencySchema.RX_SubUnitRatio, 100);
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(filter);
			Charge.JR_RX_NKCostCurrency = currency.RX_Code;
			Charge.JR_OSCostAmt = 100.123456789M;
			AssertEquals("OS Cost Amount should be rounded to 2 decimal places", 100.12M, Charge.JR_OSCostAmt);
		}

		public void TestCheckLocalSellAmountDecimals()
		{
			var jpCompany = CreateCompanyAndBranch("DMO", "BMO", "JPY");
			Factory.Save();

			JobCharge charge;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, jpCompany.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				charge = Factory.NewWithValidTestData<JobCharge>();
			}

			ErrorReporter.Clear();
			charge.JR_LocalSellAmt = 12m;
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			charge.JR_LocalSellAmt = 11.12m;
			var expectLog = @"Trying to update JR_LocalSellAmt with a value, of which the sub unit ratio is higher than expected.
Might be related to Critical Validation Error Type LineShouldHaveSameAmountAsJobCharge_11.

Additional Info:
Current Company Code: EDI.
Current Company Currency Code: AUD.

Job Charge's Company (JR_GC) Code:DMO.
Job Charge's Company (JR_GC) Currency Code:JPY.

Branch of Job Charge(JR_GB)'s Company Code:DMO.
Branch of Job Charge(JR_GB)'s Company Currency Code:JPY.

Company Currency Change Log:
Current Company[EDI] Currency Code Change Log: No local currency change log found.";
			AssertEquals("JobChargeLocalAmountDecimalsDoNotMatchCurrencySubUnitRatio", ErrorReporter.LastKeyReported);
			AssertEquals(expectLog, ErrorReporter.LastMessageReported);
			AssertEquals(1, ErrorReporter.TotalErrorCount);

			charge.JR_LocalSellAmt = 11.22m;
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		GlbCompany CreateCompanyAndBranch(string companyCode, string branchCode, string currencyCode)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = companyCode;
			company.GC_RX_NKLocalCurrency = currencyCode;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			return company;
		}

		#region Properties

		#region JobChargeAttrib

		public void TestJobChargeAttributes()
		{
			Charge = Factory.NewWithValidTestData<JobCharge>();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Product, "PRO");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib1, "PA1");
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobCharge sameCharge = newFactory.Load<JobCharge>(Charge.PK);

			AssertEquals(true, sameCharge.IsRegisteredEditableChildObject(sameCharge.JobChargeAttributes));
			AssertEquals(3, sameCharge.JobChargeAttributes.Count);
			AssertEquals("PRO", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Product));
			AssertEquals("LOC", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc));
			AssertEquals("PA1", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib1));
		}

		public void TestJobChargeAttrib_PartAttrib1()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib1, "PA1");
			AssertEquals("PA1", Charge.JobChargeAttrib_PartAttrib1);
		}

		public void TestJobChargeAttrib_PartAttrib2()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib2, "PA2");
			AssertEquals("PA2", Charge.JobChargeAttrib_PartAttrib2);
		}

		public void TestJobChargeAttrib_PartAttrib3()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib3, "PA3");
			AssertEquals("PA3", Charge.JobChargeAttrib_PartAttrib3);
		}

		public void TestJobChargeAttrib_SerialNumber()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.SerialNumber, "SER");
			AssertEquals("SER", Charge.JobChargeAttrib_SerialNumber);
		}

		public void TestJobChargeAttrib_Commodity()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Commodity, "COM");
			AssertEquals("COM", Charge.JobChargeAttrib_Commodity);
		}

		public void TestJobChargeAttrib_ContainerCode()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.ContainerCode, "CNT");
			AssertEquals("CNT", Charge.JobChargeAttrib_ContainerCode);
		}

		public void TestJobChargeAttrib_DocketReference()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.DocketReference, "REF");
			AssertEquals("REF", Charge.JobChargeAttrib_DocketReference);
		}

		public void TestJobChargeAttrib_LocationDesc()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");
			AssertEquals("LOC", Charge.JobChargeAttrib_LocationDesc);
		}

		public void TestJobChargeAttrib_LocationType()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.LocationType, "LOC");
			AssertEquals("LOC", Charge.JobChargeAttrib_LocationType);
		}

		public void TestJobChargeAttrib_Product()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Product, "PRO");
			AssertEquals("PRO", Charge.JobChargeAttrib_Product);
		}

		public void TestJobChargeAttrib_MinimumRateUsed()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.MinimumRateUsed, "1");
			AssertEquals("1", Charge.JobChargeAttrib_MinimumRateUsed);
		}

		public void TestJobChargeAttrib_ItemsToRate()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.ItemsToRate, "12.345");
			AssertEquals(12.345m, Charge.JobChargeAttrib_ItemsToRate);
		}

		public void TestJobChargeAttrib_ItemsToRateUnit()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.ItemsToRateUnit, "M3");
			AssertEquals("M3", Charge.JobChargeAttrib_ItemsToRateUnit);
		}

		public void TestJobChargeAttrib_UnroundedItemsToRate()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.UnroundedItemsToRate, "98.765");
			AssertEquals(98.765m, Charge.JobChargeAttrib_UnroundedItemsToRate);
		}

		public void TestJobChargeAttrib_CartageZoneDescription()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.CartageZoneDescription, "Zone");
			AssertEquals("Zone", Charge.JobChargeAttrib_CartageZoneDescription);
		}

		public void TestJobChargeAttrib_CartageLegPK()
		{
			var testGuid = ZGuid.NewZGuid();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.CartageLegPK, testGuid.ToString());
			AssertEquals(testGuid, Charge.JobChargeAttrib_CartageLegPK);
		}

		public void TestJobChargeAttrib_JobNumbersReference()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.JobNumbersReference, "JOBNUMBER1, JOBNUMBER2");
			AssertEquals("JOBNUMBER1, JOBNUMBER2", Charge.JobChargeAttrib_JobNumbersReference);
		}

		public void TestJobChargeAttrib_RateId()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.RateId, "McLaren");
			AssertEquals("McLaren", Charge.JobChargeAttrib_RateId);
		}

		public void TestJobChargeAttrib_ServiceId()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.ServiceID, "McLaren");
			AssertEquals("McLaren", Charge.JobChargeAttrib_ServiceId);
		}

		public void TestJobChargeAttrib_TransportProviderPK()
		{
			var testGuid = ZGuid.NewZGuid();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.TransportProviderPK, testGuid.ToString());
			AssertEquals(testGuid.ToString(), Charge.JobChargeAttrib_TransportProviderPK);
		}

		public void TestJobChargeAttrib_ServiceProviderPK()
		{
			var testGuid = ZGuid.NewZGuid();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.ServiceProviderPK, testGuid.ToString());
			AssertEquals(testGuid.ToString(), Charge.JobChargeAttrib_ServiceProviderPK);
		}

		public void TestJobChargeAttrib_CalculatorDescription()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.CalculatorDescription, "McLaren");
			AssertEquals("McLaren", Charge.JobChargeAttrib_CalculatorDescription);
		}

		public void TestJobChargeAttrib_AllCalculatorDescriptions()
		{
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.CalculatorDescription, "McLaren1");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.CalculatorDescription, "McLaren2");
			AssertContainsExactElementsInAnyOrder(new[] { "McLaren1", "McLaren2" }, Charge.JobChargeAttrib_AllCalculatorDescriptions);
		}

		public void TestJobChargeAttributesClearedOnDeletingJobCharge()
		{
			Charge = Factory.NewWithValidTestData<JobCharge>();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Product, "PRO");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib1, "PA1");
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobCharge sameCharge = newFactory.Load<JobCharge>(Charge.PK);

			AssertEquals(true, sameCharge.IsRegisteredEditableChildObject(sameCharge.JobChargeAttributes));
			AssertEquals(3, sameCharge.JobChargeAttributes.Count);
			AssertEquals("PRO", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Product));
			AssertEquals("LOC", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc));
			AssertEquals("PA1", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib1));

			var attribCollection = sameCharge.JobChargeAttributes;
			var attribs = attribCollection.ToArray();

			sameCharge.Delete();
			AssertEquals("Attributes are removed", 0, attribCollection.Count);
			foreach (var attrib in attribs)
			{
				Assert("Attributes are deleted", attrib.IsDeleted);
			}
		}

		public void TestJobChargeAttributesClearedWhenJobChargeDetetedByDataRefresh()
		{
			Charge = Factory.NewWithValidTestData<JobCharge>();
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Product, "PRO");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.LocationDesc, "LOC");
			AddJobChargeAttrib(Charge, JobChargeAttribTypeList.Codes.Attrib1, "PA1");
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobCharge sameCharge = newFactory.Load<JobCharge>(Charge.PK);

			AssertEquals(true, sameCharge.IsRegisteredEditableChildObject(sameCharge.JobChargeAttributes));
			AssertEquals(3, sameCharge.JobChargeAttributes.Count);
			AssertEquals("PRO", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Product));
			AssertEquals("LOC", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.LocationDesc));
			AssertEquals("PA1", sameCharge.JobChargeAttributes.GetValueFromName(JobChargeAttribTypeList.Codes.Attrib1));

			var attribCollection = sameCharge.JobChargeAttributes;
			var attribs = attribCollection.ToArray();

			Charge.Delete();
			Factory.Save();
			AssertEquals("Attributes are removed", 0, attribCollection.Count);
			foreach (var attrib in attribs)
			{
				Assert("Attributes are deleted", attrib.IsDeleted);
			}
		}

		#endregion

		#region TestJR_JH

		public void TestJR_JH()
		{
			var jobHeaderParent1 = Factory.New<DummyJobHeaderParent>();
			var jobHeaderParent2 = Factory.New<DummyJobHeaderParent>();
			var jobHeaderInvoiceSupporter1 = (DummyJobHeaderParentJobInvoicingSupporter)jobHeaderParent1.InvoicingSupporter;
			var jobHeaderInvoiceSupporter2 = (DummyJobHeaderParentJobInvoicingSupporter)jobHeaderParent2.InvoicingSupporter;
			var jobHeader1 = Factory.NewJobForTesting<JobHeader>();
			var jobHeader2 = Factory.NewJobForTesting<JobHeader>();
			jobHeader1.Parent = jobHeaderParent1;
			jobHeader2.JH_ParentID = jobHeaderParent2.PK;
			jobHeader2.JH_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			AssertNull("Precondition", jobHeader2.Parent);
			AssertEquals("Precondition", 0, jobHeaderInvoiceSupporter1.SetDefaultsForNewChargeCalled);
			AssertEquals("Precondition", 0, jobHeaderInvoiceSupporter2.SetDefaultsForNewChargeCalled);

			var charge = Factory.New<JobCharge>();
			charge.JR_JH = jobHeader1.PK;
			AssertEquals("When job header is assigned the charge should be updated from jobHeaderParent.", 1, jobHeaderInvoiceSupporter1.SetDefaultsForNewChargeCalled);

			charge.JR_JH = jobHeader1.PK;
			AssertEquals("Reassigning the same job header should not update defaults.", 1, jobHeaderInvoiceSupporter1.SetDefaultsForNewChargeCalled);

			charge.JR_JH = jobHeader2.PK;
			AssertNull("Assigning new job Header should not load jobs parent for performance reasons", jobHeader2.Parent);
			AssertEquals("Assigning new job header that has no parent defined should not update defaults/reload parent for performance reasons.", 0, jobHeaderInvoiceSupporter2.SetDefaultsForNewChargeCalled);

			charge.JR_JH = ZGuid.Invalid;
			AssertEquals("Assigning invalid job header should not update charge.", 1, jobHeaderInvoiceSupporter1.SetDefaultsForNewChargeCalled);
			AssertEquals("Assigning invalid job header should not update charge.", 0, jobHeaderInvoiceSupporter2.SetDefaultsForNewChargeCalled);

			jobHeader2.Parent = jobHeaderParent2;
			charge.JR_JH = jobHeader2.PK;
			AssertEquals("Assigning new job header should update charge from new header.", 1, jobHeaderInvoiceSupporter2.SetDefaultsForNewChargeCalled);
		}

		public void TestSettingJR_JHSetsProFormatCostAndRevenue()
		{
			var jobHeader1 = Factory.NewJobForTesting<JobHeader>();
			var jobHeaderParent1 = Factory.New<DummyJobHeaderParent>();
			jobHeader1.JH_ParentID = jobHeaderParent1.PK;
			jobHeader1.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			var jobHeader2 = Factory.NewJobForTesting<JobHeader>();
			var jobHeaderParent2 = Factory.New<DummyJobHeaderParent>();
			jobHeader2.JH_ParentID = jobHeaderParent2.PK;
			jobHeader2.JH_ParentTableCode = DummyBizoSchema.Constants.Prefix;

			charge = (JobCharge)GetNewBusinessObject();
			charge.JR_JH = jobHeader1.PK;
			AssertEquals("Proforma Cost field", true, charge.JR_ProFormaCost);
			AssertEquals("Proforma Revenue field", true, charge.JR_ProFormaRevenue);

			charge.JR_JH = jobHeader2.PK;
			AssertEquals("Proforma Cost field", false, charge.JR_ProFormaCost);
			AssertEquals("Proforma Revenue field", false, charge.JR_ProFormaRevenue);
		}

		public void TestJR_JH_CostReference()
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = jobHeader.PK;
			AssertEquals("No Parent, so don't default anything.", "", charge.JR_CostReference);

			var jobHeaderParent = Factory.New<DummyJobHeaderParent>();
			var jobHeaderInvoiceSupporter = (DummyJobHeaderParentJobInvoicingSupporter)jobHeaderParent.InvoicingSupporter;
			jobHeader.Parent = jobHeaderParent;
			charge.JR_JH = ZGuid.Invalid;
			charge.JR_JH = jobHeader.PK;
			AssertEquals("Has Parent, but no Cost Ref, so default to blank.", "", charge.JR_CostReference);

			jobHeaderInvoiceSupporter.OperationalJobRef = "SupRef";
			charge.JR_JH = ZGuid.Invalid;
			charge.JR_JH = jobHeader.PK;
			AssertEquals("Set Cost Reference.", "SupRef", charge.JR_CostReference);

			charge.JR_JH = ZGuid.Empty;
			AssertEquals("Keep existing Cost Reference.", "SupRef", charge.JR_CostReference);

			charge.JR_CostReference = "From Consol";
			charge.JR_E6 = new Guid("3B78C809-197A-4941-B118-9C4089D91EE0");
			charge.JR_JH = jobHeader.PK;
			AssertEquals("Cost Reference Left As Per Consol.", "From Consol", charge.JR_CostReference);
		}

		#endregion

		#endregion

		#region TestLocalCurrencyDecimals

		public void TestLocalCurrencyDecimals()
		{
			ZInt defaultValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals("Decimals should be 0", 0, Charge.LocalCurrencyDecimals);
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 100;
				AssertEquals("Decimals should be 2", 2, Charge.LocalCurrencyDecimals);
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = defaultValue;
			}
		}

		#endregion

		#region TestCompanyLocalCurrencyDecimals

		public void TestCompanyLocalCurrencyDecimals()
		{
			var twoDPCurrency = Factory.NewWithValidTestData<RefCurrency>();
			twoDPCurrency.RX_SubUnitRatio = 100;
			var oneDPCurrency = Factory.NewWithValidTestData<RefCurrency>();
			oneDPCurrency.RX_SubUnitRatio = 10;
			var zeroDPCurrency = Factory.NewWithValidTestData<RefCurrency>();
			zeroDPCurrency.RX_SubUnitRatio = 0;

			var chargeCompany = Factory.NewWithValidTestData<GlbCompany>();
			chargeCompany.GC_RX_NKLocalCurrency = twoDPCurrency.RX_Code;
			var chargeBranch = Factory.NewWithValidTestData<GlbBranch>();
			chargeBranch.GB_GC = chargeCompany.PK;

			Charge.JR_GB = chargeBranch.PK;
			AssertEquals("Decimals should be 2 via CalculatedCompany", 2, Charge.CompanyLocalCurrencyDecimals);

			chargeCompany.GC_RX_NKLocalCurrency = oneDPCurrency.RX_Code;
			AssertEquals("Decimals should be 1 via CalculatedCompany", 1, Charge.CompanyLocalCurrencyDecimals);

			chargeCompany.GC_RX_NKLocalCurrency = zeroDPCurrency.RX_Code;
			AssertEquals("Decimals should be 0 via CalculatedCompany", 0, Charge.CompanyLocalCurrencyDecimals);

			AssertEquals("Precondition: GlbCompany.CurrentCompany decimals is 2", 2, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			Charge.JR_GB = ZGuid.Empty;
			AssertEquals("Decimals should be 2 via GlbCompany.CurrentCompany when CalculatedCompany is null", 2, Charge.CompanyLocalCurrencyDecimals);
		}

		#endregion

		#region TestOSCostCurrencyDecimals

		public void TestOSCostCurrencyDecimals()
		{
			Charge.JR_RX_NKCostCurrency = string.Empty;
			AssertNull(Charge.CostCurrency);
			AssertEquals("Should be 2 when no currency specified", 2, Charge.OSCostCurrencyDecimals);

			Charge.JR_RX_NKCostCurrency = "IDR";
			AssertEquals("Decimals should be 0", 0, Charge.OSCostCurrencyDecimals);
			Charge.JR_RX_NKCostCurrency = "USD";
			AssertEquals("Decimals should be 2", 2, Charge.OSCostCurrencyDecimals);
		}

		#endregion

		#region TestOSSellCurrencyDecimals

		public void TestOSSellCurrencyDecimals()
		{
			Charge.JR_RX_NKSellCurrency = string.Empty;
			AssertNull(Charge.SellCurrency);
			AssertEquals("Should be 2 when no currency specified", 2, Charge.OSSellCurrencyDecimals);

			Charge.JR_RX_NKSellCurrency = "IDR";
			AssertEquals("Decimals should be 0", 0, Charge.OSSellCurrencyDecimals);
			Charge.JR_RX_NKSellCurrency = "USD";
			AssertEquals("Decimals should be 2", 2, Charge.OSSellCurrencyDecimals);
		}

		#endregion

		#region TestOSSellInvoiceCurrencyDecimals

		public void TestOSSellInvoiceCurrencyDecimals()
		{
			Charge.JR_RX_NKSellInvoiceCurrency = string.Empty;
			AssertNull(Charge.SellInvoiceCurrency);
			AssertEquals("Should be 2 when no currency specified", 2, Charge.OSSellInvoiceCurrencyDecimals);

			Charge.JR_RX_NKSellInvoiceCurrency = "IDR";
			AssertEquals("Decimals should be 0", 0, Charge.OSSellInvoiceCurrencyDecimals);
			Charge.JR_RX_NKSellInvoiceCurrency = "USD";
			AssertEquals("Decimals should be 2", 2, Charge.OSSellInvoiceCurrencyDecimals);
		}

		#endregion

		#region TestExchangeRateDecimalPlaces

		public virtual void TestExchangeRateDecimalPlaces()
		{
			bool defaultValue = GlbCompany.CurrentCompany.GC_IsReciprocal;
			try
			{
				AssertEquals("Pre-condition: Non Reciprocal", false, GlbCompany.CurrentCompany.GC_IsReciprocal);
				AssertEquals("Should be 6 for Non Reciprocal", 6, Charge.ExchangeRateDecimalPlaces);

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				AssertEquals("Should be 6 for Reciprocal", 6, Charge.ExchangeRateDecimalPlaces);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
			}
		}

		#endregion

		#region Concurrency testing

		public virtual void TestJR_AT_CostGSTRateConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AT_CostGSTRate, taxRate1.PK, taxRate2.PK);
		}

		public virtual void TestJR_AT_SellGSTRateConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AT_SellGSTRate, taxRate1.PK, taxRate2.PK);
		}

		public virtual void TestJR_GBConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			GlbBranch newBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch1.GB_GC = GlbCompany.CurrentCompany.PK;
			GlbBranch newBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			newBranch2.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_GB, newBranch1.PK, newBranch2.PK);
		}

		public virtual void TestJR_GEConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			GlbDepartment dept1 = Factory.NewWithValidTestData<GlbDepartment>();
			GlbDepartment dept2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_GE, dept1.PK, dept2.PK);
		}

		public virtual void TestJR_ACConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			AccChargeCode code1 = Factory.NewWithValidTestData<AccChargeCode>();
			code1.AC_GC = GlbCompany.CurrentCompany.PK;
			AccChargeCode code2 = Factory.NewWithValidTestData<AccChargeCode>();
			code2.AC_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AC, code1.PK, code2.PK);
		}

		public virtual void TestJR_OH_CostAccountConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_OH_CostAccount, org1.PK, org2.PK);
		}

		public virtual void TestJR_OH_SellAccountConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_OH_SellAccount, org1.PK, org2.PK);
		}

		public virtual void TestJR_RX_NKCostCurrencyConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKCostCurrency = "AUD";

			ZString otherCurrency = "USD";
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_RX_NKCostCurrency, ZString.Empty, otherCurrency);
		}

		public virtual void TestJR_RX_NKSellCurrencyConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			charge.JR_RX_NKSellCurrency = "AUD";

			ZString otherCurrency = "USD";
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_RX_NKSellCurrency, ZString.Empty, otherCurrency);
		}

		public virtual void TestConcurrencyPolicyDoesNotAllowToChangeJR_AL_APLineInOtherSession()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge.JR_JH = job.PK;

			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			charge.JR_AL_APLine = line.PK;

			AccTransactionLines otherLine = Factory.NewWithValidTestData<AccTransactionLines>();
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AL_APLine, ZGuid.Empty, otherLine.PK);
		}

		public virtual void TestConcurrencyPolicyDoesNotAllowToChangeJR_AL_ARLineInOtherSession()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
			charge.JR_JH = job.PK;
			charge.JR_OSSellAmt = 50m;

			// TransactionTypes.GLAutoJournal is used here to avoid problems with critical validation and automatic reversing.

			AccTransactionLines line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionTypes.GLAutoJournal;
			line.AL_LineAmount = 50m;
			charge.JR_AL_ARLine = line.PK;

			AccTransactionLines otherLine = Factory.NewWithValidTestData<AccTransactionLines>();
			otherLine.AL_LineType = TransactionTypes.GLAutoJournal;
			otherLine.AL_LineAmount = 50m;

			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_AL_ARLine, ZGuid.Empty, otherLine.PK);
		}

		public void TestJR_IsARCashAdvanceConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;
			charge.JR_IsARCashAdvance = true;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_IsARCashAdvance, ZBool.False, ZBool.False);
		}

		public void TestJR_IsAPCashAdvanceConcurrency()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;
			charge.JR_IsAPCashAdvance = true;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_IsAPCashAdvance, ZBool.False, ZBool.False);
		}

		public void TestJR_CAL_ARLineConcurrency()
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AR";
			cah1.Lines.Add(cal1);

			var cal2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah2 = cal2.RequestHeader;
			cah2.CAH_Ledger = "AR";
			cah2.Lines.Add(cal2);

			var cal3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah3 = cal3.RequestHeader;
			cah3.CAH_Ledger = "AR";
			cah3.Lines.Add(cal3);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;
			charge.JR_CAL_ARLine = cal1.PK;
			charge.JR_IsARCashAdvance = true;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_CAL_ARLine, cal2.PK, cal3.PK);
		}

		public void TestJR_CAL_APLineConcurrency()
		{
			var cal1 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah1 = cal1.RequestHeader;
			cah1.CAH_Ledger = "AP";
			cah1.Lines.Add(cal1);

			var cal2 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah2 = cal2.RequestHeader;
			cah2.CAH_Ledger = "AP";
			cah2.Lines.Add(cal2);

			var cal3 = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah3 = cal3.RequestHeader;
			cah3.CAH_Ledger = "AP";
			cah3.Lines.Add(cal3);

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			JobCharge charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;
			charge.JR_CAL_APLine = cal1.PK;
			charge.JR_IsAPCashAdvance = true;
			Factory.Save();

			AssertConcurrency(charge, JobChargeSchema.JR_CAL_APLine, cal2.PK, cal3.PK);
		}

		/// <summary>
		/// This method reloads the charge in 2 separate factories (that have data refresh turned off), sets 'value1' and 'value2' on each copy respectively.
		/// The first factory is saved and then the second, which will cause a concurrency issue.
		/// The method then asserts that the exception handler reported that the error was critical and couldn't be merged.
		/// </summary>
		/// <param name="charge">This is the charge created for testing. It will get reloaded in other factories in this method so that the concurrency can be tested</param>
		/// <param name="column">This is the column that should be set by value1 and value2</param>
		/// <param name="value1">This is the value that gets set on the first reloaded copy of the charge.</param>
		/// <param name="value2">This is the value that gets set on the second reloaded copy of the reloaded charge, which when saved will cause a concurrency exception</param>
		void AssertConcurrency(JobCharge charge, SchemaColumn column, IZType value1, IZType value2)
		{
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			JobCharge chargeInFactory1 = (JobCharge)factory1.Load(ExpectedBusinessObjectType, charge.PK);
			JobCharge chargeInFactory2 = (JobCharge)factory2.Load(ExpectedBusinessObjectType, charge.PK);

			chargeInFactory1[column] = value1;
			chargeInFactory2[column] = value2;

			factory1.Save();
			try
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				factory2.Save();
				Fail("Expected exception: The system cannot automatically merge your changes because there are conflicts with critical fields.");
			}
			catch (ZSaveConcurrencyException e)
			{
				ZExceptionReporting.HandleSaveException(e);
				AssertContains(column.Name, e.Message);
			}
			AssertContains("The system cannot automatically merge your changes because there are conflicts with critical fields.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[ExpectNoExceptions]
		public void TestConcurrencyPolicyDoesNotAllowDeleteForDataRefreshOnDeletedCharge()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			JobCharge chargeInFactory1 = (JobCharge)factory1.Load(ExpectedBusinessObjectType, charge.PK);
			JobCharge chargeInFactory2 = (JobCharge)factory2.Load(ExpectedBusinessObjectType, charge.PK);
			IBusiness chargeForIBusiness1 = chargeInFactory1;
			IBusiness chargeForIBusiness2 = chargeInFactory2;

			chargeInFactory1.Delete();
			chargeInFactory2.Delete();
			factory2.Save();

			try
			{
				factory1.Save();
				Fail("ZSaveConcurrencyException is expected.");
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		#endregion

		public void TestJobChargeConstructorStackTraceCollected_OnConstruction()
		{
			var charge1 = Factory.New<JobCharge>();
			var collectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			var criticalValidationInfo = collectorService.GetInfoSafe(charge1.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace);
			AssertContains("JobChargeConstructorStackTrace: There is no data collected", criticalValidationInfo);

			var charge2 = Factory.New<JobCharge>();
			criticalValidationInfo = collectorService.GetInfoSafe(charge2.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeConstructorStackTrace);
			AssertContains(@"JobChargeConstructorStackTrace:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.MasterFiles.Business.JobCharge.", criticalValidationInfo);
		}

		public void TestJobChargeCreatedOnClosedJobStackTraceCollected_WhenCreatedOnClosedJob()
		{
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			JobCharge charge = (JobCharge)Factory.New(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;

			var collectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			var criticalValidationInfo = collectorService.GetInfoSafe(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeCreatedOnClosedJobStackTrace);
			AssertContains(@"JobChargeCreatedOnClosedJobStackTrace:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.MasterFiles.Business.JobCharge", criticalValidationInfo);

			charge.JR_JH = job.PK;
			criticalValidationInfo = collectorService.GetInfoSafe(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeCreatedOnClosedJobStackTrace);
			AssertContains(@"JobChargeCreatedOnClosedJobStackTrace:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.MasterFiles.Business.JobCharge", criticalValidationInfo);
		}

		public void TestIsDataVersionsAutoLogged()
		{
			AccountingMasterFilesRegistry.Instance.JobChargeDataVersionAutoLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(false, ((IDataVersionLoggingSupported)Charge).IsDataVersionsAutoLogged);

			AccountingMasterFilesRegistry.Instance.JobChargeDataVersionAutoLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(true, ((IDataVersionLoggingSupported)Charge).IsDataVersionsAutoLogged);
		}

		#region TestFindJobChargeAttrib

		public void TestFindJobChargeAttrib()
		{
			var charge = Factory.New<JobCharge>();
			AssertNull(charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.Attrib1));

			var jobAttrib1 = CreateJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.Attrib1, "AAA");
			var jobAttrib2 = CreateJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.ItemsToRate, "10");
			var jobAttrib3 = CreateJobChargeAttrib(charge, JobChargeAttribTypeList.Codes.DocketReference, "TEST");
			AssertEquals(jobAttrib1, charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.Attrib1));
			AssertEquals(jobAttrib2, charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.ItemsToRate));
			AssertEquals(jobAttrib3, charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.DocketReference));
			AssertNull(charge.FindJobChargeAttrib(JobChargeAttribTypeList.Codes.CartageLegPK));
		}

		JobChargeAttrib CreateJobChargeAttrib(JobCharge charge, string name, string value)
		{
			var attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;

			return attrib;
		}

		#endregion

		#region Test JR_OSCostExRate and JR_OSSellExRate Rounding
		public void TestRoundingOnCostAndSellExRate()
		{
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();

			Action<string> setRate = (rateName) =>
			{
				var t = charge[rateName];
				t.GetType().InvokeMember("SetBuyRate_ForTestOnly", System.Reflection.BindingFlags.InvokeMethod, null, t, new object[] { 0.131506272m });
			};

			charge.JR_RX_NKCostCurrency = "USD";
			setRate("CostExchangeRate");
			AssertEquals(0.131506m, charge.JR_OSCostExRate);

			charge.JR_RX_NKSellCurrency = "USD";
			setRate("RevenueExchangeRate");
			AssertEquals(0.131506m, charge.JR_OSSellExRate);
		}
		#endregion

		public void TestEnglishOnlyDescription()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Desc = "Service";

			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_Desc = chargeCode.AC_Desc;
			AssertEquals("Service", jobCharge.EnglishOnlyDescription);

			jobCharge.JR_AC = chargeCode.PK;
			AssertEquals("Service", jobCharge.EnglishOnlyDescription);

			chargeCode.AC_LocalLanguageDescription = "役";
			AssertEquals("Service", jobCharge.EnglishOnlyDescription);

			jobCharge.JR_Desc = chargeCode.AC_LocalLanguageDescription;
			AssertEquals("Service", jobCharge.EnglishOnlyDescription);
		}

		public void TestIncorectlyDeletedChargeCauseCriticalValidationError()
		{
			var line = AccTransactionLinesCriticalValidationTest.GetLine(Factory, true, TransactionLineTypes.Cost);
			var charge = AccTransactionLinesCriticalValidationTest.GetLineLinkedCharge(line);
			Factory.Save();

			ReleaseFactory();
			var chargeLoaded = Factory.Load<JobCharge>(charge.PK);
			chargeLoaded.Delete();
			AssertExceptionThrown<OnSavingCriticalCheckException<JobCharge>>("If posted charge was tried to be deleted the Factory must not be saved.", () => Factory.Save());
			AssertEquals("Exception count", 1, ExceptionReporterTestListener.Instance.Count);
			var exception = ExceptionReporterTestListener.Instance[0].InnerException;
			AssertContains("Exception message (User Message)", "Charge deleted with posted CST", exception.Message);
			AssertContains("Exception message (Tech Message)", "Charge deleted with posted CST.\r\n\r\n\tPK = ", exception.Message);
			ErrorReporter.Clear();
		}

		public void TestResetAddressContactOnOrgChange()
		{
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_OH_SellAccount = ZGuid.NewZGuid();
			var expectedAddress = ZGuid.NewZGuid();
			var expectedContact = ZGuid.NewZGuid();
			jobCharge.JR_OA_SellInvoiceAddress = expectedAddress;
			jobCharge.JR_OC_SellInvoiceContact = expectedContact;

			AssertEquals("Precondition: JR_OA_SellInvoiceAddress", expectedAddress, jobCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("Precondition: JR_OC_SellInvoiceContact", expectedContact, jobCharge.JR_OC_SellInvoiceContact);

			jobCharge.JR_OH_SellAccount = jobCharge.JR_OH_SellAccount;
			AssertEquals("Don't change JR_OA_SellInvoiceAddress", expectedAddress, jobCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("Don't change JR_OC_SellInvoiceContact", expectedContact, jobCharge.JR_OC_SellInvoiceContact);

			jobCharge.JR_OH_SellAccount = ZGuid.NewZGuid();
			AssertEquals("Reset JR_OA_SellInvoiceAddress", ZGuid.Empty, jobCharge.JR_OA_SellInvoiceAddress);
			AssertEquals("Reset JR_OC_SellInvoiceContact", ZGuid.Empty, jobCharge.JR_OC_SellInvoiceContact);
		}

		public void TestGetAPLineValueHistory()
		{
			JobCharge jobCharge = (JobCharge)Factory.New(ExpectedBusinessObjectType);
			Assert("No value set", jobCharge.JR_AL_APLine.IsEmpty);
			AssertEquals("History must be empty", 0, jobCharge.GetAPLineValueHistory().Length);

			var value1 = ZGuid.NewZGuid();
			jobCharge.JR_AL_APLine = value1;
			AssertEquals("History must be empty", 0, jobCharge.GetAPLineValueHistory().Length);

			var value2 = ZGuid.NewZGuid();
			jobCharge.JR_AL_APLine = value2;
			AssertEquals("History Count", 1, jobCharge.GetAPLineValueHistory().Length);
			AssertEquals("First value", value1, jobCharge.GetAPLineValueHistory()[0]);

			var value3 = ZGuid.NewZGuid();
			jobCharge.JR_AL_APLine = value3;
			AssertEquals("History Count", 2, jobCharge.GetAPLineValueHistory().Length);
			Assert("First value", jobCharge.GetAPLineValueHistory().Contains(value1));
			Assert("Second value", jobCharge.GetAPLineValueHistory().Contains(value2));

			jobCharge.Delete();
			AssertEquals("History Count", 3, jobCharge.GetAPLineValueHistory().Length);
			Assert("First value", jobCharge.GetAPLineValueHistory().Contains(value1));
			Assert("Second value", jobCharge.GetAPLineValueHistory().Contains(value2));
			Assert("Third value", jobCharge.GetAPLineValueHistory().Contains(value3));
		}

		public void TestGetARLineValueHistory()
		{
			JobCharge jobCharge = (JobCharge)Factory.New(ExpectedBusinessObjectType);
			Assert("No value set", jobCharge.JR_AL_ARLine.IsEmpty);
			AssertEquals("History must be empty", 0, jobCharge.GetARLineValueHistory().Length);

			var value1 = ZGuid.NewZGuid();
			jobCharge.JR_AL_ARLine = value1;
			AssertEquals("History must be empty", 0, jobCharge.GetARLineValueHistory().Length);

			var value2 = ZGuid.NewZGuid();
			jobCharge.JR_AL_ARLine = value2;
			AssertEquals("History Count", 1, jobCharge.GetARLineValueHistory().Length);
			AssertEquals("First value", value1, jobCharge.GetARLineValueHistory()[0]);

			var value3 = ZGuid.NewZGuid();
			jobCharge.JR_AL_ARLine = value3;
			AssertEquals("History Count", 2, jobCharge.GetARLineValueHistory().Length);
			Assert("First value", jobCharge.GetARLineValueHistory().Contains(value1));
			Assert("Second value", jobCharge.GetARLineValueHistory().Contains(value2));

			jobCharge.Delete();
			AssertEquals("History Count", 3, jobCharge.GetARLineValueHistory().Length);
			Assert("First value", jobCharge.GetARLineValueHistory().Contains(value1));
			Assert("Second value", jobCharge.GetARLineValueHistory().Contains(value2));
			Assert("Third value", jobCharge.GetARLineValueHistory().Contains(value3));
		}

		[TestDate(2018, 06, 06)]
		public void TestClearingARAPLineHistory()
		{
			var apLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			var apLine2 = Factory.NewWithValidTestData<AccTransactionLines>();

			var arLine1 = Factory.NewWithValidTestData<AccTransactionLines>();
			var arLine2 = Factory.NewWithValidTestData<AccTransactionLines>();

			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			apLine1.AL_AG = glHeader.PK;
			apLine2.AL_AG = glHeader.PK;
			arLine1.AL_AG = glHeader.PK;
			arLine2.AL_AG = glHeader.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = apLine1.PK;
			charge.JR_AL_ARLine = arLine1.PK;

			AssertEquals("History not added yet", 0, charge.GetAPLineValueHistory().Length);
			AssertEquals("History not added yet", 0, charge.GetARLineValueHistory().Length);

			apLine1.AL_ReverseDate = ZDateTime.Today;
			arLine1.AL_ReverseDate = ZDate.Today;

			charge.JR_AL_APLine = apLine2.PK;
			charge.JR_AL_ARLine = arLine2.PK;

			AssertEquals("History made!", 1, charge.GetAPLineValueHistory().Length);
			Assert(charge.GetAPLineValueHistory().Contains(apLine1.PK));

			AssertEquals("History made!", 1, charge.GetARLineValueHistory().Length);
			Assert(charge.GetARLineValueHistory().Contains(arLine1.PK));

			Factory.Save();

			AssertEquals("Saving caused history to be cleared", 0, charge.GetAPLineValueHistory().Length);
			AssertEquals("Saving caused history to be cleared", 0, charge.GetARLineValueHistory().Length);
		}

		public void TestJobChargeAfterOnSavingContextIsAddedWhenObjectIsSaved()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.OnSaving();
			Assert("JobCharge should have context when saving", charge.HasContext(BusinessContext.JobChargeAfterOnSaving));
			charge.OnSaved(true);
			Assert("Context should be removed when saving is finished regardless of successful save", !charge.HasContext(BusinessContext.JobChargeAfterOnSaving));
			charge.OnSaving();
			Assert("JobCharge should have context when saving", charge.HasContext(BusinessContext.JobChargeAfterOnSaving));
			charge.OnSaved(false);
			Assert("Context should be removed when saving is finished regardless of successful save", !charge.HasContext(BusinessContext.JobChargeAfterOnSaving));
		}

		Tuple<string, CriticalValidationInfoCollectorServiceKeyType>[] GetPropertiesReturningCallStacksWithKeys() => new[]
			{
				new Tuple<string,CriticalValidationInfoCollectorServiceKeyType>(nameof(charge.JR_OSSellExRate), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OSSellExRateHasChangesAfterSaving),
				new Tuple<string,CriticalValidationInfoCollectorServiceKeyType>(nameof(charge.JR_LocalSellAmt), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LocalSellAmtHasChangesAfterSaving),
				new Tuple<string,CriticalValidationInfoCollectorServiceKeyType>(nameof(charge.JR_EstimatedCost), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_EstimatedCostHasChangesAfterSaving),
				new Tuple<string,CriticalValidationInfoCollectorServiceKeyType>(nameof(charge.JR_LineCFX), CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_LineCFXHasChangesAfterSaving)
			};

		public void TestCallStacksAreAddedForProperty()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			GetPropertiesReturningCallStacksWithKeys().ForEach(x =>
			{
				var propertyName = x.Item1;
				var key = x.Item2;
				var stackString = string.Format("{1}{0}:{1}{2} Setter Call Stack:{1}", key, System.Environment.NewLine, propertyName);
				var propertyInfo = charge.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

				charge.RemoveContext(BusinessContext.PeriodicInvoicePosting);
				charge.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
				Assert(!charge.HasContext(BusinessContext.PeriodicInvoicePosting) && !charge.HasContext(BusinessContext.JobChargeAfterOnSaving));

				var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				service.ClearServiceCache();
				service.GetInfo(charge.PK, key);
				var info = service.GetInfo(charge.PK, key);
				Assert(info.Contains("There is no data collected for this PK.") || info.Contains("There is no data collected for this key."));
				CriticalValidationInfoCollectorService.ClearKeysRequestedInThisSession_ForTestOnly();

				propertyInfo.SetValue(charge, new ZDecimal(100));
				service.GetInfo(charge.PK, key);
				info = service.GetInfo(charge.PK, key);
				Assert("Should not add message with no context", info.Contains("There is no data collected for this PK.") || info.Contains("There is no data collected for this key."));
				CriticalValidationInfoCollectorService.ClearKeysRequestedInThisSession_ForTestOnly();

				charge.SetContext(BusinessContext.PeriodicInvoicePosting);
				propertyInfo.SetValue(charge, new ZDecimal(200));
				info = service.GetInfo(charge.PK, key);
				if (propertyName == nameof(JobCharge.JR_EstimatedCost))
				{
					AssertStartsWith("Should add message immediately for JR_EstimatedCost if in period invoicing context", stackString, info);
				}
				else
				{
					Assert("Should only add message after reported if not JR_EstiamtedCost", info.Contains("There is no data collected for this PK.") || info.Contains("There is no data collected for this key."));
					propertyInfo.SetValue(charge, new ZDecimal(300));
					info = service.GetInfo(charge.PK, key);
					AssertStartsWith("Should only add message after reported if not JR_EstiamtedCost", stackString, info);
				}

				charge.RemoveContext(BusinessContext.PeriodicInvoicePosting);
				CriticalValidationInfoCollectorService.ClearKeysRequestedInThisSession_ForTestOnly();
				service.ClearServiceCache();

				charge.SetContext(BusinessContext.JobChargeAfterOnSaving);
				propertyInfo.SetValue(charge, new ZDecimal(400));
				info = service.GetInfo(charge.PK, key);
				Assert("Should add message after reported in after saving context", info.Contains("There is no data collected for this PK.") || info.Contains("There is no data collected for this key."));
				propertyInfo.SetValue(charge, new ZDecimal(500));
				info = service.GetInfo(charge.PK, key);
				AssertStartsWith("Should add message after reported in after saving context", stackString, info);
				charge.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
			});
		}

		public void TestResttingHasChangesOnModifiedChargeInDbMessageContainsCallStack_SingleStack()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			GlbCompany.CurrentCompany.SetCurrency(Core.Constants.CurrencyCodes.Australia);
			Factory.Save();
			GetPropertiesReturningCallStacksWithKeys().ForEach(x =>
			{
				var propertyInfo = charge.GetType().GetProperty(x.Item1, BindingFlags.Public | BindingFlags.Instance);
				var key = x.Item2;
				charge.SetContext(BusinessContext.JobChargeAfterOnSaving);
				var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				service.ClearServiceCache();

				propertyInfo.SetValue(charge, new ZDecimal(400));

				charge.JR_Desc = "Description";
				Factory.Save();
				charge.JR_Desc = "New Description";

				Assert("HasChanges", charge.HasChanges);

				charge.HasChanges = false;
				var expectedMessageWithoutStack =
					@"Resetting HasChanges on Charge in DB with real Changes.
Type Name: Charge
{0}
Old IsSavedByFactory: True";

				AssertMultilineASCIIEquals("Should not add call stack if property does not have changes", string.Format(expectedMessageWithoutStack, charge.GetJobChargeInfo()), ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				service.GetInfo(charge.PK, key);
				propertyInfo.SetValue(charge, new ZDecimal(500));
				var info = service.GetInfo(charge.PK, key);
				Assert("HasChanges", charge.HasChanges);
				charge.HasChanges = false;
				var expectedMessageWithStack = expectedMessageWithoutStack + System.Environment.NewLine + info;

				AssertMultilineASCIIEquals("Should add call stack if property does have changes", string.Format(expectedMessageWithStack, charge.GetJobChargeInfo()), ErrorReporter.LastMessageReported);

				charge.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
				ErrorReporter.Clear();
			});
		}

		public void TestResttingHasChangesOnModifiedChargeInDbMessageContainsCallStack_MultipleStacks()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			Factory.Save();
			charge.SetContext(BusinessContext.JobChargeAfterOnSaving);
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			service.ClearServiceCache();

			GetPropertiesReturningCallStacksWithKeys().ForEach(x =>
			{
				var propertyInfo = charge.GetType().GetProperty(x.Item1, BindingFlags.Public | BindingFlags.Instance);
				service.GetInfo(charge.PK, x.Item2);
				propertyInfo.SetValue(charge, new ZDecimal(400));
			});

			Assert("HasChanges", charge.HasChanges);

			charge.HasChanges = false;

			var expectedMessageWithStack =
				$@"Resetting HasChanges on Charge in DB with real Changes.
Type Name: Charge
{charge.GetJobChargeInfo()}
Old IsSavedByFactory: True";

			AssertStartsWith("Message reported on resetting HasChanges starts correctly", expectedMessageWithStack, ErrorReporter.LastMessageReported);

			GetPropertiesReturningCallStacksWithKeys().ForEach(x =>
			{
				var name = x.Item1;
				var key = x.Item2;
				var info = service.GetInfo(charge.PK, key);
				AssertContains(string.Format("Message must contain call stack for {0}", name), info, ErrorReporter.LastMessageReported);
			});

			charge.RemoveContext(BusinessContext.JobChargeAfterOnSaving);
			ErrorReporter.Clear();
		}

		public void TestResttingHasChangesOnModifiedChargeInDbIsReported()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = job.PK;
			var charge3 = Factory.NewWithValidTestData<JobCharge>();
			charge3.JR_JH = job.PK;
			Factory.Save();

			charge.JR_Desc = "New Description";
			Assert("HasChanges", charge.HasChanges);
			Assert("No error should be reported", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals("No error should be reported", 0, ExceptionReporterTestListener.Instance.Count);

			charge.HasChanges = false;
			AssertEquals("An error should be reported", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("ChargesInDbModifiedWithoutHasChangesSet_5", ErrorReporter.LastKeyReported);
			var expectedMessage =
$@"Resetting HasChanges on Charge in DB with real Changes.
Type Name: Charge
{charge.GetJobChargeInfo()}
Old IsSavedByFactory: True";
			AssertMultilineASCIIEquals("Message reported on resetting HasChanges", expectedMessage, ErrorReporter.LastMessageReported);

			charge2.JR_OSCostAmt = 101m;
			Assert("HasChanges", charge2.HasChanges);

			charge2.HasChanges = false;
			AssertEquals("No new error should be reported as we ReportOnce", 1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("ChargesInDbModifiedWithoutHasChangesSet_5", ErrorReporter.LastKeyReported);
			AssertMultilineASCIIEquals("Message reported on resetting HasChanges", expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			charge3.JR_DisplaySequence = 100;
			Assert("HasChanges", charge3.HasChanges);

			charge3.HasChanges = false;
			AssertEquals("No error should be reported as JR_DisplaySequence change is not treated as real change", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestSetCostExchangeRateDifferentFromOneWithCostCurrencyEqualsToLocalCurrencyShouldReportDevError()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var charge = Factory.New<JobCharge>();
			AssertEquals("AUD", charge.JR_RX_NKCostCurrency);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, charge.JR_OSCostExRate);
			var actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency);
			AssertContains("JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency: There is no data collected for this PK.", actualMessage);

			charge.JR_RX_NKCostCurrency = "USD";
			AssertNotEquals(charge.JR_RX_NKCostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.JR_OSCostExRate = 1.2m;
			actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency);
			AssertContains("JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency: There is no data collected for this PK.", actualMessage);
			charge.JR_OSCostExRate = 1m;
			actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency);
			AssertContains("JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency: There is no data collected for this PK.", actualMessage);

			charge.JR_RX_NKCostCurrency = "AUD";
			AssertEquals(charge.JR_RX_NKCostCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			charge.JR_OSCostExRate = 1.2m;
			actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostExRateShouldBeOneWhenLocalCompanyCurrencyEqualsJobChargeCostCurrency);
			AssertContains("set_JR_OSCostExRate", actualMessage);
		}

		public void TestStackTraceInfoOnceOsSellAmtNotEqualToLocalSellAmtWhenLocalCurrencyIsUsed()
		{
			var service = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var charge = Factory.New<JobCharge>();
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellExRate = 1.5m;
			charge.JR_OSSellAmt = 150m;
			charge.JR_LocalSellAmt = 100m;
			charge.SetContext(BusinessContext.InvoicingPlugInGUI);
			Assert(charge.HasContext(BusinessContext.InvoicingPlugInGUI));
			AssertEquals("USD", charge.JR_RX_NKSellCurrency);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1.5m, charge.JR_OSSellExRate);
			var actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed);
			AssertContains("OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed: There is no data collected for this PK.", actualMessage);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(1m, charge.JR_OSSellExRate);
			actualMessage = service.GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed);
			AssertContains("set_JR_OSSellExRate", actualMessage);

			var anotherService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);

			var anotherCharge = Factory.New<JobCharge>();
			anotherCharge.JR_RX_NKSellCurrency = "AUD";
			anotherCharge.JR_OSSellExRate = 1m;
			anotherCharge.JR_OSSellAmt = 100m;
			anotherCharge.JR_LocalSellAmt = 100m;
			anotherCharge.SetContext(BusinessContext.InvoicingPlugInGUI);
			Assert(anotherCharge.HasContext(BusinessContext.InvoicingPlugInGUI));
			AssertEquals("AUD", anotherCharge.JR_RX_NKSellCurrency);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(1m, anotherCharge.JR_OSSellExRate);
			var anotherMessage = anotherService.GetInfo(anotherCharge.PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed);
			AssertContains("OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed: There is no data collected for this PK.", anotherMessage);

			anotherCharge.JR_OSSellAmt = 150m;
			anotherMessage = anotherService.GetInfo(anotherCharge.PK, CriticalValidationInfoCollectorServiceKeyType.OsSellAmountNotEqualToLocalSellAmountWhenLocalCurrencyIsUsed);
			AssertContains("set_JR_OSSellAmt", anotherMessage);
		}

		public void TestJR_OSSellAmtCollectsDeveloperInfoWhenNotEqualToPostedLineAmount()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(10, 1);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_OSSellAmt = 45m;
			jobCharge.JR_AT_SellGSTRate = taxRate.PK;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_JH = job.PK;
			transactionLine.AL_OSAmount = transactionLine.AL_LineAmount = 50m;
			transactionLine.AL_AT = taxRate.PK;
			transactionLine.AL_GSTVAT = 5m;
			jobCharge.JR_AL_ARLine = transactionLine.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;

			AssertContains("Precondition: GetInfoJobChargeOSSellAmtNotEqualRelatedLineOSAmount() first time", "There is no data collected for this PK", GetInfoJobChargeOSSellAmtNotEqualRelatedLineOSAmount());

			AssertNotNull("Precondition: ", jobCharge.ARLine);
			Assert("Precondition: ", jobCharge.IsRevenuePosted);
			AssertEquals("JR_OSSellGSTAmt_Calc", 5m, jobCharge.JR_OSSellGSTAmt_Calc);
			AssertEquals("Precondition: JR_OSSellAmt + JR_OSSellGSTAmt_Calc", transactionLine.AL_OSAmount, jobCharge.JR_OSSellAmt + jobCharge.JR_OSSellGSTAmt_Calc);
			AssertContains("Should not report error when charge_OSSellAndGSTAmount and transactionLine.AL_OSAmount are equal", "There is no data collected for this PK", GetInfoJobChargeOSSellAmtNotEqualRelatedLineOSAmount());

			jobCharge.JR_OSSellAmt = 50m;
			AssertNotEquals("Precondition: JR_OSSellAmt + JR_OSSellGSTAmt_Calc", transactionLine.AL_OSAmount, jobCharge.JR_OSSellAmt + jobCharge.JR_OSSellGSTAmt_Calc);

			var actualInfo = GetInfoJobChargeOSSellAmtNotEqualRelatedLineOSAmount();
			var expectedInfo =
$@"
JobChargeOSSellAmtNotEqualRelatedLineOSAmount:
JR_OSSellGSTAmt_Calc: 5
AL_OSAmount: 50
OSAmount: 55
JR_OSSellAmt has been changed from 45 to 50 after the revenue posted.

StackTrace:
";
			AssertContains("Should report an error when the OS Sell Amount and related line OS Amount not equal", expectedInfo, actualInfo);

			string GetInfoJobChargeOSSellAmtNotEqualRelatedLineOSAmount() => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmtNotEqualRelatedLineOSAmount);
		}

		public void TestJR_OSSellExRateCollectsDeveloperInfoWhenNotEqualToPostedLineExRate()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = job.PK;
			jobCharge.JR_OSSellAmt = 50m;
			jobCharge.JR_RX_NKSellCurrency = "USD";
			jobCharge.JR_OSSellExRate = 0.5m;

			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			transactionHeader.AH_TransactionType = TransactionTypes.Invoice;

			var transactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			transactionLine.AL_AH = transactionHeader.PK;
			transactionLine.AL_JH = job.PK;
			transactionLine.AL_OSAmount = transactionLine.AL_LineAmount = 50m;
			transactionLine.AL_RX_NKTransactionCurrency = "USD";
			transactionLine.AL_ExchangeRate = 0.5m;
			jobCharge.JR_AL_ARLine = transactionLine.PK;
			transactionLine.AL_LineType = TransactionLineTypes.Revenue;

			AssertContains("Precondition: GetInfoJobChargeOSSellExRateNotEqualRelatedLineExRate() first time", "There is no data collected for this PK", GetInfoJobChargeOSSellExRateNotEqualRelatedLineExRate());

			AssertNotNull("Precondition: ", jobCharge.ARLine);
			Assert("Precondition: ", jobCharge.IsRevenuePosted);
			AssertEquals("Precondition: ", transactionLine.AL_ExchangeRate, jobCharge.JR_OSSellExRate);
			AssertContains("Should not report error when job Charge OS Sell Exchange Rate and related Line Exchange Rate are equal", "There is no data collected for this PK", GetInfoJobChargeOSSellExRateNotEqualRelatedLineExRate());

			jobCharge.JR_OSSellExRate = 2m;
			AssertNotEquals("Precondition: ", transactionLine.AL_ExchangeRate, jobCharge.JR_OSSellExRate);

			var actualInfo = GetInfoJobChargeOSSellExRateNotEqualRelatedLineExRate();
			var expectedInfo =
$@"
JobChargeOSSellExRateNotEqualRelatedLineExRate:
JR_OSSellAmt: 50
JR_OSSellGSTAmt_Calc: 0
AL_OSAmount: 50
AL_ExchangeRate: 0.5
JR_OSSellExRate has been changed from 0.5 to 2 after the revenue posted.

StackTrace:
";
			AssertContains("Should report an error when OS Sell ExRate and related line ExRate not equal", expectedInfo, actualInfo);

			string GetInfoJobChargeOSSellExRateNotEqualRelatedLineExRate() => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellExRateNotEqualRelatedLineExRate);
		}

		public void TestJR_OSCostGSTAmtCollectsDeveloperInfoWhenChangedAfterCostPosted()
		{
			var taxRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
			taxRate.SetRate_ForTestOnly(10, 1);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 40;
			charge.JR_AT_CostGSTRate = taxRate.PK;

			var apHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			apHeader.AH_Ledger = LedgerTypes.AccountsPayable;
			apHeader.AH_TransactionType = TransactionTypes.Invoice;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = apHeader.PK;
			line.AL_JH = job.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			charge.JR_AL_APLine = line.PK;
			line.AL_OSAmount = line.AL_LineAmount = 44;
			line.AL_AT = taxRate.PK;

			AssertContains("Precondition: first time", "There is no data collected for this PK", GetInfoJobChargeOSCostGSTAmountChangedWhenCostPosted());

			AssertEquals("Precondition - Cost amount is equal to related line amount", charge.APLine.AL_OSAmount, charge.JR_OSCostAmt + charge.JR_OSCostGSTAmt_Calc);
			Assert("Precondition: Cost is posted", charge.IsCostPosted);
			AssertContains("There is no data collected for this PK", GetInfoJobChargeOSCostGSTAmountChangedWhenCostPosted());

			var expectedInfo =
$@"JobChargeOSCostGSTAmountChangedWhenCostPosted:

Old value: 4.0,
New Value: 15

StackTrace:
";

			charge.JR_OSCostGSTAmt_Calc = 15;
			AssertNotEquals("Precondition - Cost amount not equal to related line amount", charge.APLine.AL_OSAmount, charge.JR_OSCostAmt + charge.JR_OSCostGSTAmt);

			AssertContains(expectedInfo, GetInfoJobChargeOSCostGSTAmountChangedWhenCostPosted());

			string GetInfoJobChargeOSCostGSTAmountChangedWhenCostPosted() => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(charge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOSCostGSTAmountChangedWhenCostPosted);
		}

		public void TestChaningSellAccountOnChargeInDbWithHasChangesSuspendedIsReported()
		{
			AssertEquals("Context is not InvoicingPlugInGUI", false, Factory.HasContext(BusinessContext.InvoicingPlugInGUI));
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			var debtor = Factory.NewWithValidTestData<OrgHeader>();
			charge.JR_OH_SellAccount = debtor.PK;

			var charge2 = Factory.NewWithValidTestData<JobCharge>();
			charge2.JR_JH = job.PK;
			var debtor2 = Factory.NewWithValidTestData<OrgHeader>();
			charge2.JR_OH_SellAccount = debtor2.PK;

			var debtor3 = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (charge.SuspendSettingHasChanges())
			{
				charge.JR_OH_SellAccount = debtor2.PK;
			}

			AssertEquals("An error should be reported", 1, ExceptionReporterTestListener.Instance.Count);
			Assert("ChargesInDbModifiedWithoutHasChangesSet_5 key was reported", ErrorReporter.HasBeenReported("ChargesInDbModifiedWithoutHasChangesSet_5"));
			AssertEquals("ChargesInDbModifiedWithoutHasChangesSet_5", ErrorReporter.LastKeyReported);
			AssertStartsWith("Message reported on setting JR_OH_SellAccount", "Setting JR_OH_SellAccount when setting HasChanges is susupended.", ErrorReporter.LastMessageReported);
			AssertContains("Should contain charge info", charge.GetJobChargeInfo(), ErrorReporter.LastMessageReported);

			using (charge2.SuspendSettingHasChanges())
			{
				charge2.JR_OH_SellAccount = debtor.PK;
			}

			AssertEquals("No new error should be reported", 1, ExceptionReporterTestListener.Instance.Count);
			Assert("ChargesInDbModifiedWithoutHasChangesSet_5 key was reported", ErrorReporter.HasBeenReported("ChargesInDbModifiedWithoutHasChangesSet_5"));
			AssertEquals("ChargesInDbModifiedWithoutHasChangesSet_5", ErrorReporter.LastKeyReported);
			AssertStartsWith("Message reported on setting JR_OH_SellAccount", "Setting JR_OH_SellAccount when setting HasChanges is susupended.", ErrorReporter.LastMessageReported);
			AssertContains("Should contain charge info", charge.GetJobChargeInfo(), ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			charge.JR_OH_SellAccount = debtor3.PK;
			AssertEquals("No error should be reported when setting HasChanges is not suspended", 0, ExceptionReporterTestListener.Instance.Count);

			AssertEquals("Current value", debtor.PK, charge2.JR_OH_SellAccount);
			using (charge2.SuspendSettingHasChanges())
			{
				charge2.JR_OH_SellAccount = debtor.PK;
			}
			AssertEquals("No error should be reported when setting the same value", 0, ExceptionReporterTestListener.Instance.Count);

			Factory.SetContext(BusinessContext.SetDefaultsForJob);
			using (charge2.SuspendSettingHasChanges())
			{
				charge2.JR_OH_SellAccount = debtor2.PK;
			}
			AssertEquals("No error should be reported when it is in SetDefaultsForJob context", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestUniversalCopyIgnoreBusinessObjectAttribute()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var componentType = charge.GetType();
			var ignoreBoAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreBusinessObjectAttribute), true);
			AssertNotNull("UniversalCopyIgnoreBusinessObject Attribute", ignoreBoAttributes[0] as UniversalCopyIgnoreBusinessObjectAttribute);
		}

		public void TestDisableWorkflowSettingPropertiesAfterOnSavingAttribute()
		{
			var charge = Factory.NewWithValidTestData<JobCharge>();
			var componentType = charge.GetType();
			var ignoreBoAttributes = componentType.GetCustomAttributes(typeof(DisableWorkflowSettingPropertiesAfterOnSavingAttribute), true);
			AssertEquals(1, ignoreBoAttributes.Length);
			AssertNotNull("DisableWorkflowSettingPropertiesAfterOnSaving Attribute", ignoreBoAttributes[0] as DisableWorkflowSettingPropertiesAfterOnSavingAttribute);
		}

		public void TestJR_OSCostAmt_JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.JR_JH = job.PK;

			AssertContains("Precondition: GetInfoForJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount() first time", "There is no data collected for this PK", GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount());

			AssertEquals(ZDecimal.Zero, jobCharge.JR_OSCostAmt);
			AssertContains("Should not report error when job charge OS Cost amount is 0", "There is no data collected for this PK", GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount());

			AssertEquals(ZDecimal.Zero, jobCharge.JR_LocalCostAmt);
			AssertContains("Should not report error when job charge Local Cost amount is 0", "There is no data collected for this PK", GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount());

			jobCharge.JR_OSCostAmt = 50m;
			AssertNotEquals("Precondition: jobCharge.JR_OSCostAmt", 0, jobCharge.JR_OSCostAmt);
			AssertNotEquals("Precondition: jobCharge.JR_LocalCostAmt", 0, jobCharge.JR_LocalCostAmt);

			AssertEquals(Math.Sign(jobCharge.JR_OSCostAmt), Math.Sign(jobCharge.JR_LocalCostAmt));
			AssertContains("Should not report error when job charge OS and Local Amounts have same signs", "There is no data collected for this PK", GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount());

			jobCharge.JR_OSCostAmt = -100m;
			((IBusinessObjectInternals)jobCharge).Row[JobChargeSchema.Constants.JR_LocalCostAmt] = 100m;
			AssertNotEquals("Precondition: Signs of Job Charge OS and Local Cost Amounts", Math.Sign(jobCharge.JR_OSCostAmt), Math.Sign(jobCharge.JR_LocalCostAmt));

			var actualInfo = GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount();
			var expectedInfo =
$@"
JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount:
{JobChargeSchema.JR_OSCostAmt.Name} has been changed from 50 to -100.
";
			AssertContains("Should report an error as the OS and Local Amounts signs not matching", expectedInfo, actualInfo);

			string GetInfoJobChargeOsCostAmountNotMatchingSignOfLocalCostAmount() => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(jobCharge.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeOsCostAmountNotMatchingSignOfLocalCostAmount);
		}

		public void TestModifyingTaxRateOnPostedChargeIsReported()
		{
			var invoiceAP = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceAP.AH_Ledger = LedgerTypes.AccountsPayable;
			invoiceAP.AH_TransactionType = TransactionTypes.Invoice;
			var invoiceAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceAR.AH_TransactionType = TransactionTypes.Invoice;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(10);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge.JR_AL_APLine = line.PK;

			var line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_LineType = TransactionLineTypes.Revenue;
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			charge.JR_AL_ARLine = line2.PK;

			line.AL_AH = invoiceAP.PK;
			line2.AL_AH = invoiceAR.PK;

			Factory.Save();

			AssertEquals(true, charge.IsInDatabase);
			AssertEquals(true, line.IsInDatabase);
			AssertEquals(true, line2.IsInDatabase);
			AssertEquals(false, charge.HasChanges);
			AssertEquals(false, line.HasChanges);
			AssertEquals(false, line2.HasChanges);
			AssertEquals(true, charge.IsCostPosted);
			AssertEquals(true, charge.IsRevenuePosted);

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_AT_CostGSTRate = taxRate.PK;

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("JobCharge.ModifyingTaxRateOnPostedCharge", ErrorReporter.LastKeyReported);
			AssertStartsWith("", "Modifying Tax Rate on Posted Charge.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_AT_SellGSTRate = taxRate.PK;

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("JobCharge.ModifyingTaxRateOnPostedCharge", ErrorReporter.LastKeyReported);
			AssertStartsWith("", "Modifying Tax Rate on Posted Charge.", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestModifyingTaxRateOnPostedChargeIsReportedWithConsolCostDetails()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.FillWithValidTestData();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 10m;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 10m;
			charge.FillWithValidTestData();

			var consol = Factory.New(ObjectFactory.GetType<IForwardingConsol>(), new Guid("4aa7cf1c-95f4-4a6e-a84b-b1e59133e300"));
			consol.FillWithValidTestData();

			var consolCost = Factory.New(ObjectFactory.GetType<IJobConsolCost>(), new Guid("19b4fb21-a8cb-428a-bec7-a6ef01249918"));
			consolCost[JobConsolCostSchema.E6_AC_ChargeCode.Name] = charge.JR_AC;
			consolCost[JobConsolCostSchema.E6_OSCostAmount.Name] = consolCost[JobConsolCostSchema.E6_LocalCostAmount.Name] = 10m;
			consolCost.FillWithValidTestData();

			charge.JR_E6 = consolCost.PK;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.SetRateNumerator_ForTestOnly(10);

			var invoiceAP = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceAP.AH_Ledger = LedgerTypes.AccountsPayable;
			invoiceAP.AH_TransactionType = TransactionTypes.Invoice;
			var invoiceAR = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoiceAR.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceAR.AH_TransactionType = TransactionTypes.Invoice;

			var aPLine = Factory.NewWithValidTestData<AccTransactionLines>();
			aPLine.AL_AH = invoiceAP.PK;
			aPLine.AL_JH = job.PK;
			aPLine.AL_LineType = TransactionLineTypes.Cost;
			aPLine.AL_OSAmount = aPLine.AL_LineAmount = -10m;
			aPLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var aRLine = Factory.NewWithValidTestData<AccTransactionLines>();
			aRLine.AL_AH = invoiceAR.PK;
			aRLine.AL_JH = job.PK;
			aRLine.AL_LineType = TransactionLineTypes.Revenue;
			aRLine.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;
			aRLine.AL_OSAmount = aRLine.AL_LineAmount = 10m;

			charge.JR_AL_APLine = aPLine.PK;
			charge.JR_AL_ARLine = aRLine.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AH_APInvoice] = invoiceAP.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AH_ARInvoice] = invoiceAR.PK;

			Factory.Save();

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			AssertEquals(true, charge.IsInDatabase);
			AssertEquals(false, charge.HasChanges);
			AssertEquals(true, charge.IsCostPosted);
			AssertEquals(true, charge.IsRevenuePosted);
			AssertEquals(true, aPLine.IsInDatabase);
			AssertEquals(false, aPLine.HasChanges);
			AssertEquals(true, aRLine.IsInDatabase);
			AssertEquals(false, aRLine.HasChanges);

			charge.JR_AT_CostGSTRate = taxRate.PK;

			AssertEquals(true, charge.HasChanges);

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("JobCharge.ModifyingTaxRateOnPostedCharge", ErrorReporter.LastKeyReported);
			AssertStartsWith("", "Modifying Tax Rate on Posted Charge.", ErrorReporter.LastMessageReported);
			AssertContains("Job Consol Cost:\tPK = 19b4fb21-a8cb-428a-bec7-a6ef01249918\r\n\tType = JobConsolCost", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			charge.JR_AT_SellGSTRate = taxRate.PK;

			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("JobCharge.ModifyingTaxRateOnPostedCharge", ErrorReporter.LastKeyReported);
			AssertStartsWith("", "Modifying Tax Rate on Posted Charge.", ErrorReporter.LastMessageReported);
			AssertContains("Job Consol Cost:\tPK = 19b4fb21-a8cb-428a-bec7-a6ef01249918\r\n\tType = JobConsolCost", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestModifyingSellAmountOnPostedChargeIsReported()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedLineAmount);
			var charge = SetupDataForModifyingAmountOnPostedChargeTest();

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 60.00m;
			AssertExceptionThrown<OnSavingCriticalCheckException<JobCharge>>(() => Factory.Save());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("JR_LocalSellAmt has been changed from 50.00 to 60.00 after the revenue posted.\r\n   at System.Environment.GetStackTrace", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ErrorReporter.Clear();
		}

		public void TestModifyingCostAmountOnPostedChargeIsReported()
		{
			CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(ZGuid.NewZGuid(), CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedLineAmount);
			var charge = SetupDataForModifyingAmountOnPostedChargeTest();

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 40.56m;
			AssertExceptionThrown<OnSavingCriticalCheckException<JobCharge>>(() => Factory.Save());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertContains("JR_LocalCostAmt has been changed from 39.64 to 40.56 after the cost posted.\r\n   at System.Environment.GetStackTrace", ExceptionReporterTestListener.Instance[0].InnerException.Message);
			ErrorReporter.Clear();
		}

		JobCharge SetupDataForModifyingAmountOnPostedChargeTest()
		{
			var apHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			apHeader.AH_Ledger = LedgerTypes.AccountsPayable;
			apHeader.AH_TransactionType = TransactionTypes.Invoice;
			var arHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			arHeader.AH_Ledger = LedgerTypes.AccountsReceivable;
			arHeader.AH_TransactionType = TransactionTypes.Invoice;

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_AH = apHeader.PK;
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_OSAmount = line.AL_LineAmount = -39.64m;
			line.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var line2 = Factory.NewWithValidTestData<AccTransactionLines>();
			line2.AL_AH = arHeader.PK;
			line2.AL_LineType = TransactionLineTypes.Revenue;
			line2.AL_OSAmount = line2.AL_LineAmount = 50.00m;
			line2.AL_AG = Factory.NewWithValidTestData<AccGLHeader>().PK;

			Factory.Save();

			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;
			charge.JR_OSCostAmt = charge.JR_LocalCostAmt = 39.64m;
			charge.JR_OSSellAmt = charge.JR_LocalSellAmt = 50.00m;
			charge.JR_AL_APLine = line.PK;
			charge.JR_AL_ARLine = line2.PK;

			AssertEquals(false, charge.IsInDatabase);
			AssertEquals(true, line.IsInDatabase);
			AssertEquals(true, line2.IsInDatabase);
			AssertEquals(false, line.HasChanges);
			AssertEquals(false, line2.HasChanges);
			AssertEquals(true, charge.IsCostPosted);
			AssertEquals(true, charge.IsRevenuePosted);

			return charge;
		}

		public void TestIsInDatabaseAndReadyForCostPostingConcurrentModification()
		{
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.FillWithValidTestData();
			jobCharge.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;

			Factory.Save();
			Assert(jobCharge.IsInDatabaseAndReadyForCostPosting);

			jobCharge.Delete();
			var result = true;
			AssertNoExceptionThrown(
				"IsInDatabaseAndReadyForCostPosting should be false if the JobCarge was detached",
				() => result = jobCharge.IsInDatabaseAndReadyForCostPosting);

			Assert(!result);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestIsInDatabaseAndReadyForRevenuePostingConcurrentModification()
		{
			var jobCharge = Factory.New<JobCharge>();
			jobCharge.FillWithValidTestData();
			jobCharge.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;

			Factory.Save();
			Assert(jobCharge.IsInDatabaseAndReadyForRevenuePosting);

			jobCharge.Delete();
			var result = true;
			AssertNoExceptionThrown(
				"IsInDatabaseAndReadyForRevenuePosting should be false if the JobCarge was detached",
				() => result = jobCharge.IsInDatabaseAndReadyForRevenuePosting);

			Assert(!result);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestSetJR_GB()
		{
			Charge.JR_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals(GlbBranch.CurrentBranch.GB_GC, Charge.JR_GC);

			Charge.JR_GC = ZGuid.Empty;
			Assert(Charge.JR_GCInfo.HasErrors());
			AssertHasError(Charge.JR_GCInfo, "Please enter valid company.");

			Charge.JR_GC = ZGuid.NewZGuid();
			Assert(Charge.JR_GCInfo.HasErrors());
			AssertHasError(Charge.JR_GCInfo, "The Company you entered doesn't match the Branch you entered.");
		}

		public void TestSetJR_RX_NKCostCurrency()
		{
			var exchangeRateFactory = new BusinessObjectFactory();
			var usd = RefCurrency.LoadFromCurrencyCode(exchangeRateFactory, USD.Code);
			RefExchangeRate todaysRate = null;

			try
			{
				todaysRate = CreateBuyRefExchangeRate(usd, ZDateTime.Today, 0.75m);
				exchangeRateFactory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();

				Charge.JR_GC = GlbCompany.CurrentCompany.PK;
				AssertEquals("AUD", GlbCompany.CurrentCompany.LocalCurrency.Code);

				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				Charge.JR_JH = jobHeader.PK;
				jobHeader.Parent = GetMockInvoicingPlugIn().Object;

				Charge.JR_RX_NKCostCurrency = USD.RX_Code;
				var jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNotNull(jobExRate);
				AssertEquals(0.75m, jobExRate.Rate);
				AssertEquals(0.75m, Charge.JR_OSCostExRate);

				Charge.JR_RX_NKCostCurrency = string.Empty; // Temporary reset currency
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNull(jobExRate);
				AssertEquals(1m, Charge.JR_OSCostExRate);

				Charge.JR_RX_NKCostCurrency = USD.RX_Code;
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNotNull(jobExRate);
				AssertEquals(0.75m, jobExRate.Rate);
				AssertEquals(0.75m, Charge.JR_OSCostExRate);

				Charge.JR_OSCostAmt = 100m;
				AssertEquals(133.33m, Charge.JR_LocalCostAmt);

				Charge.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNull(jobExRate);
				AssertEquals(1m, Charge.JR_OSCostExRate);
				AssertEquals(133.33m, Charge.JR_OSCostAmt);
				AssertEquals(133.33m, Charge.JR_LocalCostAmt);
			}
			finally
			{
				todaysRate?.Delete();
				exchangeRateFactory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		public void TestSetJR_RX_NKSellCurrency()
		{
			var exchangeRateFactory = new BusinessObjectFactory();
			var usd = RefCurrency.LoadFromCurrencyCode(exchangeRateFactory, USD.Code);
			RefExchangeRate todaysRate = null;

			try
			{
				todaysRate = CreateBuyRefExchangeRate(usd, ZDateTime.Today, 0.75m);
				exchangeRateFactory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();

				Charge.JR_GC = GlbCompany.CurrentCompany.PK;
				AssertEquals("AUD", GlbCompany.CurrentCompany.LocalCurrency.Code);
				var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				Charge.JR_JH = jobHeader.PK;
				jobHeader.Parent = GetMockInvoicingPlugIn().Object;

				Charge.JR_RX_NKSellCurrency = USD.RX_Code;
				var jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNotNull(jobExRate);
				AssertEquals(0.75m, jobExRate.Rate);
				jobExRate.SetBuyRate_ForTestOnly(0.75m);

				Charge.JR_RX_NKSellCurrency = string.Empty;   // Temporary reset currency
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNull(jobExRate);
				AssertEquals(0.75m, Charge.JR_OSSellExRate);

				Charge.JR_RX_NKSellCurrency = USD.RX_Code;
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNotNull(jobExRate);
				AssertEquals(0.75m, jobExRate.Rate);
				AssertEquals(0.75m, Charge.JR_OSSellExRate);

				Charge.JR_OSSellAmt = 100m;
				AssertEquals(133.33m, Charge.JR_LocalSellAmt);

				Charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.LocalCurrency.Code;
				jobExRate = ((IExchangeRateSourceBase)jobHeader).FirstOrDefault(x => x.CurrencyCode == USD.RX_Code);
				AssertNull(jobExRate);
				AssertEquals(1m, Charge.JR_OSSellExRate);
				AssertEquals(133.33m, Charge.JR_OSSellAmt);//100m
				AssertEquals(133.33m, Charge.JR_LocalSellAmt);//100m
			}
			finally
			{
				todaysRate?.Delete();
				exchangeRateFactory.Save();
				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		#region BillInInvoiceCurrency

		public void TestJR_RX_NKSellInvoiceCurrency()
		{
			Assert("Default value", Charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);

			Charge.JR_RX_NKSellInvoiceCurrency = USD.RX_Code;
			AssertEquals("Assigned value", USD.RX_Code, Charge.JR_RX_NKSellInvoiceCurrency);

			Assert("InvoiceType not for BillInInvoiceCurrency", !InvoiceTypeCalculationProvider.BillInLocalCurrency(InvoiceTypesList.Codes.ForeignCurrencyInvoice));
			Charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			Assert("JR_RX_NKSellInvoiceCurrency should be reset by setting unsupported InvoiceType", Charge.JR_RX_NKSellInvoiceCurrency.IsEmpty);
		}

		public void TestBillInInvoiceCurrency_BillInLocalCurrency_BillInInvoiceCurrencyWithLocalSellCurrency_IsApplyCFX()
		{
			var supportedInvoiceTypes = new string[] {
				InvoiceTypesList.Codes.FinalInvoice,
				InvoiceTypesList.Codes.FinalInvoice_Batching,
				InvoiceTypesList.Codes.DisbursementInvoice,
				InvoiceTypesList.Codes.DisbursementInvoice_Batching,
				InvoiceTypesList.Codes.InvoicePerTaxCode,
				InvoiceTypesList.Codes.InvoicePerTaxCode_Batching,
				InvoiceTypesList.Codes.DestinationChargesInvoice,
				InvoiceTypesList.Codes.DestinationChargesInvoice_Batching,
				InvoiceTypesList.Codes.DoNotPost,
				AgencyInvoiceTypesList.Codes.LocalPrePaid,
				AgencyInvoiceTypesList.Codes.LocalPrePaid_Batching,
				AgencyInvoiceTypesList.Codes.LocalCollect,
				AgencyInvoiceTypesList.Codes.LocalCollect_Batching,
				AgencyInvoiceTypesList.Codes.Misc,
				AgencyInvoiceTypesList.Codes.Misc_Batching,
			};
			var companyPK = Charge.JR_GC;

			AssertEquals("Precondition: Sell Currency should not be foreign", false, Charge.IsSellForeign);
			AssertEquals("Precondition: Charge.IsApplyCFX should be false", false, Charge.IsApplyCFX);

			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.JobInvoicingCFXEnabled(It.IsAny<Guid>())).Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			{
				CombineAssertions(() =>
				{
					Charge.JR_RX_NKSellCurrency = "EUR";

					foreach (var invoiceType in new InvoiceTypesList().GetAllCodes().Union(new AgencyInvoiceTypesList().GetAllCodes()))
					{
						Assert("Precondition: Sell Currency should be foreign", Charge.IsSellForeign);
						var expectedResult = supportedInvoiceTypes.Contains(invoiceType);

						Charge.JR_InvoiceType = invoiceType;
						AssertEquals($"InvoiceTypeCalculationProvider.BillInLocalCurrency for InvoiceType '{invoiceType}'", expectedResult, InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType));
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Should be false when JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", expectedResult, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = USD.Code;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", expectedResult, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = Charge.Company.GC_RX_NKLocalCurrency;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' when JR_RX_NKSellInvoiceCurrency is local", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Charge.BillInInvoiceCurrency false when JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency and JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' when JR_RX_NKSellInvoiceCurrency is local", expectedResult, Charge.IsApplyCFX);

						Charge.JR_GC = ZGuid.Empty;
						AssertEquals("Charge.BillInLocalCurrency false when JR_GC is not set", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' when JR_GC is not set", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals("Charge.IsApplyCFX false when JR_GC is not set", false, Charge.IsApplyCFX);
						Charge.JR_GC = companyPK;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' when JR_GC is set back and we can check that JR_RX_NKSellInvoiceCurrency is local", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Should be false when JR_GC is set back and we can check that JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' when JR_GC is set back", expectedResult, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Should be false when JR_RX_NKSellInvoiceCurrency is empty", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with foreign JR_RX_NKSellCurrency", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", expectedResult, Charge.IsApplyCFX);
					}
				});

				CombineAssertions(() =>
				{
					Charge.JR_RX_NKSellCurrency = Charge.Company.GC_RX_NKLocalCurrency;

					foreach (var invoiceType in new InvoiceTypesList().GetAllCodes().Union(new AgencyInvoiceTypesList().GetAllCodes()))
					{
						Assert("Precondition: Sell Currency should be local", Charge.IsSellLocal);
						var expectedResult = supportedInvoiceTypes.Contains(invoiceType);

						Charge.JR_InvoiceType = invoiceType;
						AssertEquals($"InvoiceTypeCalculationProvider.BillInLocalCurrency for InvoiceType '{invoiceType}'", expectedResult, InvoiceTypeCalculationProvider.BillInLocalCurrency(invoiceType));
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Should be false when JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", false, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = USD.Code;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", expectedResult, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency set", expectedResult, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = Charge.Company.GC_RX_NKLocalCurrency;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' when JR_RX_NKSellInvoiceCurrency is local", expectedResult, Charge.BillInLocalCurrency);
						AssertEquals("Charge.BillInInvoiceCurrency false when JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency false when JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' when JR_RX_NKSellInvoiceCurrency is local", false, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellInvoiceCurrency = USD.Code;
						Charge.JR_GC = ZGuid.Empty;
						AssertEquals("Charge.BillInLocalCurrency false when JR_GC is not set", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' when JR_GC is not set", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' when JR_GC is not set", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals("Charge.IsApplyCFX false when JR_GC is not set", false, Charge.IsApplyCFX);
						Charge.JR_GC = companyPK;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' when JR_GC is set back and we can check that JR_RX_NKSellInvoiceCurrency is local", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' when JR_GC is set back", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' when JR_GC is set back", expectedResult, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' when JR_GC is set back", expectedResult, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellCurrency = ZString.Empty;
						Charge.JR_InvoiceType = invoiceType;
						Charge.JR_RX_NKSellInvoiceCurrency = USD.Code;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' when JR_RX_NKSellCurrency is not set", false, Charge.BillInLocalCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrency for InvoiceType '{invoiceType}' when JR_RX_NKSellCurrency is not set", expectedResult, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' when JR_RX_NKSellCurrency is not set", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX for InvoiceType '{invoiceType}' when JR_GC is set back", false, Charge.IsApplyCFX);

						Charge.JR_RX_NKSellCurrency = Charge.Company.GC_RX_NKLocalCurrency;
						Charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
						AssertEquals($"Charge.BillInLocalCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", true, Charge.BillInLocalCurrency);
						AssertEquals("Should be false when JR_RX_NKSellInvoiceCurrency is empty", false, Charge.BillInInvoiceCurrency);
						AssertEquals($"Charge.BillInInvoiceCurrencyWithLocalSellCurrency for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", false, Charge.BillInInvoiceCurrencyWithLocalSellCurrency);
						AssertEquals($"Charge.IsApplyCFX false for InvoiceType '{invoiceType}' with JR_RX_NKSellInvoiceCurrency not set", false, Charge.IsApplyCFX);
					}
				});
			}
		}

		#endregion

		public void TestHasSecurityErrorChangeJobStatusForCostPosting()
		{
			var oldSecurityValue = Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed;

			try
			{
				Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed = false;

				//JobReadyForCostPosting
				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				charge1.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForCostPosting", false, charge1.HasSecurityErrorChangeJobStatusForCostPosting);
				charge1.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should have security error", true, charge1.HasSecurityErrorChangeJobStatusForCostPosting);
				charge1.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
				AssertEquals("Should not have security error", false, charge1.HasSecurityErrorChangeJobStatusForCostPosting);

				//JobReadyForRevenueAndCostPosting
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForCostPosting", false, charge2.HasSecurityErrorChangeJobStatusForCostPosting);
				charge2.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should have security error", true, charge2.HasSecurityErrorChangeJobStatusForCostPosting);
				charge2.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
				AssertEquals("Should not have security error", false, charge2.HasSecurityErrorChangeJobStatusForCostPosting);

				//JobReadyForRevenuePosting
				var charge3 = Factory.NewWithValidTestData<JobCharge>();
				charge3.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForCostPosting", false, charge3.HasSecurityErrorChangeJobStatusForCostPosting);
				charge3.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should not have security error", false, charge3.HasSecurityErrorChangeJobStatusForCostPosting);

				//InvoiceOnHold
				var charge4 = Factory.NewWithValidTestData<JobCharge>();
				charge4.Job.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForCostPosting", false, charge4.HasSecurityErrorChangeJobStatusForCostPosting);
				charge4.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should not have security error", false, charge4.HasSecurityErrorChangeJobStatusForCostPosting);
			}
			finally
			{
				Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed = oldSecurityValue;
			}
		}

		public void TestHasSecurityErrorChangeJobStatusForRevenuePosting()
		{
			var oldSecurityValue = Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed;

			try
			{
				Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed = false;

				//JobReadyForRevenuePosting
				var charge1 = Factory.NewWithValidTestData<JobCharge>();
				charge1.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForRevenuePosting", false, charge1.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge1.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should have security error", true, charge1.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge1.Job.JH_Status = JobHeaderStatus.JobReadyForRevenuePosting.Code;
				AssertEquals("Should not have security error", false, charge1.HasSecurityErrorChangeJobStatusForRevenuePosting);

				//JobReadyForRevenueAndCostPosting
				var charge2 = Factory.NewWithValidTestData<JobCharge>();
				charge2.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForRevenuePosting", false, charge2.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge2.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should have security error", true, charge2.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge2.Job.JH_Status = JobHeaderStatus.JobReadyForRevenueAndCostPosting.Code;
				AssertEquals("Should not have security error", false, charge2.HasSecurityErrorChangeJobStatusForRevenuePosting);

				//JobReadyForCostPosting
				var charge3 = Factory.NewWithValidTestData<JobCharge>();
				charge3.Job.JH_Status = JobHeaderStatus.JobReadyForCostPosting.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForRevenuePosting", false, charge3.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge3.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should not have security error", false, charge3.HasSecurityErrorChangeJobStatusForRevenuePosting);

				//InvoiceOnHold
				var charge4 = Factory.NewWithValidTestData<JobCharge>();
				charge4.Job.JH_Status = JobHeaderStatus.InvoiceOnHold.Code;
				Factory.Save();

				AssertEquals("Precondition: HasSecurityErrorChangeJobStatusForRevenuePosting", false, charge4.HasSecurityErrorChangeJobStatusForRevenuePosting);
				charge4.Job.JH_Status = JobHeaderStatus.Working.Code;
				AssertEquals("Should not have security error", false, charge4.HasSecurityErrorChangeJobStatusForRevenuePosting);
			}
			finally
			{
				Env.Security.ChangeStatusOfReadyToPostJobs.IsAllowed = oldSecurityValue;
			}
		}

		public void TestIsSellForeignAndIsSellLocal()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			uSCompany.GC_Name = "Your US Company";
			uSCompany.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			var chicagoBranch = Factory.NewWithValidTestData<GlbBranch>();
			chicagoBranch.GB_GC = uSCompany.PK;

			Factory.Save();

			Charge.JR_GC = GlbCompany.CurrentCompany.PK;
			Charge.JR_RX_NKSellCurrency = "USD";

			AssertEquals("Eagle Datamation International", GlbCompany.CurrentCompany.GC_Name);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Charge belongs to AU company and sell currency is USD", Charge.IsSellForeign);
			Assert("Charge belongs to AU company and sell currency is USD", !Charge.IsSellLocal);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, chicagoBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Your US Company", GlbCompany.CurrentCompany.GC_Name);
				AssertEquals("USD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Charge belongs to AU company and sell currency is USD", Charge.IsSellForeign);
				Assert("Charge belongs to AU company and sell currency is USD", !Charge.IsSellLocal);
			}

			Charge.JR_RX_NKSellCurrency = "AUD";

			AssertEquals("Eagle Datamation International", GlbCompany.CurrentCompany.GC_Name);
			AssertEquals("AUD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			Assert("Charge belongs to AU company and sell currency is AUD", !Charge.IsSellForeign);
			Assert("Charge belongs to AU company and sell currency is AUD", Charge.IsSellLocal);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, chicagoBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AssertEquals("Your US Company", GlbCompany.CurrentCompany.GC_Name);
				AssertEquals("USD", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				Assert("Charge belongs to AU company and sell currency is AUD", !Charge.IsSellForeign);
				Assert("Charge belongs to AU company and sell currency is AUD", Charge.IsSellLocal);
			}
		}

		public void TestIsSellInvoiceForeign()
		{
			Charge.JR_GC = GlbCompany.CurrentCompany.PK;

			Assert("AU company and null SellInvoiceCurrency", !Charge.IsSellInvoiceForeign);

			Charge.JR_RX_NKSellInvoiceCurrency = ZString.Empty;
			Assert("AU company and Empty SellInvoiceCurrency Code", !Charge.IsSellInvoiceForeign);

			Charge.JR_RX_NKSellInvoiceCurrency = "AUD";
			Assert("AU company and AUD SellInvoiceCurrency Code", !Charge.IsSellInvoiceForeign);

			Charge.JR_RX_NKSellInvoiceCurrency = "USD";
			Assert("AU company and USD SellInvoiceCurrency Code", Charge.IsSellInvoiceForeign);
		}

		public void TestJobChargeTypeDecider()
		{
			Type iChargeType = ObjectFactory.GetType<ICharge>();

			var newCharge = Factory.NewWithValidTestData<JobCharge>();
			Assert("New JobCharge is ICharge", newCharge is ICharge);
			AssertEquals("New JobCharge has ICharge type", iChargeType, newCharge.GetType());
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedCharge = newFactory.Load<JobCharge>(newCharge.PK);
			Assert("Loaded JobCharge is ICharge", loadedCharge is ICharge);
			AssertEquals("Loaded JobCharge has ICharge type", iChargeType, loadedCharge.GetType());
		}

		public void TestDeletingChargeWithLinkedREVLineNotInDbHasStackTrace()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_JH = job.PK;

			var invoice = Factory.NewWithValidTestData<AccTransactionHeader>();
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_TransactionType = TransactionTypes.Invoice;

			var line = Factory.NewWithValidTestData<AccTransactionLines>();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AH = invoice.PK;
			line.AL_JH = job.PK;

			charge.JR_AL_ARLine = line.PK;

			IBusiness chargeForIBusiness = charge;

			chargeForIBusiness.DeleteForDataRefresh();

			string expectedErrMsg = "REV transaction line that does not have a related job charge";
			string expectedStackTraceMsg = "A stack trace should appear here";
			var collectorService = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
			collectorService.AddInfoWhenAllowed(line.PK, CriticalValidationInfoCollectorServiceKeyType.DeletingChargeWithLinkedREVLineNotInDb, () => expectedStackTraceMsg, CriticalValidationInfoCollectorService.CollectionFrequency.CollectAlways_ForEveryUserAndAction_THIS_CAN_IMPACT_PERFORMANCE);

			try
			{
				Factory.Save();
				Fail("Critical Validation should prevent saving");
			}
			catch (OnSavingCriticalCheckException ex)
			{
				AssertContains("Critical Validation Error", expectedErrMsg, ex.Message);
				AssertContains("Critical Validation Error", expectedStackTraceMsg, ex.DeveloperErrorMessage);
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestGetIsRevenuePosted()
		{
			Assert(JobCharge.GetIsRevenuePosted(TransactionLineTypes.Revenue));

			Assert(!JobCharge.GetIsRevenuePosted(TransactionLineTypes.Accrual));
			Assert(!JobCharge.GetIsRevenuePosted(TransactionLineTypes.Cost));
			Assert(!JobCharge.GetIsRevenuePosted(TransactionLineTypes.UnapprovedCost));
			Assert(!JobCharge.GetIsRevenuePosted(TransactionLineTypes.WIP));
		}

		public void TestValidateJR_LocalCostAmt_IgnoreValidationSuspended()
		{
			Factory.SuspendValidation();

			var consolcost = Factory.New(ObjectFactory.GetType<IJobConsolCost>());
			var charge = Factory.New<JobCharge>();
			charge.JR_E6 = consolcost.PK;
			AssertNoErrors(charge.JR_LocalCostAmtInfo);

			charge.JR_OSCostAmt = 210m;
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSCostExRate = 0m;
			AssertEquals(false, charge.JR_LocalCostAmtInfo.HasErrors());

			Factory.SetContext(BusinessContext.PostManagerCreatingTransaction);
			consolcost.SetContext(ConsolCostStrategy.ConsolCostCalculationStrategyWithCalculationsDuringPosting);
			charge.JR_OSCostExRate = 1m;
			charge.JR_OSCostExRate = 0m;
			AssertHasError(charge.JR_LocalCostAmtInfo, "Local amount cannot be zero when Overseas Cost Amount is non zero.");
		}

		public void TestARCashAdvanceRequestLine()
		{
			var cal = Factory.NewWithValidTestData<AccCashAdvanceRequestLine>();
			var cah = cal.RequestHeader;
			cah.CAH_Ledger = "AR";
			cah.Lines.Add(cal);

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge = (JobCharge)Factory.NewWithValidTestData(ExpectedBusinessObjectType);
			charge.JR_JH = job.PK;
			charge.JR_CAL_ARLine = cal.PK;
			charge.JR_IsARCashAdvance = true;

			AssertEquals(charge.ARCashAdvanceRequestLine, cal);
		}

		public void TestErrorIsReportedWhenJR_ProFormaRevenueAndCostIsNotSetFromSetProFormaRevenueAndCost()
		{
			var charge = Factory.New<JobCharge>();
			ErrorReporter.Clear();
			charge.JR_ProFormaCost = true;
			AssertEquals("Developer report when setting JR_ProFormaCost", "JR_ProFormaCost and JR_ProFormaRevenue can only be set from SetProFormaRevenueAndCost method", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
			charge.JR_ProFormaRevenue = false;
			AssertEquals("Developer report when setting JR_ProFormaCost", "JR_ProFormaCost and JR_ProFormaRevenue can only be set from SetProFormaRevenueAndCost method", ErrorReporter.LastExceptionReported.Message);
			ErrorReporter.Clear();
		}

		public void TestJR_ProFormaRevenueCollectsDeveloperInfoWhenJR_ProFormaRevenueIsInvalid()
		{
			var ratingHeaderJobCharge = GetJobCharge(RatingHeaderSchema.Constants.Prefix);

			AssertContains("Precondition: first time", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(ratingHeaderJobCharge.PK));

			AssertEquals("Precondition: JH_ParentTableCode", RatingHeaderSchema.Constants.Prefix, ratingHeaderJobCharge.Job.JH_ParentTableCode);
			AssertEquals("Precondition: JR_ProformaRevenue",true, ratingHeaderJobCharge.JR_ProFormaRevenue);
			AssertContains("Should not report an error","There is no data collected for this PK", GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(ratingHeaderJobCharge.PK));

			ratingHeaderJobCharge.JR_ProFormaRevenue = false;
			var expectedInfo =
$@"JobChargeJR_ProFormaRevenueSetterCallStack:

JR_ProFormaRevenue: N
Linked Job JH_ParentTableCode: TH 

StackTrace:";
			AssertContains("Should report an error", expectedInfo, GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(ratingHeaderJobCharge.PK));

			var nonratingHeaderJobCharge = GetJobCharge(DummyBizoSchema.Constants.Prefix);
			AssertContains("Precondition: first time", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(nonratingHeaderJobCharge.PK));

			AssertNotEquals("Precondition: JH_ParentTableCode", RatingHeaderSchema.Constants.Prefix, nonratingHeaderJobCharge.Job.JH_ParentTableCode);
			AssertEquals("Precondition: JR_ProformaRevenue", false, ratingHeaderJobCharge.JR_ProFormaRevenue);
			AssertContains("Should not report an error", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(nonratingHeaderJobCharge.PK));

			nonratingHeaderJobCharge.JR_ProFormaRevenue = true;
			expectedInfo =
$@"JobChargeJR_ProFormaRevenueSetterCallStack:

JR_ProFormaRevenue: Y
Linked Job JH_ParentTableCode: Z0 

StackTrace:";
			AssertContains("Should report an error", expectedInfo, GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(nonratingHeaderJobCharge.PK));
			ErrorReporter.Clear();

			string GetInfoJobChargeJR_ProFormaRevenueSetterCallStack(ZGuid pk) => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(pk, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaRevenueSetterCallStack);
		}

		public void TestJR_ProFormaCostCollectsDeveloperInfoWhenJR_ProFormaCostIsInvalid()
		{
			var ratingHeaderJobCharge = GetJobCharge(RatingHeaderSchema.Constants.Prefix);

			AssertContains("Precondition: first time", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaCostSetterCallStack(ratingHeaderJobCharge.PK));

			AssertEquals("Precondition: JH_ParentTableCode", RatingHeaderSchema.Constants.Prefix, ratingHeaderJobCharge.Job.JH_ParentTableCode);
			AssertEquals("Precondition: JR_ProFormaCost", true, ratingHeaderJobCharge.JR_ProFormaCost);
			AssertContains("Should not report an error", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaCostSetterCallStack(ratingHeaderJobCharge.PK));

			ratingHeaderJobCharge.JR_ProFormaCost = false;
			var expectedInfo =
$@"
JobChargeJR_ProFormaCostSetterCallStack:

JR_ProFormaCost: N
Linked Job JH_ParentTableCode: TH

StackTrace:
";
			AssertContains("Should report an error", expectedInfo, GetInfoJobChargeJR_ProFormaCostSetterCallStack(ratingHeaderJobCharge.PK));

			var nonratingHeaderJobCharge = GetJobCharge(DummyBizoSchema.Constants.Prefix);
			AssertContains("Precondition: first time", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaCostSetterCallStack(nonratingHeaderJobCharge.PK));

			AssertNotEquals("Precondition: JH_ParentTableCode", RatingHeaderSchema.Constants.Prefix, nonratingHeaderJobCharge.Job.JH_ParentTableCode);
			AssertEquals("Precondition: JR_ProFormaCost", false, nonratingHeaderJobCharge.JR_ProFormaCost);
			AssertContains("Should not report an error", "There is no data collected for this PK", GetInfoJobChargeJR_ProFormaCostSetterCallStack(nonratingHeaderJobCharge.PK));

			nonratingHeaderJobCharge.JR_ProFormaCost = true;
			expectedInfo =
$@"
JobChargeJR_ProFormaCostSetterCallStack:

JR_ProFormaCost: Y
Linked Job JH_ParentTableCode: Z0

StackTrace:
";
			AssertContains("Should report an error", expectedInfo, GetInfoJobChargeJR_ProFormaCostSetterCallStack(nonratingHeaderJobCharge.PK));
			ErrorReporter.Clear();

			string GetInfoJobChargeJR_ProFormaCostSetterCallStack(ZGuid pk) => CriticalValidationInfoCollectorService.GetOrCreateService(Factory).GetInfo(pk, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_ProFormaCostSetterCallStack);
		}

		public void TestJobChargeDeleteStackTraceCollected_OnDelete()
		{
			var charge1 = Factory.New<JobCharge>();
			charge1.Delete();
			var collectorService = CriticalValidationInfoCollectorService.GetService(Factory);
			var criticalValidationInfo = collectorService.GetInfoSafe(charge1.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeDeleteStackTrace);
			AssertContains("JobChargeDeleteStackTrace: There is no data collected", criticalValidationInfo);

			var charge2 = Factory.New<JobCharge>();
			charge2.Delete();
			criticalValidationInfo = collectorService.GetInfoSafe(charge2.PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeDeleteStackTrace);
			AssertContains(@"JobChargeDeleteStackTrace:
   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)
   at System.Environment.get_StackTrace()
   at Enterprise.MasterFiles.Business.JobCharge.", criticalValidationInfo);
		}

		#region Data Refresh Bus Update Tests

		[SuspendCriticalValidation]
		public void TestShouldApplyDataRefreshBusUpdate()
		{
			var expectedType = GetExpectedBusinessObjectType();
			var chargeSubscriber = (JobCharge)Factory.NewWithValidTestData(expectedType);
			Factory.Save();

			var factoryOfPublisher = new BusinessObjectFactory();
			var chargePublisher = factoryOfPublisher.Load<JobCharge>(chargeSubscriber.PK);

			var dataRefreshBusUpdateDeciderMock = new Mock<IDataRefreshBusUpdateActionDecider>();

			JobCharge passedSubscriber = null, passedPublisher = null;
			ZPropertyInfo[] passedStrictPropertyInfo = null;
			dataRefreshBusUpdateDeciderMock
				.Setup(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<JobCharge>(), It.IsAny<JobCharge>(), It.IsAny<ZPropertyInfo[]>()))
				.Returns(true)
				.Callback<DataRefreshAction, BusinessObject, BusinessObject, ZPropertyInfo[]>((action, subscriber, publisher, strictPropertyInfo) =>
				{
					if (subscriber.GetType() == expectedType) //to overcome multiple instances around the row
					{
						passedSubscriber = (JobCharge)subscriber;
						passedPublisher = (JobCharge)publisher;
						passedStrictPropertyInfo = strictPropertyInfo;
					}
				});
			ObjectFactory.Substitute(dataRefreshBusUpdateDeciderMock.Object);

			chargePublisher.JR_Desc = "some other description";
			factoryOfPublisher.Save();

			dataRefreshBusUpdateDeciderMock.Verify(x => x.ShouldApplyDataRefreshBusUpdate(DataRefreshAction.UpdateNonDeletedSubscriberWhenPublisherUpdated, It.IsAny<JobCharge>(), It.IsAny<JobCharge>(), It.IsAny<ZPropertyInfo[]>()));
			AssertEquals("Subscriber passed into data refresh bus decider.", chargeSubscriber, passedSubscriber);
			AssertEquals("Publisher passed into data refresh bus decider.", chargePublisher, passedPublisher);

			var currentPropertiesWithStrictConcurrency = chargeSubscriber.ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.IsPersistent && info.ConcurrencyPolicy == ConcurrencyPolicy.Strict);
			var typeName = GetExpectedBusinessObjectType().Name;
			var errorMessage = $"Concurrency policy of some {typeName} Properties have changed. Please update list of strict concurrency property infos passed into data refresh bus decider in {typeName} class.";
			AssertContainsExactElementsInAnyOrder(errorMessage, currentPropertiesWithStrictConcurrency, passedStrictPropertyInfo);
		}

		#endregion

		#region Implementation

		void AddJobChargeAttrib(JobCharge charge, string name, string value)
		{
			JobChargeAttrib attrib = charge.JobChargeAttributes.AddNew();
			attrib.EC_Name = name;
			attrib.EC_Value = value;
		}

		public override void TestBizObjectFields()
		{
			Assert(true);
		}

		JobCharge charge;
		JobCharge Charge
		{
			get { return charge ?? (charge = Factory.New<JobCharge>()); }
			set { charge = value; }
		}

		protected RefExchangeRate CreateBuyRefExchangeRate(RefCurrency currency, ZDateTime date, ZDecimal rate)
		{
			var newRate = currency.ExchangeRates.AddNew();
			newRate.RE_RX_NKExCurrency = currency.RX_Code;
			newRate.RE_GC = GlbCompany.CurrentCompany.PK;
			newRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			newRate.RE_StartDate = date;
			newRate.RE_ExpiryDate = date.AddDays(1).AddSeconds(-1);
			newRate.RE_SellRate = rate;
			return newRate;
		}

		protected Mock<IJobInvoicingPlugIn> GetMockInvoicingPlugIn()
		{
			var result = new Mock<IJobInvoicingPlugIn>();
			result.Setup(m => m.Factory).Returns(Factory);
			result.Setup(m => m.TableName).Returns(JobShipmentSchema.Constants.TableName);
			result.Setup(m => m.IsDeleted).Returns(false);

			var mockSupporter = new Mock<IJobInvoicingSupporter>();
			mockSupporter.Setup(m => m.IsImport).Returns(true);
			mockSupporter.Setup(m => m.IsExport).Returns(false);
			mockSupporter.Setup(m => m.IsDomestic).Returns(false);
			mockSupporter.Setup(m => m.TransportMode).Returns("SEA");
			mockSupporter.Setup(m => m.OperationalJobRef).Returns(ZString.Empty);
			mockSupporter.Setup(m => m.EditSecurityLock).Returns(false);

			result.Setup(m => m.InvoicingSupporter).Returns(mockSupporter.Object);

			return result;
		}

		JobCharge GetJobCharge(string parentTableCode)
		{
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_ParentTableCode = parentTableCode;
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = jobHeader.PK;
			return charge;
		}

		#endregion
	}
}
