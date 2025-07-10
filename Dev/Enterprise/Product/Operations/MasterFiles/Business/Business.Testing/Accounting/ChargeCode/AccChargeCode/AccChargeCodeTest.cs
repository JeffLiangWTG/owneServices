using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AuthenticationService.Client.Models;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business.Accounting.JobConfigurationHelpers;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCode))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI007:CustomisableDataTranslationRule", Justification = "Test code")]
	public sealed class AccChargeCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccChargeCode>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			//Expected but not found - Available in DB but not in Accounting.xml
			//Resource strings will not be available for these charge codes.
			var expectedButNotFoundChargeCodes = new List<ZString>();
			expectedButNotFoundChargeCodes.Add("Destination AQIS Inspection Fee");
			expectedButNotFoundChargeCodes.Add("Claims");
			expectedButNotFoundChargeCodes.Add("Pre-Loading Fee");
			expectedButNotFoundChargeCodes.Add("Customs Deferred Charge (For information only)");
			expectedButNotFoundChargeCodes.Add("Bond Charges");
			expectedButNotFoundChargeCodes.Add("Origin Cargo Automation Fee");
			expectedButNotFoundChargeCodes.Add("Destination Cargo Automation Fee");
			expectedButNotFoundChargeCodes.Add("Origin AQIS Inspection Fee");
			expectedButNotFoundChargeCodes.Add("Office Suppliers - Beverages");
			var query = new ZQuery(AccChargeCodeSchema.AC_Desc, expectedButNotFoundChargeCodes);
			var chargeCodes = Factory.Load<AccChargeCode>(query);
			chargeCodes.DeleteAll();
			Factory.Save();

			//Found but not expected - Available in Accounting.xml but not in DB
			//Create new charge codes
			var foundButNotExpectedChargeCodes = new Dictionary<ZString, ZString>();
			foundButNotExpectedChargeCodes.Add("TSTOR", "Transport Storage");
			foundButNotExpectedChargeCodes.Add("DQIF", "Destination Quarantine Inspection Fee");
			foundButNotExpectedChargeCodes.Add("OCRF", "Origin Cargo Reporting Fee");
			foundButNotExpectedChargeCodes.Add("DCRF", "Destination Cargo Reporting Fee");
			foundButNotExpectedChargeCodes.Add("OFFSUP", "Office Supplies");
			foundButNotExpectedChargeCodes.Add("TPLF", "Transport Pre-Loading Fee");
			foundButNotExpectedChargeCodes.Add("TT", "Tolls");
			foundButNotExpectedChargeCodes.Add("TFS", "Transport Fuel Surcharge");
			foundButNotExpectedChargeCodes.Add("OQIF", "Origin Quarantine Inspection Fee");
			CreateChargeCode(foundButNotExpectedChargeCodes);

			base.TestBizObjectFields();
		}

		void CreateChargeCode(Dictionary<ZString, ZString> chargeCodeDetails)
		{
			foreach (var chargeCodeDetail in chargeCodeDetails)
			{
				var chargeCode = Factory.New<AccChargeCode>();
				chargeCode.AC_Code = chargeCodeDetail.Key;
				chargeCode.AC_Desc = chargeCodeDetail.Value;
			}
			Factory.Save();
		}

		public void TestAC_DescMultilingual()
		{
			var chargeCodeDescriptionInEnglish = "My Test Charge Code";
			var chargeCodeDescriptionInGerman = "Mein Testgebührencode";
			TestChargeCode.AC_Desc = chargeCodeDescriptionInEnglish;

			var resKey = TestChargeCode.AC_DescInfo.CustomizableDataResourceStrings.GetMultilingualString(TestChargeCode, chargeCodeDescriptionInEnglish).ResourceKey;
			AssertEquals(chargeCodeDescriptionInEnglish, TestChargeCode.AC_DescMultilingual);

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, chargeCodeDescriptionInGerman));
				AssertEquals(chargeCodeDescriptionInGerman, TestChargeCode.AC_DescMultilingual);
			}
		}

		#region TestVATRecoverable

		public void TestVATRecoverableReadonly()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			foreach (var field in typeof(Constants.ChargeType).GetFields(BindingFlags.Static | BindingFlags.Public))
			{
				var chargeType = (string)field.GetValue(null);
				if (chargeType == Constants.ChargeType.Overhead)
				{
					continue;
				}

				chargeCode.AC_ChargeType = chargeType;
				Assert(chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
				Assert(chargeCode.AC_InputGSTVATRecoverableInfo.ReadOnly);
			}

			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			Assert(!chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
			Assert(!chargeCode.AC_InputGSTVATRecoverableInfo.ReadOnly);

			chargeCode.AC_GC = ZGuid.Empty;
			Assert("Precondition: charge code is global", chargeCode.IsGlobal);
			Assert(chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
			Assert(chargeCode.AC_InputGSTVATRecoverableInfo.ReadOnly);

			chargeCode.AC_GC = GlbCompany.CurrentCompany.PK;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			Assert(chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
			Assert(chargeCode.AC_InputGSTVATRecoverableInfo.ReadOnly);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			Assert(!chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo.ReadOnly);
			Assert(!chargeCode.AC_InputGSTVATRecoverableInfo.ReadOnly);
		}

		public void TestVATRecoverable()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			AssertVATRecoverableFields(chargeCode, "default values", 100m, 1m, null);

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 100000000.00m;
			AssertVATRecoverableFields(chargeCode, "101.00%", 100000000.00m, 1000000.00m, "GST Recoverable % must be between 0 and 100.");

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 101.00m;
			AssertVATRecoverableFields(chargeCode, "101.00%", 101.00m, 1.0100m, "GST Recoverable % must be between 0 and 100.");

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 100.00m;
			AssertVATRecoverableFields(chargeCode, "100.00%", 100.00m, 1.0000m, null);

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 99.99m;
			AssertVATRecoverableFields(chargeCode, "99.99%", 99.99m, 0.9999m, "GST Recoverable % must be 100% for all charge codes that are not Overheads.");

			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			AssertVATRecoverableFields(chargeCode, "reset on charge type changes", 100m, 1m, null);

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 99.99m;
			AssertVATRecoverableFields(chargeCode, "99.99%", 99.99m, 0.9999m, null);

			chargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			AssertVATRecoverableFields(chargeCode, "reset on charge type changes", 100m, 1m, null);
			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 0.01m;
			AssertVATRecoverableFields(chargeCode, "0.01%", 0.01m, 0.0001m, null);

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = 0.00m;
			AssertVATRecoverableFields(chargeCode, "0.00%", 0.00m, 0.0000m, null);

			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			AssertVATRecoverableFields(chargeCode, "reset on charge type changes", 100m, 1m, null);
			chargeCode.AC_ChargeType = Constants.ChargeType.Overhead;

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = -0.01m;
			AssertVATRecoverableFields(chargeCode, "-0.01%", -0.01m, -0.0001m, "GST Recoverable % must be between 0 and 100.");

			chargeCode.AC_Calc_InputGSTVATRecoverablePercentage = -100000000.00m;
			AssertVATRecoverableFields(chargeCode, "-100000000.00%", -100000000.00m, -1000000.00m, "GST Recoverable % must be between 0 and 100.");

			var decimalPlacesAttribute = typeof(AccChargeCode).GetProperty(AccChargeCode.Schema.AC_Calc_InputGSTVATRecoverablePercentage).GetCustomAttributes(typeof(DecimalPlacesAttribute), true)[0] as DecimalPlacesAttribute;
			AssertEquals("Decimal Places for AC_Calc_InputGSTVATRecoverablePercentage", 2, decimalPlacesAttribute.DecimalPlaces);

			var bindablePropertyAttribute = typeof(AccChargeCode).GetProperty(AccChargeCode.Schema.AC_InputGSTVATRecoverable).GetCustomAttributes(typeof(ZUnbindablePropertyAttribute), true)[0] as ZUnbindablePropertyAttribute;
			AssertEquals("AC_InputGSTVATRecoverable should not be a bindable property", true, bindablePropertyAttribute != null);
		}

		void AssertVATRecoverableFields(AccChargeCode chargeCode, string description, decimal userEditableValue, decimal databaseValue, params string[] errorMessages)
		{
			CombineAssertions(description, delegate
			{
				AssertEquals("User Editable Field", userEditableValue, chargeCode.AC_Calc_InputGSTVATRecoverablePercentage);
				AssertEquals("Database Field", databaseValue, chargeCode.AC_InputGSTVATRecoverable);

				if (errorMessages == null)
				{
					AssertNoErrors(chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo);
					AssertNoErrors(chargeCode.AC_InputGSTVATRecoverableInfo);
				}
				else
				{
					foreach (var message in errorMessages)
					{
						AssertHasError(chargeCode.AC_Calc_InputGSTVATRecoverablePercentageInfo, message);
						AssertHasError(chargeCode.AC_InputGSTVATRecoverableInfo, message);
					}
				}
			});
		}

		#endregion

		public void TestGetDuplicateValidationCollection()
		{
			var item1 = TestChargeCode.ChargeComplianceDescriptions.AddNew();
			var item2 = TestChargeCode.ChargeComplianceDescriptions.AddNew();
			var collection = ((IDuplicateValidationCollectionProvider<AccChargeComplianceDescription>)TestChargeCode).GetDuplicateValidationCollection();
			AssertContainsExactElementsInAnyOrder(new[] { item1, item2 }, collection);
		}

		public void TestIsValidInConsolCosting()
		{
			AssertEquals(false, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.Comment));
			AssertEquals(true, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.Disbursement));
			AssertEquals(true, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.ManualJobAccrual));
			AssertEquals(true, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.Margin));
			AssertEquals(false, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.NonAccrual));
			AssertEquals(false, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.Overhead));
			AssertEquals(false, AccChargeCode.IsValidInConsolCosting(Constants.ChargeType.Revenue));
		}

		public void TestIsValidOnJob()
		{
			AssertEquals(true, AccChargeCode.IsValidOnJob(Constants.ChargeType.Comment));
			AssertEquals(true, AccChargeCode.IsValidOnJob(Constants.ChargeType.Disbursement));
			AssertEquals(true, AccChargeCode.IsValidOnJob(Constants.ChargeType.ManualJobAccrual));
			AssertEquals(true, AccChargeCode.IsValidOnJob(Constants.ChargeType.Margin));
			AssertEquals(false, AccChargeCode.IsValidOnJob(Constants.ChargeType.NonAccrual));
			AssertEquals(false, AccChargeCode.IsValidOnJob(Constants.ChargeType.Overhead));
			AssertEquals(true, AccChargeCode.IsValidOnJob(Constants.ChargeType.Revenue));
		}

		public void TestIsValidInAP()
		{
			bool isJobEntered = true;
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Comment, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Disbursement, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.ManualJobAccrual, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Margin, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.NonAccrual, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Overhead, isJobEntered));
			AssertEquals(false, AccChargeCode.IsValidInAP(Constants.ChargeType.Revenue, isJobEntered));

			isJobEntered = false;
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Comment, isJobEntered));
			AssertEquals(false, AccChargeCode.IsValidInAP(Constants.ChargeType.Disbursement, isJobEntered));
			AssertEquals(false, AccChargeCode.IsValidInAP(Constants.ChargeType.ManualJobAccrual, isJobEntered));
			AssertEquals(false, AccChargeCode.IsValidInAP(Constants.ChargeType.Margin, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.NonAccrual, isJobEntered));
			AssertEquals(true, AccChargeCode.IsValidInAP(Constants.ChargeType.Overhead, isJobEntered));
			AssertEquals(false, AccChargeCode.IsValidInAP(Constants.ChargeType.Revenue, isJobEntered));
		}

		public void TestIsValidInAR()
		{
			AssertEquals(true, AccChargeCode.IsValidInAR(Constants.ChargeType.Comment));
			AssertEquals(false, AccChargeCode.IsValidInAR(Constants.ChargeType.Disbursement));
			AssertEquals(false, AccChargeCode.IsValidInAR(Constants.ChargeType.ManualJobAccrual));
			AssertEquals(false, AccChargeCode.IsValidInAR(Constants.ChargeType.Margin));
			AssertEquals(false, AccChargeCode.IsValidInAR(Constants.ChargeType.NonAccrual));
			AssertEquals(false, AccChargeCode.IsValidInAR(Constants.ChargeType.Overhead));
			AssertEquals(true, AccChargeCode.IsValidInAR(Constants.ChargeType.Revenue));
		}

		public void TestRequiredProperties()
		{
			var chargeCode = TestChargeCode;

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			var property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("Margin Charge Type", true, property.AccrualAccount);
			AssertEquals("Margin Charge Type", true, property.CostAccount);
			AssertEquals("Margin Charge Type", true, property.MarginPercentage);
			AssertEquals("Margin Charge Type", true, property.RevenueAccount);
			AssertEquals("Margin Charge Type", true, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("Disbursement Charge Type", true, property.AccrualAccount);
			AssertEquals("Disbursement Charge Type", true, property.CostAccount);
			AssertEquals("Disbursement Charge Type", true, property.MarginPercentage);
			AssertEquals("Disbursement Charge Type", true, property.RevenueAccount);
			AssertEquals("Disbursement Charge Type", true, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("ManualJobAccrual Charge Type", true, property.AccrualAccount);
			AssertEquals("ManualJobAccrual Charge Type", true, property.CostAccount);
			AssertEquals("ManualJobAccrual Charge Type", false, property.MarginPercentage);
			AssertEquals("ManualJobAccrual Charge Type", true, property.RevenueAccount);
			AssertEquals("ManualJobAccrual Charge Type", true, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("Revenue Charge Type", false, property.AccrualAccount);
			AssertEquals("Revenue Charge Type", false, property.CostAccount);
			AssertEquals("Revenue Charge Type", false, property.MarginPercentage);
			AssertEquals("Revenue Charge Type", true, property.RevenueAccount);
			AssertEquals("Revenue Charge Type", true, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("Overhead Charge Type", false, property.AccrualAccount);
			AssertEquals("Overhead Charge Type", true, property.CostAccount);
			AssertEquals("Overhead Charge Type", false, property.MarginPercentage);
			AssertEquals("Overhead Charge Type", false, property.RevenueAccount);
			AssertEquals("Overhead Charge Type", false, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("NonAccrual Charge Type", false, property.AccrualAccount);
			AssertEquals("NonAccrual Charge Type", true, property.CostAccount);
			AssertEquals("NonAccrual Charge Type", false, property.MarginPercentage);
			AssertEquals("NonAccrual Charge Type", true, property.RevenueAccount);
			AssertEquals("NonAccrual Charge Type", false, property.WIPAccount);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			property = chargeCode.RequiredProperties(chargeCode.AC_ChargeType);
			AssertEquals("Comment Charge Type", false, property.AccrualAccount);
			AssertEquals("Comment Charge Type", false, property.CostAccount);
			AssertEquals("Comment Charge Type", false, property.MarginPercentage);
			AssertEquals("Comment Charge Type", false, property.RevenueAccount);
			AssertEquals("Comment Charge Type", false, property.WIPAccount);
		}

		public void TestGetOverriddenBranch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_OH_OrgProxy = organisation.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			var chargeCode = TestChargeCode;
			var chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.All;
			chargeBranchOverride.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.SpecificBranchAlways;
			chargeBranchOverride.YA_GB_SpecificBranch = branch2.PK;

			chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Import;
			chargeBranchOverride.YA_TransportMode = Constants.TransportModes.Air;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker;

			chargeBranchOverride = chargeCode.BranchOverrides.AddNew();
			chargeBranchOverride.YA_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			chargeBranchOverride.YA_Direction = Constants.FreightShipmentDirection.Code.Export;
			chargeBranchOverride.YA_TransportMode = AccChargeBranchOverrideLookups.TransportModeAdditionalCodes.All;
			chargeBranchOverride.YA_DefaultingRule = Constants.ChargeCodeBranchDefaultingRule.ArrivalCTO;
			Factory.Save();

			Func<ZString, OrgHeader> getOrganisationByBranchDefaultingRule = rule => rule == Constants.ChargeCodeBranchDefaultingRule.ShipmentImportBroker ? organisation : null;

			AssertEquals(null, chargeCode.GetOverriddenBranch(null, "", "", null));
			AssertEquals(null, chargeCode.GetOverriddenBranch(null, "", "", rule => organisation));
			AssertEquals(null, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, "", "", rule => organisation));
			AssertEquals(branch2, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Import, "", rule => organisation));
			AssertEquals(null, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, null));
			AssertEquals(branch1, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air, getOrganisationByBranchDefaultingRule));
			AssertEquals(branch2, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea, getOrganisationByBranchDefaultingRule));
			AssertEquals(null, chargeCode.GetOverriddenBranch(JobInvoicingConsumerTypes.Shipment, Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Air, getOrganisationByBranchDefaultingRule));
		}

		public void TestBranchOverrides()
		{
			var chargeRevRecOverride1 = Factory.NewWithValidTestData<AccChargeBranchOverride>();
			var chargeRevRecOverride2 = Factory.NewWithValidTestData<AccChargeBranchOverride>();
			var chargeRevRecOverride3 = Factory.NewWithValidTestData<AccChargeBranchOverride>();
			chargeRevRecOverride1.YA_AC_ChargeCode = TestChargeCode.PK;
			chargeRevRecOverride2.YA_AC_ChargeCode = TestChargeCode.PK;
			AssertEquals(2, TestChargeCode.BranchOverrides.Count);
			AssertEquals("BranchOverrides is registered as editable child.", true, TestChargeCode.IsRegisteredEditableChildObject(TestChargeCode.BranchOverrides));
		}

		public void TestRevenueRecOverrides()
		{
			AccChargeRevRecOverride chargeRevRecOverride1 = Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			AccChargeRevRecOverride chargeRevRecOverride2 = Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			AccChargeRevRecOverride chargeRevRecOverride3 = Factory.NewWithValidTestData<AccChargeRevRecOverride>();
			chargeRevRecOverride1.AE_AC = TestChargeCode.PK;
			chargeRevRecOverride2.AE_AC = TestChargeCode.PK;
			AssertEquals(2, TestChargeCode.RevenueRecOverrides.Count);
		}

		public void TestSupplyTypeOverrides()
		{
			var chargeSupplyTypeOverride1 = Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			var chargeSupplyTypeOverride2 = Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			var chargeSupplyTypeOverride3 = Factory.NewWithValidTestData<AccChargeSupplyTypeOverride>();
			chargeSupplyTypeOverride1.ACS_ParentTableCode = "AC";
			chargeSupplyTypeOverride1.ACS_ParentID = TestChargeCode.PK;
			chargeSupplyTypeOverride2.ACS_ParentTableCode = "AC";
			chargeSupplyTypeOverride2.ACS_ParentID = TestChargeCode.PK;
			AssertEquals(2, TestChargeCode.SupplyTypeOverrides.Count);
		}

		public void TestChargeComplianceDescriptions()
		{
			var chargeComplianceDescription1 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			var chargeComplianceDescription2 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			var chargeComplianceDescription3 = Factory.NewWithValidTestData<AccChargeComplianceDescription>();
			chargeComplianceDescription1.ADE_AC = TestChargeCode.PK;
			chargeComplianceDescription2.ADE_AC = TestChargeCode.PK;
			AssertEquals(2, TestChargeCode.ChargeComplianceDescriptions.Count);
			AssertEquals("ChargeComplianceDescriptions is registered as editable child.", true, TestChargeCode.IsRegisteredEditableChildObject(TestChargeCode.ChargeComplianceDescriptions));
		}

		public void TestChargeComplianceDescriptionCollectionIsReadOnly()
		{
			var chargeCode = Factory.New<AccChargeCode>();

			Env.Security.ChargeCodesSellComplianceDescription.IsAllowed = true;
			Assert("Access Allowed - Not ReadOnly", !chargeCode.ChargeComplianceDescriptions.ReadOnly);

			chargeCode = Factory.New<AccChargeCode>();

			Env.Security.ChargeCodesSellComplianceDescription.IsAllowed = false;
			Assert("Access Disallowed - ReadOnly", chargeCode.ChargeComplianceDescriptions.ReadOnly);
		}

		public void TestGLPostingOverrides()
		{
			AccChargeGLPostingOverride chargeGlPostingOverride1 = Factory.NewWithValidTestData<AccChargeGLPostingOverride>();
			AccChargeGLPostingOverride chargeGlPostingOverride2 = Factory.NewWithValidTestData<AccChargeGLPostingOverride>();
			AccChargeGLPostingOverride chargeGlPostingOverride3 = Factory.NewWithValidTestData<AccChargeGLPostingOverride>();
			chargeGlPostingOverride1.Y1_AC = TestChargeCode.PK;
			chargeGlPostingOverride2.Y1_AC = TestChargeCode.PK;
			AssertEquals(2, TestChargeCode.GLPostingOverrides.Count);
		}

		public void TestOverridesCopiedWhenAccChargeCodeCopiedUsingDifferentFactories()
		{
			AccChargeCode chargeCode = GetSavedTestChargeCode();

			AccChargeTaxOverride taxOver1 = chargeCode.TaxOverrides.AddNew();
			taxOver1.AO_Direction = "IMP";
			taxOver1.AO_IncoTerm = "ALL";
			taxOver1.AO_JobType = "ALL";
			taxOver1.AO_CostSellAll = "ALL";
			taxOver1.AO_Origin = "ALX";
			taxOver1.AO_Destination = "DE";
			taxOver1.AO_TaxRegCntryOrGroup = "EUX";
			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			taxOver1.AO_AT = rate.PK;
			taxOver1.AO_CustomsStatus = "PMT";

			chargeCode.Factory.Save();

			AccChargeTaxOverride taxOver2 = chargeCode.TaxOverrides.AddNew();
			taxOver2.AO_Direction = "EXP";
			taxOver2.AO_IncoTerm = "EXW";
			taxOver2.AO_JobType = "SHP";
			taxOver2.AO_CostSellAll = "COS";
			taxOver2.AO_Origin = "AU";
			taxOver2.AO_Destination = "DE";
			taxOver2.AO_TaxRegCntryOrGroup = "EUX";
			AccTaxRate rate2 = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			taxOver2.AO_AT = rate2.PK;
			taxOver2.AO_CustomsStatus = "PMT";

			chargeCode.Factory.Save();

			var supplyTypeOverride1 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride1.ACS_JobType = "SHP";
			supplyTypeOverride1.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			supplyTypeOverride1.ACS_TransportMode = "AIR";
			supplyTypeOverride1.ACS_IncoTerm = "DAT";
			supplyTypeOverride1.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride1.ACS_SupplyType = "LOA";
			var supplyTypeOverride2 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride2.ACS_JobType = "FCN";
			supplyTypeOverride2.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			supplyTypeOverride2.ACS_TransportMode = "SEA";
			supplyTypeOverride2.ACS_IncoTerm = "DDP";
			supplyTypeOverride2.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride2.ACS_SupplyType = "LOC";

			chargeCode.Factory.Save();

			AccChargeRevRecOverride revRecOver1 = chargeCode.RevenueRecOverrides.AddNew();
			revRecOver1.AE_JobType = "SHP";
			revRecOver1.AE_Direction = "IMP";
			revRecOver1.AE_Mode = "FAS";
			revRecOver1.BrokerCode = "INT";
			revRecOver1.AE_RecognitionType = "ARV";
			AccChargeRevRecOverride revRecOver2 = chargeCode.RevenueRecOverrides.AddNew();
			revRecOver2.AE_JobType = "SHP";
			revRecOver2.AE_Direction = "IMP";
			revRecOver2.AE_Mode = "SEA";
			revRecOver2.BrokerCode = "EXT";
			revRecOver2.AE_RecognitionType = "IMM";

			chargeCode.Factory.Save();

			var govtChargeCodeOverride1 = chargeCode.GovtChargeCodeOverrides.AddNew();
			govtChargeCodeOverride1.ACG_JobType = "SHP";
			govtChargeCodeOverride1.ACG_Direction = "IMP";
			govtChargeCodeOverride1.ACG_TransportMode = "FAS";
			govtChargeCodeOverride1.ACG_GovtChargeCode = "12344";
			var govtChargeCodeOverride2 = chargeCode.GovtChargeCodeOverrides.AddNew();
			govtChargeCodeOverride2.ACG_JobType = "SHP";
			govtChargeCodeOverride2.ACG_Direction = "IMP";
			govtChargeCodeOverride2.ACG_TransportMode = "SEA";
			govtChargeCodeOverride2.ACG_GovtChargeCode = "12355";

			chargeCode.Factory.Save();

			var apportionmentMethodOverride1 = chargeCode.ApportionmentMethodOverrides.AddNew();
			apportionmentMethodOverride1.AAM_Module = ApportionmentMethod.AllCode;
			apportionmentMethodOverride1.AAM_ApportionmentMethod = AllocationMethod.Manual;
			var apportionmentMethodOverride2 = chargeCode.ApportionmentMethodOverrides.AddNew();
			apportionmentMethodOverride2.AAM_Module = ApportionmentMethodModules.Forwarding;
			apportionmentMethodOverride2.AAM_ApportionmentMethod = AllocationMethod.Shipment;

			chargeCode.Factory.Save();

			AccGLHeader header1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header2 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header3 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header4 = Factory.NewWithValidTestData<AccGLHeader>();

			AccChargeGLPostingOverride postingOverride = chargeCode.GLPostingOverrides.AddNew();
			// add  values to that override
			postingOverride.Y1_AG_CST = header1.PK;
			postingOverride.Y1_AG_ACR = header2.PK;
			postingOverride.Y1_AG_REV = header3.PK;
			postingOverride.Y1_AG_WIP = header4.PK;
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;

			chargeCode.Factory.Save();

			AccChargeTypeOverride typeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = "EXP";
			typeOverride.AN_JobType = "SHP";
			typeOverride.AN_ChargeType = "REV";
			typeOverride.AN_MarginPercentage = 0.00;
			typeOverride.AN_InvoiceType = "FIN";

			chargeCode.Factory.Save();

			AccChargeTypeOverride typeOverride2 = chargeCode.ChargeTypeOverrides.AddNew();
			typeOverride2.AN_JobDirection = "IMP";
			typeOverride2.AN_JobType = "SHP";
			typeOverride2.AN_ChargeType = "MRG";
			typeOverride2.AN_MarginPercentage = 40.00;
			typeOverride2.AN_InvoiceType = "DES";

			StmNote noteToSave = chargeCode.Notes.AddNew();

			noteToSave.ST_NoteText = "The note that should not be copied";
			((IDocManagerSupport)chargeCode).DocManagerInfo.MasterFactory.AddFileOrDocument(chargeCode.PK, "XXX", (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "sample.pdf", "pdf", "DEF", false);

			chargeCode.Factory.Save();
			chargeCode.docManagerInfo.MasterFactory.Save();

			ITemplateCopyable chargeCodeReadyToCopy = chargeCode;
			AccChargeCode copiedChargeCode = (AccChargeCode)chargeCodeReadyToCopy.TemplateCopy();
			copiedChargeCode.AC_Code = "TEST1";
			copiedChargeCode.Factory.Save();

			//reload and retest

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			AccChargeCode reloadedOriginal = newFactory.Load<AccChargeCode>(chargeCode.PK);
			AccChargeCode reloadedCopy = newFactory.Load<AccChargeCode>(copiedChargeCode.PK);
			Assert("Tax Overrides Collection copied", reloadedCopy.TaxOverrides.Count == reloadedOriginal.TaxOverrides.Count);
			Assert("Type Overrides Collection copied", reloadedCopy.ChargeTypeOverrides.Count == reloadedOriginal.ChargeTypeOverrides.Count);
			Assert("GL Posting Overrides Collection copied", reloadedCopy.GLPostingOverrides.Count == reloadedOriginal.GLPostingOverrides.Count);
			Assert("Supply Type Overrides Collection copied", reloadedCopy.SupplyTypeOverrides.Count == reloadedOriginal.SupplyTypeOverrides.Count);
			Assert("Revenue Recognition Overrides Collection copied", reloadedCopy.RevenueRecOverrides.Count == reloadedOriginal.RevenueRecOverrides.Count);
			Assert("Government Charge Code Overrides Collection copied", reloadedCopy.GovtChargeCodeOverrides.Count == reloadedOriginal.GovtChargeCodeOverrides.Count);
			Assert("Apportionment Method Override Collection copied", reloadedCopy.ApportionmentMethodOverrides.Count == reloadedOriginal.ApportionmentMethodOverrides.Count);
			Assert("eDocs have not been copied", reloadedCopy.docManagerInfo == null);
			Assert("Notes have not been copied", reloadedCopy.Notes.VisibleNotes.Count == 0);
			// A log will have been added to add a log to the system
			Assert("Logs have not been copied", reloadedCopy.Logs.GetAllLogs().Count < 15);
		}

		public void TestOverridesCopiedWhenAccChargeCodeCopied()
		{
			AccChargeCode chargeCode = GetSavedTestChargeCode();

			AccChargeTaxOverride taxOver1 = chargeCode.TaxOverrides.AddNew();
			taxOver1.AO_Direction = "IMP";
			taxOver1.AO_IncoTerm = "ALL";
			taxOver1.AO_JobType = "ALL";
			taxOver1.AO_CostSellAll = "ALL";
			taxOver1.AO_Origin = "ALX";
			taxOver1.AO_Destination = "DE";
			taxOver1.AO_TaxRegCntryOrGroup = "EUX";
			AccTaxRate rate = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			taxOver1.AO_AT = rate.PK;
			taxOver1.AO_CustomsStatus = "PMT";
			chargeCode.Factory.Save();

			AccChargeTaxOverride taxOver2 = chargeCode.TaxOverrides.AddNew();
			taxOver2.AO_Direction = "EXP";
			taxOver2.AO_IncoTerm = "EXW";
			taxOver2.AO_JobType = "SHP";
			taxOver2.AO_CostSellAll = "COS";
			taxOver2.AO_Origin = "AU";
			taxOver2.AO_Destination = "DE";
			taxOver2.AO_TaxRegCntryOrGroup = "EUX";
			AccTaxRate rate2 = Factory.LoadTop1<AccTaxRate>(new ZQuery());
			taxOver2.AO_AT = rate2.PK;
			taxOver2.AO_CustomsStatus = "PMT";
			chargeCode.Factory.Save();

			var supplyTypeOverride1 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride1.ACS_JobType = "SHP";
			supplyTypeOverride1.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Domestic;
			supplyTypeOverride1.ACS_TransportMode = "AIR";
			supplyTypeOverride1.ACS_IncoTerm = "DAT";
			supplyTypeOverride1.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride1.ACS_SupplyType = "LOA";
			var supplyTypeOverride2 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride2.ACS_JobType = "FCN";
			supplyTypeOverride2.ACS_Direction = Core.Constants.FreightShipmentDirection.Code.Export;
			supplyTypeOverride2.ACS_TransportMode = "SEA";
			supplyTypeOverride2.ACS_IncoTerm = "DDP";
			supplyTypeOverride2.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride2.ACS_SupplyType = "LOC";

			chargeCode.Factory.Save();

			AccChargeRevRecOverride revRecOver1 = chargeCode.RevenueRecOverrides.AddNew();
			revRecOver1.AE_JobType = "SHP";
			revRecOver1.AE_Direction = "IMP";
			revRecOver1.AE_Mode = "FAS";
			revRecOver1.BrokerCode = "INT";
			revRecOver1.AE_RecognitionType = "ARV";
			AccChargeRevRecOverride revRecOver2 = chargeCode.RevenueRecOverrides.AddNew();
			revRecOver2.AE_JobType = "SHP";
			revRecOver2.AE_Direction = "IMP";
			revRecOver2.AE_Mode = "SEA";
			revRecOver2.BrokerCode = "EXT";
			revRecOver2.AE_RecognitionType = "IMM";
			chargeCode.Factory.Save();

			var govtChargeCodeOverride1 = chargeCode.GovtChargeCodeOverrides.AddNew();
			govtChargeCodeOverride1.ACG_JobType = "SHP";
			govtChargeCodeOverride1.ACG_Direction = "IMP";
			govtChargeCodeOverride1.ACG_TransportMode = "FAS";
			govtChargeCodeOverride1.ACG_GovtChargeCode = "12344";
			var govtChargeCodeOverride2 = chargeCode.GovtChargeCodeOverrides.AddNew();
			govtChargeCodeOverride2.ACG_JobType = "SHP";
			govtChargeCodeOverride2.ACG_Direction = "IMP";
			govtChargeCodeOverride2.ACG_TransportMode = "SEA";
			govtChargeCodeOverride2.ACG_GovtChargeCode = "12355";
			chargeCode.Factory.Save();

			AccGLHeader header1 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header2 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header3 = Factory.NewWithValidTestData<AccGLHeader>();
			AccGLHeader header4 = Factory.NewWithValidTestData<AccGLHeader>();

			AccChargeGLPostingOverride postingOverride = chargeCode.GLPostingOverrides.AddNew();
			// add  values to that override
			postingOverride.Y1_AG_CST = header1.PK;
			postingOverride.Y1_AG_ACR = header2.PK;
			postingOverride.Y1_AG_REV = header3.PK;
			postingOverride.Y1_AG_WIP = header4.PK;
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			chargeCode.Factory.Save();

			AccChargeTypeOverride typeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = "EXP";
			typeOverride.AN_JobType = "SHP";
			typeOverride.AN_ChargeType = "REV";
			typeOverride.AN_MarginPercentage = 0.00;
			typeOverride.AN_InvoiceType = "FIN";
			chargeCode.Factory.Save();

			AccChargeTypeOverride typeOverride2 = chargeCode.ChargeTypeOverrides.AddNew();
			typeOverride2.AN_JobDirection = "IMP";
			typeOverride2.AN_JobType = "SHP";
			typeOverride2.AN_ChargeType = "MRG";
			typeOverride2.AN_MarginPercentage = 40.00;
			typeOverride2.AN_InvoiceType = "DES";

			StmNote noteToSave = chargeCode.Notes.AddNew();

			noteToSave.ST_NoteText = "The note that should not be copied";
			((IDocManagerSupport)chargeCode).DocManagerInfo.MasterFactory.AddFileOrDocument(chargeCode.PK, "XXX", (SubStreamableStream)new MemoryStream(new byte[] { 1, 2, 3 }), "sample.pdf", "pdf", "DEF", false);

			chargeCode.Factory.Save();
			chargeCode.docManagerInfo.MasterFactory.Save();

			// copy the charge code
			ITemplateCopyable chargeCodeReadyToCopy = chargeCode;
			AccChargeCode copiedChargeCode = (AccChargeCode)chargeCodeReadyToCopy.TemplateCopy();
			copiedChargeCode.AC_Code = "COPY";
			copiedChargeCode.Factory.Save();
			Assert("Tax Overrides Collection copied", copiedChargeCode.TaxOverrides.Count == chargeCode.TaxOverrides.Count);
			Assert("Supply Type Collection copied", copiedChargeCode.SupplyTypeOverrides.Count == chargeCode.SupplyTypeOverrides.Count);
			Assert("Type Overrides Collection copied", copiedChargeCode.ChargeTypeOverrides.Count == chargeCode.ChargeTypeOverrides.Count);
			Assert("GL Posting Overrides Collection copied", copiedChargeCode.GLPostingOverrides.Count == chargeCode.GLPostingOverrides.Count);

			Assert("Revenue Recognition Overrides Collection copied", copiedChargeCode.RevenueRecOverrides.Count == chargeCode.RevenueRecOverrides.Count);
			Assert("Government Charge Code Overrides Collection copied", copiedChargeCode.GovtChargeCodeOverrides.Count == chargeCode.GovtChargeCodeOverrides.Count);
			Assert("eDocs have not been copied", copiedChargeCode.docManagerInfo == null);
			Assert("Notes have not been copied", copiedChargeCode.Notes.VisibleNotes.Count == 0);
			// A log will have been added to add a log to the system
			Assert("Logs have not been copied", copiedChargeCode.Logs.GetAllLogs().Count < 13);
		}

		public void TestSupplyTypeOverridesDeletedWhenAccChargeCodeDeleted()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var supplyTypeOverride1 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride1.FillWithValidTestData();
			var supplyTypeOverride2 = chargeCode.SupplyTypeOverrides.AddNew();
			supplyTypeOverride2.FillWithValidTestData();

			Factory.Save();

			AssertEquals(2, chargeCode.SupplyTypeOverrides.Count);

			chargeCode.Delete();
			Factory.Save();

			var loadedSupplyTypeOverride1 = Factory.Load<AccChargeSupplyTypeOverride>(supplyTypeOverride1.PK);
			AssertNull(loadedSupplyTypeOverride1);
			var loadedSupplyTypeOverride2 = Factory.Load<AccChargeSupplyTypeOverride>(supplyTypeOverride2.PK);
			AssertNull(loadedSupplyTypeOverride2);
		}

		public void TestAC_IsCommissionableWhenChargeTypeChanged()
		{
			TestChargeCode.AC_IsCommissionable = ZBool.False;
			AssertEquals("Precondition", ZBool.False, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertEquals(ZBool.False, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals(ZBool.True, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_IsCommissionable = ZBool.True;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals(ZBool.True, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_IsCommissionable = ZBool.False;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals(ZBool.True, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_IsCommissionable = ZBool.True;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals(ZBool.True, TestChargeCode.AC_IsCommissionable);

			TestChargeCode.AC_IsCommissionable = ZBool.False;
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			AssertEquals(ZBool.True, TestChargeCode.AC_IsCommissionable);
		}

		public void TestGroupageChargeSetOnChargeGroup()
		{
			AssertGroupageCharge(ChargeCodeGroupList.Codes.Brokerage, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.BrokerageOnly, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.CFSLoadList, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.CFSShipment, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.CustomsDuty, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.Destination, false);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.Freight, true);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.Insurance, false);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.Loading, true);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.NonJobRelated, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.NotGrouped, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.Origin, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.Transport, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.TransportBooking, false);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.Unloading, true);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.WHSInwards, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.WHSOutwards, false);
			AssertGroupageCharge(ChargeCodeGroupList.Codes.WHSStorage, false);

			AssertGroupageCharge(ChargeCodeGroupList.Codes.ShippingDisbursements, false);
		}

		void AssertGroupageCharge(string chargeGroup, bool shouldBeGroupage)
		{
			TestChargeCode.AC_ChargeGroup = chargeGroup;
			AssertEquals(shouldBeGroupage, TestChargeCode.AC_IsGroupageCharge);
		}

		public void TestTemplateCopy()
		{
			AccChargeTaxOverride taxOverride = TestChargeCode.TaxOverrides.AddNew();
			taxOverride.AO_JobType = "SHP";
			taxOverride.AO_Direction = "IMP";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_Origin = "ZA";
			taxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_TaxRegCntryOrGroup = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_AT = ZGuid.NewZGuid();
			taxOverride.AO_A9_DefaultVATClass = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestChargeCode.PK, taxOverride.AO_ParentID);
			AssertEquals("Precondition: ", TestChargeCode.TablePrefix, taxOverride.AO_ParentTableCode);

			AccChargeTaxOverride taxOverride2 = TestChargeCode.TaxOverrides.AddNew();
			taxOverride2.AO_JobType = "SHP";
			taxOverride2.AO_Direction = "DOM";
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_TaxRegCntryOrGroup = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride2.AO_AT = ZGuid.NewZGuid();
			taxOverride2.AO_A9_DefaultVATClass = ZGuid.NewZGuid();
			AssertEquals("Precondition: ", TestChargeCode.PK, taxOverride2.AO_ParentID);
			AssertEquals("Precondition: ", TestChargeCode.TablePrefix, taxOverride2.AO_ParentTableCode);

			AccChargeCode newChargeCode = (AccChargeCode)((ITemplateCopyable)TestChargeCode).TemplateCopy();
			AssertEquals(newChargeCode.AC_Code, TestChargeCode.AC_Code);
			AssertEquals(newChargeCode.AC_Desc, TestChargeCode.AC_Desc);
			AssertEquals(newChargeCode.AC_GC, TestChargeCode.AC_GC);
			AssertEquals("TaxOverrides.Count", newChargeCode.TaxOverrides.Count, TestChargeCode.TaxOverrides.Count);
			AssertNotEquals("ChargeCode PKs different", TestChargeCode.PK, newChargeCode.PK);

			AccChargeTaxOverride copiedTaxOverride = newChargeCode.TaxOverrides[0];
			AssertEquals(taxOverride.AO_JobType, copiedTaxOverride.AO_JobType);
			AssertEquals(taxOverride.AO_Direction, copiedTaxOverride.AO_Direction);
			AssertEquals(taxOverride.AO_IncoTerm, copiedTaxOverride.AO_IncoTerm);
			AssertEquals(taxOverride.AO_Origin, copiedTaxOverride.AO_Origin);
			AssertEquals(taxOverride.AO_Destination, copiedTaxOverride.AO_Destination);
			AssertEquals(taxOverride.AO_TaxRegCntryOrGroup, copiedTaxOverride.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride.AO_AT, copiedTaxOverride.AO_AT);
			AssertEquals(taxOverride.AO_A9_DefaultVATClass, copiedTaxOverride.AO_A9_DefaultVATClass);
			AssertEquals(taxOverride.AO_ParentTableCode, copiedTaxOverride.AO_ParentTableCode);
			AssertEquals(newChargeCode.PK, copiedTaxOverride.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride.PK, copiedTaxOverride.PK);

			AccChargeTaxOverride copiedTaxOverride2 = newChargeCode.TaxOverrides[1];
			AssertEquals(taxOverride2.AO_JobType, copiedTaxOverride2.AO_JobType);
			AssertEquals(taxOverride2.AO_Direction, copiedTaxOverride2.AO_Direction);
			AssertEquals(taxOverride2.AO_IncoTerm, copiedTaxOverride2.AO_IncoTerm);
			AssertEquals(taxOverride2.AO_Origin, copiedTaxOverride2.AO_Origin);
			AssertEquals(taxOverride2.AO_Destination, copiedTaxOverride2.AO_Destination);
			AssertEquals(taxOverride2.AO_TaxRegCntryOrGroup, copiedTaxOverride2.AO_TaxRegCntryOrGroup);
			AssertEquals(taxOverride2.AO_AT, copiedTaxOverride2.AO_AT);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, copiedTaxOverride2.AO_A9_DefaultVATClass);
			AssertEquals(taxOverride2.AO_ParentTableCode, copiedTaxOverride2.AO_ParentTableCode);
			AssertEquals(newChargeCode.PK, copiedTaxOverride2.AO_ParentID);
			AssertNotEquals("TaxOverride PKs different", taxOverride2.PK, copiedTaxOverride2.PK);
		}

		public void TestAC_Desc()
		{
			TestChargeCode.AC_Desc = "testdesc";
			AssertEquals("Local language description should NOT be set", "", TestChargeCode.AC_LocalLanguageDescription);
		}

		public void TestSubGroupValidity()
		{
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Brokerage, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.BrokerageOnly, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.CFSLoadList, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.CFSShipment, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.CustomsDuty, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Destination, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Freight, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Insurance, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Loading, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.NonJobRelated, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.NotGrouped, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Origin, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Transport, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.TransportBooking, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.Unloading, true);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.WHSInwards, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.WHSOutwards, false);
			AssertSubGroupReadOnly(ChargeCodeGroupList.Codes.WHSStorage, true);
		}

		void AssertSubGroupReadOnly(string chargeGroup, bool expected)
		{
			TestChargeCode.AC_ChargeGroup = chargeGroup;
			AssertEquals("Sub-group readonly", expected, TestChargeCode.AC_ChargeSubGroupInfo.ReadOnly);
		}

		public void TestGlobalChargeCodeValidatingLocalChargeCodeChildWithDifferentCountryCode()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();
			EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();

			var chargeCodeLinked = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == uSCompany.PK);
			var taxMessage2 = Factory.NewWithValidTestData<AccInvMsg>();
			taxMessage2.A9_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var taxRate = CreateTaxRate("TAX2");
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			Factory.Save();

			var taxOverride = CreateTaxOverride(chargeCodeLinked, Core.Constants.CountryCodes.UnitedStates, taxRate);
			taxOverride.AO_A9_DefaultVATClass = taxMessage2.PK;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				globalChargeCode.HasChanges = true;
				Factory.Save();
				AssertNoErrors(globalChargeCode);
			}
		}

		public void TestGlAccountSetupReadOnly()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, chargeType: Constants.ChargeType.Disbursement);

			bool originalChargeCodesEditGLAccountSetup = Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed;
			bool originalGlobalChargeCodesEditGLAccountSetup = Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed;
			bool originalChargeCodesLTGEditGLAccountSetup = Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed;

			try
			{
				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = true;
				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = true;
				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = true;

				AssertGlAccountSetupFieldsReadOnly(normalChargeCode, false);
				AssertGlAccountSetupFieldsReadOnly(normalChargeCodeLinked, false);
				AssertGlAccountSetupFieldsReadOnly(globalChargeCode, false);

				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = false;
				AssertGlAccountSetupFieldsReadOnly(normalChargeCode, true);
				AssertGlAccountSetupFieldsReadOnly(normalChargeCodeLinked, false);
				AssertGlAccountSetupFieldsReadOnly(globalChargeCode, false);

				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = false;
				AssertGlAccountSetupFieldsReadOnly(normalChargeCode, true);
				AssertGlAccountSetupFieldsReadOnly(normalChargeCodeLinked, false);
				AssertGlAccountSetupFieldsReadOnly(globalChargeCode, true);

				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = false;
				AssertGlAccountSetupFieldsReadOnly(normalChargeCode, true);
				AssertGlAccountSetupFieldsReadOnly(normalChargeCodeLinked, true);
				AssertGlAccountSetupFieldsReadOnly(globalChargeCode, true);
			}
			finally
			{
				Env.Security.ChargeCodesEditGLAccountSetup.IsAllowed = originalChargeCodesEditGLAccountSetup;
				Env.Security.GlobalChargeCodesEditGLAccountSetup.IsAllowed = originalGlobalChargeCodesEditGLAccountSetup;
				Env.Security.ChargeCodesLTGEditGLAccountSetup.IsAllowed = originalChargeCodesLTGEditGLAccountSetup;
			}
		}

		void AssertGlAccountSetupFieldsReadOnly(AccChargeCode codeToTest, bool expectedValue)
		{
			AssertEquals(expectedValue, codeToTest.AC_AR_SalesGroupInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AR_ExpenseGroupInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_RevenueAccountInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_WIPAccountInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_CostAccountInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_AccrualAccountInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestRatingAndQuotationsReadOnly()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			bool originalChargeCodesRatingAndQuotationsConfiguration = Env.Security.ChargeCodesRatingAndQuotationsConfiguration.IsAllowed;
			bool originalGlobalChargeCodesRatingAndQuotationsConfiguration = Env.Security.GlobalChargeCodesRatingAndQuotationsConfiguration.IsAllowed;
			bool originalChargeCodesLTGRatingAndQuotationsConfiguration = Env.Security.ChargeCodesLTGRatingAndQuotationsConfiguration.IsAllowed;

			try
			{
				Env.Security.ChargeCodesRatingAndQuotationsConfiguration.IsAllowed = true;
				Env.Security.GlobalChargeCodesRatingAndQuotationsConfiguration.IsAllowed = true;
				Env.Security.ChargeCodesLTGRatingAndQuotationsConfiguration.IsAllowed = true;

				AssertTestRatingAndQuotationsReadOnly(normalChargeCode, false);
				AssertTestRatingAndQuotationsReadOnly(normalChargeCodeLinked, false);
				AssertTestRatingAndQuotationsReadOnly(globalChargeCode, false);

				Env.Security.ChargeCodesRatingAndQuotationsConfiguration.IsAllowed = false;
				AssertTestRatingAndQuotationsReadOnly(normalChargeCode, true);
				AssertTestRatingAndQuotationsReadOnly(normalChargeCodeLinked, false);
				AssertTestRatingAndQuotationsReadOnly(globalChargeCode, false);

				Env.Security.GlobalChargeCodesRatingAndQuotationsConfiguration.IsAllowed = false;
				AssertTestRatingAndQuotationsReadOnly(normalChargeCode, true);
				AssertTestRatingAndQuotationsReadOnly(normalChargeCodeLinked, false);
				AssertTestRatingAndQuotationsReadOnly(globalChargeCode, true);

				Env.Security.ChargeCodesLTGRatingAndQuotationsConfiguration.IsAllowed = false;
				AssertTestRatingAndQuotationsReadOnly(normalChargeCode, true);
				AssertTestRatingAndQuotationsReadOnly(normalChargeCodeLinked, true);
				AssertTestRatingAndQuotationsReadOnly(globalChargeCode, true);
			}
			finally
			{
				Env.Security.ChargeCodesRatingAndQuotationsConfiguration.IsAllowed = originalChargeCodesRatingAndQuotationsConfiguration;
				Env.Security.GlobalChargeCodesRatingAndQuotationsConfiguration.IsAllowed = originalGlobalChargeCodesRatingAndQuotationsConfiguration;
				Env.Security.ChargeCodesLTGRatingAndQuotationsConfiguration.IsAllowed = originalChargeCodesLTGRatingAndQuotationsConfiguration;
			}
		}

		void AssertTestRatingAndQuotationsReadOnly(AccChargeCode codeToTest, bool expectedValue)
		{
			AssertEquals(expectedValue, codeToTest.AC_ChargeGroup_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_IsGroupageCharge_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_ChargeSubGroup_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_RateCalculator_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_ShowOnQuotation_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_SuppressOnQuoteIfZero_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_IsCommissionable_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_DefaultCommissionProduct_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_DefaultCommissionService_ReadOnly);
			AssertEquals(expectedValue, codeToTest.AC_DefaultCommissionSubModule_ReadOnly);
		}

		public void TestAC_ChargeType_List()
		{
			Assert("AC_ChargeType_List.Count > 0", TestChargeCode.Lookups.AC_ChargeType_List.Count > 0);
		}

		public void TestAC_RateCalculator_List()
		{
			Assert("AC_RateCalculator_List.Count > 0", TestChargeCode.Lookups.AC_RateCalculator_List.Count > 0);
		}

		public void TestChargeGroupList()
		{
			Assert("ChargeGroupList.Count > 0", TestChargeCode.Lookups.ChargeGroupList.Count > 0);
		}

		public void TestAC_ChargeType()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Comment;
			Assert("Must be readonly", TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;
			Assert("Must not be readonly", !TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.NonAccrual;
			Assert("Must be readonly", TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			Assert("Must not be readonly", !TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;
			Assert("Must be readonly", TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Revenue;
			Assert("Must not be readonly", !TestChargeCode.ChargeTypeOverrides.ReadOnly);
			TestChargeCode.AC_ChargeType = Constants.ChargeType.ManualJobAccrual;
			Assert("Must not be readonly", !TestChargeCode.ChargeTypeOverrides.ReadOnly);
		}

		public void TestAC_IsCommissionable()
		{
			TestChargeCode.AC_IsCommissionable = true;
			AssertEquals(false, TestChargeCode.AC_DefaultCommissionProductInfo.ReadOnly);
			AssertEquals(false, TestChargeCode.AC_DefaultCommissionServiceInfo.ReadOnly);
			AssertEquals(false, TestChargeCode.AC_DefaultCommissionSubModuleInfo.ReadOnly);

			TestChargeCode.AC_DefaultCommissionProduct = "AAA";
			TestChargeCode.AC_DefaultCommissionService = "BBB";
			TestChargeCode.AC_DefaultCommissionSubModule = "CCC";

			TestChargeCode.AC_IsCommissionable = false;
			AssertEquals(true, TestChargeCode.AC_DefaultCommissionProductInfo.ReadOnly);
			AssertEquals(true, TestChargeCode.AC_DefaultCommissionServiceInfo.ReadOnly);
			AssertEquals(true, TestChargeCode.AC_DefaultCommissionSubModuleInfo.ReadOnly);
			AssertEquals("Should be cleared", "", TestChargeCode.AC_DefaultCommissionProduct);
			AssertEquals("Should be cleared", "", TestChargeCode.AC_DefaultCommissionService);
			AssertEquals("Should be cleared", "", TestChargeCode.AC_DefaultCommissionSubModule);
		}

		public void TestDefaultCommissionPropertiesMaxLength()
		{
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccChargeCode.Schema.AC_DefaultCommissionProductMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccChargeCode.Schema.AC_DefaultCommissionServiceMaxLength);
			AssertEquals(OrgCommissionAgreementItem.Schema.CAI_CodeMaxLength, AccChargeCode.Schema.AC_DefaultCommissionSubModuleMaxLength);
		}

		public void TestIsFOBCharge()
		{
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Freight should NOT be an FOB charge", !TestChargeCode.IsFOBCharge);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Origin should be an FOB charge", TestChargeCode.IsFOBCharge);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Loading;
			Assert("Loading should be an FOB charge", TestChargeCode.IsFOBCharge);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Insurance;
			Assert("Insurance should NOT be an FOB charge", !TestChargeCode.IsFOBCharge);
		}

		public void TestIsFreight()
		{
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;
			Assert("Should be a freight charge", TestChargeCode.IsFreight);
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			Assert("Should NOT be a freight charge", !TestChargeCode.IsFreight);
		}

		public void TestChargeSubGroupList()
		{
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			AssertEquals(18, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CFSShipment;
			AssertEquals(16, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.ContainerStorage;
			AssertEquals(2, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
			AssertEquals(25, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerage;
			AssertEquals(18, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.OriginBrokerageOnly;
			AssertEquals(18, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;
			AssertEquals(26, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Brokerage;
			AssertEquals(22, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.BrokerageOnly;
			AssertEquals(22, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Transport;
			AssertEquals(16, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.TransportBooking;
			AssertEquals(15, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSInwards;
			AssertEquals(16, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.WHSOutwards;
			AssertEquals(16, TestChargeCode.Lookups.ChargeSubGroupList.Count);

			var testList = FreightDataRegistry.Instance.JobServices.Value;
			testList.Add("AD1", (NoResString)"Additional Service 1", false);
			testList.Add("AD2", (NoResString)"Additional Service 2", false);

			using (FreightDataRegistry.Instance.JobServices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testList))
			{
				TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Origin;
				Factory.ClearCachedValue<ChargeCodeSubGroupList>("ChargeSubGroupList" + TestChargeCode.AC_ChargeGroup + TestChargeCode.AC_GC.ToString());
				AssertEquals(27, TestChargeCode.Lookups.ChargeSubGroupList.Count);

				AccChargeCode code2 = Factory.New<AccChargeCode>();
				code2.AC_ChargeGroup = "ORG";
				AssertEquals(27, code2.Lookups.ChargeSubGroupList.Count);
			}
		}

		public void TestHasTaxOverrides()
		{
			TestChargeCode.TaxOverrides.RemoveAll();
			AssertEquals("Should have no tax overrides", false, TestChargeCode.HasTaxOverrides);

			AccChargeTaxOverride taxOverride = TestChargeCode.TaxOverrides.AddNew();
			taxOverride.AO_JobType = "SHP";
			taxOverride.AO_Direction = "IMP";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_Origin = "ZA";
			taxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_TaxRegCntryOrGroup = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxOverride.AO_AT = ZGuid.NewZGuid();
			AssertEquals("Should have tax overrides", true, TestChargeCode.HasTaxOverrides);

			TestChargeCode.TaxOverrides.RemoveAll();
			AssertEquals("Should have no tax overrides again", false, TestChargeCode.HasTaxOverrides);
		}

		public void TestHasTaxOverridesReadOnly()
		{
			Assert(TestChargeCode.HasTaxOverrides_ReadOnly);
		}

		public void TestHasTypeOverrides()
		{
			TestChargeCode.ChargeTypeOverrides.RemoveAll();
			AssertEquals("Should have no charge overrides", false, TestChargeCode.HasTypeOverrides);

			AccChargeTypeOverride typeOverride = TestChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertEquals("Should have charge overrides", true, TestChargeCode.HasTypeOverrides);

			TestChargeCode.ChargeTypeOverrides.RemoveAll();
			AssertEquals("Should have no charge overrides again", false, TestChargeCode.HasTypeOverrides);
		}

		public void TestHasTypeOverridesReadOnly()
		{
			Assert(TestChargeCode.HasTypeOverrides_ReadOnly);
		}

		/// <summary>
		/// The Charge Types which doesn't map dependent property info,
		/// should have it done at "SetupChargeTypeDepedentInfo()" - region "ChargeType-Depedent Info"
		/// </summary>
		public void TestAllChargeTypesHaveDependentInfoMapped()
		{
			foreach (CodeDescriptionPair chargeTypePair in TestChargeCode.Lookups.AC_ChargeType_List)
			{
				Assert("Charge Type [" + chargeTypePair.CodeAndDescription + "] should have dependent property info mapped",
					TestChargeCode.RequiredProperties(chargeTypePair.Code).IsValid);
			}
		}

		public void TestDefaultValuesSet()
		{
			AssertEquals("Default Company", Env.CurrentCompany.PK, TestChargeCode.AC_GC);
			AssertEquals("Charge Other Groups", ChargeOtherGroupsList.Codes.Principal, TestChargeCode.AC_ChargeOtherGroups);
		}

		public void TestGLCostAccountCollection()
		{
			SetUpAccountCollectionTests();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccGLHeaderCollection gLCostAccountCollection = testAccChargeCode.Lookups.GLCostAccountCollection;
			gLCostAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", gLCostAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLCostAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			gLCostAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !gLCostAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLCostAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			gLCostAccountCollection.Load();

			Assert("Total account should not be in the collection", !gLCostAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", gLCostAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		public void TestGLRevenueAccountCollection()
		{
			SetUpAccountCollectionTests();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccGLHeaderCollection gLRevenueAccountCollection = testAccChargeCode.Lookups.GLRevenueAccountCollection;
			gLRevenueAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", gLRevenueAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLRevenueAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			gLRevenueAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !gLRevenueAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLRevenueAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			gLRevenueAccountCollection.Load();

			Assert("Total account should not be in the collection", !gLRevenueAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", gLRevenueAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		public void TestGLWIPAccountCollection()
		{
			SetUpAccountCollectionTests();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccGLHeaderCollection gLWIPAccountCollection = testAccChargeCode.Lookups.GLWIPAccountCollection;
			gLWIPAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", gLWIPAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLWIPAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			gLWIPAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !gLWIPAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLWIPAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			gLWIPAccountCollection.Load();

			Assert("Total account should not be in the collection", !gLWIPAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", gLWIPAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		public void TestGLAccrualAccountCollection()
		{
			SetUpAccountCollectionTests();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccGLHeaderCollection gLAccrualAccountCollection = testAccChargeCode.Lookups.GLAccrualAccountCollection;
			gLAccrualAccountCollection.Load();

			Assert("Profit and Loss GL Accounts should be in the collection", gLAccrualAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLAccrualAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = true;
			Factory.Save();
			gLAccrualAccountCollection.Load();

			Assert("Control accounts should not be in the collection", !gLAccrualAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
			Assert("Balance sheet GL Accounts should be in the collection", gLAccrualAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));

			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.Total;
			Factory.Save();
			gLAccrualAccountCollection.Load();

			Assert("Total account should not be in the collection", !gLAccrualAccountCollection.Contains(TestBalanceSheetAccountGLHeader.PK));
			Assert("Profit and Loss GL Accounts should be in the collection", gLAccrualAccountCollection.Contains(TestProfitAndLossGLHeader.PK));
		}

		public static void InsertGSTREVTaxRate(GlbCompany company)
		{
			var newFactory = new BusinessObjectFactory();

			var rate = newFactory.New<AccTaxRate>();
			rate.AT_Code = "GSTx";
			rate.AT_Type = AccTaxRate.Types.Rated;
			rate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
			rate.AT_Description = "GST Description";

			var query = new ZQuery(StmDataSchema.SD_Name, AccTaxRate.Helper.MainGSTTaxRegistryID);
			query.AddToFilter(StmDataSchema.SD_Owner, company.PK);
			var item = newFactory.LoadTop1<StmData>(query);

			if (item == null)
			{
				item = newFactory.New<StmData>();
				item.SD_Name = AccTaxRate.Helper.MainGSTTaxRegistryID;
				item.SD_Owner = company.PK;
			}
			item.SD_GuidValue = rate.PK;

			rate = newFactory.New<AccTaxRate>();
			rate.AT_Code = "GSTREVx";
			rate.AT_Type = AccTaxRate.Types.ReverseRated;
			rate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
			rate.AT_Description = "GSTREV Description";

			query = new ZQuery(StmDataSchema.SD_Name, AccTaxRate.Helper.MainGSTReverseTaxRegistryID);
			query.AddToFilter(StmDataSchema.SD_Owner, company.PK);
			item = newFactory.LoadTop1<StmData>(query);

			if (item == null)
			{
				item = newFactory.New<StmData>();
				item.SD_Name = AccTaxRate.Helper.MainGSTReverseTaxRegistryID;
				item.SD_Owner = company.PK;
			}
			item.SD_GuidValue = rate.PK;

			rate = newFactory.New<AccTaxRate>();
			rate.AT_Code = "NOTREPORTx";
			rate.AT_Type = AccTaxRate.Types.NotReportable;
			rate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
			rate.AT_Description = "NOTREPort Description";

			query = new ZQuery(StmDataSchema.SD_Name, AccTaxRate.Helper.MainNotReportableTaxRegistryID);
			query.AddToFilter(StmDataSchema.SD_Owner, company.PK);
			item = newFactory.LoadTop1<StmData>(query);

			if (item == null)
			{
				item = newFactory.New<StmData>();
				item.SD_Name = AccTaxRate.Helper.MainNotReportableTaxRegistryID;
				item.SD_Owner = company.PK;
			}
			item.SD_GuidValue = rate.PK;

			newFactory.Save();
		}

		public void TestCalculateCorrectGSTRateForOrgsWithSameTaxRegDetailsAsLoginCompany_withNOTREPORToverride()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1651351";
				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);

				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";

				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);
			}
		}

		public void TestCalculateCorrectGSTRateForOrgsWithSameTaxRegDetailsAsLoginCompany()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1651351";
				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);

				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";

				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);
			}

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(AccTaxRate.Types.NotReportable, rate.AT_Type);
		}

		public void TestCalculateCorrectGSTRateForOrgThatIsProxyOfLoginCompany()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader companyProxy = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader branchProxy = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = branchProxy.PK;

			ZGuid overrideInvTaxMsg;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				var rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, companyProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);

				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, branchProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);
			}
		}

		public void TestCalculateCorrectGSTRateForOrgThatIsProxyOfLoginCompany_reportable()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader companyProxy = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader branchProxy = Factory.NewWithValidTestData<OrgHeader>();

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = branchProxy.PK;

			ZGuid overrideInvTaxMsg;
			AccTaxRate rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, companyProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(AccTaxRate.Types.NotReportable, rate.AT_Type);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, branchProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(AccTaxRate.Types.NotReportable, rate.AT_Type);
		}

		public void TestCommentChargeCodeSetsEmptyTaxID()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			OrgHeader companyProxy = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader branchProxy = Factory.NewWithValidTestData<OrgHeader>();

			ZGuid overrideInvTaxMsg;
			AccTaxRate rate = TestChargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, companyProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals("Comment Charge Code should not set a GST Rate", null, rate);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			GlbCompany.CurrentCompany.Branches[0].GB_OH_OrgProxy = branchProxy.PK;

			rate = TestChargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, companyProxy, null, null, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals("Comment Charge Code should not set a GST Rate", null, rate);
		}

		public void TestGetGSTRateRecalculatedDirectionForEU()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AccChargeTaxOverride taxOverride = code.TaxOverrides.AddNew();
				taxOverride.AO_CostSellAll = "ALL";
				taxOverride.AO_Direction = "IMP";
				taxOverride.AO_JobType = "ALL";
				taxOverride.AO_Origin = EconomicGroupList.Codes.EuropeanUnion;
				taxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxOverride.AO_IncoTerm = "ALL";
				taxOverride.AO_TaxRegCntryOrGroup = "ALL";
				taxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				ZGuid overrideInvTaxMsg;
				AccTaxRate rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, org, countryFR, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(taxOverride.AO_AT, rate.PK);
				AssertEquals(taxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			}
		}

		public void TestGetGSTRateRecalculatedDirectionForDomestic()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AccChargeCode correctTaxCode = Factory.NewWithValidTestData<AccChargeCode>();
				correctTaxCode.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AccChargeTaxOverride correctTaxOverride = correctTaxCode.TaxOverrides.AddNew();
				correctTaxOverride.AO_CostSellAll = "ALL";
				correctTaxOverride.AO_Direction = "DOM";
				correctTaxOverride.AO_JobType = "ALL";
				correctTaxOverride.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				correctTaxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				correctTaxOverride.AO_IncoTerm = "ALL";
				correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				correctTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				AccChargeTaxOverride incorrectTaxOverride = correctTaxCode.TaxOverrides.AddNew();
				incorrectTaxOverride.AO_CostSellAll = "ALL";
				incorrectTaxOverride.AO_Direction = "IMP";
				incorrectTaxOverride.AO_JobType = "ALL";
				incorrectTaxOverride.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				incorrectTaxOverride.AO_Destination = countryAU.RN_Code;
				incorrectTaxOverride.AO_IncoTerm = "ALL";
				incorrectTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				incorrectTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				incorrectTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				ZGuid overrideInvTaxMsg;
				AccTaxRate rate = correctTaxCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(correctTaxOverride.AO_AT, rate.PK);
				AssertEquals(correctTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			}
		}

		public void TestGetGSTRateUsesOverrideTaxMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AccChargeCode correctTaxCode = Factory.NewWithValidTestData<AccChargeCode>();
				correctTaxCode.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AccChargeTaxOverride correctTaxOverride = correctTaxCode.TaxOverrides.AddNew();
				correctTaxOverride.AO_CostSellAll = "ALL";
				correctTaxOverride.AO_Direction = "DOM";
				correctTaxOverride.AO_JobType = "ALL";
				correctTaxOverride.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				correctTaxOverride.AO_Destination = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				correctTaxOverride.AO_IncoTerm = "ALL";
				correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				correctTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				ZGuid overrideInvTaxMsg;
				AccTaxRate rate = correctTaxCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(correctTaxOverride.AO_AT, rate.PK);
				AssertEquals(correctTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			}
		}

		public void TestGetGSTRateWithSupplyType()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var taxRate1 = CreateTaxRate("TAX1");
				var taxRate2 = CreateTaxRate("TAX2");
				var taxRate3 = CreateTaxRate("TAX3");
				var taxRate4 = CreateTaxRate("TAX4");

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "TST";
				chargeCode.AC_AT_GSTRate = taxRate1.PK;

				var taxOverride2 = CreateTaxOverride(chargeCode, "SG", taxRate2);
				taxOverride2.AO_SupplyType = SupplyTypeClassificationCodes.DSB;

				var taxOverride3 = CreateTaxOverride(chargeCode, "SG", taxRate3);
				taxOverride3.AO_SupplyType = SupplyTypeClassificationCodes.LOC;

				var taxOverride4 = CreateTaxOverride(chargeCode, "SG", taxRate4);
				taxOverride4.AO_SupplyType = SupplyTypeClassificationCodes.LOA;

				var organisationSGSIN = CreateOrganisation(Factory, "Org1", "SGSIN");

				Factory.Save();

				var montreal = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA");
				var quebec = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA");

				ZGuid overrideInvTaxMsg;

				var taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, null, SupplyTypeClassificationCodes.DSB, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is null, should use fall back to organisation (SG)", taxRate2.AT_Code, taxRate.AT_Code);

				taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, null, SupplyTypeClassificationCodes.LOC, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is in AU", taxRate3.AT_Code, taxRate.AT_Code);

				taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, null, SupplyTypeClassificationCodes.LOA, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is in US", taxRate4.AT_Code, taxRate.AT_Code);
			}
		}

		public void TestGetGSTRateWithFixedPlaceOfSupply()
		{
			using (AccountingMasterFilesRegistry.Instance.UseCusClearPortAsHomeCntryForTaxOvrds.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var taxRate1 = CreateTaxRate("TAX1");
				var taxRate2 = CreateTaxRate("TAX2");
				var taxRate3 = CreateTaxRate("TAX3");
				var taxRate4 = CreateTaxRate("TAX4");

				var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "TST";
				chargeCode.AC_AT_GSTRate = taxRate1.PK;

				var taxOverride2 = CreateTaxOverride(chargeCode, "SG", taxRate2);
				var taxOverride3 = CreateTaxOverride(chargeCode, "AU", taxRate3);
				var taxOverride4 = CreateTaxOverride(chargeCode, "US", taxRate4);

				var organisationSGSIN = CreateOrganisation(Factory, "Org1", "SGSIN");
				var organisationCATOR = CreateOrganisation(Factory, "Org2", "CATOR");

				Factory.Save();

				var montreal = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA");
				var quebec = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA");
				var sydney = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Sydney", "AU");
				var newYork = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "New York", "US");

				ZGuid overrideInvTaxMsg;

				var taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationCATOR,
					montreal, quebec, QuebecBranch, null, null, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is null, should use setting from charge code because no override for CA", taxRate1.AT_Code, taxRate.AT_Code);

				taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, null, null, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is null, should use fall back to organisation (SG)", taxRate2.AT_Code, taxRate.AT_Code);

				taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, sydney, null, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is in AU", taxRate3.AT_Code, taxRate.AT_Code);

				taxRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, organisationSGSIN,
					montreal, quebec, QuebecBranch, newYork, null, out overrideInvTaxMsg);
				AssertEquals("fixed place of supply is in US", taxRate4.AT_Code, taxRate.AT_Code);
			}
		}

		AccTaxRate CreateTaxRate(string taxCode)
		{
			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = taxCode;
			return taxRate;
		}

		AccChargeTaxOverride CreateTaxOverride(AccChargeCode chargeCode, string homeCountryOrZone, AccTaxRate taxRate)
		{
			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_TransportMode = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_HomeCountryOrZone = homeCountryOrZone;
			taxOverride.AO_AT = taxRate.PK;
			return taxOverride;
		}

		OrgHeader CreateOrganisation(BusinessObjectFactory factory, string orgCode, string closestPort)
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_Code = orgCode;
			organisation.OH_RL_NKClosestPort = closestPort;
			organisation.CompanyData.SetARTaxApplicable(true);
			return organisation;
		}

		public void TestGetGSTOverrideTaxMessage()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "NOT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = "RAT";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			code.AC_AT_GSTRate = taxRate2.PK;

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType =
				ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);

			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.SetMainNotReportableTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), taxRate.PK.ToGuid());

			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");
				accounting.SetMainNotReportableTaxIDConfiguration(company.PK.ToGuid(), Guid.Empty);
				code.AC_GC = GlbCompany.CurrentCompany.PK;
				var taxMsgPK = Factory.NewWithValidTestData<AccInvMsg>().PK;
				mock.Setup(m => m.GroupMemberBillingDefaultInvoiceTaxMessage).Returns(taxMsgPK);
				(rate, overrideInvTaxMsg) = GetGSTRateTaxMessage();
				AssertEquals(taxMsgPK, overrideInvTaxMsg);
			}

			(AccTaxRate, ZGuid) GetGSTRateTaxMessage()
			{
				var gstRate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue,
					JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null,
					out ZGuid taxMessage);

				return (gstRate, taxMessage);
			}
		}

		public void TestGetGSTOverrideTaxMessage_notReportable()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "NOT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = "RAT";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			code.AC_AT_GSTRate = taxRate2.PK;

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType =
				ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);

			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.SetMainNotReportableTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), taxRate.PK.ToGuid());

			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");
				accounting.SetMainNotReportableTaxIDConfiguration(company.PK.ToGuid(), Guid.Empty);
				code.AC_GC = company.PK;
				AssertEquals("Precondition: code.GSTRate", taxRate2.PK, code.GSTRate.PK);
				(rate, overrideInvTaxMsg) = GetGSTRateTaxMessage();
				AssertEquals(taxRate2.AT_A9_DefaultVatClass, overrideInvTaxMsg);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Instance.Clear();
			}

			(AccTaxRate, ZGuid) GetGSTRateTaxMessage()
			{
				var gstRate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue,
					JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null,
					out ZGuid taxMessage);

				return (gstRate, taxMessage);
			}
		}

		public void TestGetGSTOverrideTaxMessage_emptyDefaultTaxMessage()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Type = "NOT";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate2.AT_Type = "RAT";
			taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate2.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			code.AC_AT_GSTRate = taxRate2.PK;

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType =
				ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);

			var accounting = ObjectFactory.Get<IAccounting>();
			accounting.SetMainNotReportableTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), taxRate.PK.ToGuid());

			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "SIN");
				accounting.SetMainNotReportableTaxIDConfiguration(company.PK.ToGuid(), Guid.Empty);
				code.AC_GC = company.PK;
				AssertEquals("Precondition: code.GSTRate", taxRate2.PK, code.GSTRate.PK);
				(rate, overrideInvTaxMsg) = GetGSTRateTaxMessage();
				AssertEquals(taxRate2.AT_A9_DefaultVatClass, overrideInvTaxMsg);
				AssertEquals(0, ErrorReporter.TotalErrorCount);
				ErrorReporter.Instance.Clear();

				code.AC_GC = GlbCompany.CurrentCompany.PK;
				mock.Setup(m => m.GroupMemberBillingDefaultInvoiceTaxMessage).Returns(ZGuid.Empty);
				(rate, overrideInvTaxMsg) = GetGSTRateTaxMessage();
				AssertEquals(taxRate.AT_A9_DefaultVatClass, overrideInvTaxMsg);
			}

			(AccTaxRate, ZGuid) GetGSTRateTaxMessage()
			{
				var gstRate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue,
					JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null,
					out ZGuid taxMessage);

				return (gstRate, taxMessage);
			}
		}

		public void TestGetGSTRateRecalculatedDirectionForOther()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
				AccChargeTaxOverride correctTaxOverride = code.TaxOverrides.AddNew();
				correctTaxOverride.AO_CostSellAll = "ALL";
				correctTaxOverride.AO_Direction = "OTH";
				correctTaxOverride.AO_JobType = "ALL";
				correctTaxOverride.AO_Origin = AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry;
				correctTaxOverride.AO_Destination = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
				correctTaxOverride.AO_IncoTerm = "ALL";
				correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				correctTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				AccChargeTaxOverride incorrectTaxOverride = code.TaxOverrides.AddNew();
				incorrectTaxOverride.AO_CostSellAll = "ALL";
				incorrectTaxOverride.AO_Direction = "IMP";
				incorrectTaxOverride.AO_JobType = "ALL";
				incorrectTaxOverride.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				incorrectTaxOverride.AO_Destination = countryAU.RN_Code;
				incorrectTaxOverride.AO_IncoTerm = "ALL";
				incorrectTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				incorrectTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				incorrectTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();

				ZGuid overrideInvTaxMsg;
				AccTaxRate rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryFR, countryAU, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(correctTaxOverride.AO_AT, rate.PK);
				AssertEquals(correctTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			}
		}

		public void TestGetGSTRateUsesCache()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);

			ZGuid overrideInvTaxMsg;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "1651351";
			AssertEquals(false, org.IsInterOfficeBillingOrgNotReportableForTax);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(code.AC_AT_GSTRate, rate.PK);

			int dbHitsCount = Factory.DatabaseLoadCount;
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";
			AssertEquals(true, org.IsInterOfficeBillingOrgNotReportableForTax);

			Assert("Condition to get a NotRportable tax", ObjectFactory.Get<IAccounting>().OverrideInterOfficeBillingTaxIDToNOTREPORT);
			Assert("Condition to get a NotRportable tax", org.IsInterOfficeBillingOrgNotReportableForTax);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertNotEquals("Cache should not work for a NonReportable Tax condition", code.AC_AT_GSTRate, rate.PK);
			AssertEquals("SHould be a NonReportable Tax", AccTaxRate.Types.NotReportable, rate.AT_Type);
		}

		public void TestGetGSTRateUsesCache_reportable()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
			taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
			taxRegCode.OK_CustomsRegNo = "123 4 5 6";

			AccTaxRate rate;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(false);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);

			ZGuid overrideInvTaxMsg;
			using (ObjectFactory.Substitute(mock.Object))
			{
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1651351";
				AssertEquals(false, org.IsInterOfficeBillingOrgNotReportableForTax);

				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);

				int dbHitsCount = Factory.DatabaseLoadCount;
				GlbCompany.CurrentCompany.GC_BusinessRegNo = "1 23 4 5 6";
				AssertEquals(true, org.IsInterOfficeBillingOrgNotReportableForTax);

				rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryAU, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(code.AC_AT_GSTRate, rate.PK);
				AssertEquals("Should be no new DB hits", dbHitsCount, Factory.DatabaseLoadCount);
			}
		}

		[TestDate(2009, 12, 25)]
		public void TestGetSellTaxCodeForEUCompanyWithDomesticNonLocalJobs_Before20100101()
		{
			InsertGSTREVTaxRate(GlbCompany.CurrentCompany);

			AccTaxRate[] taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Australia));
			foreach (AccTaxRate taxRate in taxRates)
			{
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.UnitedKingdom;
			}
			Factory.Save();

			RefCountry countryUK = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France);
			RefCountry countryDE = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Germany);

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryUK.Code))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "DEFRA"; // Germany
				OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
				taxRegCode.OK_RN_NKCodeCountry = countryDE.Code;
				taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.OK_RN_NKCodeCountry);
				taxRegCode.OK_CustomsRegNo = "123456";

				AccTaxRate rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryFR, countryFR);
				AssertEquals("Rate should be NOTREPORT", AccTaxRate.Types.NotReportable, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryFR, countryFR);
				AssertEquals("Rate should be NOTREPORT", AccTaxRate.Types.NotReportable, rate.AT_Type);

				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryFR, countryDE);
				AssertEquals("Rate should be GST", AccTaxRate.Types.NotReportable, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryFR, countryDE);
				AssertEquals("Rate should be GSTREV", AccTaxRate.Types.ReverseRated, rate.AT_Type);

				org.OH_RL_NKClosestPort = "UKLON";
				taxRegCode.OK_RN_NKCodeCountry = countryUK.Code;
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryUK, countryUK);
				AssertEquals("Rate should be GST", AccTaxRate.Types.Rated, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryUK, countryUK);
				AssertEquals("Rate should be GST", AccTaxRate.Types.Rated, rate.AT_Type);
			}
		}

		public void TestGetSellTaxCodeForEUCompanyWithDomesticNonLocalJobs()
		{
			InsertGSTREVTaxRate(GlbCompany.CurrentCompany);

			AccTaxRate[] taxRates = Factory.Load<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, Constants.CountryCodes.Australia));
			foreach (AccTaxRate taxRate in taxRates)
			{
				taxRate.AT_RN_NKCountry = Constants.CountryCodes.UnitedKingdom;
			}
			Factory.Save();

			RefCountry countryUK = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France);
			RefCountry countryDE = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Germany);

			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryUK.Code))
			{
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_RL_NKClosestPort = "DEFRA"; // Germany
				OrgCusCode taxRegCode = org.CustomsCodes.AddNew();
				taxRegCode.OK_RN_NKCodeCountry = countryDE.Code;
				taxRegCode.OK_CodeType = ZArchitecture.Environment.Country.GetConsumptionTaxRegistrationOrgCusCode(taxRegCode.CodeCountry.Code);
				taxRegCode.OK_CustomsRegNo = "123456";

				AccTaxRate rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryFR, countryFR);
				AssertEquals("Rate should be GSTREV", AccTaxRate.Types.ReverseRated, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryFR, countryFR);
				AssertEquals("Rate should be GSTREV", AccTaxRate.Types.ReverseRated, rate.AT_Type);

				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryFR, countryDE);
				AssertEquals("Rate should be GSTREV", AccTaxRate.Types.ReverseRated, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryFR, countryDE);
				AssertEquals("Rate should be GSTREV", AccTaxRate.Types.ReverseRated, rate.AT_Type);

				org.OH_RL_NKClosestPort = "UKLON";
				taxRegCode.OK_RN_NKCodeCountry = countryUK.Code;
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryUK, countryUK);
				AssertEquals("Rate should be GST", AccTaxRate.Types.Rated, rate.AT_Type);
				rate = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, org, countryUK, countryUK);
				AssertEquals("Rate should be GST", AccTaxRate.Types.Rated, rate.AT_Type);
			}
		}

		OrgHeader GetOrganisationWithTaxRegistration(RefCountry countryOfRegistration)
		{
			OrgHeader result = Factory.NewWithValidTestData<OrgHeader>();
			OrgCusCode code = result.CustomsCodes.AddNew();
			code.OK_RN_NKCodeCountry = countryOfRegistration.Code;
			code.OK_CodeType = Country.GetConsumptionTaxRegistrationOrgCusCode(countryOfRegistration.RN_Code);
			code.OK_CustomsRegNo = "ABCDE1234";

			return result;
		}

		[TestDate(2009, 12, 25)]
		public void TestGetCostTaxCodeForCompanyInEuropeanUnion_UK_Before20100101()
		{
			var countryUK = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			var countryFR = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France);
			var countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			var orgInUK = GetOrganisationWithTaxRegistration(countryUK);
			var orgInFR = GetOrganisationWithTaxRegistration(countryFR);
			var orgInAU = GetOrganisationWithTaxRegistration(countryAU);

			var chargeCode = Factory.New<AccChargeCode>();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryUK.Code))
			{
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryUK).AT_Code);

				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryFR).AT_Code);

				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryAU).AT_Code);

				// Treat EU --> CU the same as EU --> EU
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryFR, countryUK).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryFR, countryUK).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryFR, countryUK).AT_Code);

				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryFR, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryFR, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryFR, countryFR).AT_Code);

				// Treat EU --> "  " the same as EU --> EU
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryAU).AT_Code);

				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryUK).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryUK).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryUK).AT_Code);

				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryFR).AT_Code);

				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryAU).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryAU).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryAU).AT_Code);
			}
		}

		public void TestGetCostTaxCodeForCompanyInEuropeanUnion_UK()
		{
			var countryUK = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			var countryFR = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.France);
			var countryAU = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			var orgInUK = GetOrganisationWithTaxRegistration(countryUK);
			var orgInFR = GetOrganisationWithTaxRegistration(countryFR);
			var orgInAU = GetOrganisationWithTaxRegistration(countryAU);

			var chargeCode = Factory.New<AccChargeCode>();

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryUK.Code))
			{
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryUK).AT_Code);

				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryFR).AT_Code);

				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryAU).AT_Code);

				// Treat EU --> CU the same as EU --> EU
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryFR, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryFR, countryUK).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryFR, countryUK).AT_Code);

				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryFR, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryFR, countryFR).AT_Code);
				AssertEquals("Tax Code", "VAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryFR, countryFR).AT_Code);

				// Treat EU --> "  " the same as EU --> EU
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryUK, countryAU).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryUK, countryAU).AT_Code);

				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryUK).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryUK).AT_Code);
				AssertEquals("Tax Code", "FREEVAT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryUK).AT_Code);

				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryFR).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryFR).AT_Code);

				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryAU, countryAU).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInFR, countryAU, countryAU).AT_Code);
				AssertEquals("Tax Code", "NOTREPORT", chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInAU, countryAU, countryAU).AT_Code);
			}
		}

		public void TestGetCostTaxCodeForCompanyInEuropeanUnion_UK_CachedWithFactory()
		{
			var countryUK = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.UnitedKingdom);
			var orgInUK = GetOrganisationWithTaxRegistration(countryUK);
			var chargeCode = Factory.New<AccChargeCode>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryUK.Code))
			{
				var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate1.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				Factory.Save();

				var collection = new EUTaxIDDefaultingRuleCollection();
				var rule = collection.AddNew();
				rule.Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry;
				rule.Destination = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin;
				rule.SellTaxRateForOrganisationRegisteredInMyCountry = taxRate1.PK;
				rule.CostTaxRateForOrganisationRegisteredInOtherEUCountry = taxRate2.PK;
				AccChargeCodeRegistry.Instance.EUTaxIDDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				var result = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryUK);
				AssertEquals($"GetGSTRateForCompanyInEuropeanUnion should return taxRate1", taxRate1.PK, result.PK);

				rule.SellTaxRateForOrganisationRegisteredInMyCountry = taxRate2.PK;
				rule.CostTaxRateForOrganisationRegisteredInOtherEUCountry = taxRate1.PK;
				AccChargeCodeRegistry.Instance.EUTaxIDDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

				result = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryUK);
				AssertEquals($"GetGSTRateForCompanyInEuropeanUnion should still return taxRate1 as the factory already cached the value.", taxRate1.PK, result.PK);

				Factory.Save();

				result = chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, orgInUK, countryUK, countryUK);
				AssertEquals($"GetGSTRateForCompanyInEuropeanUnion should still return taxRate2 as factory cleared cached value when factory saved.", taxRate2.PK, result.PK);
			}
		}

		#region GetTaxCodeForIndia

		public void TestGSTRateHandlesGSTANDEDU()
		{
			ZString originalCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			RefCountry countryIN = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.India);
			try
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryIN.Code;

				AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode.AC_Code = "TST";

				AccTaxRate gSTAndEDURate = Factory.LoadTop1<AccTaxRate>(new ZQuery(new ZQuery(AccTaxRateSchema.AT_Code, "GSTANDEDU"), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)));
				if (gSTAndEDURate != null)
				{
					gSTAndEDURate.Delete();
				}

				AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
				taxRate.AT_Code = "TAX1";
				chargeCode.AC_AT_GSTRate = taxRate.PK;

				AccTaxRate newGSTAndEDURate = Factory.NewWithValidTestData<AccTaxRate>();
				newGSTAndEDURate.AT_Code = "GSTANDEDU";
				newGSTAndEDURate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				newGSTAndEDURate.AT_Type = AccTaxRate.Types.Rated;
				newGSTAndEDURate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax;
				newGSTAndEDURate.AT_Description = "GST and EDU";
				newGSTAndEDURate.AT_IsActive = ZBool.True;
				newGSTAndEDURate.SetRateNumerator_ForTestOnly(10);
				newGSTAndEDURate.SetExtraRate_ForTestOnly(3, 1);

				AccTaxOverrideGroup group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				AccChargeTaxOverride correctTaxOverride = group.TaxOverrides.AddNew();
				correctTaxOverride.AO_CostSellAll = "ALL";
				correctTaxOverride.AO_Direction = "ALL";
				correctTaxOverride.AO_JobType = "ALL";
				correctTaxOverride.AO_Origin = "IN";
				correctTaxOverride.AO_Destination = "ALL";
				correctTaxOverride.AO_IncoTerm = "ALL";
				correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				correctTaxOverride.AO_AT = newGSTAndEDURate.PK;
				correctTaxOverride.AO_CustomsStatus = "";
				correctTaxOverride.AO_HomeCountryOrZone = "";
				correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
				correctTaxOverride.AO_VATExemptOnExportCharges = false;

				chargeCode.AC_AX_TaxOverrideGroup = group.PK;
				Factory.Save();

				ZGuid overrideInvTaxMsg;
				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
				GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();

				gSTAndEDURate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Domestic, AccChargeTaxOverride.ALL, org,
					RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Mumbai", "IN"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Delhi", "IN"), branch, null, null, out overrideInvTaxMsg);

				AssertEquals("Tax Code must be GSTANDEDU", newGSTAndEDURate.PK, gSTAndEDURate.PK);

				gSTAndEDURate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Domestic, AccChargeTaxOverride.ALL, org,
					RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA"), branch, null, null, out overrideInvTaxMsg);

				AssertEquals("Tax Code must be GSTANDEDU", taxRate.PK, gSTAndEDURate.PK);

				gSTAndEDURate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Domestic, AccChargeTaxOverride.ALL, org,
					RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Mumbai", "IN"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA"), branch, null, null, out overrideInvTaxMsg);

				AssertEquals("Tax Code must be GSTANDEDU", newGSTAndEDURate.PK, gSTAndEDURate.PK);
			}
			finally
			{
				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = originalCountry;
			}
		}

		#endregion

		#region GetTaxCodeForQuebec

		public void TestGetGSTRateHandlesGSTANDQST()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			AccTaxRate gSTAndQSTRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(new ZQuery(AccTaxRateSchema.AT_Code, "GSTANDQST"), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)));
			if (gSTAndQSTRate != null)
			{
				gSTAndQSTRate.Delete();
			}

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			chargeCode.AC_AT_GSTRate = taxRate.PK;

			AccTaxRate newGSTAndQSTRate = Factory.NewWithValidTestData<AccTaxRate>();
			newGSTAndQSTRate.AT_Code = "GSTANDQST";
			newGSTAndQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newGSTAndQSTRate.AT_Type = AccTaxRate.Types.Rated;
			newGSTAndQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			newGSTAndQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			newGSTAndQSTRate.AT_Description = "GST and QST";
			newGSTAndQSTRate.AT_IsActive = ZBool.True;
			newGSTAndQSTRate.SetRateNumerator_ForTestOnly(5);
			newGSTAndQSTRate.SetExtraRate_ForTestOnly(75, 10);

			AccTaxOverrideGroup group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AccChargeTaxOverride correctTaxOverride = group.TaxOverrides.AddNew();
			correctTaxOverride.AO_CostSellAll = "ALL";
			correctTaxOverride.AO_Direction = "OTH";
			correctTaxOverride.AO_TransportMode = "ALL";
			correctTaxOverride.AO_JobType = "ALL";
			correctTaxOverride.AO_Origin = "CA";
			correctTaxOverride.AO_Destination = "CA";
			correctTaxOverride.AO_IncoTerm = "ALL";
			correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
			correctTaxOverride.AO_AT = newGSTAndQSTRate.PK;
			correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

			chargeCode.AC_AX_TaxOverrideGroup = group.PK;
			Factory.Save();

			ZGuid overrideInvTaxMsg;

			gSTAndQSTRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, QuebecOrganisation,
				RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Quebec", "CA"), QuebecBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(newGSTAndQSTRate.PK, gSTAndQSTRate.PK);

			gSTAndQSTRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, QuebecOrganisation,
				RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA"), NotQuebecBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(newGSTAndQSTRate.PK, gSTAndQSTRate.PK);

			gSTAndQSTRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, NotQuebecOrg,
				RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA"), RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Montreal", "CA"), NotQuebecBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(newGSTAndQSTRate.PK, gSTAndQSTRate.PK);
		}

		GlbBranch notQuebecBranch;
		GlbBranch NotQuebecBranch
		{
			get
			{
				if (notQuebecBranch == null)
				{
					notQuebecBranch = Factory.LoadTop1<GlbBranch>(new ZQuery());
					if (notQuebecBranch.HomePort != null)
					{
						if (QuebecZone.UNLOCOs.Contains(notQuebecBranch.HomePort))
						{
							QuebecZone.UNLOCOs.Remove(notQuebecBranch.HomePort);
						}
					}
				}
				return notQuebecBranch;
			}
		}

		OrgHeader notQuebecOrg;
		OrgHeader NotQuebecOrg
		{
			get
			{
				if (notQuebecOrg == null)
				{
					notQuebecOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "ABIGAS"));
					RefUNLOCO notQuebecOrgPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, notQuebecOrg.MainAddress.OA_RL_NKRelatedPortCode));
					if (notQuebecOrgPort != null)
					{
						if (QuebecZone.UNLOCOs.Contains(notQuebecOrgPort))
						{
							QuebecZone.UNLOCOs.Remove(notQuebecOrgPort);
						}
					}
				}
				return notQuebecOrg;
			}
		}

		GlbBranch QuebecBranch
		{
			get
			{
				if (quebecBranch == null)
				{
					quebecBranch = Factory.New<GlbBranch>();
					quebecBranch.GB_Code = "QCB";
					quebecBranch.GB_GC = GlbCompany.CurrentCompany.PK;

					RefUNLOCO quebecUnloco1 = Factory.New<RefUNLOCO>();
					quebecUnloco1.Code = "QCUL1";
					quebecUnloco1.Description = "Quebec Unloco 1";
					quebecUnloco1.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
					QuebecZone.UNLOCOs.Add(quebecUnloco1);

					quebecBranch.GB_RL_NKHomePort = quebecUnloco1.Code;
					Factory.Save();
				}
				return quebecBranch;
			}
		}
		GlbBranch quebecBranch;

		OrgHeader quebecOrganisation;
		OrgHeader QuebecOrganisation
		{
			get
			{
				if (quebecOrganisation == null)
				{
					quebecOrganisation = Factory.NewWithValidTestData<OrgHeader>();
					quebecOrganisation.OH_Code = "QCOrg";
					quebecOrganisation.OH_FullName = "Quebec Organisation";
					quebecOrganisation.MainAddress.OA_Address1 = "Quebec Org Address";
					RefUNLOCO quebecUnloco2 = Factory.New<RefUNLOCO>();
					quebecUnloco2.Code = "QCUL2";
					quebecUnloco2.Description = "Quebec Unloco 2";
					quebecUnloco2.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
					QuebecZone.UNLOCOs.Add(quebecUnloco2);

					quebecOrganisation.MainAddress.OA_RL_NKRelatedPortCode = "QCUL2";
					Factory.Save();
				}
				return quebecOrganisation;
			}
		}

		RefZoneHeader QuebecZone
		{
			get
			{
				if (quebecZone == null)
				{
					quebecZone = Factory.LoadTop1<RefZoneHeader>(new ZQuery(RefZoneHeaderSchema.FZ_Code, "QUBC"));
					if (quebecZone == null)
					{
						quebecZone = Factory.New<RefZoneHeader>();
						quebecZone.FZ_Code = "QUBC";
						quebecZone.FZ_Description = "Quebec Services Tax Zone";
						quebecZone.FZ_IsActive = ZBool.True;
						quebecZone.FZ_ZoneType = ZoneTypeCodeDescriptionPair.Tax.Code;
						Factory.Save();
					}
				}
				return quebecZone;
			}
		}
		RefZoneHeader quebecZone;

		#endregion

		public void TestGetGSTRateForCompanyInEuropeanUnion()
		{
			RefCountry australia = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			australia.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUMEL";
			OrgCusCode customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CodeType = Country.GetConsumptionTaxRegistrationOrgCusCode(Constants.CountryCodes.Australia);
			customsCode.OK_CustomsRegNo = "1234";
			AccTaxRate taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_RN_NKCountry = taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			EUTaxIDDefaultingRuleCollection collection = new EUTaxIDDefaultingRuleCollection();
			EUTaxIDDefaultingRule rule = collection.AddNew();
			rule.Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry;
			rule.Destination = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin;
			rule.CostTaxRateForOrganisationRegisteredInMyCountry = taxRate1.PK;
			rule.CostTaxRateForOrganisationRegisteredInOtherEUCountry = taxRate2.PK;

			AccChargeCodeRegistry.Instance.EUTaxIDDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			AssertEquals(taxRate1.PK, chargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Cost, orgHeader, australia, australia).PK);
		}

		public void TestGetGSTRateWithFallbackToTaxOverrideGroup()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				AccTaxOverrideGroup group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
				AccChargeTaxOverride correctTaxOverride = group.TaxOverrides.AddNew();
				correctTaxOverride.AO_CostSellAll = "ALL";
				correctTaxOverride.AO_Direction = "OTH";
				correctTaxOverride.AO_JobType = "ALL";
				correctTaxOverride.AO_Origin = AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry;
				correctTaxOverride.AO_Destination = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
				correctTaxOverride.AO_IncoTerm = "ALL";
				correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				correctTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
				code.AC_AX_TaxOverrideGroup = group.PK;
				AccChargeTaxOverride incorrectTaxOverride = code.TaxOverrides.AddNew();
				incorrectTaxOverride.AO_CostSellAll = "ALL";
				incorrectTaxOverride.AO_Direction = "IMP";
				incorrectTaxOverride.AO_JobType = "ALL";
				incorrectTaxOverride.AO_Origin = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				incorrectTaxOverride.AO_Destination = countryAU.RN_Code;
				incorrectTaxOverride.AO_IncoTerm = "ALL";
				incorrectTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
				incorrectTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
				incorrectTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

				OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

				Factory.Save();
				ZGuid overrideInvTaxMsg;
				AccTaxRate rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryFR, countryAU, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
				AssertEquals(correctTaxOverride.AO_AT, rate.PK);
				AssertEquals(correctTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			}
		}

		public void TestGetGSTRateWithExporterExemption()
		{
			AccChargeCode code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			AccChargeTaxOverride taxOverride = code.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = AccChargeTaxOverride.ALL;
			taxOverride.AO_Destination = AccChargeTaxOverride.ALL;
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxOverride.AO_VATExemptOnExportCharges = ZBool.True;
			taxOverride.AO_HomeCountryOrZone = Constants.CountryCodes.UnitedKingdom;

			AccChargeTaxOverride taxOverride2 = code.TaxOverrides.AddNew();
			taxOverride2.AO_CostSellAll = "ALL";
			taxOverride2.AO_Direction = "ALL";
			taxOverride2.AO_JobType = "ALL";
			taxOverride2.AO_Origin = AccChargeTaxOverride.ALL;
			taxOverride2.AO_Destination = AccChargeTaxOverride.ALL;
			taxOverride2.AO_IncoTerm = "ALL";
			taxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride2.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxOverride2.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxOverride2.AO_VATExemptOnExportCharges = ZBool.False;
			taxOverride2.AO_HomeCountryOrZone = Constants.CountryCodes.UnitedStates;

			AccChargeTaxOverride taxOverride3 = code.TaxOverrides.AddNew();
			taxOverride3.AO_CostSellAll = "ALL";
			taxOverride3.AO_Direction = "ALL";
			taxOverride3.AO_JobType = "ALL";
			taxOverride3.AO_Origin = AccChargeTaxOverride.ALL;
			taxOverride3.AO_Destination = AccChargeTaxOverride.ALL;
			taxOverride3.AO_IncoTerm = "ALL";
			taxOverride3.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride3.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxOverride3.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxOverride3.AO_VATExemptOnExportCharges = ZBool.True;
			taxOverride3.AO_HomeCountryOrZone = Constants.CountryCodes.UnitedStates;

			AccTaxOverrideGroup group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AccChargeTaxOverride groupTaxOverride = group.TaxOverrides.AddNew();
			groupTaxOverride.AO_CostSellAll = "ALL";
			groupTaxOverride.AO_Direction = "ALL";
			groupTaxOverride.AO_JobType = "ALL";
			groupTaxOverride.AO_Origin = AccChargeTaxOverride.ALL;
			groupTaxOverride.AO_Destination = AccChargeTaxOverride.ALL;
			groupTaxOverride.AO_IncoTerm = "ALL";
			groupTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
			groupTaxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			groupTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			groupTaxOverride.AO_VATExemptOnExportCharges = ZBool.False;

			AccChargeTaxOverride groupTaxOverride2 = group.TaxOverrides.AddNew();
			groupTaxOverride2.AO_CostSellAll = "ALL";
			groupTaxOverride2.AO_Direction = "ALL";
			groupTaxOverride2.AO_JobType = "ALL";
			groupTaxOverride2.AO_Origin = AccChargeTaxOverride.ALL;
			groupTaxOverride2.AO_Destination = AccChargeTaxOverride.ALL;
			groupTaxOverride2.AO_IncoTerm = "ALL";
			groupTaxOverride2.AO_TaxRegCntryOrGroup = "ALL";
			groupTaxOverride2.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			groupTaxOverride2.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			groupTaxOverride2.AO_VATExemptOnExportCharges = ZBool.True;

			code.AC_AX_TaxOverrideGroup = group.PK;

			OrgHeader orgUK = Factory.NewWithValidTestData<OrgHeader>();
			orgUK.OH_RL_NKClosestPort = "GBLON";
			JobRequiredDocument jobDocument = orgUK.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(2);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Australia;

			OrgHeader orgUS = Factory.NewWithValidTestData<OrgHeader>();
			orgUS.OH_RL_NKClosestPort = "USLAX";
			jobDocument = orgUS.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(2);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Australia;

			OrgHeader orgUS2 = Factory.NewWithValidTestData<OrgHeader>();
			orgUS2.OH_RL_NKClosestPort = "USLAX";
			jobDocument = orgUS2.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(2);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Debtor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.UnitedStates;

			OrgHeader orgUS3 = Factory.NewWithValidTestData<OrgHeader>();
			orgUS3.OH_RL_NKClosestPort = "USLAX";
			jobDocument = orgUS3.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VetinaryCertificate;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(2);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Australia;

			OrgHeader orgAU = Factory.NewWithValidTestData<OrgHeader>();
			orgAU.OH_RL_NKClosestPort = "AUSYD";
			jobDocument = orgAU.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(-2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(2);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Australia;

			OrgHeader orgAU2 = Factory.NewWithValidTestData<OrgHeader>();
			orgAU2.OH_RL_NKClosestPort = "AUSYD";
			jobDocument = orgAU2.RequiredDocuments.AddNew();
			jobDocument.EQ_DocCategory = Constants.ReferenceTypes.ClientSupplierRelationship;
			jobDocument.EQ_DocType = Constants.RefDocTypes.VATExporterExemption;
			jobDocument.EQ_DateReceived = ZDateTimeOffset.Now.AddDays(2);
			jobDocument.EQ_ValidToDate = ZDateTime.Now.AddDays(4);
			jobDocument.EQ_DocUsage = JobRequiredDocument.DocUsage.Creditor;
			jobDocument.EQ_RN_NKRelatedCountry = Constants.CountryCodes.Australia;

			Factory.Save();

			ZGuid overrideInvTaxMsg;
			AccTaxRate rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUK,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(groupTaxOverride.AO_AT, rate.PK);
			AssertEquals(groupTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUK,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride.AO_AT, rate.PK);
			AssertEquals(taxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride3.AO_AT, rate.PK);
			AssertEquals(taxOverride3.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride2.AO_AT, rate.PK);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS2,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride2.AO_AT, rate.PK);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS2,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride2.AO_AT, rate.PK);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS3,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride2.AO_AT, rate.PK);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgUS3,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride2.AO_AT, rate.PK);
			AssertEquals(taxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgAU,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(groupTaxOverride.AO_AT, rate.PK);
			AssertEquals(groupTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgAU,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(groupTaxOverride2.AO_AT, rate.PK);
			AssertEquals(groupTaxOverride2.AO_A9_DefaultVATClass, overrideInvTaxMsg);

			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgAU2,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(groupTaxOverride.AO_AT, rate.PK);
			AssertEquals(groupTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
			rate = code.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Cost, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, orgAU2,
				GlbCompany.CurrentCompany.Country, GlbCompany.CurrentCompany.Country, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(groupTaxOverride.AO_AT, rate.PK);
			AssertEquals(groupTaxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
		}

		public void TestGetMappingForOrganisation()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());

			AssertEquals(ZString.Empty, chargeCode.GetMappingForOrganisation(orgHeader.PK));

			OrgPatternMatchOverride ov = orgHeader.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = chargeCode.AC_Code;
			Factory.Save();

			AssertEquals("ABC", chargeCode.GetMappingForOrganisation(orgHeader.PK));
		}

		public void TestUpdateEDICodeMappingWhenCodeChanges()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "OLD";
			OrgHeader orgHeader = Factory.LoadTop1<OrgHeader>(new ZQuery());
			OrgPatternMatchOverride ov = orgHeader.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov.OO_ForeignCode = "ABC";
			ov.OO_LocalCode = chargeCode.AC_Code;
			Factory.Save();

			AssertEquals("ABC", chargeCode.GetMappingForOrganisation(orgHeader.PK));

			chargeCode.AC_Code = "NEW";
			Factory.Save();
			AssertEquals(string.Empty, chargeCode.GetMappingForOrganisation(orgHeader.PK));
		}

		public void TestUpdateEDICodeMappingWhenCodeChangesWithRegistrySetting()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			AccChargeCode chargeCode1 = null;
			AccChargeCode chargeCode2 = null;
			OrgPatternMatchOverride ov1 = null;
			OrgPatternMatchOverride ov2 = null;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
				chargeCode1.AC_Code = "OLD";
				ov1 = orgHeader1.CreatePatternMatchOverrideForTest();
				ov1.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
				ov1.OO_ForeignCode = "TEST1";
				ov1.OO_LocalCode = chargeCode1.AC_Code;
			}

			chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "OLD";
			ov2 = orgHeader2.CreatePatternMatchOverrideForTest();
			ov2.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.ChargeCodes;
			ov2.OO_ForeignCode = "TEST2";
			ov2.OO_LocalCode = chargeCode2.AC_Code;

			Factory.Save();

			AssertEquals("OLD", ov1.OO_LocalCode);
			AssertEquals("TEST1", ov1.OO_ForeignCode);
			AssertEquals("OLD", ov2.OO_LocalCode);
			AssertEquals("TEST2", ov2.OO_ForeignCode);

			chargeCode1.AC_Code = "NEW";
			Factory.Save();

			Assert("default value is false", !OrganisationsDataRegistry.Instance.
				UpdateEDICodeMappingwhenChargeCodeIsRenamed.
				GetFallBackValueAtAllLevels(chargeCode1.AC_GC.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("default value is false", !OrganisationsDataRegistry.Instance.
				UpdateEDICodeMappingwhenChargeCodeIsRenamed.
				GetFallBackValueAtAllLevels(chargeCode2.AC_GC.ToGuid(), Guid.Empty, Guid.Empty));

			AssertEquals("OLD", ov1.OO_LocalCode);
			AssertEquals("TEST1", ov1.OO_ForeignCode);
			AssertEquals("OLD", ov2.OO_LocalCode);
			AssertEquals("TEST2", ov2.OO_ForeignCode);

			#region Set CurrentCompany UpdateEDICodeMappingwhenChargeCodeIsRenamed Is True

			OrganisationsDataRegistry.Instance.UpdateEDICodeMappingwhenChargeCodeIsRenamed.
						SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			chargeCode1.AC_Code = "NEW1";
			Factory.Save();

			Assert("default value is false", !OrganisationsDataRegistry.Instance.
				UpdateEDICodeMappingwhenChargeCodeIsRenamed.
				GetFallBackValueAtAllLevels(chargeCode1.AC_GC.ToGuid(), Guid.Empty, Guid.Empty));
			Assert("CurrentCompany's value is true", OrganisationsDataRegistry.Instance.
				UpdateEDICodeMappingwhenChargeCodeIsRenamed.
				GetFallBackValueAtAllLevels(chargeCode2.AC_GC.ToGuid(), Guid.Empty, Guid.Empty));

			AssertEquals("OLD", ov1.OO_LocalCode);
			AssertEquals("TEST1", ov1.OO_ForeignCode);
			AssertEquals("OLD", ov2.OO_LocalCode);
			AssertEquals("TEST2", ov2.OO_ForeignCode);

			chargeCode2.AC_Code = "NEW2";
			Factory.Save();

			AssertEquals("NEW2", ov1.OO_LocalCode);
			AssertEquals("TEST1", ov1.OO_ForeignCode);
			AssertEquals("NEW2", ov2.OO_LocalCode);
			AssertEquals("TEST2", ov2.OO_ForeignCode);

			#endregion

			chargeCode1.AC_Code = "NEW2";
			Factory.Save();

			#region Set System level UpdateEDICodeMappingwhenChargeCodeIsRenamed Is True

			OrganisationsDataRegistry.Instance.UpdateEDICodeMappingwhenChargeCodeIsRenamed.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			chargeCode1.AC_Code = "NEW3";
			Factory.Save();

			Assert("chargeCode1's company level is not set, but system level value is true. It will fall back to system level.",
				OrganisationsDataRegistry.Instance.
				UpdateEDICodeMappingwhenChargeCodeIsRenamed.
				GetFallBackValueAtAllLevels(chargeCode1.AC_GC.ToGuid(), Guid.Empty, Guid.Empty));
			AssertEquals("NEW3", ov1.OO_LocalCode);
			AssertEquals("TEST1", ov1.OO_ForeignCode);
			AssertEquals("NEW3", ov2.OO_LocalCode);
			AssertEquals("TEST2", ov2.OO_ForeignCode);

			#endregion

		}

		public void TestDeleteClientSequenceSetupsOnSetInactive()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCodeToMakeInactive = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCodeActive = Factory.NewWithValidTestData<AccChargeCode>();
			AccClientInvoiceOrder org1InvoiceOrderSetupToDelete = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org2InvoiceOrderSetupToDelete = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org1InvoiceOrderSetup = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org2InvoiceOrderSetup = Factory.NewWithValidTestData<AccClientInvoiceOrder>();

			org1InvoiceOrderSetupToDelete.AI_OH_Client = org1.PK;
			org1InvoiceOrderSetupToDelete.AI_AC = chargeCodeToMakeInactive.PK;
			org1InvoiceOrderSetup.AI_OH_Client = org1.PK;
			org1InvoiceOrderSetup.AI_AC = chargeCodeActive.PK;

			org2InvoiceOrderSetupToDelete.AI_OH_Client = org2.PK;
			org2InvoiceOrderSetupToDelete.AI_AC = chargeCodeToMakeInactive.PK;
			org2InvoiceOrderSetup.AI_OH_Client = org2.PK;
			org2InvoiceOrderSetup.AI_AC = chargeCodeActive.PK;

			Factory.Save();

			AccClientInvoiceOrderCollection org1InvoiceOrderSetups = new AccClientInvoiceOrderCollection(Factory, org1);
			AccClientInvoiceOrderCollection org2InvoiceOrderSetups = new AccClientInvoiceOrderCollection(Factory, org2);

			AssertEquals("Should be two Invoice Order Setups inserted", 2, org1InvoiceOrderSetups.Count);
			AssertEquals("Should be two Invoice Order Setups inserted", 2, org2InvoiceOrderSetups.Count);

			chargeCodeToMakeInactive.AC_IsActive = false;

			Factory.Save();

			Assert("Invoice Order Setup should not exist", !org1InvoiceOrderSetups.Contains(org1InvoiceOrderSetupToDelete));
			Assert("Invoice Order Setup should exist", org1InvoiceOrderSetups.Contains(org1InvoiceOrderSetup));
			Assert("Invoice Order Setup should not exist", !org2InvoiceOrderSetups.Contains(org2InvoiceOrderSetupToDelete));
			Assert("Invoice Order Setup should exist", org2InvoiceOrderSetups.Contains(org2InvoiceOrderSetup));
		}

		public void TestDeleteClientSequenceSetupsOnDelete()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			AccChargeCode chargeCodeToMakeInactive = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCodeActive = Factory.NewWithValidTestData<AccChargeCode>();
			AccClientInvoiceOrder org1InvoiceOrderSetupToDelete = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org2InvoiceOrderSetupToDelete = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org1InvoiceOrderSetup = Factory.NewWithValidTestData<AccClientInvoiceOrder>();
			AccClientInvoiceOrder org2InvoiceOrderSetup = Factory.NewWithValidTestData<AccClientInvoiceOrder>();

			org1InvoiceOrderSetupToDelete.AI_OH_Client = org1.PK;
			org1InvoiceOrderSetupToDelete.AI_AC = chargeCodeToMakeInactive.PK;
			org1InvoiceOrderSetup.AI_OH_Client = org1.PK;
			org1InvoiceOrderSetup.AI_AC = chargeCodeActive.PK;

			org2InvoiceOrderSetupToDelete.AI_OH_Client = org2.PK;
			org2InvoiceOrderSetupToDelete.AI_AC = chargeCodeToMakeInactive.PK;
			org2InvoiceOrderSetup.AI_OH_Client = org2.PK;
			org2InvoiceOrderSetup.AI_AC = chargeCodeActive.PK;

			Factory.Save();

			AccClientInvoiceOrderCollection org1InvoiceOrderSetups = new AccClientInvoiceOrderCollection(Factory, org1);
			AccClientInvoiceOrderCollection org2InvoiceOrderSetups = new AccClientInvoiceOrderCollection(Factory, org2);

			AssertEquals("Should be two Invoice Order Setups inserted", 2, org1InvoiceOrderSetups.Count);
			AssertEquals("Should be two Invoice Order Setups inserted", 2, org2InvoiceOrderSetups.Count);

			chargeCodeToMakeInactive.Delete();

			Factory.Save();

			Assert("Invoice Order Setup should not exist", !org1InvoiceOrderSetups.Contains(org1InvoiceOrderSetupToDelete));
			Assert("Invoice Order Setup should exist", org1InvoiceOrderSetups.Contains(org1InvoiceOrderSetup));
			Assert("Invoice Order Setup should not exist", !org2InvoiceOrderSetups.Contains(org2InvoiceOrderSetupToDelete));
			Assert("Invoice Order Setup should exist", org2InvoiceOrderSetups.Contains(org2InvoiceOrderSetup));
		}

		public void TestGetGLPostingAccounts()
		{
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_AccrualAccount = glHeader1.PK;
			chargeCode.AC_AG_CostClearingAccount = glHeader1.PK;
			chargeCode.AC_AG_RevenueClearingAccount = glHeader1.PK;

			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			AssertEquals("Should return data from charge code regardless of the department because there are no overrides", glHeader1.PK, chargeCode.GetGLPostingAccounts(department1, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").AccrualAccount);
			AssertEquals("Should return data from charge code regardless of the department because there are no overrides", glHeader1.PK, chargeCode.GetGLPostingAccounts(department1, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostClearingAccount);
			AssertEquals("Should return data from charge code regardless of the department because there are no overrides", glHeader1.PK, chargeCode.GetGLPostingAccounts(department1, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").RevenueClearingAccount);
			AssertEquals("Should return data from charge code regardless of the department because there are no overrides", glHeader1.PK, chargeCode.GetGLPostingAccounts(department2, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").AccrualAccount);

			var override1 = chargeCode.GLPostingOverrides.AddNew();
			override1.Y1_GE = department1.PK;
			override1.Y1_AG_ACR = glHeader2.PK;

			Factory.Save();

			AssertEquals("Should still return data from charge code because there is no override for this department", glHeader1.PK, chargeCode.GetGLPostingAccounts(department2, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").AccrualAccount);

			var override2 = chargeCode.GLPostingOverrides.AddNew();
			override2.Y1_GE = department2.PK;
			override2.Y1_AG_ACR = glHeader3.PK;
			override2.Y1_AG_CST_Clearing = glHeader3.PK;
			override2.Y1_AG_REV_Clearing = glHeader3.PK;

			Factory.Save();

			AssertEquals("Should still return data from the override", glHeader3.PK, chargeCode.GetGLPostingAccounts(department2, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").AccrualAccount);
			AssertEquals("Should still return data from the override", glHeader3.PK, chargeCode.GetGLPostingAccounts(department2, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostClearingAccount);
			AssertEquals("Should still return data from the override", glHeader3.PK, chargeCode.GetGLPostingAccounts(department2, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").RevenueClearingAccount);
		}

		public void TestGetGLPostingAccountsWithOrgHeader()
		{
			var department1 = Factory.NewWithValidTestData<GlbDepartment>();
			var department2 = Factory.NewWithValidTestData<GlbDepartment>();
			var department3 = Factory.NewWithValidTestData<GlbDepartment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CompanyData.OB_IsDebtor = true;

			var chargeCode = Factory.New<AccChargeCode>();
			var costAccount = Factory.NewWithValidTestData<AccGLHeader>();
			chargeCode.AC_AG_CostAccount = costAccount.PK;

			var glPostingOverride1 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			glPostingOverride1.Y1_GE = department1.PK;
			glPostingOverride1.Y1_AG_CST = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.ThirdParty;
			glPostingOverride2.Y1_GE = department2.PK;
			glPostingOverride2.Y1_AG_CST = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride3 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride3.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride3.Y1_AG_CST = Factory.NewWithValidTestData<AccGLHeader>().PK;

			var glPostingOverride4 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride4.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride4.Y1_GE = department2.PK;
			glPostingOverride4.Y1_AG_CST = Factory.NewWithValidTestData<AccGLHeader>().PK;

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";

			Factory.Save();

			AssertEquals(glPostingOverride4.Y1_AG_CST, chargeCode.GetGLPostingAccounts(department2, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			AssertEquals(glPostingOverride3.Y1_AG_CST, chargeCode.GetGLPostingAccounts(department1, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "WHO";
			AssertEquals(glPostingOverride3.Y1_AG_CST, chargeCode.GetGLPostingAccounts(null, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			AssertEquals(glPostingOverride2.Y1_AG_CST, chargeCode.GetGLPostingAccounts(department2, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			AssertEquals(glPostingOverride1.Y1_AG_CST, chargeCode.GetGLPostingAccounts(department1, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			AssertEquals(costAccount.PK, chargeCode.GetGLPostingAccounts(department3, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);

			orgHeader.CompanyData.OB_ARConsolidatedAccountingCategory = "UNR";
			AssertEquals(costAccount.PK, chargeCode.GetGLPostingAccounts(null, orgHeader, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL").CostAccount);
		}

		public void Test_GetGLPostingAccounts_ReturnsDefaultAccounts_WhenOverrideGLAccountsAreNull()
		{
			var glHeader1 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader2 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader4 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader5 = Factory.NewWithValidTestData<AccGLHeader>();
			var glHeader6 = Factory.NewWithValidTestData<AccGLHeader>();
			var glOverrideAccount = Factory.NewWithValidTestData<AccGLHeader>();
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_AG_AccrualAccount = glHeader1.PK;
			chargeCode.AC_AG_CostClearingAccount = glHeader2.PK;
			chargeCode.AC_AG_RevenueClearingAccount = glHeader3.PK;
			chargeCode.AC_AG_WIPAccount = glHeader4.PK;
			chargeCode.AC_AG_RevenueAccount = glHeader5.PK;
			chargeCode.AC_AG_CostAccount = glHeader6.PK;

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var glpostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glpostingOverride.Y1_GE = department.PK;
			glpostingOverride.Y1_AG_ACR = glOverrideAccount.PK;

			Factory.Save();

			var glPostingAccounts = chargeCode.GetGLPostingAccounts(department, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL");

			AssertEquals("Should return AccrualAccount from the GL Posting Override", glOverrideAccount.PK, glPostingAccounts.AccrualAccount);
			AssertEquals("Should return CostClearingAccount from the Charge Code", glHeader2.PK, glPostingAccounts.CostClearingAccount);
			AssertEquals("Should return RevenueClearingAccount from the Charge Code", glHeader3.PK, glPostingAccounts.RevenueClearingAccount);
			AssertEquals("Should return WIPAccount from the Charge Code", glHeader4.PK, glPostingAccounts.WIPAccount);
			AssertEquals("Should return RevenueAccount from the Charge Code", glHeader5.PK, glPostingAccounts.RevenueAccount);
			AssertEquals("Should return CostAccount from the Charge Code", glHeader6.PK, glPostingAccounts.CostAccount);

			glpostingOverride.Y1_AG_ACR = ZGuid.Empty;
			glPostingAccounts = chargeCode.GetGLPostingAccounts(department, null, "ALL", "ALL", "ALL", "ALL", "ALL", "ALL");

			AssertEquals("Should return AccrualAccount from the Charge Code", glHeader1.PK, glPostingAccounts.AccrualAccount);
		}

		#region Charge types

		public void TestWhenMargin()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Margin;

			AssertEquals("IsMargin", true, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenDisbursement()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Disbursement;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", true, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenRevenue()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Revenue;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", true, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenNonAccrual()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.NonAccrual;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", true, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenOverhead()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Overhead;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", true, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenComment()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.Comment;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", true, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenNone()
		{
			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", false, TestChargeCode.IsManualJobAccrual);
		}

		public void TestWhenManualJobAccrual()
		{
			TestChargeCode.AC_ChargeType = Constants.ChargeType.ManualJobAccrual;

			AssertEquals("IsMargin", false, TestChargeCode.IsMargin);
			AssertEquals("IsDisbursement", false, TestChargeCode.IsDisbursement);
			AssertEquals("IsRevenue", false, TestChargeCode.IsRevenue);
			AssertEquals("IsNonAccrual", false, TestChargeCode.IsNonAccrual);
			AssertEquals("IsOverhead", false, TestChargeCode.IsOverhead);
			AssertEquals("IsComment", false, TestChargeCode.IsComment);
			AssertEquals("IsManualJobAccrual", true, TestChargeCode.IsManualJobAccrual);
		}

		public void TestHighestChargeType()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals("Highest charge type is Main charge code type", Core.Constants.ChargeType.Revenue, TestChargeCode.HighestChargeType);

			AccChargeTypeOverride typeOverride = TestChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertEquals("Highest charge type is type override type", Core.Constants.ChargeType.Disbursement, TestChargeCode.HighestChargeType);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForRevenue()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(!TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(!TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForMargin()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(!TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(!TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForDisbursement()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(!TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(!TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForOverHead()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForNonAccrual()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(!TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(!TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForComment()
		{
			Assert("Precondition: GC_IsGSTRegistered = true", TestChargeCode.Company.GC_IsGSTRegistered);
			Assert("Precondition: GC_IsWHTRegistered = false", !TestChargeCode.Company.GC_IsWHTRegistered);
			TestChargeCode.Company.GC_IsWHTRegistered = true;

			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AT_GSTRate = ZGuid.NewZGuid();
			TestChargeCode.AC_AW_WithholdingTaxRate = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AT_GSTRateInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AW_WithholdingTaxRateInfo.ReadOnly);

			Assert(TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(TestChargeCode.AC_AG_CostAccount.IsEmpty);

			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;
			TestChargeCode.AC_AW_WithholdingTaxRate = ZGuid.Empty;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();
			Assert(TestChargeCode.AC_AT_GSTRateInfo.ReadOnly);
			Assert(TestChargeCode.AC_AW_WithholdingTaxRateInfo.ReadOnly);
		}

		public void TestUpdateChargeTypeDependentReadOnlyInfoForManualJobAccrual()
		{
			TestChargeCode.AC_AG_RevenueAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_WIPAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_AccrualAccount = ZGuid.NewZGuid();
			TestChargeCode.AC_AG_CostAccount = ZGuid.NewZGuid();

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			TestChargeCode.UpdateChargeTypeDependentReadOnlyInfo();

			Assert(!TestChargeCode.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!TestChargeCode.AC_AG_CostAccountInfo.ReadOnly);

			Assert(!TestChargeCode.AC_AG_RevenueAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_WIPAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_AccrualAccount.IsEmpty);
			Assert(!TestChargeCode.AC_AG_CostAccount.IsEmpty);
		}

		public void TestCommentChargeDoNotValidateGST()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;

			Assert(!TestChargeCode.AC_AT_GSTRateInfo.HasErrors());

			TestChargeCode.AC_AT_GSTRate = ZGuid.NewZGuid();

			Assert(TestChargeCode.AC_AT_GSTRateInfo.HasErrors());
			TestChargeCode.AC_AT_GSTRate = ZGuid.Empty;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode.Validation.ValidateAC_AT_GSTRate();

			Assert(TestChargeCode.AC_AT_GSTRateInfo.HasErrors());
		}

		public void TestCommentChargeAllowsForAllGroup()
		{
			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.Freight;

			Assert(!TestChargeCode.AC_ChargeGroupInfo.HasErrors());

			TestChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;

			Assert(!TestChargeCode.AC_ChargeGroupInfo.HasErrors());

			TestChargeCode.AC_ChargeGroup = "@#$";

			Assert(TestChargeCode.AC_ChargeGroupInfo.HasErrors());
		}

		public void TestShowOnQuoteReadOnly()
		{
			TestChargeCode.AC_ShowOnQuotation = ZBool.True;
			TestChargeCode.AC_SuppressOnQuoteIfZero = ZBool.True;

			TestChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			Assert(TestChargeCode.AC_ShowOnQuotationInfo.ReadOnly);
			Assert(TestChargeCode.AC_SuppressOnQuoteIfZeroInfo.ReadOnly);
			AssertEquals(ZBool.False, TestChargeCode.AC_ShowOnQuotation);
			AssertEquals(ZBool.False, TestChargeCode.AC_SuppressOnQuoteIfZero);
		}

		#endregion

		#region ICancellable

		public void TestPreventDelete()
		{
			AssertEquals("PreventDelete", false, PreventDeleteAttribute.IsTrue(TestChargeCode.GetType()));
		}

		#endregion

		#region Logging Tests

		#region Implementation

		AccChargeCode GetSavedTestChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			return chargeCode;
		}

		#endregion

		#region Government Charge Code Overrides

		public void TestGovtChargeCodeOverride()
		{
			var govtChargeCodeOverride1 = Factory.NewWithValidTestData<AccChargeGovtChargeCodeOverride>();
			govtChargeCodeOverride1.ACG_AC = TestChargeCode.PK;
			var govtChargeCodeOverride2 = Factory.NewWithValidTestData<AccChargeGovtChargeCodeOverride>();
			govtChargeCodeOverride2.ACG_AC = TestChargeCode.PK;

			Factory.NewWithValidTestData<AccChargeGovtChargeCodeOverride>();

			AssertEquals(2, TestChargeCode.GovtChargeCodeOverrides.Count);
			AssertEquals(TestChargeCode.PK, govtChargeCodeOverride2.ACG_AC);
		}

		#endregion

		#region Charge Type Override

		public void TestChargeTypeOverridesReadonly()
		{
			var chargeCode = GetSavedTestChargeCode();

			chargeCode.ChargeTypeOverrides.AddNew();
			chargeCode.ChargeTypeOverrides[0].AN_JobDirection = "EXP";
			chargeCode.ChargeTypeOverrides[0].AN_JobType = "SHP";
			chargeCode.ChargeTypeOverrides[0].AN_ChargeType = "REV";
			chargeCode.ChargeTypeOverrides[0].AN_MarginPercentage = 0.00;
			chargeCode.ChargeTypeOverrides[0].AN_InvoiceType = "FIN";

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Default", false, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(false, chargeCode.ChargeTypeOverrides[0].ReadOnly);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			AssertEquals("Overhead charge", true, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(true, chargeCode.ChargeTypeOverrides[0].ReadOnly);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			AssertEquals("Comment charge", true, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(true, chargeCode.ChargeTypeOverrides[0].ReadOnly);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			AssertEquals("NonAccrual charge", true, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(true, chargeCode.ChargeTypeOverrides[0].ReadOnly);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Pre-condition", false, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(false, chargeCode.ChargeTypeOverrides[0].ReadOnly);
			chargeCode.Delete();
			AssertEquals("Deleted charge", true, chargeCode.ChargeTypeOverrides.ReadOnly);
			AssertEquals(0, chargeCode.ChargeTypeOverrides.Count);
		}

		#endregion

		#region Tax Overrides

		public void TestNoAuditLogsWithAttibute_TaxOverrides()
		{
			var chargeCode = GetSavedTestChargeCode();
			var group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var postingOverride = group.TaxOverrides.AddNew();

			postingOverride.AO_CostSellAll = "ALL";
			postingOverride.AO_Direction = "OTH";
			postingOverride.AO_JobType = "ALL";
			postingOverride.AO_Origin = AccChargeTaxOverride.EuropeanUnionExcludingLoginCountry;
			postingOverride.AO_Destination = AccChargeTaxOverride.AllCountriesExceptLoginCountry;
			postingOverride.AO_IncoTerm = "ALL";
			postingOverride.AO_TaxRegCntryOrGroup = "ALL";
			postingOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			group.AX_Code = "TESTGROUP";
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.AC_AX_TaxOverrideGroup = group.PK;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.AC_AX_TaxOverrideGroup = ZGuid.Empty;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#endregion

		public void TestNoAuditLogsWithAttibute_ChargeCode()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";
			Factory.Save();

			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, chargeCode.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				chargeCode.AC_ChargeSubGroup = "FUS";
				chargeCode.AC_Code = "MRG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestGlobalChargeCodeAddsLogToLocalChargeCode()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var normalChargeCodeLinkedADDLogs = normalChargeCodeLinked.Logs.Find(x => x.SL_SE_NKEvent == Events.AddedARecordToTheSystem.Code).ToList();
			AssertEquals("There should be no event for linked local charge code", 0, normalChargeCodeLinkedADDLogs.Count);

			var normalChargeCodeLinkedLogs = normalChargeCodeLinked.Logs.GetAllLogs();

			globalChargeCode.AC_Desc = "Hello";
			Factory.Save();

			normalChargeCodeLinkedLogs = normalChargeCodeLinked.Logs.GetAllLogs();
			AssertEquals("There should be no event for linked local charge code", 0, normalChargeCodeLinkedADDLogs.Count);
		}

		public void TestCopyToCompany()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var taxRate = CreateTaxRate("TAX2");
				taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxRate.AT_Type = AccTaxRate.Types.ServiceTax;
				Factory.Save();

				var accounting = ObjectFactory.Get<IAccounting>();
				accounting.SetMainGSTTaxIDConfiguration(GlbCompany.CurrentCompany.PK.ToGuid(), taxRate.PK.ToGuid());

				var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				globalChargeCode.AC_GC = ZGuid.Empty;
				globalChargeCode.AC_Code = "CC3";
				globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
				globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
				globalChargeCode.AC_Desc = "global Desc";

				AssertNoExceptionThrown(() => globalChargeCode.CopyToCompany(GlbCompany.CurrentCompany));
			}
		}

		public void TestChangeGlobalChargeCodeToMarginShouldAlsoChangeLocalChargeCodeValue()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Factory.Save();
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_MarginPercentage = 90M;
			Factory.Save();
			AssertEquals(90M, globalChargeCode.childChargeCodes[0].AC_MarginPercentage);

			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Factory.Save();
			globalChargeCode.childChargeCodes[0].AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Factory.Save();
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_MarginPercentage = 90M;
			Factory.Save();
			AssertEquals(Core.Constants.ChargeType.Revenue, globalChargeCode.childChargeCodes[0].AC_ChargeType);
			AssertEquals(0M, globalChargeCode.childChargeCodes[0].AC_MarginPercentage);
		}

		public void TestEnterpriseBusinessObjectIsAudited()
		{
			var columns = new SchemaColumn[]
			{
				AccChargeCodeSchema.PK,
				AccChargeCodeSchema.AC_Desc
			};
			AuditLogsHelperForTesting.AssertColumnsExistsInAuditDb(Factory, AccChargeCodeSchema.PK.TableSchema.SqlSchemaName, AccChargeCodeSchema.PK.TableName, columns);
		}

		public void TestGlobalChargeCodeCreationProducesExpectedEdtLog()
		{
			var uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);
			Factory.Save();

			var localChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			localChargeCode.AC_GC = uSCompany.PK;
			localChargeCode.AC_Code = "CC1";

			Factory.Save();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();
			AssertEquals(false, globalChargeCode.Logs.GetAllLogs().Any());
		}

		#endregion

		#region TestMarginPercentageCalculation

		public void TestMarginPercentageCalculation()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Margin Percentage (Margin)", 100M, TestChargeCode2.AC_MarginPercentage);
			Assert("Margin Percentage should not be readonly (Margin)", !TestChargeCode2.AC_MarginPercentageInfo.ReadOnly);

			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertEquals("Margin Percentage (Revenue)", 0M, TestChargeCode2.AC_MarginPercentage);
			Assert("Margin Percentage should be readonly (Revenue)", TestChargeCode2.AC_MarginPercentageInfo.ReadOnly);

			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertEquals("Margin Percentage (Disbursement)", 100M, TestChargeCode2.AC_MarginPercentage);
			Assert("Margin Percentage should be readonly (Disbursement)", TestChargeCode2.AC_MarginPercentageInfo.ReadOnly);
		}

		public void TestMarginPercentageNotResetOnValidatingChargeType()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Margin Percentage should be 100%", 100M, TestChargeCode2.AC_MarginPercentage);
			TestChargeCode2.AC_MarginPercentage = 90.00M;
			AssertEquals("Margin Percentage should be 90%", 90M, TestChargeCode2.AC_MarginPercentage);
			TestChargeCode2.Validation.ValidateAC_ChargeType();
			AssertEquals("Margin Percentage should still be 90%", 90M, TestChargeCode2.AC_MarginPercentage);
		}

		#endregion

		#region TestGLAccountReadOnly

		public void TestGLAccountReadOnly()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			Assert("revenue account should not be read only", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should be read only", TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			Assert("revenue account should not be read only", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should be read only", TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			Assert("revenue account should be read only", TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should be read only", TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);
		}

		#endregion

		public void TestCMTChargeCodeIsNotReadOnly()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			TestChargeCode2.OnLoaded();
			Assert("Comment Charge code should not be read only", !TestChargeCode2.ReadOnly);
		}

		public void TestREVChargeCodeIsReadOnly()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			TestChargeCode2.OnLoaded();
			Assert("WIP Account code should not be read only", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
		}

		#region Test REV ChargeType

		public void TestChangesFromREVChargeCode()
		{
			AccGLHeader glHeader = Factory.NewWithValidTestData<AccGLHeader>();

			AccTransactionLines testTransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			testTransactionLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testTransactionLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testTransactionLine.AL_AC = TestChargeCode2.PK;
			testTransactionLine.AL_AG = glHeader.PK;
			ChargeTypeAssertionsForREVChargeCodeWithMatchingTransactionLines();

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testTransactionLine.AL_AC = chargeCode.PK;
			//TestTransactionLine.AL_AC = ZGuid.NewZGuid();
			ChargeTypeAssertionsForChargeCodeWithNoMatchingTransactionLines(ZArchitecture.Core.TransactionLineTypes.Revenue);
		}

		void ChargeTypeAssertionsForREVChargeCodeWithMatchingTransactionLines()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from REV", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is MRG", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is MRG", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is MRG", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is MRG", !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from REV", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from REV", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Not allowed to change to OVR from REV since there is a transaction line referencing this charge code", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is OVR
			//Assert("Revenue account should not be read only since current charge type is REV", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should be not read only since current charge type is REV", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should be read only since current charge type is REV", TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should be read only since current charge type is REV", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(Core.Constants.ChargeType.Revenue);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Not allowed to change to NON from REV since there is a transaction line referencing this charge code", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is NON
			//Assert("Revenue account should not be read only since current charge type is REV", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is REV", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should be read only since current charge type is REV", TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should be read only since current charge type is REV", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);
		}

		#endregion

		#region Test MRG and MJA ChargeType

		[SuspendCriticalValidation]
		public void TestChangesFromMRGChargeCode()
		{
			ChangesFromMRGOrMJAChargeCode(Core.Constants.ChargeType.Margin);
		}

		[SuspendCriticalValidation]
		public void TestChangesFromMJAChargeCode()
		{
			ChangesFromMRGOrMJAChargeCode(Core.Constants.ChargeType.ManualJobAccrual);
		}

		public void ChangesFromMRGOrMJAChargeCode(string chargeCodeType)
		{
			Factory.Save();
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			header.AH_Ledger = "AR";
			header.AH_TransactionType = "INV";
			AccTransactionLines testTransactionLine = Factory.New(typeof(AccTransactionLines)) as AccTransactionLines;
			testTransactionLine.AL_AC = TestChargeCode2.PK;

			JobHeader testJobHeader = Factory.NewJobForTesting<JobHeader>();
			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_JH = testJobHeader.PK;
			jobCharge.JR_AL_APLine = testTransactionLine.PK;
			jobCharge.JR_AL_ARLine = ZGuid.Empty;
			testJobHeader.JH_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			testJobHeader.JH_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			testJobHeader.JH_JobNum = "Job1";

			testTransactionLine.AL_JH = testJobHeader.PK;
			testTransactionLine.AL_AH = header.PK;
			testTransactionLine.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			testTransactionLine.AL_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			testTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			ChargeTypeAssertionsForMRGAndMJAChargeCode_CST_ACR_WIPTransactionLines(chargeCodeType);

			testTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Accrual;
			ChargeTypeAssertionsForMRGAndMJAChargeCode_CST_ACR_WIPTransactionLines(chargeCodeType);

			jobCharge.JR_AL_ARLine = testTransactionLine.PK;
			jobCharge.JR_AL_APLine = ZGuid.Empty;
			testTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.WIP;
			ChargeTypeAssertionsForMRGAndMJAChargeCode_CST_ACR_WIPTransactionLines(chargeCodeType);

			testTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			ChargeTypeAssertionsForMRGAndMJAChargeCode_REVTransactionLines(chargeCodeType);

			jobCharge.JR_AL_ARLine = ZGuid.Empty;
			jobCharge.JR_AL_APLine = testTransactionLine.PK;
			testTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			testTransactionLine.AL_JH = ZGuid.Empty;
			ChargeTypeAssertionsForMRGAndMJAChargeCode_JobHeaderIsNull(chargeCodeType);

			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			testTransactionLine.AL_AC = chargeCode3.PK;
			ChargeTypeAssertionsForChargeCodeWithNoMatchingTransactionLines(chargeCodeType);
		}

		void ChargeTypeAssertionsForMRGAndMJAChargeCode_JobHeaderIsNull(string chargeCodeType)
		{
			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from " + chargeCodeType, TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
			AssertEquals("Previous Charge type should be " + chargeCodeType, chargeCodeType, TestChargeCode2.AC_ChargeTypeInfo.OriginalValue.ToString());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Not allowed to change to REV from " + chargeCodeType + " since there is a posted cost transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is REV
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Allowed to change to OVR from " + chargeCodeType + " since posted cost transaction line does not have a job", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should be read only since current charge type is OVR", TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should be read only since current charge type is OVR", TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since OVR", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only since current charge type is OVR", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from " + chargeCodeType + " since posted cost transaction line does not have a job", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is NON", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should be read only since current charge type is NON", TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is NON", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only since current charge type is NON", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);
		}

		void ChargeTypeAssertionsForMRGAndMJAChargeCode_CST_ACR_WIPTransactionLines(string chargeCodeType)
		{
			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from " + chargeCodeType, TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Not allowed to change to OVR from " + chargeCodeType + " because there is a posted CST/ACR/WIP transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is OVR
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Not allowed to change to NON from " + chargeCodeType + " because there is a posted CST/ACR/WIP transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is NON
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Not allowed to change to REV from " + chargeCodeType + " because there is a posted CST/ACR/WIP transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is REV
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);
		}

		void ChargeTypeAssertionsForMRGAndMJAChargeCode_REVTransactionLines(string chargeCodeType)
		{
			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from " + chargeCodeType, TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is DSB", !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Not allowed to change to OVR from " + chargeCodeType + " since there is a posted revenue transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is OVR
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = chargeCodeType;
			Assert("Allowed to change to " + chargeCodeType + " from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is " + chargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should not be read only since current charge type is " + chargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should not be read only since current charge type is " + chargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should not be read only since current charge type is " + chargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Not allowed to change to NON from " + chargeCodeType + " since there is a posted revenue transaction line", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			// NOTE: Actually, current charge type is NON
			//Assert("Revenue account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			//Assert("WIP account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			//Assert("Cost account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			//Assert("Accrual account should not be read only since current charge type is " + ChargeCodeType, !TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Allowed to change to REV from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			Assert("Revenue account should not be read only since current charge type is REV", !TestChargeCode2.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert("WIP account should be not read only since current charge type is REV", !TestChargeCode2.AC_AG_WIPAccountInfo.ReadOnly);
			Assert("Cost account should be read only since current charge type is REV", TestChargeCode2.AC_AG_CostAccountInfo.ReadOnly);
			Assert("Accrual account should be read only since current charge type is REV", TestChargeCode2.AC_AG_AccrualAccountInfo.ReadOnly);
		}

		#endregion

		#region Test NON ChargeType

		[SuspendCriticalValidation]
		public void TestChangesFromNONChargeCode()
		{
			AccTransactionHeader header = Factory.NewWithValidTestData<AccTransactionHeader>();
			AccTransactionLines rEVTransactionLine = Factory.New(typeof(AccTransactionLines)) as AccTransactionLines;
			rEVTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Revenue;
			rEVTransactionLine.AL_AH = header.PK;
			rEVTransactionLine.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			rEVTransactionLine.AL_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;
			AccTransactionLines cSTTransactionLine = Factory.New(typeof(AccTransactionLines)) as AccTransactionLines;
			cSTTransactionLine.AL_LineType = ZArchitecture.Core.TransactionLineTypes.Cost;
			cSTTransactionLine.AL_AH = header.PK;
			cSTTransactionLine.AL_GE = Factory.LoadTop1(typeof(GlbDepartment), new ZQuery()).PK;
			cSTTransactionLine.AL_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			rEVTransactionLine.AL_AC = TestChargeCode2.PK;
			cSTTransactionLine.AL_AC = TestChargeCode2.PK;
			ChargeTypeAssertionsForNONChargeCodeReferencedByCST_REVTransactionLines();

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode3 = Factory.NewWithValidTestData<AccChargeCode>();
			AccChargeCode chargeCode4 = Factory.NewWithValidTestData<AccChargeCode>();

			rEVTransactionLine.AL_AC = chargeCode.PK;
			ChargeTypeAssertionsForNONChargeCodeReferencedByCSTTransactionLinesOnly();

			rEVTransactionLine.AL_AC = TestChargeCode2.PK;
			cSTTransactionLine.AL_AC = chargeCode3.PK;
			ChargeTypeAssertionsForNONChargeCodeReferencedByREVTransactionLinesOnly();

			rEVTransactionLine.AL_AC = chargeCode2.PK;
			cSTTransactionLine.AL_AC = chargeCode4.PK;
			ChargeTypeAssertionsForChargeCodeWithNoMatchingTransactionLines(Core.Constants.ChargeType.NonAccrual);
		}

		void ChargeTypeAssertionsForNONChargeCodeReferencedByCST_REVTransactionLines()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from NON since there are posted CST/REV transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Not allowed to change to OVR from NON since there are posted CST/REV transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Not allowed to change to REV from NON since there are posted CST/REV transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
		}

		void ChargeTypeAssertionsForNONChargeCodeReferencedByCSTTransactionLinesOnly()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from NON since there are posted CST transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Allowed to change to OVR from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Not allowed to change to REV from NON since there are posted CST transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
		}

		void ChargeTypeAssertionsForNONChargeCodeReferencedByREVTransactionLinesOnly()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from NON since there are posted REV transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Not allowed to change to OVR from NON since there are posted REV transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.NonAccrual);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Allowed to change to REV from NON", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
		}

		#endregion

		#region Test OVR ChargeType

		public void TestChangesFromOVRChargeCode()
		{
			var glHeader = Factory.NewWithValidTestData<AccGLHeader>();
			var testTransactionLine = Factory.NewWithValidTestData<AccTransactionLines>();
			testTransactionLine.AL_AC = TestChargeCode2.PK;
			testTransactionLine.AL_GE = GlbDepartment.CurrentDepartment.PK;
			testTransactionLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testTransactionLine.AL_AG = glHeader.PK;

			ChargeTypeAssertionsForOVRChargeCodeWithMatchingTransactionLines();

			testTransactionLine.AL_AC = ZGuid.Empty;
			ChargeTypeAssertionsForChargeCodeWithNoMatchingTransactionLines(Core.Constants.ChargeType.Overhead);
		}

		void ChargeTypeAssertionsForOVRChargeCodeWithMatchingTransactionLines()
		{
			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from OVR", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from OVR since there are posted transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from OVR", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Allowed to change to OVR from OVR", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from OVR", !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(Core.Constants.ChargeType.Overhead);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Not allowed to change to REV from OVR since there are posted transaction lines", TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
		}

		#endregion

		public void TestOnLoadedReadOnlyFieldsForREV()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(!chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestOnLoadedReadOnlyFieldsForMRG()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(!chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestOnLoadedReadOnlyFieldsForDSB()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(!chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestOnLoadedReadOnlyFieldsForOVR()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestOnLoadedReadOnlyFieldsForNON()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(!chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestOnLoadedReadOnlyFieldsForMJA()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), testLoadedChargeCode.PK) as AccChargeCode;

			Assert(!chargeCodeLoadedFromDB.AC_AG_RevenueAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_WIPAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_AccrualAccountInfo.ReadOnly);
			Assert(!chargeCodeLoadedFromDB.AC_AG_CostAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementSurplusAccountInfo.ReadOnly);
			Assert(chargeCodeLoadedFromDB.AC_AG_DisbursementShortfallAccountInfo.ReadOnly);
		}

		public void TestCommentChargeCanBeModified()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), TestChargeCode2.PK) as AccChargeCode;
			chargeCodeLoadedFromDB.OnLoaded();
			chargeCodeLoadedFromDB.AC_RateCalculator = "CDB";
			chargeCodeLoadedFromDB.Validation.ValidateAC_ChargeType();

			Assert(!chargeCodeLoadedFromDB.AC_ChargeTypeInfo.HasErrors());
		}

		public void TestTransitionFromComment()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), TestChargeCode2.PK) as AccChargeCode;
			chargeCodeLoadedFromDB.OnLoaded();
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;

			AssertHasError(chargeCodeLoadedFromDB.AC_ChargeTypeInfo, "Comment Charge Type cannot be change to other Type");
		}

		public void TestTransitionToComment()
		{
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Factory.Save();

			AccChargeCode chargeCodeLoadedFromDB = Factory.Load(typeof(AccChargeCode), TestChargeCode2.PK) as AccChargeCode;
			chargeCodeLoadedFromDB.OnLoaded();
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;

			AssertHasError(chargeCodeLoadedFromDB.AC_ChargeTypeInfo, "You Cannot change existing charge type to Comment Charge Type");
		}

		public void TestSettingItToCommentFromEmpty()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			Assert(!testLoadedChargeCode.AC_ChargeTypeInfo.HasError("Comment Charge Type cannot be change to other Type"));
		}

		public void TestCommentChargeTypeCanBeChangedIfNotSaved()
		{
			AccChargeCode testLoadedChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;

			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(!testLoadedChargeCode.AC_ChargeTypeInfo.HasError("Comment Charge Type cannot be change to other Type"));

			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert(!testLoadedChargeCode.AC_ChargeTypeInfo.HasError("Comment Charge Type cannot be change to other Type"));

			Factory.Save();
			testLoadedChargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(testLoadedChargeCode.AC_ChargeTypeInfo.HasError("Comment Charge Type cannot be change to other Type"));
		}

		public void TestCheckForInvalidGSTRateForCMTChargeType()
		{
			AccChargeCode testChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			Assert("Precondition: GC_IsGSTRegistered = true", testChargeCode2.Company.GC_IsGSTRegistered);
			Assert("Precondition: GC_IsWHTRegistered = false", !testChargeCode2.Company.GC_IsWHTRegistered);
			testChargeCode2.Company.GC_IsWHTRegistered = true;

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;

			AssertEquals("Property AC_AT_GSTRateInfo should be readonly", true, testChargeCode2.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("Property AC_AW_WithholdingTaxRateInfo should be readonly", true, testChargeCode2.AC_AW_WithholdingTaxRateInfo.ReadOnly);

			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			testChargeCode2.AC_AT_GSTRate = taxRate2.PK;
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeTaxOverride taxOverride = testChargeCode2.TaxOverrides.AddNew();

			taxOverride.AO_AT = taxRate.PK;
			taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";

			Factory.Save();

			testChargeCode2.AC_AW_WithholdingTaxRate = ZGuid.NewZGuid();

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;

			AssertHasError(testChargeCode2.AC_AT_GSTRateInfo, "This data is invalid. There is no sense in it if the charge type is 'CMT'");
			AssertHasError(testChargeCode2.AC_AW_WithholdingTaxRateInfo, "This data is invalid. There is no sense in it if the charge type is 'CMT'");
			AssertHasError(taxOverride.AO_ATInfo, "This data is invalid. There is no sense in it if the charge type is 'CMT'");
			AssertHasError(taxOverride.AO_A9_DefaultVATClassInfo, "This data is invalid. There is no sense in it if the charge type is 'CMT'");
		}

		public void TestCheckForInvalidDataForCMTChargeType()
		{
			AccChargeCode testChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();

			Assert("Precondition: GC_IsGSTRegistered = true", testChargeCode2.Company.GC_IsGSTRegistered);
			Assert("Precondition: GC_IsWHTRegistered = false", !testChargeCode2.Company.GC_IsWHTRegistered);
			testChargeCode2.Company.GC_IsWHTRegistered = true;

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Property AC_AT_GSTRateInfo should not be readonly", false, testChargeCode2.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("Property AC_AW_WithholdingTaxRateInfo should not be readonly", false, testChargeCode2.AC_AW_WithholdingTaxRateInfo.ReadOnly);

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			AssertEquals("Property AC_AT_GSTRateInfo should be readonly", true, testChargeCode2.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("Property AC_AW_WithholdingTaxRateInfo should be readonly", true, testChargeCode2.AC_AW_WithholdingTaxRateInfo.ReadOnly);

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			AssertEquals("Property AC_AT_GSTRateInfo should not be readonly", false, testChargeCode2.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("Property AC_AW_WithholdingTaxRateInfo should not be readonly", false, testChargeCode2.AC_AW_WithholdingTaxRateInfo.ReadOnly);

			AccTaxRate taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			testChargeCode2.AC_AT_GSTRate = taxRate2.PK;
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeTaxOverride taxOverride = testChargeCode2.TaxOverrides.AddNew();

			taxOverride.AO_AT = taxRate.PK;
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";

			AccChargeTypeOverride typeOverride = testChargeCode2.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = "IMP";
			typeOverride.AN_JobType = "TRN";
			typeOverride.AN_ChargeType = "REV";
			typeOverride.AN_MarginPercentage = 0m;

			Factory.Save();

			testChargeCode2.AC_AW_WithholdingTaxRate = ZGuid.NewZGuid();

			testChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			AssertHasErrors(testChargeCode2.AC_ChargeTypeInfo);
			AssertEquals("Property AC_AT_GSTRateInfo should not be readonly", false, testChargeCode2.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("Property AC_AW_WithholdingTaxRateInfo should not be readonly", false, testChargeCode2.AC_AW_WithholdingTaxRateInfo.ReadOnly);
		}

		public void TestGetGSTRateUseChargeCode()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			AccTaxRate gSTAndQSTRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(new ZQuery(AccTaxRateSchema.AT_Code, "GSTANDQST"), new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)));
			if (gSTAndQSTRate != null)
			{
				gSTAndQSTRate.Delete();
			}

			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			chargeCode.AC_AT_GSTRate = taxRate.PK;

			AccTaxRate newGSTAndQSTRate = Factory.NewWithValidTestData<AccTaxRate>();
			newGSTAndQSTRate.AT_Code = "GSTANDQST";
			newGSTAndQSTRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			newGSTAndQSTRate.AT_Type = AccTaxRate.Types.Rated;
			newGSTAndQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQST;
			newGSTAndQSTRate.AT_ExtraTaxRateType = AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase;
			newGSTAndQSTRate.AT_Description = "GST and QST";
			newGSTAndQSTRate.AT_IsActive = ZBool.True;
			newGSTAndQSTRate.SetRateNumerator_ForTestOnly(5);
			newGSTAndQSTRate.SetExtraRate_ForTestOnly(75, 10);

			AccTaxOverrideGroup group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			AccChargeTaxOverride correctTaxOverride = group.TaxOverrides.AddNew();
			correctTaxOverride.AO_CostSellAll = "ALL";
			correctTaxOverride.AO_Direction = "ALL";
			correctTaxOverride.AO_JobType = "ALL";
			correctTaxOverride.AO_Origin = "ALL";
			correctTaxOverride.AO_Destination = "ALL";
			correctTaxOverride.AO_IncoTerm = "ALL";
			correctTaxOverride.AO_TaxRegCntryOrGroup = "ALL";
			correctTaxOverride.AO_AT = newGSTAndQSTRate.PK;
			correctTaxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			correctTaxOverride.AO_HomeCountryOrZone = AccChargeTaxOverride.AllCountriesExceptLoginCountry;

			chargeCode.AC_AX_TaxOverrideGroup = group.PK;
			Factory.Save();

			ZGuid overrideInvTaxMsg;
			RefCountry countryAU = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			OrgHeader orgAU = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "AUSYD"));
			RefCountry countryNZ = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.NewZealand);
			OrgHeader orgNZ = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "NZAKL"));

			gSTAndQSTRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, orgNZ,
				countryNZ, countryNZ, QuebecBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(newGSTAndQSTRate.PK, gSTAndQSTRate.PK);

			gSTAndQSTRate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.CrossTrade, AccChargeTaxOverride.ALL, orgAU,
				countryAU, countryAU, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxRate.PK, gSTAndQSTRate.PK);
		}

		public void TestGetGSTRateTuple_Comment_notReport()
		{
			var country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Australia);
			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = Constants.IncoTerms.ExWorks,
				CostOrSell = CostSell.Cost,
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Direction = Directions.CrossTrade,
				TransportMode = "ALL",
				Organisation = GlbCompany.CurrentCompany.OrgProxy,
				Origin = country,
				Destination = country,
			};

			// ChargeType is comment
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";
			chargeCode.AC_ChargeType = Constants.ChargeType.Comment;

			var gstRateTuple = chargeCode.GetGSTRateTuple(parameters);

			AssertNull(gstRateTuple.TaxRate);
			AssertEquals(ZGuid.Empty, gstRateTuple.OverrideInvTaxMsg);
			AssertNull(gstRateTuple.TaxOverride);

			//notReportTaxRate
			chargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			parameters.Organisation = GlbCompany.CurrentCompany.OrgProxy;

			var taxMsgPK = Factory.NewWithValidTestData<AccInvMsg>().PK;
			var mock = new Mock<IAccounting>();
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORT).Returns(true);
			mock.Setup(m => m.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy).Returns(false);
			mock.Setup(m => m.GroupMemberBillingDefaultInvoiceTaxMessage).Returns(taxMsgPK);

			using (ObjectFactory.Substitute(mock.Object))
			{
				gstRateTuple = chargeCode.GetGSTRateTuple(parameters);

				AssertEquals("NOTREPORT", gstRateTuple.TaxRate.AT_Code);
				AssertEquals(taxMsgPK, gstRateTuple.OverrideInvTaxMsg);
				AssertNull(gstRateTuple.TaxOverride);
			}
		}

		public void TestGetGSTRateTuple_Cache()
		{
			var code = Factory.NewWithValidTestData<AccChargeCode>();
			code.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;
			code.GSTRate.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = Constants.IncoTerms.ExWorks,
				CostOrSell = CostSell.Revenue,
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Direction = Directions.Export,
				TransportMode = "ALL",
				Organisation = org,
			};
			var gstRateTuple = code.GetGSTRateTuple(parameters);

			AssertEquals(code.AC_AT_GSTRate, gstRateTuple.TaxRate.PK);
			AssertEquals(code.GSTRate.AT_A9_DefaultVatClass, gstRateTuple.OverrideInvTaxMsg);
			AssertNull(gstRateTuple.TaxOverride);

			int dbHitsCount = Factory.DatabaseLoadCount;

			gstRateTuple = code.GetGSTRateTuple(parameters);
			AssertEquals(code.AC_AT_GSTRate, gstRateTuple.TaxRate.PK);
			AssertEquals(code.GSTRate.AT_A9_DefaultVatClass, gstRateTuple.OverrideInvTaxMsg);
			AssertNull(gstRateTuple.TaxOverride);

			AssertEquals("Should be no new DB hits", dbHitsCount, Factory.DatabaseLoadCount);
		}

		public void TestGetGSTRateTuple_CompanyInEuropeanUnion()
		{
			var australia = RefCountry.LoadFromCountryCode(Factory, Constants.CountryCodes.Australia);
			australia.RN_EconomicGrouping = EconomicGroupList.Codes.EuropeanUnion;

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_RL_NKClosestPort = "AUMEL";
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			customsCode.OK_CodeType = Country.GetConsumptionTaxRegistrationOrgCusCode(Constants.CountryCodes.Australia);
			customsCode.OK_CustomsRegNo = "1234";
			var taxRate1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_A9_DefaultVatClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			var taxRate2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate1.AT_RN_NKCountry = taxRate2.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			var collection = new EUTaxIDDefaultingRuleCollection();
			var rule = collection.AddNew();
			rule.Origin = EUTaxIDDefaultingRule.OriginDestinationCode.MyEUCountry;
			rule.Destination = EUTaxIDDefaultingRule.OriginDestinationCode.SameAsOrigin;
			rule.CostTaxRateForOrganisationRegisteredInMyCountry = taxRate1.PK;
			rule.CostTaxRateForOrganisationRegisteredInOtherEUCountry = taxRate2.PK;

			AccChargeCodeRegistry.Instance.EUTaxIDDefaulting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, collection);

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = Constants.IncoTerms.ExWorks,
				CostOrSell = CostSell.Cost,
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Direction = Directions.Export,
				TransportMode = "ALL",
				Organisation = orgHeader,
				Origin = australia,
				Destination = australia,
			};

			var gstRateTuple = chargeCode.GetGSTRateTuple(parameters);
			AssertEquals(taxRate1.PK, gstRateTuple.TaxRate.PK);
			AssertEquals(taxRate1.AT_A9_DefaultVatClass, gstRateTuple.OverrideInvTaxMsg);
			AssertNull(gstRateTuple.TaxOverride);
		}

		public void TestGetGSTRateTuple_TaxOverride()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			chargeCode.AC_AT_GSTRate = taxRate.PK;

			var taxRateForTaxOverride = Factory.NewWithValidTestData<AccTaxRate>();
			taxRateForTaxOverride.AT_Code = "TAX1";

			var taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_TransportMode = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = taxRateForTaxOverride.PK;
			taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			Factory.Save();

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = Constants.IncoTerms.ExWorks,
				CostOrSell = CostSell.Revenue,
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Direction = Directions.Export,
				TransportMode = "ALL",
			};
			var gstRateTuple = chargeCode.GetGSTRateTuple(parameters);

			AssertEquals(taxRateForTaxOverride.PK, gstRateTuple.TaxRate.PK);
			AssertEquals(taxOverride.AO_A9_DefaultVATClass, gstRateTuple.OverrideInvTaxMsg);
			AssertEquals(taxOverride.PK, gstRateTuple.TaxOverride.PK);
		}

		public void TestGetGSTRateTuple_TaxOverrideGroup()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TST";

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			chargeCode.AC_AT_GSTRate = taxRate.PK;

			var taxRateForTaxOverride = Factory.NewWithValidTestData<AccTaxRate>();
			taxRateForTaxOverride.AT_Code = "TAX1";

			var group = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverride = group.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_TransportMode = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = taxRateForTaxOverride.PK;
			taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;

			chargeCode.AC_AX_TaxOverrideGroup = group.PK;
			Factory.Save();

			var parameters = new AccChargeTaxOverrideMatcher.TaxCalculationParameters
			{
				IncoTerm = Constants.IncoTerms.ExWorks,
				CostOrSell = CostSell.Revenue,
				JobType = JobInvoicingConsumerTypes.Shipment.Code,
				Direction = Directions.Export,
				TransportMode = "ALL",
			};
			var gstRateTuple = chargeCode.GetGSTRateTuple(parameters);

			AssertEquals(taxRateForTaxOverride.PK, gstRateTuple.TaxRate.PK);
			AssertEquals(taxOverride.AO_A9_DefaultVATClass, gstRateTuple.OverrideInvTaxMsg);
			AssertEquals(taxOverride.PK, gstRateTuple.TaxOverride.PK);
		}

		public void TestAccChargeGLPostingOverrideFetchHint()
		{
			/*
			AccChargeCode: 1
			AccChargeGLPostingOverride: 1

			Hits: 2/2
			*/
			var values = new Tuple<SchemaColumn, string[]>(AccChargeGLPostingOverrideSchema.Y1_JobType, new string[2] { "ALL", "SHP" });
			AccountingAssertionHelper.AssertFetchHint<AccChargeCode, AccChargeGLPostingOverride>(AccChargeGLPostingOverrideSchema.Y1_AC, valuesToAvoidUniqueIndexCheck: values);
		}

		#region Global Charge Code Tests

		public const string GLHeader_Revenue = "1010.10.00"; // FREIGHT REVENUE
		public const string GLHeader_Revenue2 = "1020.10.00"; // AGENCY REVENUE
		public const string GLHeader_Revenue3 = "1030.10.00"; // PORT AND TERMINAL REVENUE
		public const string GLHeader_Cost = "1010.20.00"; // FREIGHT COST
		public const string GLHeader_Cost2 = "1020.20.00"; // AGENCY COSTS
		public const string GLHeader_Cost3 = "1030.20.00"; // PORT AND TERMINAL COSTS
		public const string GLHeader_Cost4 = "1040.20.00"; // DOCUMENTATION COSTS
		public const string GLHeader_Cost5 = "1050.20.00"; // CONTAINER COSTS
		public const string GLHeader_CostClearing = "5110.00.00";
		public const string GLHeader_CostClearing2 = "5200.00.00";
		public const string GLHeader_CostClearing3 = "5300.00.00";
		public const string GLHeader_RevenueClearing = "6510.10.00";
		public const string GLHeader_RevenueClearing2 = "6510.20.00";
		public const string GLHeader_RevenueClearing3 = "6510.30.00";

		public static ZGuid AccGLHeaderPK(BusinessObjectFactory factory, ZString account)
		{
			return factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, account)).PK;
		}

		public static void EnsureAllGSTRegisteredCompaniesHaveRatedGST(BusinessObjectFactory factory)
		{
			var newFactory = new BusinessObjectFactory();
			var accounting = ObjectFactory.Get<IAccounting>();
			int counter = 0;
			foreach (var company in newFactory.Load<GlbCompany>(new ZQuery()))
			{
				if (company.GC_IsGSTRegistered)
				{
					var mainGSTPK = AccTaxRate.Helper.FindTaxRatePK(newFactory, AccTaxRate.Helper.MainGSTTaxRegistryID, company.PK.ToGuid());
					if (!mainGSTPK.IsValid)
					{
						var taxRate = newFactory.New<AccTaxRate>();
						taxRate.AT_Type = AccTaxRate.Types.Rated;
						taxRate.AT_Code = "RATED" + (++counter);
						taxRate.AT_RN_NKCountry = company.GC_RN_NKCountryCode;
						accounting.SetMainGSTTaxIDConfiguration(company.PK.ToGuid(), taxRate.PK.ToGuid());
					}
				}
			}

			newFactory.Save();
			factory.ClearQueryCache();
		}

		public void TestGlobalChargeCodeProperties()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			AssertEquals("IsGlobal for normal charge code", false, normalChargeCode.IsGlobal);
			AssertEquals("IsGlobal for normal but linked-to-global charge code", false, normalChargeCodeLinked.IsGlobal);
			AssertEquals("IsGlobal for global charge code", true, globalChargeCode.IsGlobal);

			AssertEquals("IsLinkedToGlobalChargeCode for normal charge code", false, normalChargeCode.IsLinkedToGlobalChargeCode);
			AssertEquals("IsLinkedToGlobalChargeCode for normal but linked-to-global charge code", true, normalChargeCodeLinked.IsLinkedToGlobalChargeCode);
			AssertEquals("IsLinkedToGlobalChargeCode for global charge code", false, globalChargeCode.IsLinkedToGlobalChargeCode);

			AssertEquals("GlobalChargeCode for normal charge code", null, normalChargeCode.GlobalChargeCode);
			AssertEquals("GlobalChargeCode for normal but linked-to-global charge code", globalChargeCode, normalChargeCodeLinked.GlobalChargeCode);
			AssertEquals("GlobalChargeCode for global charge code", null, globalChargeCode.GlobalChargeCode);
		}

		public void TestReadOnlyTaxPropertiesForGlobalChargeCodes()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);
			normalChargeCode.Company.GC_IsGSTRegistered = true;
			normalChargeCode.Company.GC_IsWHTRegistered = true;
			normalChargeCodeLinked.Company.GC_IsGSTRegistered = true;
			normalChargeCodeLinked.Company.GC_IsWHTRegistered = true;

			AssertEquals("GST Readonly for normal charge code", false, normalChargeCode.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("GST Readonly for normal but linked-to-global charge code", false, normalChargeCodeLinked.AC_AT_GSTRateInfo.ReadOnly);
			AssertEquals("GST Readonly for global charge code", true, globalChargeCode.AC_AT_GSTRateInfo.ReadOnly);

			AssertEquals("WHT Readonly for normal charge code", false, normalChargeCode.AC_AW_WithholdingTaxRateInfo.ReadOnly);
			AssertEquals("WHT Readonly for normal but linked-to-global charge code", false, normalChargeCodeLinked.AC_AW_WithholdingTaxRateInfo.ReadOnly);
			AssertEquals("WHT Readonly for global charge code", true, globalChargeCode.AC_AW_WithholdingTaxRateInfo.ReadOnly);
		}

		public void TestGlobalChargeCodeDeletionWithoutReferences()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			globalChargeCode.Delete();

			AssertEquals("Global charge code is deleted", true, globalChargeCode.IsDeleted);
			AssertEquals("Linked charge code is deleted", true, normalChargeCodeLinked.IsDeleted);
			AssertEquals("Other charge code is not deleted", false, normalChargeCode.IsDeleted);
		}

		public void TestGlobalChargeCodeDeletionWithReferences()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);
			var charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AC = normalChargeCodeLinked.PK;
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			Exception caughtEx = null;
			try
			{
				factory2.Load<AccChargeCode>(globalChargeCode.PK).Delete();
				factory2.Save();
			}
			catch (Exception ex)
			{
				caughtEx = ex;
			}

			AssertNotNull("Exception was raised due to being referenced by line on child", caughtEx);
			Assert("Exception is due to FK", IsFKViolationException(caughtEx));
		}

		bool IsFKViolationException(Exception ex)
		{
			return (ex.InnerException != null && ex.InnerException.Message.Contains("The DELETE statement conflicted with the REFERENCE constraint"));
		}

		public void TestGlobalChargeCodeEditExisting()
		{
			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);

			// Sanity test that initial save worked as expected (also covered in TestGlobalChargeCodeSaveNew)				
			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes.Length);
			var testChargeCode = generatedChargeCodes[0];

			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Code, testChargeCode.AC_Code);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_AccrualAccount, testChargeCode.AC_AG_AccrualAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Desc, testChargeCode.AC_Desc);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_MarginPercentage, testChargeCode.AC_MarginPercentage);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", 1, testChargeCode.GLPostingOverrides.Count);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_GE, testChargeCode.GLPostingOverrides[0].Y1_GE);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_ACR, testChargeCode.GLPostingOverrides[0].Y1_AG_ACR);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_CST, testChargeCode.GLPostingOverrides[0].Y1_AG_CST);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_REV, testChargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_JobType, testChargeCode.GLPostingOverrides[0].Y1_JobType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_TransportMode, testChargeCode.GLPostingOverrides[0].Y1_TransportMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_Direction, testChargeCode.GLPostingOverrides[0].Y1_Direction);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode, testChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType, testChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_HousePaymentType, testChargeCode.GLPostingOverrides[0].Y1_HousePaymentType);

			// Test updating of fields on child
			globalChargeCode.AC_Code = "CC2A";
			globalChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(Factory, GLHeader_Cost2);
			globalChargeCode.AC_Desc = "DESC0";
			globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Cost2);
			globalChargeCode.AC_MarginPercentage = 33M;
			Factory.Save();

			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Code, testChargeCode.AC_Code);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_AccrualAccount, testChargeCode.AC_AG_AccrualAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Desc, testChargeCode.AC_Desc);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_MarginPercentage, testChargeCode.AC_MarginPercentage);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_GE, testChargeCode.GLPostingOverrides[0].Y1_GE);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_ACR, testChargeCode.GLPostingOverrides[0].Y1_AG_ACR);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_CST, testChargeCode.GLPostingOverrides[0].Y1_AG_CST);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_REV, testChargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_JobType, testChargeCode.GLPostingOverrides[0].Y1_JobType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_TransportMode, testChargeCode.GLPostingOverrides[0].Y1_TransportMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_Direction, testChargeCode.GLPostingOverrides[0].Y1_Direction);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode, testChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType, testChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_HousePaymentType, testChargeCode.GLPostingOverrides[0].Y1_HousePaymentType);

			// Test if you only change child records (i.e. parent object doesn't know it has changes)
			globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Cost3);
			Factory.Save();
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);

			// Test disconnection of fields from child and parent
			testChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(Factory, GLHeader_Cost4);
			testChargeCode.AC_Desc = "DESC1";
			testChargeCode.AC_MarginPercentage = 11M;
			testChargeCode.GLPostingOverrides[0].Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Cost4);
			Factory.Save();

			globalChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(Factory, GLHeader_Cost5);
			globalChargeCode.AC_Desc = "DESC2";
			globalChargeCode.AC_MarginPercentage = 77M;
			globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Cost5);
			Factory.Save();

			AssertNotEquals("Testing values not copied from global code", globalChargeCode.AC_AG_AccrualAccount, testChargeCode.AC_AG_AccrualAccount);
			AssertNotEquals("Testing values not copied from global code", globalChargeCode.AC_Desc, testChargeCode.AC_Desc);
			AssertNotEquals("Testing values not copied from global code", globalChargeCode.AC_MarginPercentage, testChargeCode.AC_MarginPercentage);
			AssertNotEquals("Testing GLPostingOverride child not copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);
		}

		public void TestInvalidChargeCodeFromCompanySaveShowsCorrectError()
		{
			Factory.SuspendValidation();
			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			globalChargeCode.AC_DepartmentFilterList = "";
			globalChargeCode.AC_Code = "AAA";

			Factory.Save();

			Factory.ResumeValidation();
			var company = Factory.New<GlbCompany>();
			company.GC_Name = "Eagle Datamation International";

			bool expectionCaught = false;
			try
			{
				Factory.Save();
			}
			catch (AccChargeCode.ValidationOnLocalChargeCodeException expectedException)
			{
				expectionCaught = true;
				AssertEquals("Exception message", @"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company 'Eagle Datamation International' would cause these errors:

Error - AC_DepartmentFilterList: Please enter a Department Filter.

The invalid charge code 'AAA' can be found in Maintain > Account > Global Charge Codes.
Please edit the charge code to correct the error shown above.
To highlight errors to be corrected choose ""File"" then ""Validate All"" after editing the charge code(s).", expectedException.Message);
			}

			Assert("We expect a ValidationOnLocalChargeCodeException", expectionCaught);
		}

		public void TestGlobalChargeCodeEditExisting_PerformsValidationChargeCode()
		{
			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();

			// Ensure that the "Eagle Datamation International" company is the one with the updated local charge code, otherwise we cannot
			// predict which company it will create the error for (because it stops at the first problem it finds).
			foreach (var chargeCode in globalChargeCode.ChildChargeCodes)
			{
				if (!(chargeCode.Company.GC_Name == "Eagle Datamation International"))
				{
					chargeCode.AC_MarginPercentage = 1;
				}
			}
			Factory.Save();

			var testChargeCode = globalChargeCode.ChildChargeCodes[0];

			var errors = globalChargeCode.AC_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());
			errors = testChargeCode.AC_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());

			globalChargeCode.AC_Desc = "A new description";
			globalChargeCode.AC_MarginPercentage = 101; // Invalid should be 0 to 100, so will cause validation error

			globalChargeCode.Validation.ValidateAll();
			errors = globalChargeCode.AC_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 1, errors.Count());
			errors = testChargeCode.AC_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());

			bool expectionCaught = false;
			try
			{
				Factory.Save();
			}
			catch (AccChargeCode.ValidationOnLocalChargeCodeException expectedException)
			{
				expectionCaught = true;
				AssertEquals("Exception message", @"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company 'Eagle Datamation International' would cause these errors:

Error - AC_MarginPercentage: Margin Percentage must be a value between 0.01 and 100.", expectedException.Message);
				foreach (var chargeCode in globalChargeCode.ChildChargeCodes)
				{
					AssertEquals("No changes were made", false, chargeCode.HasChanges);
					AssertEquals("No changes were made", "Aaa Bee See One", chargeCode.AC_Desc);
				}
			}

			Assert("We expect a ValidationOnLocalChargeCodeException", expectionCaught);
		}

		[ExpectExceptionMessage(typeof(AccChargeCode.ValidationOnLocalChargeCodeException), @"It is not possible to save the Global Charge Code, because the changes made to the Charge Code for company 'Eagle Datamation International Pte Ltd' would cause these errors:

Error - AN_MarginPercentage: Margin Percentage must be a value between 0.01 and 100.")]
		public void TestGlobalChargeCodeEditExisting_PerformsValidationChildChargeCodes()
		{
			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();
			var testChargeCode = globalChargeCode.ChildChargeCodes[0];

			var errors = globalChargeCode.ChargeTypeOverrides[0].AN_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());
			errors = testChargeCode.ChargeTypeOverrides[0].AN_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());

			globalChargeCode.ChargeTypeOverrides[0].AN_MarginPercentage = 101; // Invalid should be 0 to 100, so will cause validation error

			globalChargeCode.Validation.ValidateAll();
			errors = globalChargeCode.ChargeTypeOverrides[0].AN_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 1, errors.Count());
			errors = testChargeCode.ChargeTypeOverrides[0].AN_MarginPercentageInfo.GetErrors();
			AssertEquals("precondition", 0, errors.Count());
			Factory.Save();
			errors = testChargeCode.ChargeTypeOverrides[0].AN_MarginPercentageInfo.GetErrors();
			// Exception happens after this save.
		}

		public void TestGlobalChargeCodeSaveNew()
		{
			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);

			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes.Length);

			foreach (AccChargeCode chargecode in generatedChargeCodes)
			{
				AssertNotEquals("No charge code in demo company", GlbCompany.DemoCompanyCode, chargecode.Company.GC_Code);
			}

			var testChargeCode = generatedChargeCodes[0];

			// Parent records
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Code, testChargeCode.AC_Code);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_AccrualAccount, testChargeCode.AC_AG_AccrualAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_CostAccount, testChargeCode.AC_AG_CostAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_RevenueAccount, testChargeCode.AC_AG_RevenueAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_WIPAccount, testChargeCode.AC_AG_WIPAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AllowDescriptionOvertype, testChargeCode.AC_AllowDescriptionOvertype);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AR_ExpenseGroup, testChargeCode.AC_AR_ExpenseGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AR_SalesGroup, testChargeCode.AC_AR_SalesGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeGroup, testChargeCode.AC_ChargeGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeOtherGroups, testChargeCode.AC_ChargeOtherGroups);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeSubGroup, testChargeCode.AC_ChargeSubGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeType, testChargeCode.AC_ChargeType);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_DepartmentFilterList, testChargeCode.AC_DepartmentFilterList);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Desc, testChargeCode.AC_Desc);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ENettChargeCodeMap, testChargeCode.AC_ENettChargeCodeMap);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_GoodsServiceType, testChargeCode.AC_GoodsServiceType);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IATA_ChargeCodeMap, testChargeCode.AC_IATA_ChargeCodeMap);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsActive, testChargeCode.AC_IsActive);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsCommissionable, testChargeCode.AC_IsCommissionable);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsGroupageCharge, testChargeCode.AC_IsGroupageCharge);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_MarginPercentage, testChargeCode.AC_MarginPercentage);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_PrintSequence, testChargeCode.AC_PrintSequence);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_RateCalculator, testChargeCode.AC_RateCalculator);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ShowOnQuotation, testChargeCode.AC_ShowOnQuotation);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_SuppressOnQuoteIfZero, testChargeCode.AC_SuppressOnQuoteIfZero);
			AssertEquals("Testing values that are not copied from global code", ZGuid.Empty, testChargeCode.AC_AW_WithholdingTaxRate);
			AssertEquals("Testing values that are not copied from global code", ZGuid.Empty, testChargeCode.AC_AX_TaxOverrideGroup);
			AssertEquals("Testing values that are not copied from global code", ZString.Empty, testChargeCode.AC_LocalLanguageDescription);

			// Child records
			AssertEquals("Testing BranchOverride child rows copied from global code", 0, testChargeCode.BranchOverrides.Count);
			AssertEquals("Testing TaxOverride child rows copied from global code", 0, testChargeCode.TaxOverrides.Count);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", 1, testChargeCode.ChargeTypeOverrides.Count);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", globalChargeCode.ChargeTypeOverrides[0].AN_JobDirection, testChargeCode.ChargeTypeOverrides[0].AN_JobDirection);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", globalChargeCode.ChargeTypeOverrides[0].AN_JobType, testChargeCode.ChargeTypeOverrides[0].AN_JobType);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", globalChargeCode.ChargeTypeOverrides[0].AN_ChargeType, testChargeCode.ChargeTypeOverrides[0].AN_ChargeType);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", globalChargeCode.ChargeTypeOverrides[0].AN_InvoiceType, testChargeCode.ChargeTypeOverrides[0].AN_InvoiceType);
			AssertEquals("Testing ChargeTypeOverride child rows copied from global code", globalChargeCode.ChargeTypeOverrides[0].AN_MarginPercentage, testChargeCode.ChargeTypeOverrides[0].AN_MarginPercentage);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", 1, testChargeCode.RevenueRecOverrides.Count);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", globalChargeCode.RevenueRecOverrides[0].AE_JobType, testChargeCode.RevenueRecOverrides[0].AE_JobType);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", globalChargeCode.RevenueRecOverrides[0].AE_Direction, testChargeCode.RevenueRecOverrides[0].AE_Direction);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", globalChargeCode.RevenueRecOverrides[0].AE_Mode, testChargeCode.RevenueRecOverrides[0].AE_Mode);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", globalChargeCode.RevenueRecOverrides[0].AE_BrokerType, testChargeCode.RevenueRecOverrides[0].AE_BrokerType);
			AssertEquals("Testing RevenueRecOverride child rows copied from global code", globalChargeCode.RevenueRecOverrides[0].AE_RecognitionType, testChargeCode.RevenueRecOverrides[0].AE_RecognitionType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", 1, testChargeCode.GLPostingOverrides.Count);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_GE, testChargeCode.GLPostingOverrides[0].Y1_GE);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_ACR, testChargeCode.GLPostingOverrides[0].Y1_AG_ACR);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_CST, testChargeCode.GLPostingOverrides[0].Y1_AG_CST);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_REV, testChargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_JobType, testChargeCode.GLPostingOverrides[0].Y1_JobType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_TransportMode, testChargeCode.GLPostingOverrides[0].Y1_TransportMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_Direction, testChargeCode.GLPostingOverrides[0].Y1_Direction);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode, testChargeCode.GLPostingOverrides[0].Y1_ConsolContainerMode);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType, testChargeCode.GLPostingOverrides[0].Y1_MasterPaymentType);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_HousePaymentType, testChargeCode.GLPostingOverrides[0].Y1_HousePaymentType);
		}

		public void TestGlobalToLocalCopyClearingAccounts()
		{
			AccChargeCode SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet2()
			{
				EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);
				Factory.Save();

				var globChCode = Factory.NewWithValidTestData<AccChargeCode>();
				globChCode.AC_GC = ZGuid.Empty;
				globChCode.AC_Code = "CC2";
				globChCode.AC_AG_AccrualAccount = AccGLHeaderPK(Factory, GLHeader_Cost);
				globChCode.AC_AG_CostAccount = AccGLHeaderPK(Factory, GLHeader_Cost);
				globChCode.AC_AG_RevenueAccount = AccGLHeaderPK(Factory, GLHeader_Revenue);
				globChCode.AC_AG_CostClearingAccount = AccGLHeaderPK(Factory, GLHeader_CostClearing);
				globChCode.AC_AG_RevenueClearingAccount = AccGLHeaderPK(Factory, GLHeader_RevenueClearing);
				globChCode.AC_AG_WIPAccount = AccGLHeaderPK(Factory, GLHeader_Revenue);
				globChCode.AC_AllowDescriptionOvertype = ZBool.True;
				globChCode.AC_AR_ExpenseGroup = Factory.NewWithValidTestData<AccGroups>().PK;
				globChCode.AC_AR_SalesGroup = Factory.NewWithValidTestData<AccGroups>().PK;
				globChCode.AC_AT_GSTRate = ZGuid.Empty;
				globChCode.AC_AW_WithholdingTaxRate = ZGuid.Empty;
				globChCode.AC_AX_TaxOverrideGroup = ZGuid.Empty;
				globChCode.AC_ChargeGroup = globChCode.Lookups.ChargeGroupList[0].Code;
				globChCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal; // Shown as "Principal / Agent" on form
				globChCode.AC_ChargeSubGroup = globChCode.Lookups.ChargeSubGroupList[0].Code;
				globChCode.AC_ChargeType = Constants.ChargeType.NonAccrual;
				globChCode.AC_DepartmentFilterList = "ALL";
				globChCode.AC_Desc = "Aaa Bee See One";
				globChCode.AC_ENettChargeCodeMap = "Whatever";
				globChCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
				globChCode.AC_IsActive = true;
				globChCode.AC_IsCommissionable = ZBool.True;
				globChCode.AC_IsGroupageCharge = ZBool.True;
				globChCode.AC_LocalLanguageDescription = "Aaa Bee See Uno";
				globChCode.AC_MarginPercentage = 0M;
				globChCode.AC_PrintSequence = 4;
				globChCode.AC_RateCalculator = globChCode.Lookups.AC_RateCalculator_List[0].Code;
				globChCode.AC_ShowOnQuotation = ZBool.True;
				globChCode.AC_SuppressOnQuoteIfZero = ZBool.True;
				globChCode.AC_AC_RevenueChargeCode = Factory.NewWithValidTestData<AccChargeCode>().PK;

				var glPostingOverride = Factory.New<AccChargeGLPostingOverride>();
				glPostingOverride.Y1_AC = globChCode.PK;
				glPostingOverride.Y1_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
				glPostingOverride.Y1_AG_ACR = AccGLHeaderPK(Factory, GLHeader_Cost);
				glPostingOverride.Y1_AG_CST = AccGLHeaderPK(Factory, GLHeader_Cost);
				glPostingOverride.Y1_AG_REV = AccGLHeaderPK(Factory, GLHeader_Revenue);
				glPostingOverride.Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Revenue);
				glPostingOverride.Y1_AG_CST_Clearing = AccGLHeaderPK(Factory, GLHeader_CostClearing2);
				glPostingOverride.Y1_AG_REV_Clearing = AccGLHeaderPK(Factory, GLHeader_RevenueClearing2);

				return globChCode;
			}

			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet2();
			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);

			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes.Length);

			foreach (AccChargeCode chargecode in generatedChargeCodes)
			{
				AssertNotEquals("No charge code in demo company", GlbCompany.DemoCompanyCode, chargecode.Company.GC_Code);
			}

			var testChargeCode = generatedChargeCodes[0];

			// Parent records
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Code, testChargeCode.AC_Code);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_AccrualAccount, testChargeCode.AC_AG_AccrualAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_CostAccount, testChargeCode.AC_AG_CostAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_RevenueAccount, testChargeCode.AC_AG_RevenueAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_CostClearingAccount, testChargeCode.AC_AG_CostClearingAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_RevenueClearingAccount, testChargeCode.AC_AG_RevenueClearingAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AG_WIPAccount, testChargeCode.AC_AG_WIPAccount);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AllowDescriptionOvertype, testChargeCode.AC_AllowDescriptionOvertype);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AR_ExpenseGroup, testChargeCode.AC_AR_ExpenseGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_AR_SalesGroup, testChargeCode.AC_AR_SalesGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeGroup, testChargeCode.AC_ChargeGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeOtherGroups, testChargeCode.AC_ChargeOtherGroups);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeSubGroup, testChargeCode.AC_ChargeSubGroup);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ChargeType, testChargeCode.AC_ChargeType);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_DepartmentFilterList, testChargeCode.AC_DepartmentFilterList);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_Desc, testChargeCode.AC_Desc);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ENettChargeCodeMap, testChargeCode.AC_ENettChargeCodeMap);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_GoodsServiceType, testChargeCode.AC_GoodsServiceType);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IATA_ChargeCodeMap, testChargeCode.AC_IATA_ChargeCodeMap);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsActive, testChargeCode.AC_IsActive);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsCommissionable, testChargeCode.AC_IsCommissionable);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_IsGroupageCharge, testChargeCode.AC_IsGroupageCharge);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_MarginPercentage, testChargeCode.AC_MarginPercentage);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_PrintSequence, testChargeCode.AC_PrintSequence);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_RateCalculator, testChargeCode.AC_RateCalculator);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_ShowOnQuotation, testChargeCode.AC_ShowOnQuotation);
			AssertEquals("Testing values copied from global code", globalChargeCode.AC_SuppressOnQuoteIfZero, testChargeCode.AC_SuppressOnQuoteIfZero);
			AssertEquals("Testing values that are not copied from global code", ZGuid.Empty, testChargeCode.AC_AW_WithholdingTaxRate);
			AssertEquals("Testing values that are not copied from global code", ZGuid.Empty, testChargeCode.AC_AX_TaxOverrideGroup);
			AssertEquals("Testing values that are not copied from global code", ZString.Empty, testChargeCode.AC_LocalLanguageDescription);

			// Child records
			AssertEquals("Testing BranchOverride child rows copied from global code", 0, testChargeCode.BranchOverrides.Count);
			AssertEquals("Testing TaxOverride child rows copied from global code", 0, testChargeCode.TaxOverrides.Count);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", 1, testChargeCode.GLPostingOverrides.Count);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_GE, testChargeCode.GLPostingOverrides[0].Y1_GE);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_ACR, testChargeCode.GLPostingOverrides[0].Y1_AG_ACR);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_CST, testChargeCode.GLPostingOverrides[0].Y1_AG_CST);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_REV, testChargeCode.GLPostingOverrides[0].Y1_AG_REV);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_WIP, testChargeCode.GLPostingOverrides[0].Y1_AG_WIP);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_REV_Clearing, testChargeCode.GLPostingOverrides[0].Y1_AG_REV_Clearing);
			AssertEquals("Testing GLPostingOverride child rows copied from global code", globalChargeCode.GLPostingOverrides[0].Y1_AG_CST_Clearing, testChargeCode.GLPostingOverrides[0].Y1_AG_CST_Clearing);
		}

		public void TestGlobalChargeCodeSaveNew_AC_AT_GSTRate()
		{
			var noGstCompany = AllCompaniesExceptDemo[0];
			noGstCompany.GC_IsGSTRegistered = false;
			var gstCompanyWithNoReport = Array.Find(AllCompaniesExceptDemo, c => c.GC_IsGSTRegistered && AccTaxRate.GetNOTREPORTTaxID(Factory, c) != null);
			Factory.Save();

			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);

			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes.Length);
			var noGstCompanyChargeCode = Array.Find(generatedChargeCodes, cc => cc.AC_GC == noGstCompany.PK);
			var gstCompanyChargeCode = Array.Find(generatedChargeCodes, cc => cc.AC_GC == gstCompanyWithNoReport.PK);

			AssertEquals("Testing tax rate set when there is gst registation and is set to GST", "GST", gstCompanyChargeCode.GSTRate.AT_Code);
			AssertEquals("Testing tax rate not set when there is no gst registation", ZGuid.Empty, noGstCompanyChargeCode.AC_AT_GSTRate);
		}

		#region AirlineIataCode

		public void TestGlobalChargeCodeSaveNewAndUpdate_AirlineIataCode()
		{
			var carrier1 = CreateCarrier("CARRIER1");
			var carrier2 = CreateCarrier("CARRIER2");
			var carrier3 = CreateCarrier("CARRIER3");
			var carrier4 = CreateCarrier("CARRIER4");
			var carrier5 = CreateCarrier("CARRIER5");

			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();

			var airlineCode1 = globalChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			airlineCode1.ACI_OH_Carrier = carrier1.PK;

			var airlineCode2 = globalChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode2.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AC;
			airlineCode2.ACI_OH_Carrier = carrier2.PK;

			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);
			Assert(generatedChargeCodes.Length > 0);
			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes.Length);

			var airlineCodes = generatedChargeCodes[0].AccChargeCodeCarrierIataMappings;
			AssertEquals(2, airlineCodes.Count);

			var dbAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.DB);
			AssertNotNull(dbAirlineCode);
			AssertEquals(carrier1.PK, dbAirlineCode.ACI_OH_Carrier);

			var acAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.AC);
			AssertNotNull(acAirlineCode);
			AssertEquals(carrier2.PK, acAirlineCode.ACI_OH_Carrier);

			airlineCode1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DF;
			airlineCode1.ACI_OH_Carrier = carrier3.PK;

			globalChargeCode.AccChargeCodeCarrierIataMappings.Delete(airlineCode2);

			var airlineCode3 = globalChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode3.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.PU;
			airlineCode3.ACI_OH_Carrier = carrier4.PK;

			var airlineCode4 = globalChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode4.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.IN;
			airlineCode4.ACI_OH_Carrier = carrier5.PK;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var generatedChargeCodes2 = factory2.Load<AccChargeCode>(generatedChargeCodeQuery);
			Assert(generatedChargeCodes2.Length > 0);
			AssertEquals("Charge Code has been created in each company", AllCompaniesExceptDemo.Length, generatedChargeCodes2.Length);

			var airlineCodes2 = generatedChargeCodes[0].AccChargeCodeCarrierIataMappings;
			AssertEquals(3, airlineCodes2.Count);

			var dfAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.DF);
			AssertNotNull(dfAirlineCode);
			AssertEquals("Updated", carrier3.PK, dfAirlineCode.ACI_OH_Carrier);

			var acAirlineCode2 = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.AC);
			AssertNull("Deleted", acAirlineCode2);

			var puAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.PU);
			AssertNotNull(puAirlineCode);
			AssertEquals("Added", carrier4.PK, puAirlineCode.ACI_OH_Carrier);

			var inAirlineCode = airlineCodes.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.IN);
			AssertNotNull(inAirlineCode);
			AssertEquals("Added", carrier5.PK, inAirlineCode.ACI_OH_Carrier);
		}

		public void TestNewGlobalChargeCodeAndOldLocalChargeCodeSave_AirlineIataCode()
		{
			var carrier1 = CreateCarrier("CARRIER1");

			var globalChargeCode = SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet();
			Factory.Save();

			var generatedChargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "CC2");
			generatedChargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, null);
			var generatedChargeCodes = Factory.Load<AccChargeCode>(generatedChargeCodeQuery);
			Assert(generatedChargeCodes.Length > 0);

			var localChargeCode = generatedChargeCodes[0];
			var airlineCode1 = localChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode1.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.DB;
			airlineCode1.ACI_OH_Carrier = carrier1.PK;
			Factory.Save();

			var airlineCode2 = globalChargeCode.AccChargeCodeCarrierIataMappings.AddNew();
			airlineCode2.ACI_IATAChargeCodeMap = Core.Constants.AWB.ChargeCodes.AS;
			airlineCode2.ACI_OH_Carrier = carrier1.PK;

			AssertNoExceptionThrown(() => Factory.Save());

			var factory2 = new BusinessObjectFactory();
			var localChargeCode2 = factory2.Load<AccChargeCode>(localChargeCode.PK);
			var iataCode1 = localChargeCode2.AccChargeCodeCarrierIataMappings.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.DB);
			AssertNotNull("Airline IATA Code won't be updated when different with global IATA code", iataCode1);
			var iataCode2 = localChargeCode2.AccChargeCodeCarrierIataMappings.FirstOrDefault(a => a.ACI_IATAChargeCodeMap == Core.Constants.AWB.ChargeCodes.AS);
			AssertNull(iataCode2);
		}

		OrgHeader CreateCarrier(ZString code)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsAirLine = true;
			carrier.OH_Code = code;

			return carrier;
		}

		#endregion

		public void TestIsDifferentToGlobalChargeCode()
		{
			AccTaxRate taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_Code = "TAX1";
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, chargeType: Constants.ChargeType.Disbursement);

			AssertEquals("Normal charge code always returns false", false, normalChargeCode.IsDifferentToGlobalChargeCode);
			normalChargeCode.AC_Desc = "Something Else";
			AssertEquals("Normal charge code always returns false", false, normalChargeCode.IsDifferentToGlobalChargeCode);

			AssertEquals("Linked charge code returns false when same", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
			normalChargeCodeLinked.AC_Desc = "Something Else";
			AssertEquals("Linked charge code returns true when field different", true, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
			normalChargeCodeLinked.AC_Desc = "CC2 Desc";
			AssertEquals("Linked charge code returns false when set back to same", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
			normalChargeCodeLinked.AC_AT_GSTRate = taxRate.PK;
			AssertEquals("Linked charge code returns false when same except for a field that is not copied", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);

			AccChargeTypeOverride typeOverride = normalChargeCodeLinked.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = "IMP";
			typeOverride.AN_JobType = "TRN";
			typeOverride.AN_ChargeType = "REV";
			typeOverride.AN_MarginPercentage = 0m;
			AssertEquals("Linked charge code returns true when overrides different", true, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);

			typeOverride = globalChargeCode.ChargeTypeOverrides.AddNew();
			typeOverride.AN_JobDirection = "IMP";
			typeOverride.AN_JobType = "TRN";
			typeOverride.AN_ChargeType = "REV";
			typeOverride.AN_MarginPercentage = 0m;
			AssertEquals("Linked charge code returns true when overrides different but will be same when saved", true, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
			Factory.Save();
			AssertEquals("Linked charge code returns false when overrides same again", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);

			AccChargeTaxOverride taxOverride = normalChargeCodeLinked.TaxOverrides.AddNew();
			taxOverride.AO_AT = taxRate.PK;
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "ALL";
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_Origin = "ALL";
			taxOverride.AO_Destination = "ALL";
			AssertEquals("Linked charge code returns false when overrides same but different tax rates (because they are not copied)", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
		}

		public void TestIsDifferentToGlobalChargeCode_CreditorOverrides()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			AccChargeCreditorOverride creditorOverride1 = normalChargeCodeLinked.CreditorOverrides.AddNew();
			creditorOverride1.ACC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			creditorOverride1.ACC_Direction = Constants.FreightShipmentDirection.Code.Import;
			creditorOverride1.ACC_CreditorRole = DocAddressTypes.Codes.OverseasAgent;
			creditorOverride1.ACC_DefaultingRule = "SCA";
			creditorOverride1.ACC_PaymentTerm = Constants.PaymentType.Prepaid;
			creditorOverride1.ACC_TransportMode = "ALL";
			AssertEquals("Linked charge code returns true when creditor overrides are different", true, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);

			AccChargeCreditorOverride creditorOverride2 = globalChargeCode.CreditorOverrides.AddNew();
			creditorOverride2.ACC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			creditorOverride2.ACC_Direction = Constants.FreightShipmentDirection.Code.Import;
			creditorOverride2.ACC_CreditorRole = DocAddressTypes.Codes.OverseasAgent;
			creditorOverride2.ACC_DefaultingRule = "SCA";
			creditorOverride2.ACC_PaymentTerm = Constants.PaymentType.Prepaid;
			creditorOverride2.ACC_TransportMode = "ALL";

			AssertEquals("Linked charge code returns true when creditor overrides different but will be same when saved", true, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
			Factory.Save();
			AssertEquals("Linked charge code returns false when creditor overrides same again", false, normalChargeCodeLinked.IsDifferentToGlobalChargeCode);
		}

		public void TestAddGlobalCommentChargeCode()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, false, false, "", "CC2");
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Factory.Save();

			AssertEquals(2, globalChargeCode.ChildChargeCodes.Count);

			foreach (var chargeCode in globalChargeCode.ChildChargeCodes)
			{
				AssertEquals("Global charge code logs should always default tax rate for comment charge code to empty", ZGuid.Empty, chargeCode.AC_AT_GSTRate);
			}
		}

		public void TestChargeCodesWithMissingPostingOverridesNotGettingValidated()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var header1 = Factory.NewWithValidTestData<AccGLHeader>();
			var header2 = Factory.NewWithValidTestData<AccGLHeader>();

			chargeCode.AC_ChargeType = "REV";
			var postingOverride = chargeCode.GLPostingOverrides.AddNew();
			postingOverride.Y1_AG_REV = header1.PK;
			postingOverride.Y1_AG_WIP = header2.PK;
			postingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedChargeCode = newFactory.Load<AccChargeCode>(chargeCode.PK);

			loadedChargeCode.AC_ChargeType = "DSB";
			loadedChargeCode.RunPreSaveValidation();

			Assert(loadedChargeCode.GLPostingOverrides.HasErrors());
		}

		#region UsingChargeCodeCompanyNotCurrentCompany

		GlbCompany CurrentCompany;
		GlbCompany ChargeCompany;
		AccChargeCode ChargeCode;

		void SetupUsingChargeCodeCompanyNotCurrentCompany()
		{
			CurrentCompany = GlbCompany.CurrentCompany;
			ChargeCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			ChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			ChargeCode.AC_GC = ChargeCompany.PK;
			AssertNotEquals(ChargeCode.Company.Country, CurrentCompany.Country);
		}

		public void TestIsCompanyGSTAndWHTRegisteredUsesChargeCodeCompany()
		{
			SetupUsingChargeCodeCompanyNotCurrentCompany();
			CurrentCompany.GC_IsGSTRegistered = false;
			CurrentCompany.GC_IsWHTRegistered = false;
			ChargeCompany.GC_IsGSTRegistered = true;
			ChargeCompany.GC_IsWHTRegistered = true;
			AssertEquals(false, ChargeCode.AC_AT_GSTRate_ReadOnly);
			AssertEquals(false, ChargeCode.AC_AW_WithholdingTaxRate_ReadOnly);
		}

		public void TestGetGSTRateUsesChargeCodeCompany()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			ZGuid overrideInvTaxMsg;
			SetupUsingChargeCodeCompanyNotCurrentCompany();

			ChargeCompany.SetCountry("FR");
			AccTaxRate taxRate = ChargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Export, AccChargeTaxOverride.ALL, org, countryFR, countryFR, GlbBranch.CurrentBranch, null, null, out overrideInvTaxMsg);

			AssertNotNull(taxRate);
			AssertEquals(taxRate.AT_Type, "RAT");
		}

		public void TestGetGSTRateForCompanyInEuropeanUnionUsesChargeCodeCompany()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			SetupUsingChargeCodeCompanyNotCurrentCompany();

			AccTaxRate taxRate = ChargeCode.GetGSTRateForCompanyInEuropeanUnion(CostSell.Revenue, org, countryFR, countryFR);
			AssertEquals(taxRate.AT_Type, "RAT");
		}

		public void TestRecalculateDirectionBasedOnOriginAndDestinationUsesChargeCodeCompany()
		{
			RefCountry countryFR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.France);
			RefCountry countryBR = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Brazil);
			var companyBR = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			companyBR.SetCountry(Core.Constants.CountryCodes.Brazil);

			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = companyBR.PK;
			chargeCode.AC_AT_GSTRate = Factory.NewWithValidTestData<AccTaxRate>().PK;

			AccChargeTaxOverride taxOverride = chargeCode.TaxOverrides.AddNew();
			taxOverride.AO_CostSellAll = "ALL";
			taxOverride.AO_Direction = "IMP";
			taxOverride.AO_JobType = "ALL";
			taxOverride.AO_Origin = EconomicGroupList.Codes.EuropeanUnion;
			taxOverride.AO_Destination = chargeCode.Company.GC_RN_NKCountryCode;
			taxOverride.AO_IncoTerm = "ALL";
			taxOverride.AO_TaxRegCntryOrGroup = "ALL";
			taxOverride.AO_AT = Factory.NewWithValidTestData<AccTaxRate>().PK;
			taxOverride.AO_A9_DefaultVATClass = Factory.NewWithValidTestData<AccInvMsg>().PK;
			taxOverride.DefaultVATClass.A9_RN_NKCountryCode = companyBR.GC_RN_NKCountryCode;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			ZGuid overrideInvTaxMsg;
			AccTaxRate rate = chargeCode.GetGSTRateForTestOnly(Constants.IncoTerms.ExWorks, CostSell.Revenue, JobInvoicingConsumerTypes.Shipment.Code, Directions.Import, AccChargeTaxOverride.ALL, org, countryFR, chargeCode.Company.Country, chargeCode.Company.FirstActiveBranch, null, null, out overrideInvTaxMsg);
			AssertEquals(taxOverride.AO_AT, rate.PK);
			AssertEquals(taxOverride.AO_A9_DefaultVATClass, overrideInvTaxMsg);
		}

		#endregion

		#endregion

		#region AccChargeCodeCarrierIataMappings Test

		public void TestAccChargeCodeCarrierIataMappings()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var accChargeCodeCarrierIataMappings = new AccChargeCodeCarrierIataMappingCollection(chargeCode);
			accChargeCodeCarrierIataMappings.AddNew();
			accChargeCodeCarrierIataMappings.AddNew();

			AssertEquals(2, chargeCode.AccChargeCodeCarrierIataMappings.Count);
		}

		public void TestGetIATACodeFallback()
		{
			var chargeCodeGlobal = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeGlobal.AC_GC = ZGuid.Empty;
			chargeCodeGlobal.AC_Code = "CODE";
			chargeCodeGlobal.AC_Desc = "code desc";
			chargeCodeGlobal.AC_ChargeType = "REV";
			chargeCodeGlobal.AC_IsActive = true;
			chargeCodeGlobal.AC_IATA_ChargeCodeMap = "AC";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			AssertEquals("should fall back to global code", "AC", chargeCodeGlobal.GetIATACodeWithFallback(org.PK));

			var chargeCodeLocal = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeLocal.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeLocal.AC_Code = "CODE";
			chargeCodeLocal.AC_Desc = "code desc";
			chargeCodeLocal.AC_ChargeType = "REV";
			chargeCodeLocal.AC_IsActive = true;
			chargeCodeLocal.AC_IATA_ChargeCodeMap = "BD";

			AssertEquals("should fall back to local code", "BD", chargeCodeLocal.GetIATACodeWithFallback(org.PK));

			var globalOverride = chargeCodeGlobal.AccChargeCodeCarrierIataMappings.AddNew();
			globalOverride.ACI_OH_Carrier = org.PK;
			globalOverride.ACI_IATAChargeCodeMap = "EE";

			AssertEquals("should fall back to local code", "BD", chargeCodeLocal.GetIATACodeWithFallback(org.PK));
			AssertEquals("should fall back to global override code", "EE", chargeCodeGlobal.GetIATACodeWithFallback(org.PK));

			var localOverride = chargeCodeLocal.AccChargeCodeCarrierIataMappings.AddNew();
			localOverride.ACI_OH_Carrier = org.PK;
			localOverride.ACI_IATAChargeCodeMap = "FF";

			AssertEquals("should fall back to local override code", "FF", chargeCodeLocal.GetIATACodeWithFallback(org.PK));
			AssertEquals("should fall back to global override code", "EE", chargeCodeGlobal.GetIATACodeWithFallback(org.PK));
		}

		public void TestGetIATACodeFallback_LocalChargeCode()
		{
			var chargeCodeGlobal = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeGlobal.AC_GC = ZGuid.Empty;
			chargeCodeGlobal.AC_Code = "CODE";
			chargeCodeGlobal.AC_Desc = "code desc";
			chargeCodeGlobal.AC_ChargeType = "REV";
			chargeCodeGlobal.AC_IsActive = true;
			chargeCodeGlobal.AC_IATA_ChargeCodeMap = "AC";

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var chargeCodeLocal = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCodeLocal.AC_GC = GlbCompany.CurrentCompany.PK;
			chargeCodeLocal.AC_Code = "CODE";
			chargeCodeLocal.AC_Desc = "code desc";
			chargeCodeLocal.AC_ChargeType = "REV";
			chargeCodeLocal.AC_IsActive = true;
			AssertEquals("should fall back to global non-carrier specific code", "AC", chargeCodeLocal.GetIATACodeWithFallback(org.PK));

			var globalOverride = chargeCodeGlobal.AccChargeCodeCarrierIataMappings.AddNew();
			globalOverride.ACI_OH_Carrier = org.PK;
			globalOverride.ACI_IATAChargeCodeMap = "EE";
			AssertEquals("should fall back to global carrier specific code", "EE", chargeCodeLocal.GetIATACodeWithFallback(org.PK));

			chargeCodeLocal.AC_IATA_ChargeCodeMap = "BD";
			AssertEquals("should fall back to local code", "BD", chargeCodeLocal.GetIATACodeWithFallback(org.PK));

			var localOverride = chargeCodeLocal.AccChargeCodeCarrierIataMappings.AddNew();
			localOverride.ACI_OH_Carrier = org.PK;
			localOverride.ACI_IATAChargeCodeMap = "FF";
			AssertEquals("should fall back to local carrier specific code", "FF", chargeCodeLocal.GetIATACodeWithFallback(org.PK));
		}

		#endregion

		public void TestGlobalChargeCode()
		{
			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC1";
			globalChargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_Desc = "global Desc";
			Factory.Save();

			var localChargeCode = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompanyPK);
			AssertEquals(globalChargeCode, localChargeCode.GlobalChargeCode);

			localChargeCode.AC_Code = "ZZZ";
			AssertNull(localChargeCode.GlobalChargeCode);

			localChargeCode.AC_Code = "CC1";
			AssertEquals(globalChargeCode, localChargeCode.GlobalChargeCode);

			globalChargeCode.AC_Code = "CC2";
			AssertNull(localChargeCode.GlobalChargeCode);
		}

		#region PlaceOfSupplyGroup

		public void TestPlaceOfSupplyGroup()
		{
			var groupGRP = Factory.New<AccPOSChargeCodeGroup>();
			groupGRP.GRO_Code = "GRP";
			groupGRP.GRO_Description = "GRP Description";
			var groupGRP2 = Factory.New<AccPOSChargeCodeGroup>();
			groupGRP2.GRO_Code = "GRP2";
			groupGRP2.GRO_Description = "GRP2 Description";

			var query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "BAF");
			var chargeCodeBAF = Factory.LoadTop1<AccChargeCode>(query);
			var pivot = groupGRP.ChargeCodePivots.AddNew();
			pivot.GRP_GroupType = "POS";
			pivot.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot.GRP_MemberID = chargeCodeBAF.PK;

			query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "FRT");
			var chargeCodeFRT = Factory.LoadTop1<AccChargeCode>(query);
			var pivot2 = groupGRP2.ChargeCodePivots.AddNew();
			pivot2.GRP_GroupType = "POS";
			pivot2.GRP_MemberTableCode = AccChargeCodeSchema.Constants.Prefix;
			pivot2.GRP_MemberID = chargeCodeFRT.PK;

			query = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, "OLAB");
			var chargeCodeOLAB = Factory.LoadTop1<AccChargeCode>(query);

			Factory.Save();

			AssertNotNull(chargeCodeBAF.PlaceOfSupplyGroup);
			AssertEquals("BAF should be in GRP Place of Supply Group", "GRP", chargeCodeBAF.PlaceOfSupplyGroup.GRO_Code);

			AssertNotNull(chargeCodeBAF.PlaceOfSupplyGroup);
			AssertEquals("FRT should be in GRP2 Place of Supply Group", "GRP2", chargeCodeFRT.PlaceOfSupplyGroup.GRO_Code);

			AssertNull("OLAB should be in no Place of Supply Group", chargeCodeOLAB.PlaceOfSupplyGroup);
		}

		#endregion

		#region AccPlaceOfSupplyConfigurations

		public void TestAccPlaceOfSupplyConfigurations_IsNotNullAndCorrectLevel_WhenNormalChargeCode()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();

			AssertNotNull(chargeCode.PlaceOfSupplyConfigurations);
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, chargeCode.PlaceOfSupplyConfigurations.Level);
			AssertEquals(false, chargeCode.PlaceOfSupplyConfigurations.ReadOnly);

			var bizo = chargeCode.PlaceOfSupplyConfigurations.AddNew();
			AssertEquals(AccPOSConfigurationLevel.ChargeCode, bizo.Level);
		}

		public void TestAccPlaceOfSupplyConfigurations_IsNotNullAndNullLevel_WhenGlobalChargeCode()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = ZGuid.Empty;

			// Place of Supply is not supported for global charge codes. However, the collection should not be null.
			AssertNotNull(chargeCode.PlaceOfSupplyConfigurations);
			AssertEquals(AccPOSConfigurationLevel.Null, chargeCode.PlaceOfSupplyConfigurations.Level);
			AssertEquals("Place of Supply Configuration for Global Charge Codes should be read only as it is not supported", true, chargeCode.PlaceOfSupplyConfigurations.ReadOnly);
			chargeCode.PlaceOfSupplyConfigurations.Load();
			AssertEquals("Place of Supply Configuration for Global Charge Codes should be empty as it is not supported", 0, chargeCode.PlaceOfSupplyConfigurations.Count);

			AssertExceptionThrown<NotSupportedException>("Place of Supply Configuration for Global Charge Codes is not supported", () => chargeCode.PlaceOfSupplyConfigurations.AddNew());
		}

		#endregion

		public void TestUniversalCopyIgnoreElement()
		{
			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var componentType = accChargeCode.GetType();
			var ignoreElementAttributes = componentType.GetCustomAttributes(typeof(UniversalCopyIgnoreElementAttribute), true);
			var attribute = ignoreElementAttributes[0] as UniversalCopyIgnoreElementAttribute;
			AssertCollectionContains("AC_GC", AccChargeCode.Schema.AC_GC, attribute.ElementNames);
		}

		public void TestGetFallbackGovtChargeCode()
		{
			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyVoyageAccountingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: null
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Domestic, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: null,
				fallbackToDefaults: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Unknown, Constants.TransportModes.Sea),
				}
			);
		}

		public void TestGetFallbackGovtChargeCodes()
		{
			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Air),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyVoyageAccountingCode, Directions.Import, Constants.TransportModes.All),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				fallbackToDefaults: null
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Domestic, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyDetentionInvoiceCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.AirSea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode,Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode,Core.Constants.FreightShipmentDirection.Code.Export, Constants.TransportModes.AirSea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode,Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode,Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Export, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.All),
				},
				fallbackToDefaults: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
				}
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.Other, Constants.TransportModes.Sea),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.CFSShipmentCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.Sea),
				},
				equalExpecteds: new[] {
					(CostSell.Cost, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.CrossTrade, Constants.TransportModes.Sea),
				},
				notEqualExpecteds: new[] {
					(CostSell.Revenue, JobInvoicingConsumerTypes.CFSShipmentCode, Directions.Unknown, Constants.TransportModes.Sea),
				},
				fallbackToDefaults: null
			);

			AssertGetFallbackGovtChargeCode(
				expected: (AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
				settings: new[] {
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.All, JobInvoicingConsumerTypes.ShipmentCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Core.Constants.FreightShipmentDirection.Code.Import, Constants.TransportModes.All),
					(AccChargeGovtChargeCodeOverrideLookups.CostSellAllCodes.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Core.Constants.FreightShipmentDirection.Code.All, Constants.TransportModes.All)
				},
				equalExpecteds: new[]
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Import, Constants.TransportModes.Air),
				},
				notEqualExpecteds: new[]
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Import, Constants.TransportModes.Storage),
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Import, Constants.TransportModes.Air),
					(CostSell.Revenue, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Sea),
				},
				fallbackToDefaults: new[]
				{
					(CostSell.Cost, JobInvoicingConsumerTypes.AgencyBillOfLadingCode, Directions.Import, Constants.TransportModes.All),
					(CostSell.Cost, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Export, Constants.TransportModes.Air),
					(CostSell.Cost, JobInvoicingConsumerTypes.QuotedBookingCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ShipmentCode, Directions.Export, Constants.TransportModes.Sea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.ForwardingConsolCode, Directions.Import, Constants.TransportModes.AirSea),
					(CostSell.Revenue, JobInvoicingConsumerTypes.GatewayConsolCode, Directions.Import, Constants.TransportModes.Storage),
				}
			);
		}

		void AssertGetFallbackGovtChargeCode((ZString CostSellAll, ZString JobType, ZString ServiceDirectionCode, ZString TransportMode) expected,
			(string CostSellAll, string JobType, string ServiceDirection, string TransportMode)[] settings,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] equalExpecteds,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] notEqualExpecteds,
			(CostSell CostOrSell, string JobType, Directions ServiceDirection, string TransportMode)[] fallbackToDefaults)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GovtChargeCode = "defaultGovtCode";

			for (var i = 0; i < settings.Length; i++)
			{
				var overrideGovt = chargeCode.GovtChargeCodeOverrides.AddNew();
				overrideGovt.ACG_CostSellAll = settings[i].CostSellAll;
				overrideGovt.ACG_JobType = settings[i].JobType;
				overrideGovt.ACG_Direction = settings[i].ServiceDirection;
				overrideGovt.ACG_TransportMode = settings[i].TransportMode;
				overrideGovt.ACG_GovtChargeCode = $"1234_{i}";
			}

			var expectedGovt = chargeCode.GovtChargeCodeOverrides
				.Select(x => x)
				.FirstOrDefault(x =>
					x.ACG_JobType == expected.JobType
					&& x.ACG_CostSellAll == expected.CostSellAll
					&& x.ACG_Direction == expected.ServiceDirectionCode
					&& x.ACG_TransportMode == expected.TransportMode);

			AssertNotNull(expectedGovt);
			Assert(expectedGovt.PK.IsValid);
			expectedGovt.ACG_GovtChargeCode = "expectedGovtCode";

			Factory.Save();

			CombineAssertions(() =>
			{
				foreach (var equalExpected in equalExpecteds)
				{
					var fallbackResult = chargeCode.GetFallbackGovtChargeCode(
						ConfigurationMatcherHelper.GetParameters(
							equalExpected.CostOrSell,
							equalExpected.JobType,
							equalExpected.ServiceDirection,
							equalExpected.TransportMode,
							GlbBranch.CurrentBranch,
							null, null));
					AssertEquals(expectedGovt.ACG_GovtChargeCode, fallbackResult);
					AssertEquals("expectedGovtCode", fallbackResult);
				}
			});

			if (notEqualExpecteds != null)
			{
				Assert(notEqualExpecteds.Any());
				CombineAssertions(() =>
				{
					foreach (var notEqualExpected in notEqualExpecteds)
					{
						var fallbackResult = chargeCode.GetFallbackGovtChargeCode(
							ConfigurationMatcherHelper.GetParameters(
								notEqualExpected.CostOrSell,
								notEqualExpected.JobType,
								notEqualExpected.ServiceDirection,
								notEqualExpected.TransportMode,
								GlbBranch.CurrentBranch, null, null)
							);
						AssertNotEquals("expectedGovtCode", fallbackResult);
					}
				});
			}

			if (fallbackToDefaults != null)
			{
				Assert(fallbackToDefaults.Any());
				CombineAssertions(() =>
				{
					foreach (var fallbackToDefualt in fallbackToDefaults)
					{
						var fallbackResult = chargeCode.GetFallbackGovtChargeCode(
							ConfigurationMatcherHelper.GetParameters(
								fallbackToDefualt.CostOrSell,
								fallbackToDefualt.JobType,
								fallbackToDefualt.ServiceDirection,
								fallbackToDefualt.TransportMode,
								GlbBranch.CurrentBranch, null, null)
							);

						AssertEquals("defaultGovtCode", fallbackResult);
					}
				});
			}

			expectedGovt.Delete();
			chargeCode.Delete();
			Factory.Save();
		}

		public void TestShouldDefaultLocalChargeDescription_ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors()
		{
			var isLocalClient = false;
			var dynamicMock = new Mock<IAccounting>();
			dynamicMock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			dynamicMock.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(false);
			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			accChargeCode.AC_LocalLanguageDescription = "Test";

			using (ObjectFactory.Substitute(dynamicMock.Object))
			{
				AssertEquals("ShouldDefaultLocalChargeDescription is false.", false, accChargeCode.ShouldDefaultLocalChargeDescription(isLocalClient));
			}

			var dynamicMock2 = new Mock<IAccounting>();
			dynamicMock2.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			dynamicMock2.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(true);

			using (ObjectFactory.Substitute(dynamicMock2.Object))
			{
				AssertEquals("ShouldDefaultLocalChargeDescription is true.", true, accChargeCode.ShouldDefaultLocalChargeDescription(isLocalClient));
			}
		}

		public void TestGetDescriptionOrLocalLanguageDescription()
		{
			var isLocalClient = false;
			var dynamicMock = new Mock<IAccounting>();
			dynamicMock.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			dynamicMock.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(false);
			var accChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			accChargeCode.AC_LocalLanguageDescription = "Local Description";
			accChargeCode.AC_Desc = "Description";

			using (ObjectFactory.Substitute(dynamicMock.Object))
			{
				AssertEquals("Result of GetDescriptionOrLocalLanguageDescription is Description.", "Description", accChargeCode.GetDescriptionOrLocalLanguageDescription(isLocalClient));
			}

			var dynamicMock2 = new Mock<IAccounting>();
			dynamicMock2.Setup(m => m.EnableLocalChargeCodeDescriptionDefault).Returns(true);
			dynamicMock2.Setup(m => m.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors).Returns(true);

			using (ObjectFactory.Substitute(dynamicMock2.Object))
			{
				AssertEquals("Result of GetDescriptionOrLocalLanguageDescription is Local Description.", "Local Description", accChargeCode.GetDescriptionOrLocalLanguageDescription(isLocalClient));
			}
		}

		#region Test CanDelete

		public void TestChargeCodeCanBeDeleted()
		{
			var chargeCode = CreateAccChargeCode("T3T", Constants.ChargeType.Margin);

			Assert("This charge code should be deletable.", chargeCode.CanDelete);
		}

		public void TestChargeCodeCanNotBeDeleted_ForReason_CommentType()
		{
			var chargeCode = CreateAccChargeCode("T3T", Constants.ChargeType.Comment);

			Assert("This comment charge code T3T should not be deletable.", !chargeCode.CanDelete);
			AssertEquals("The comment charge code 'T3T' cannot be deleted", chargeCode.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		public void TestChargeCodeCanNotBeDeleted_ForReason_ElectronicProcessingChargeCode()
		{
			var globalChargeCode = CreateAccChargeCode("T3T", Constants.ChargeType.Disbursement, true);
			Factory.Save();

			var query = new ZQuery();
			query.AddToFilter(AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);
			query.AddToFilter(AccChargeCodeSchema.AC_Code, globalChargeCode.AC_Code);
			var localChargeCode = Factory.LoadTop1<AccChargeCode>(query);
			AssertNotNull(localChargeCode);

			Assert("This global charge code should be deletable.", globalChargeCode.CanDelete);
			Assert("This local charge code should be deletable.", localChargeCode.CanDelete);

			ObjectFactory.Get<IAccountingRegistryProvider>().ElectronicProcessingChargeCode = globalChargeCode.PK.ToGuid();

			Assert("This global charge code should not be deletable.", !globalChargeCode.CanDelete);
			Assert("This local charge code should not be deletable.", !localChargeCode.CanDelete);
			AssertEquals("The Charge Code 'T3T' is a system defined Charge Code used in Electronic Processing Fee management and cannot be deleted.",
				globalChargeCode.ReasonForNotAbleToDelete.GetUnresolvedString());
			AssertEquals("The Charge Code 'T3T' is a system defined Charge Code used in Electronic Processing Fee management and cannot be deleted.",
				localChargeCode.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		public void TestChargeCodeCanNotBeDeleted_ForReason_BeingReferenced()
		{
			var chargeCode = CreateAccChargeCode("T3T", Constants.ChargeType.Revenue);
			Factory.Save();

			Assert(chargeCode.CanDelete);

			var registryItem = AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode;
			registryItem.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, chargeCode.PK.ToGuid());
			string registryPath = $"• {registryItem.Category}{RegistryItemSet.Delimiter}{registryItem.Caption}";

			Assert("This registry-referenced charge code should not be deletable.", !chargeCode.CanDelete);
			AssertEquals($"Cannot delete charge code 'T3T' because it is being referenced by the following Registry items:\r\n\r\n{registryPath}",
				chargeCode.ReasonForNotAbleToDelete.GetUnresolvedString());
		}

		#endregion

		public AccChargeCode CreateAccChargeCode(string code, string chargeType, bool isGlobal = true)
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = code + " Desc";
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_GC = isGlobal ? ZGuid.Empty : GlbCompany.CurrentCompany.PK;
			return chargeCode;
		}

		#region Human Readable Name

		public void TestHumanReadableNameCore()
		{
			var charge = Factory.NewWithValidTestData<AccChargeCode>();
			charge.AC_Code = "TST";

			AssertEquals("Charge Code (TST)", charge.HumanReadableName);
		}

		#endregion

		#region Implementation

		AccChargeCode TestChargeCode;

		AccGLHeader TestProfitAndLossGLHeader;
		AccGLHeader TestBalanceSheetAccountGLHeader;

		protected override void SetUp()
		{
			base.SetUp();
			TestChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			TestChargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			TestChargeCode2.AC_Code = "HPY";
			TestChargeCode.AC_Code = "LME";

			if (!ObjectFactory.HasBeenSubstituted<IAuthTokenProvider>())
			{
				var authTokenProvider = new Mock<IAuthTokenProvider>();
				authTokenProvider
					.Setup(m => m.GetToken(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<Action<LoginInfo>>(),
						It.IsAny<CancellationToken>(), It.IsAny<bool>())).Returns(("some token", string.Empty));

				ObjectFactory.Substitute(authTokenProvider.Object);
			}
		}

		protected override void TearDown()
		{
			ObjectFactory.DisposeSubstitutions();
			base.TearDown();
		}

		void SetUpAccountCollectionTests()
		{
			TestProfitAndLossGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			TestProfitAndLossGLHeader.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			TestProfitAndLossGLHeader.AG_ControlAccount = false;
			TestProfitAndLossGLHeader.AG_AccountNum = "123";

			TestBalanceSheetAccountGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			TestBalanceSheetAccountGLHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			TestBalanceSheetAccountGLHeader.AG_ControlAccount = false;
			TestBalanceSheetAccountGLHeader.AG_AccountNum = "456";

			Factory.Save();
		}

		public AccChargeCode TestChargeCode2;

		void ChargeTypeAssertionsForChargeCodeWithNoMatchingTransactionLines(string chargeCodeType)
		{
			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Margin;
			Assert("Allowed to change to MRG from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert("Not allowed to change to CMT from " + chargeCodeType, TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert("Allowed to change to DSB from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			Assert("Allowed to change to OVR from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			Assert("Allowed to change to NON from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());

			ResetChargeTypeTo(chargeCodeType);
			TestChargeCode2.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert("Allowed to change to REV from " + chargeCodeType, !TestChargeCode2.AC_ChargeTypeInfo.HasErrors());
		}

		void ResetChargeTypeTo(string chargeCodeType)
		{
			TestChargeCode2.AC_ChargeType = chargeCodeType;
			Factory.Save();
		}

		public static void SetupGlobalChargeCodeScenario(BusinessObjectFactory factory, out AccChargeCode normalChargeCode, out AccChargeCode normalChargeCodeLinked, out AccChargeCode globalChargeCode, string chargeType = "")
		{
			SetupGlobalChargeCodeScenario(factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode, true, true, "CC1", "CC2", chargeType);
		}

		public static void SetupGlobalChargeCodeScenario(BusinessObjectFactory factory, out AccChargeCode normalChargeCode, out AccChargeCode normalChargeCodeLinked, out AccChargeCode globalChargeCode,
			bool doSave, bool createNormalChargeCode, string normalChargeCode_ACCode, string globalChargeCode_ACCode, string chargeType = "")
		{
			EnsureAllGSTRegisteredCompaniesHaveRatedGST(factory);

			if (createNormalChargeCode)
			{
				normalChargeCode = factory.NewWithValidTestData<AccChargeCode>();

				normalChargeCode.AC_GC = Env.CurrentCompany.PK;
				normalChargeCode.AC_Code = normalChargeCode_ACCode;
				normalChargeCode.AC_AG_CostClearingAccount = AccGLHeaderPK(factory, GLHeader_CostClearing);
				normalChargeCode.AC_AG_RevenueClearingAccount = AccGLHeaderPK(factory, GLHeader_RevenueClearing);
				normalChargeCode.AC_ChargeType = string.IsNullOrWhiteSpace(chargeType) ? Constants.ChargeType.Margin : chargeType;
				normalChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(factory, GLHeader_Cost);
				normalChargeCode.AC_AG_CostAccount = AccGLHeaderPK(factory, GLHeader_Cost);
				normalChargeCode.AC_AG_RevenueAccount = AccGLHeaderPK(factory, GLHeader_Revenue);
				normalChargeCode.AC_AG_WIPAccount = AccGLHeaderPK(factory, GLHeader_Revenue);
				normalChargeCode.AC_AG_DisbursementSurplusAccount = chargeType == Constants.ChargeType.Disbursement ? AccGLHeaderPK(factory, GLHeader_Revenue) : ZGuid.Empty;
				normalChargeCode.AC_AG_DisbursementShortfallAccount = chargeType == Constants.ChargeType.Disbursement ? AccGLHeaderPK(factory, GLHeader_Revenue) : ZGuid.Empty;
				normalChargeCode.AC_ChargeGroup = normalChargeCode.Lookups.ChargeGroupList[0].Code;
				normalChargeCode.AC_Desc = normalChargeCode_ACCode + " Desc";
				var taxRate = AccTaxRate.GetNOTREPORTTaxID(factory, GlbCompany.CurrentCompany);
				normalChargeCode.AC_AT_GSTRate = taxRate != null ? taxRate.PK : ZGuid.Empty;
			}
			else
			{
				normalChargeCode = null;
			}

			factory.Save();

			globalChargeCode = factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = globalChargeCode_ACCode;
			globalChargeCode.AC_AG_CostClearingAccount = AccGLHeaderPK(factory, GLHeader_CostClearing);
			globalChargeCode.AC_AG_RevenueClearingAccount = AccGLHeaderPK(factory, GLHeader_RevenueClearing);
			globalChargeCode.AC_ChargeType = string.IsNullOrWhiteSpace(chargeType) ? Constants.ChargeType.Margin : chargeType;
			globalChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(factory, GLHeader_Cost);
			globalChargeCode.AC_AG_CostAccount = AccGLHeaderPK(factory, GLHeader_Cost);
			globalChargeCode.AC_AG_RevenueAccount = AccGLHeaderPK(factory, GLHeader_Revenue);
			globalChargeCode.AC_AG_WIPAccount = AccGLHeaderPK(factory, GLHeader_Revenue);
			globalChargeCode.AC_AG_DisbursementSurplusAccount = chargeType == Constants.ChargeType.Disbursement ? AccGLHeaderPK(factory, GLHeader_Revenue) : ZGuid.Empty;
			globalChargeCode.AC_AG_DisbursementShortfallAccount = chargeType == Constants.ChargeType.Disbursement ? AccGLHeaderPK(factory, GLHeader_Revenue) : ZGuid.Empty;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_ChargeSubGroup = globalChargeCode.Lookups.ChargeSubGroupList[0].Code;
			globalChargeCode.AC_Desc = globalChargeCode_ACCode + " Desc";

			if (doSave)
			{
				factory.Save();
				normalChargeCodeLinked = globalChargeCode.ChildChargeCodes.FirstOrDefault(c => c.AC_GC == Env.CurrentCompany.PK);
			}
			else
			{
				normalChargeCodeLinked = null;
			}
		}

		AccChargeCode SetupGlobalChargeCodeWithAllFieldsAndChildFieldsSet()
		{
			EnsureAllGSTRegisteredCompaniesHaveRatedGST(Factory);
			Factory.Save();

			var globalChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			globalChargeCode.AC_GC = ZGuid.Empty;
			globalChargeCode.AC_Code = "CC2";
			globalChargeCode.AC_AG_AccrualAccount = AccGLHeaderPK(Factory, GLHeader_Cost);
			globalChargeCode.AC_AG_CostAccount = AccGLHeaderPK(Factory, GLHeader_Cost);
			globalChargeCode.AC_AG_RevenueAccount = AccGLHeaderPK(Factory, GLHeader_Revenue);
			globalChargeCode.AC_AG_CostClearingAccount = AccGLHeaderPK(Factory, GLHeader_Cost);
			globalChargeCode.AC_AG_RevenueClearingAccount = AccGLHeaderPK(Factory, GLHeader_Revenue);
			globalChargeCode.AC_AG_WIPAccount = AccGLHeaderPK(Factory, GLHeader_Revenue);
			globalChargeCode.AC_AllowDescriptionOvertype = ZBool.True;
			globalChargeCode.AC_AR_ExpenseGroup = Factory.NewWithValidTestData<AccGroups>().PK;
			globalChargeCode.AC_AR_SalesGroup = Factory.NewWithValidTestData<AccGroups>().PK;
			globalChargeCode.AC_AT_GSTRate = ZGuid.Empty;
			globalChargeCode.AC_AW_WithholdingTaxRate = ZGuid.Empty;
			globalChargeCode.AC_AX_TaxOverrideGroup = ZGuid.Empty;
			globalChargeCode.AC_ChargeGroup = globalChargeCode.Lookups.ChargeGroupList[0].Code;
			globalChargeCode.AC_ChargeOtherGroups = ChargeOtherGroupsList.Codes.Principal; // Shown as "Principal / Agent" on form
			globalChargeCode.AC_ChargeSubGroup = globalChargeCode.Lookups.ChargeSubGroupList[0].Code;
			globalChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			globalChargeCode.AC_DepartmentFilterList = "ALL";
			globalChargeCode.AC_Desc = "Aaa Bee See One";
			globalChargeCode.AC_ENettChargeCodeMap = "Whatever";
			globalChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			globalChargeCode.AC_IATA_ChargeCodeMap = globalChargeCode.Lookups.AC_IATACode_List[0].Code;
			globalChargeCode.AC_IsActive = true;
			globalChargeCode.AC_IsCommissionable = ZBool.True;
			globalChargeCode.AC_IsGroupageCharge = ZBool.True;
			globalChargeCode.AC_LocalLanguageDescription = "Aaa Bee See Uno";
			globalChargeCode.AC_MarginPercentage = 66M;
			globalChargeCode.AC_PrintSequence = 4;
			globalChargeCode.AC_RateCalculator = globalChargeCode.Lookups.AC_RateCalculator_List[0].Code;
			globalChargeCode.AC_ShowOnQuotation = ZBool.True;
			globalChargeCode.AC_SuppressOnQuoteIfZero = ZBool.True;
			globalChargeCode.AC_AC_RevenueChargeCode = Factory.NewWithValidTestData<AccChargeCode>().PK;

			var typeOverride = Factory.New<AccChargeTypeOverride>();
			typeOverride.AN_AC_ChargeCode = globalChargeCode.PK;
			typeOverride.AN_JobDirection = typeOverride.Lookups.DirectionList[0].Code;
			typeOverride.AN_JobType = typeOverride.Lookups.JobTypes[0].Code;
			typeOverride.AN_ChargeType = typeOverride.Lookups.AC_ChargeType_List[0].Code;
			typeOverride.AN_InvoiceType = typeOverride.Lookups.InvoiceTypes[0].Code;
			typeOverride.AN_MarginPercentage = 50;

			var revenueRecognitionOverride = Factory.New<AccChargeRevRecOverride>();
			revenueRecognitionOverride.AE_AC = globalChargeCode.PK;
			revenueRecognitionOverride.AE_JobType = revenueRecognitionOverride.JobTypeList[0].Code;
			revenueRecognitionOverride.AE_Direction = revenueRecognitionOverride.DirectionList[0].Code;
			revenueRecognitionOverride.AE_Mode = revenueRecognitionOverride.ModeList[0].Code;
			revenueRecognitionOverride.AE_BrokerType = revenueRecognitionOverride.BrokerList[0].Code;
			revenueRecognitionOverride.AE_RecognitionType = revenueRecognitionOverride.RecognitionDateOptionList[0].Code;

			var supplyTypeOverride = Factory.New<AccChargeSupplyTypeOverride>();
			supplyTypeOverride.ACS_ParentID = globalChargeCode.PK;
			supplyTypeOverride.ACS_ParentTableCode = "AC";
			supplyTypeOverride.ACS_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			supplyTypeOverride.ACS_Direction = Constants.FreightShipmentDirection.Code.Export;
			supplyTypeOverride.ACS_TransportMode = Constants.TransportModes.Air;
			supplyTypeOverride.ACS_IncoTerm = Constants.IncoTerms.CarriagePaidTo;
			supplyTypeOverride.ACS_GE = GlbDepartment.CurrentDepartment.PK;
			supplyTypeOverride.ACS_SupplyType = SupplyTypeClassificationCodes.LOA;

			var glPostingOverride = Factory.New<AccChargeGLPostingOverride>();
			glPostingOverride.Y1_AC = globalChargeCode.PK;
			glPostingOverride.Y1_GE = Factory.NewWithValidTestData<GlbDepartment>().PK;
			glPostingOverride.Y1_AG_ACR = AccGLHeaderPK(Factory, GLHeader_Cost);
			glPostingOverride.Y1_AG_CST = AccGLHeaderPK(Factory, GLHeader_Cost);
			glPostingOverride.Y1_AG_REV = AccGLHeaderPK(Factory, GLHeader_Revenue);
			glPostingOverride.Y1_AG_WIP = AccGLHeaderPK(Factory, GLHeader_Revenue);
			glPostingOverride.Y1_JobType = JobInvoicingConsumerTypes.ShipmentCode;
			glPostingOverride.Y1_TransportMode = Core.Constants.TransportModes.Air;
			glPostingOverride.Y1_Direction = Constants.FreightShipmentDirection.Code.Export;
			glPostingOverride.Y1_ConsolContainerMode = Core.Constants.ContainerModes.LCL;
			glPostingOverride.Y1_MasterPaymentType = Core.Constants.PaymentType.Collect;
			glPostingOverride.Y1_HousePaymentType = Core.Constants.PaymentType.Collect;

			return globalChargeCode;
		}

		GlbCompany[] allCompaniesExceptDemo;

		GlbCompany[] AllCompaniesExceptDemo
		{
			get
			{
				if (allCompaniesExceptDemo == null)
				{
					allCompaniesExceptDemo = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, GlbCompany.DemoCompanyCode));
				}
				return allCompaniesExceptDemo;
			}
		}

		#endregion
	}
}
