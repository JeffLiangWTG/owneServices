using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccChargeTypeOverrideValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMarginPercentage()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeTypeOverride @override = chargeCode.ChargeTypeOverrides.AddNew();

			@override.AN_ChargeType = "MRG";
			@override.AN_MarginPercentage = -5m;
			AssertHasErrors(@override.AN_MarginPercentageInfo);
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_MarginPercentageInfo));
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert(HasInvalidDataForCMTChargeTypeError(@override, @override.AN_MarginPercentageInfo));
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_MarginPercentageInfo));

			@override.AN_MarginPercentage = 20m;
			AssertNoErrors(@override.AN_MarginPercentageInfo);

			@override.AN_MarginPercentage = 200m;
			AssertHasErrors(@override.AN_MarginPercentageInfo);

			@override.AN_ChargeType = "REV";
			@override.AN_MarginPercentage = 20m;
			AssertHasErrors(@override.AN_MarginPercentageInfo);

			@override.AN_MarginPercentage = 0m;
			AssertNoErrors(@override.AN_MarginPercentageInfo);

			@override.AN_ChargeType = "DSB";
			@override.AN_MarginPercentage = 100m;
			AssertNoErrors(@override.AN_MarginPercentageInfo);
		}

		public void TestInvoiceType()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeTypeOverride @override = chargeCode.ChargeTypeOverrides.AddNew();

			@override.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			@override.AN_InvoiceType = "XXX";
			AssertHasErrors(@override.AN_InvoiceTypeInfo);

			@override.AN_InvoiceType = Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.FinalInvoice;
			AssertNoErrors(@override.AN_InvoiceTypeInfo);

			@override.AN_InvoiceType = "";
			AssertHasErrors(@override.AN_InvoiceTypeInfo);
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_InvoiceTypeInfo));
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_InvoiceTypeInfo));
			@override.AN_InvoiceType = "XXX";
			Assert(HasInvalidDataForCMTChargeTypeError(@override, @override.AN_InvoiceTypeInfo));
		}

		public void TestChargeType()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeTypeOverride @override = chargeCode.ChargeTypeOverrides.AddNew();

			@override.AN_ChargeType = "XXX";
			AssertHasErrors(@override.AN_ChargeTypeInfo);

			@override.AN_ChargeType = "REV";
			AssertNoErrors(@override.AN_ChargeTypeInfo);

			@override.AN_ChargeType = "";
			AssertHasErrors(@override.AN_ChargeTypeInfo);
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_ChargeTypeInfo));
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_ChargeTypeInfo));
			@override.AN_ChargeType = "XXX";
			Assert(HasInvalidDataForCMTChargeTypeError(@override, @override.AN_ChargeTypeInfo));

			string mrgDsbMJAError = "The default charge type should be more comprehensive than the override please change your default to either: MRG, DSB, MJA";
			string revError = mrgDsbMJAError + ", REV";

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			AssertNoErrors(@override.AN_ChargeTypeInfo);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			AssertNoErrors(@override.AN_ChargeTypeInfo);

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Assert(!@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Revenue;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			AssertNoErrors(@override.AN_ChargeTypeInfo);
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.NonAccrual;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert(@override.AN_ChargeTypeInfo.HasError(revError));
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Overhead;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert(@override.AN_ChargeTypeInfo.HasError(revError));
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));

			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			@override.AN_ChargeType = Core.Constants.ChargeType.Margin;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Disbursement;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
			@override.AN_ChargeType = Core.Constants.ChargeType.Revenue;
			Assert(@override.AN_ChargeTypeInfo.HasError(revError));
			@override.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;
			Assert(@override.AN_ChargeTypeInfo.HasError(mrgDsbMJAError));
		}

		public void TestJobType()
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			AccChargeTypeOverride @override = chargeCode.ChargeTypeOverrides.AddNew();

			@override.AN_JobType = "XXX";
			AssertHasErrors(@override.AN_JobTypeInfo);

			@override.AN_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			AssertNoErrors(@override.AN_JobTypeInfo);

			@override.AN_JobType = "";
			AssertHasErrors(@override.AN_JobTypeInfo);
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_JobTypeInfo));
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Comment;
			Assert(!HasInvalidDataForCMTChargeTypeError(@override, @override.AN_JobTypeInfo));
			@override.AN_JobType = "XXX";
			Assert(HasInvalidDataForCMTChargeTypeError(@override, @override.AN_JobTypeInfo));
		}

		public void TestChargeTypeForDsbCharge()
		{
			var mock = new Mock<IAccounting>();
			ObjectFactory.Substitute(mock.Object);
			mock.SetupGet(x => x.EnableBulkDisbursementJobsClosure).Returns(true);

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			var chargeTypeOverride = chargeCode.ChargeTypeOverrides.AddNew();
			chargeTypeOverride.AN_ChargeType = Core.Constants.ChargeType.Margin;

			chargeCode.AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
			chargeCode.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;

			chargeTypeOverride.Validation.ValidateAN_ChargeType();
			AssertNoErrors("DSB charge without surplus/shortfall GL account configuration", chargeTypeOverride.AN_ChargeTypeInfo);

			var glAccount = Factory.New<AccGLHeader>();
			glAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			chargeCode.AC_AG_DisbursementSurplusAccount = chargeCode.AC_AG_DisbursementShortfallAccount = glAccount.PK;

			chargeTypeOverride.Validation.ValidateAN_ChargeType();
			AssertHasError("DSB charge with proper GL account configuration and feature enabled", chargeTypeOverride.AN_ChargeTypeInfo, "Charge Type Override is not supported if the default charge type is 'DSB' and DSB Surplus/Shortfall Accounts have been specified.");

			mock.SetupGet(x => x.EnableBulkDisbursementJobsClosure).Returns(false);

			chargeTypeOverride.Validation.ValidateAN_ChargeType();
			AssertNoErrors("DSB job close feature is disabled", chargeTypeOverride.AN_ChargeTypeInfo);

			mock.SetupGet(x => x.EnableBulkDisbursementJobsClosure).Returns(true);
			chargeCode.AC_ChargeType = Core.Constants.ChargeType.Margin;
			chargeCode.AC_AG_DisbursementSurplusAccount = chargeCode.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
			chargeTypeOverride.AN_ChargeType = Core.Constants.ChargeType.ManualJobAccrual;

			chargeTypeOverride.Validation.ValidateAN_ChargeType();
			AssertNoErrors("Non DSB charge", chargeTypeOverride.AN_ChargeTypeInfo);
		}

		#region CheckAN_AC_ChargeCode

		public void TestNoErrorWhenAccChargeCodeIsInactive()
		{
			var inactiveAccChargeCode = Factory.New<AccChargeCode>();
			inactiveAccChargeCode.AC_IsActive = false;

			var typeOverride = inactiveAccChargeCode.ChargeTypeOverrides.AddNew();

			AssertNoErrors(typeOverride.AN_AC_ChargeCodeInfo);
		}

		#endregion

		#region Implementation

		bool HasInvalidDataForCMTChargeTypeError(AccChargeTypeOverride @override, ZPropertyInfo infoToCheck)
		{
			@override.Validation.ValidateAll();
			return infoToCheck.HasError("This data is invalid for default charge type 'CMT'");
		}

		#endregion
	}
}
