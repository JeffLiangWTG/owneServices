using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeGLPostingOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestY1_AG_ACRIsOnlyValidatedWhenRequired()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			AssertValidatedOnlyWhenRequired(glPostingOverride.Y1_AG_ACRInfo, glPostingOverride.Validation.ValidateY1_AG_ACR);
		}

		public void TestY1_AG_REVIsOnlyValidatedWhenRequired()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			AssertValidatedOnlyWhenRequired(glPostingOverride.Y1_AG_REVInfo, glPostingOverride.Validation.ValidateY1_AG_REV);
		}

		public void TestY1_AG_CSTIsOnlyValidatedWhenRequired()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			AssertValidatedOnlyWhenRequired(glPostingOverride.Y1_AG_CSTInfo, glPostingOverride.Validation.ValidateY1_AG_CST);
		}

		public void TestY1_AG_WIPIsOnlyValidatedWhenRequired()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			AssertValidatedOnlyWhenRequired(glPostingOverride.Y1_AG_WIPInfo, glPostingOverride.Validation.ValidateY1_AG_WIP);
		}

		public void TestY1_GEIsUnique()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.Y1_GE = GlbDepartment.CurrentDepartment.PK;

			AccChargeGLPostingOverride glPostingOverride2 = glPostingOverride.ChargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_GE = GlbDepartment.CurrentDepartment.PK;

			AssertHasError(glPostingOverride2.Y1_GEInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_AG_ACR()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			AssertMessageWhenWrongAccountSelected(glPostingOverride.Y1_AG_ACRInfo, glPostingOverride.Validation.ValidateY1_AG_ACR);
		}

		public void TestInactiveGLAccountsCreatesValidationError()
		{
			AccGLHeader activelGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			activelGLHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			activelGLHeader.AG_ControlAccount = false;
			activelGLHeader.AG_IsGlobal = true;

			AccGLHeader inActiveGlobalGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			inActiveGlobalGLHeader.AG_IsActive = false;
			inActiveGlobalGLHeader.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			inActiveGlobalGLHeader.AG_ControlAccount = false;
			inActiveGlobalGLHeader.AG_IsGlobal = true;

			Factory.Save();

			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			glPostingOverride.ChargeCode.AC_GC = ZGuid.Empty;

			glPostingOverride.Y1_AG_ACR = glPostingOverride.Y1_AG_CST = glPostingOverride.Y1_AG_WIP = glPostingOverride.Y1_AG_REV = inActiveGlobalGLHeader.PK;
			AssertHasError(glPostingOverride.Y1_AG_REVInfo, "This Revenue GL Account is inactive - it may not be used.");
			AssertHasError(glPostingOverride.Y1_AG_CSTInfo, "This Cost GL Account is inactive - it may not be used.");
			AssertHasError(glPostingOverride.Y1_AG_WIPInfo, "This WIP GL Account is inactive - it may not be used.");
			AssertHasError(glPostingOverride.Y1_AG_ACRInfo, "This Accrual GL Account is inactive - it may not be used.");

			glPostingOverride.Y1_AG_ACR = glPostingOverride.Y1_AG_CST = glPostingOverride.Y1_AG_WIP = glPostingOverride.Y1_AG_REV = activelGLHeader.PK;
			AssertNoErrors(glPostingOverride.Y1_AG_REVInfo);
			AssertNoErrors(glPostingOverride.Y1_AG_CSTInfo);
			AssertNoErrors(glPostingOverride.Y1_AG_WIPInfo);
			AssertNoErrors(glPostingOverride.Y1_AG_ACRInfo);
		}

		public void TestCheckY1_AG_REV()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			AssertMessageWhenWrongAccountSelected(glPostingOverride.Y1_AG_REVInfo, glPostingOverride.Validation.ValidateY1_AG_REV);
		}

		public void TestCheckY1_AG_CST()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			AssertMessageWhenWrongAccountSelected(glPostingOverride.Y1_AG_CSTInfo, glPostingOverride.Validation.ValidateY1_AG_CST);
		}

		public void TestCheckY1_AG_WIP()
		{
			AccChargeGLPostingOverride glPostingOverride = GetNewBusinessObjectToTest();
			glPostingOverride.ChargeCode.AC_ChargeType = Constants.ChargeType.Margin;
			AssertMessageWhenWrongAccountSelected(glPostingOverride.Y1_AG_WIPInfo, glPostingOverride.Validation.ValidateY1_AG_WIP);
		}

		public void TestCheckY1_ConsolidationAccountingCategoryClass()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride1 = chargeCode.GLPostingOverrides.AddNew();

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ZString.Empty;
			chargeCode.RunPreSaveValidation();
			AssertHasError(glPostingOverride1.Y1_ConsolidationAccountingCategoryClassInfo, "Please enter a Consolidated Accounting Category Class.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			chargeCode.RunPreSaveValidation();
			AssertHasError(glPostingOverride1.Y1_GEInfo, "A department must be specified if the 'Class' value is 'ALL' or 'TPY.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.ThirdParty;
			chargeCode.RunPreSaveValidation();
			AssertHasError(glPostingOverride1.Y1_GEInfo, "A department must be specified if the 'Class' value is 'ALL' or 'TPY.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			chargeCode.RunPreSaveValidation();
			AssertNoError(glPostingOverride1.Y1_GEInfo, "A department must be specified if the 'Class' value is 'ALL' or 'TPY.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			glPostingOverride1.Y1_GE = GlbDepartment.CurrentDepartment.PK;

			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			glPostingOverride2.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			chargeCode.RunPreSaveValidation();
			AssertHasError(glPostingOverride1.Y1_GEInfo, "At least one more record already sets a behavior for the same Job parameters.");
			AssertHasError(glPostingOverride2.Y1_GEInfo, "At least one more record already sets a behavior for the same Job parameters.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride1.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			glPostingOverride2.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.Intercompany;
			glPostingOverride2.Y1_GE = GlbDepartment.CurrentDepartment.PK;
			chargeCode.RunPreSaveValidation();
			AssertHasError(glPostingOverride1.Y1_GEInfo, "There must be at least one row of GL Posting Override without department specified for 'INT' class.");
			AssertHasError(glPostingOverride2.Y1_GEInfo, "There must be at least one row of GL Posting Override without department specified for 'INT' class.");

			glPostingOverride2.Y1_GE = ZGuid.Empty;
			chargeCode.RunPreSaveValidation();
			AssertNoError(glPostingOverride1.Y1_GEInfo, "There must be at least one row of GL Posting Override without department specified for 'INT' class.");
			AssertNoError(glPostingOverride2.Y1_GEInfo, "There must be at least one row of GL Posting Override without department specified for 'INT' class.");

			glPostingOverride1.Y1_ConsolidationAccountingCategoryClass = "XXX";
			AssertHasError(glPostingOverride1.Y1_ConsolidationAccountingCategoryClassInfo, "Enter a valid Consolidated Accounting Category Class.");
		}

		public void TestCheckY1_JobType()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride1 = chargeCode.GLPostingOverrides.AddNew();

			glPostingOverride1.Y1_JobType = ZString.Empty;

			AssertHasError(glPostingOverride1.Y1_JobTypeInfo, "Please enter a Job Type.");

			glPostingOverride1.Y1_JobType = "XXX";

			AssertHasError(glPostingOverride1.Y1_JobTypeInfo, "Enter a valid Job Type.");

			glPostingOverride1.Y1_JobType = "SHP";
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";

			AssertHasError(glPostingOverride2.Y1_JobTypeInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_TransportMode()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_JobType = "SHP";

			glPostingOverride.Y1_TransportMode = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Please enter a Transport Mode.");

			glPostingOverride.Y1_TransportMode = "XXX";

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Enter a valid Transport Mode.");

			glPostingOverride.Y1_JobType = JobInvoicingConsumerTypes.CFSShipmentCode;
			glPostingOverride.Y1_TransportMode = Core.Constants.TransportModes.SeaAir;

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type.");

			glPostingOverride.Y1_JobType = JobInvoicingConsumerTypes.CFSLoadListCode;
			glPostingOverride.Y1_TransportMode = Core.Constants.TransportModes.AirSea;

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type.");

			glPostingOverride.Y1_TransportMode = Core.Constants.TransportModes.Air;
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";
			glPostingOverride2.Y1_JobType = JobInvoicingConsumerTypes.CFSLoadListCode;
			glPostingOverride2.Y1_TransportMode = Core.Constants.TransportModes.Air;

			AssertHasError(glPostingOverride2.Y1_TransportModeInfo, "At least one more record already sets a behavior for the same Job parameters.");

			glPostingOverride.Y1_JobType = "BRK";
			glPostingOverride.Y1_TransportMode = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Please enter a Transport Mode.");

			glPostingOverride.Y1_TransportMode = "XXX";

			AssertHasError(glPostingOverride.Y1_TransportModeInfo, "Enter a valid Transport Mode.");

			glPostingOverride.Y1_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "BRK";
			glPostingOverride2.Y1_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

			AssertHasError(glPostingOverride2.Y1_TransportModeInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_Direction()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_JobType = "SHP";

			glPostingOverride.Y1_Direction = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_DirectionInfo, "Please enter a Direction.");

			glPostingOverride.Y1_Direction = "XXX";

			AssertHasError(glPostingOverride.Y1_DirectionInfo, "Enter a valid Direction.");

			glPostingOverride.Y1_Direction = "EXP";
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";
			glPostingOverride2.Y1_Direction = "EXP";

			AssertHasError(glPostingOverride2.Y1_DirectionInfo, "At least one more record already sets a behavior for the same Job parameters.");

			glPostingOverride.Y1_JobType = "BRK";
			glPostingOverride.Y1_Direction = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_DirectionInfo, "Please enter a Direction.");

			glPostingOverride.Y1_Direction = "XXX";

			AssertHasError(glPostingOverride.Y1_DirectionInfo, "Enter a valid Direction.");

			glPostingOverride.Y1_Direction = "EXP";
			glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "BRK";
			glPostingOverride2.Y1_Direction = "EXP";

			AssertHasError(glPostingOverride2.Y1_DirectionInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_ConsolContainerMode()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_JobType = "SHP";

			glPostingOverride.Y1_ConsolContainerMode = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_ConsolContainerModeInfo, "Please enter a Consol Container Mode.");

			glPostingOverride.Y1_ConsolContainerMode = "XXX";

			AssertHasError(glPostingOverride.Y1_ConsolContainerModeInfo, "Enter a valid Consol Container Mode.");

			glPostingOverride.Y1_ConsolContainerMode = "LCL";
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";
			glPostingOverride2.Y1_ConsolContainerMode = "LCL";

			AssertHasError(glPostingOverride2.Y1_ConsolContainerModeInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_MasterPaymentType()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_JobType = "SHP";

			glPostingOverride.Y1_MasterPaymentType = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_MasterPaymentTypeInfo, "Please enter a Master Payment Term.");

			glPostingOverride.Y1_MasterPaymentType = "XXX";

			AssertHasError(glPostingOverride.Y1_MasterPaymentTypeInfo, "Enter a valid Master Payment Term.");

			glPostingOverride.Y1_MasterPaymentType = "CCX";
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";
			glPostingOverride2.Y1_MasterPaymentType = "CCX";

			AssertHasError(glPostingOverride2.Y1_MasterPaymentTypeInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		public void TestCheckY1_HousePaymentType()
		{
			var chargeCode = Factory.New<AccChargeCode>();
			var glPostingOverride = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride.Y1_JobType = "SHP";

			glPostingOverride.Y1_HousePaymentType = ZString.Empty;

			AssertHasError(glPostingOverride.Y1_HousePaymentTypeInfo, "Please enter a House Payment Term.");

			glPostingOverride.Y1_HousePaymentType = "XXX";

			AssertHasError(glPostingOverride.Y1_HousePaymentTypeInfo, "Enter a valid House Payment Term.");

			glPostingOverride.Y1_HousePaymentType = "CCX";
			var glPostingOverride2 = chargeCode.GLPostingOverrides.AddNew();
			glPostingOverride2.Y1_JobType = "SHP";
			glPostingOverride2.Y1_HousePaymentType = "CCX";

			AssertHasError(glPostingOverride2.Y1_HousePaymentTypeInfo, "At least one more record already sets a behavior for the same Job parameters.");
		}

		#region CheckY1_AC

		public void TestNoErrorWhenAccChargeCodeIsInactive()
		{
			var inactiveAccChargeCode = Factory.New<AccChargeCode>();
			inactiveAccChargeCode.AC_IsActive = false;

			var glPostingOverride = inactiveAccChargeCode.GLPostingOverrides.AddNew();

			AssertNoErrors(glPostingOverride.Y1_ACInfo);
		}

		#endregion

		#region Implementation

		AccChargeGLPostingOverride GetNewBusinessObjectToTest()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			AccChargeGLPostingOverride result = chargeCode.GLPostingOverrides.AddNew();

			return result;
		}

		void AssertValidatedOnlyWhenRequired(ZPropertyInfo propertyInfo, AnonymousMethod validate)
		{
			AccChargeCode chargeCode = ((AccChargeGLPostingOverride)propertyInfo.BizObj).ChargeCode;

			foreach (string chargeType in new[] { Constants.ChargeType.Margin, Constants.ChargeType.Comment, Constants.ChargeType.ManualJobAccrual })
			{
				foreach (bool isFilled in new[] { false, true })
				{
					bool required = (chargeType == Constants.ChargeType.Margin) || (chargeType == Constants.ChargeType.ManualJobAccrual);
					AccChargeCode.PropertyRequired requiredProperties = chargeCode.RequiredProperties(chargeType);
					AssertEquals("Prerequisite: AccrualAccount is required", required, requiredProperties.AccrualAccount);
					AssertEquals("Prerequisite: CostAccount is required", required, requiredProperties.CostAccount);
					AssertEquals("Prerequisite: RevenueAccount is required", required, requiredProperties.RevenueAccount);
					AssertEquals("Prerequisite: WIPAccount is required", required, requiredProperties.WIPAccount);

					chargeCode.AC_ChargeType = chargeType;

					if (isFilled)
					{
						propertyInfo.SetValueFromString("1010.10.10");
					}
					else
					{
						propertyInfo.ClearValue();
					}

					validate();

					if (required)
					{
						AssertHasErrorContaining(propertyInfo, MandatoryValidation.MustBeEntered);
						AssertHasErrorContaining(propertyInfo, propertyInfo.HumanReadableName);
					}
					else
					{
						AssertNoErrors(propertyInfo);
					}
				}
			}
		}

		void AssertMessageWhenWrongAccountSelected(ZPropertyInfo propertyInfo, AnonymousMethod validate)
		{
			propertyInfo.Value = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "1000.00.00")).PK;
			validate();
			AssertHasError(propertyInfo, @"Please choose a different account.
Only P&L or non-control BSH accounts can be selected.");
		}

		#endregion
	}
}
