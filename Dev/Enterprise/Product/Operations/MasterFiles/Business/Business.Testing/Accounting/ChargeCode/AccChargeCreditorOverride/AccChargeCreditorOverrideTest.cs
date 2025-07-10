using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccChargeCreditorOverride))]
	sealed class AccChargeCreditorOverrideTest : EnterpriseBusinessObjectTestCase
	{
		#region JobType

		public void TestACCJobType_ShouldSetHasError_WhenSetToNull()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = null;
			AssertEquals(true, chargecreditor.ACC_JobTypeInfo.HasErrors());
		}

		public void TestACCJobType_ShouldSetHasError_WhenSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(true, chargecreditor.ACC_JobTypeInfo.HasErrors());
		}

		public void TestACCJobType_ShouldSetHasError_WhenSetToInvalidJobType()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "WWW";
			AssertEquals(true, chargecreditor.ACC_JobTypeInfo.HasErrors());
		}

		public void TestACCJobType_ShouldNotSetHasError_When_SetToValidJobType()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals(false, chargecreditor.ACC_JobTypeInfo.HasErrors());
		}

		public void TestAccJobType_ShouldValidateDirection_WhenJobTypeSetToDirectionSupportedJob()
		{
			var chargeCreditor = ChargeCreditor;
			chargeCreditor.ACC_Direction = "WWW";
			chargeCreditor.ACC_JobType = "SHP";
			AssertEquals(true, chargeCreditor.ACC_DirectionInfo.HasErrors());

			chargeCreditor.ACC_Direction = "WWW";
			chargeCreditor.ACC_JobType = "BRK";
			AssertEquals(true, chargeCreditor.ACC_DirectionInfo.HasErrors());
		}

		public void TestAccJobType_ShouldNotValidateDirection_WhenJobTypeSetToNonDirectionSupportedJob()
		{
			var chargeCreditor = ChargeCreditor;
			chargeCreditor.ACC_Direction = "WWW";
			chargeCreditor.ACC_JobType = JobInvoicingConsumerTypes.PostClearanceBrokerageCode;
			AssertEquals(false, chargeCreditor.ACC_DirectionInfo.HasErrors());
		}

		public void TestAccJobType_ShouldValidateTransportMode_WhenJobTypeSetToTransportModeSupportedJob()
		{
			var chargeCreditor = ChargeCreditor;
			chargeCreditor.ACC_TransportMode = "WWW";
			chargeCreditor.ACC_JobType = "SHP";
			AssertEquals(true, chargeCreditor.ACC_TransportModeInfo.HasErrors());

			chargeCreditor.ACC_TransportMode = "WWW";
			chargeCreditor.ACC_JobType = "BRK";
			AssertEquals(true, chargeCreditor.ACC_TransportModeInfo.HasErrors());
		}

		public void TestAccJobType_ShouldNotValidateTransportMode_WhenJobTypeSetToNonDirectionSupportedJob()
		{
			var chargeCreditor = ChargeCreditor;
			chargeCreditor.ACC_TransportMode = "WWW";
			chargeCreditor.ACC_JobType = JobInvoicingConsumerTypes.PostClearanceBrokerageCode;
			AssertEquals(false, chargeCreditor.ACC_TransportModeInfo.HasErrors());
		}

		#endregion

		#region TransportMode tests

		public void TestAccTransportMode_Should_SetToStringEmpty_When_JobTypeIsNull()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_SetToEmpty_When_ACCJobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_SetToEmpty_When_JobTypeSetToNonTransportModeSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = "WKI";
			AssertEquals(ZString.Empty, chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_SetToEmpty_When_JobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(ZString.Empty, chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_SetBackToItsPreviousState_When_JobTypeSetToTransportModeSupportedJob()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_TransportMode);
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals("ALL", chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_NotChange_When_JobTypeSetToTransportModeSupportedJob()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals("ALL", chargecreditor.ACC_TransportMode);
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals("ALL", chargecreditor.ACC_TransportMode);
		}

		public void TestAccTransportMode_Should_SetToReadOnly_When_ACCJobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(true, chargecreditor.TransportModeIsReadOnly);
		}

		public void TestAccTransportMode_Should_SetToReadOnly_When_ACCJobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals(false, chargecreditor.TransportModeIsReadOnly);
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(true, chargecreditor.TransportModeIsReadOnly);
		}

		public void TestAccTransportMode_Should_SetToReadOnly_When_ACCJobTypeSetToNonTransportModeSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = "WKI";
			AssertEquals(true, chargecreditor.TransportModeIsReadOnly);
		}

		public void TestAccTransportMode_Should_SetToNotReadOnly_When_ACCJobTypeSetToTransportModeSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_TransportMode = "ALL";
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals(false, chargecreditor.TransportModeIsReadOnly);

			chargecreditor.ACC_JobType = "BRK";
			AssertEquals(false, chargecreditor.TransportModeIsReadOnly);
		}

		#endregion

		#region Direction Tests

		public void TestAccDirection_Should_SetToStringEmpty_When_JobTypeIsNull()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_SetToEmpty_When_JobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_SetToEmpty_When_JobTypeSetToNonDirectionSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "WKI";
			AssertEquals(ZString.Empty, chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_SetToEmpty_When_JobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(ZString.Empty, chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_SetBackToItsPreviousState_When_JobTypeSetToDirectionSupportedJob()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZString.Empty, chargecreditor.ACC_Direction);
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals("ALL", chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_NotChange_When_JobTypeSetToDirectionSupportedJob()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals("ALL", chargecreditor.ACC_Direction);
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals("ALL", chargecreditor.ACC_Direction);
		}

		public void TestAccDirection_Should_SetToReadOnly_When_ACCJobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(true, chargecreditor.DirectionIsReadOnly);
		}

		public void TestAccDirection_Should_SetToReadOnly_When_ACCJobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(true, chargecreditor.DirectionIsReadOnly);
		}

		public void TestAccDirection_Should_SetToReadOnly_When_ACCJobTypeSetToNonTransportModeSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "WKI";
			AssertEquals(true, chargecreditor.DirectionIsReadOnly);
		}

		public void TestAccDirection_Should_SetToNotReadOnly_When_ACCJobTypeSetToDirectionSupported()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "FCN";
			chargecreditor.ACC_Direction = "ALL";
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals(false, chargecreditor.DirectionIsReadOnly);

			chargecreditor.ACC_JobType = "BRK";
			AssertEquals(false, chargecreditor.DirectionIsReadOnly);
		}

		#endregion

		#region Department Tests

		public void TestACCGEDepartment_Should_SetToEmpty_When_JobTypeIsNull()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZGuid.Empty, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_SetToEmpty_When_JobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(ZGuid.Empty, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_SetToEmpty_When_JobTypeSetToNotActive()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals(ZGuid.Empty, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_SetToEmpty_When_JobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(ZGuid.Empty, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_SetBackToItsPreviousState_When_JobTypeSetToActiveJobType()
		{
			var departmentId = ZGuid.NewZGuid();
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_GE_Department = departmentId;
			chargecreditor.ACC_JobType = string.Empty;
			AssertEquals(ZGuid.Empty, chargecreditor.ACC_GE_Department);
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals(departmentId, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_NotChange_When_JobTypeSetToActiveJob()
		{
			var departmentID = ZGuid.NewZGuid();
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_GE_Department = departmentID;
			chargecreditor.ACC_JobType = "TBM";
			AssertEquals(departmentID, chargecreditor.ACC_GE_Department);
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals(departmentID, chargecreditor.ACC_GE_Department);
		}

		public void TestACCGEDepartment_Should_SetToReadOnly_When_ACCJobTypeSetToEmpty()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_JobType = ZString.Empty;
			AssertEquals(true, chargecreditor.DepartmentIsReadOnly);
		}

		public void TestACCGEDepartment_Should_SetToReadOnly_When_ACCJobTypeHasErrors()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = "AAA";
			AssertEquals(true, chargecreditor.DepartmentIsReadOnly);
		}

		public void TestACCGEDepartment_Should_SetToReadOnly_When_ACCJobTypeSetToInactiveJobType()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "SHP";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = "FCN";
			AssertEquals(true, chargecreditor.DepartmentIsReadOnly);
		}

		public void TestACCGEDepartment_Should_SetToNotReadOnly_When_ACCJobTypeSetToActiveJobType()
		{
			var chargecreditor = ChargeCreditor;
			chargecreditor.ACC_JobType = "TBN";
			chargecreditor.ACC_GE_Department = ZGuid.NewZGuid();
			chargecreditor.ACC_JobType = "SHP";
			AssertEquals(false, chargecreditor.DepartmentIsReadOnly);
		}

		#endregion

		#region PaymentTerm Tests

		public void TestEmptyPaymentTerm()
		{
			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			var creditorOverrides = new AccChargeCreditorOverrideCollection(chargeCode);
			var creditorOverride = creditorOverrides.AddNew();
			creditorOverride.ACC_JobType = AccChargeBranchOverrideLookups.JobTypeAdditionalCodes.All;
			creditorOverride.ACC_OH_Creditor = Factory.NewWithValidTestData<OrgHeader>().PK;

			AssertNullOrEmpty("Precondition: empty PaymentTerm", creditorOverride.ACC_PaymentTerm);

			AssertNoExceptionThrown
			(
				"GIVEN ChargeCreditorOverride JobType=ALL and PaymentTerm='' WHEN saving THEN should not throw sql exception",
				() => Factory.Save()
			);
		}

		public void TestPaymentTerm_ReadOnlyAndValue()
		{
			var chargeCreditor = ChargeCreditor;
			var lookups = new AccChargeCreditorOverrideLookups(chargeCreditor);
			var jobTypesList = lookups.JobTypeList.GetAllCodes();

			foreach (var jobType in jobTypesList)
			{
				chargeCreditor.ACC_PaymentTerm = Core.Constants.PaymentType.Collect;
				chargeCreditor.ACC_JobType = jobType;

				var isJobTypeSupported = jobType.In(supportedJobTypes);
				AssertEquals($"JobType={jobType} - PaymentTerm ReadOnly", !isJobTypeSupported, chargeCreditor.ACC_PaymentTermInfo.ReadOnly);
				AssertEquals
				(
					$"JobType={jobType} - PaymentTerm Value",
					isJobTypeSupported ? Core.Constants.PaymentType.Collect : string.Empty,
					chargeCreditor.ACC_PaymentTerm
				);
			}
		}

		readonly string[] supportedJobTypes = new[]
		{
			JobInvoicingConsumerTypes.GatewayConsolCode,
			JobInvoicingConsumerTypes.ForwardingConsolCode,
			JobInvoicingConsumerTypes.ShipmentCode
		};

		#endregion

		#region CreditorRole Tests

		public void TestCreditorRole_ReadOnlyAndValue_JobType()
		{
			var chargeCreditor = ChargeCreditor;
			var lookups = new AccChargeCreditorOverrideLookups(chargeCreditor);
			var jobTypesList = lookups.JobTypeList.GetAllCodes();
			chargeCreditor.ACC_Direction = Core.Constants.FreightShipmentDirection.Code.Import;

			foreach (var jobType in jobTypesList)
			{
				chargeCreditor.ACC_CreditorRole = DocAddressTypes.Codes.OverseasAgent;
				chargeCreditor.ACC_JobType = jobType;

				var isJobTypeSupported = jobType.In(supportedJobTypes);
				AssertEquals($"JobType={jobType} - CreditorRole ReadOnly", !isJobTypeSupported, chargeCreditor.ACC_CreditorRoleInfo.ReadOnly);
				AssertEquals
				(
					$"JobType={jobType} - CreditorRole Value",
					isJobTypeSupported ? DocAddressTypes.Codes.OverseasAgent : string.Empty,
					chargeCreditor.ACC_CreditorRole
				);
			}
		}

		public void TestCreditorRole_ReadOnlyAndValue_Direction()
		{
			var chargeCreditor = ChargeCreditor;
			var lookups = new AccChargeCreditorOverrideLookups(chargeCreditor);
			var directionsList = lookups.DirectionList.GetAllCodes().ToList<string>();

			foreach (var direction in directionsList)
			{
				chargeCreditor.ACC_JobType = JobInvoicingConsumerTypes.ShipmentCode;
				chargeCreditor.ACC_CreditorRole = DocAddressTypes.Codes.OverseasAgent;
				chargeCreditor.ACC_Direction = direction;

				var isDirectionSupported = direction.In(supportedDirections);
				AssertEquals($"Direction={direction} - CreditorRole ReadOnly", !isDirectionSupported, chargeCreditor.ACC_CreditorRoleInfo.ReadOnly);
				AssertEquals
				(
					$"Direction={direction} - CreditorRole Value",
					isDirectionSupported ? DocAddressTypes.Codes.OverseasAgent : string.Empty,
					chargeCreditor.ACC_CreditorRole
				);
			}
		}

		readonly string[] supportedDirections = new[] { Core.Constants.FreightShipmentDirection.Code.Import, Core.Constants.FreightShipmentDirection.Code.Export };

		#endregion

		#region EnterpriseBusinessObjectTestCase Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			AccChargeCreditorOverride result = Factory.New<AccChargeCreditorOverride>();
			result.ACC_AC_ChargeCode = Factory.New<AccChargeCode>().PK;
			result.ACC_TransportMode = "ALL";
			result.ACC_PaymentTerm = "ALL";
			result.ACC_DefaultingRule = "SCA";
			result.ACC_Direction = "ALL";
			result.ACC_JobType = "ALL";
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_Code = "XYZ";
			result.ACC_OH_Creditor = creditor.PK;
			return result;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			BusinessObjectFactory factory = NewFactory();
			BusinessObject bO;

			if (!IsDeleteSupported())
			{
				bO = GetNewBusinessObject();
				try
				{
					bO.Delete();
					AssertNotNullOrEmpty("Deleting is not supported on " + bO.GetType().FullName + " but no developer error is raised", ErrorReporter.LastMessageReported);
				}
				catch (NotSupportedException) // if it is very bad to have the object deleted, throw not supported exception
				{
					Assert(true);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
			else
			{
				bO = GetNewBusinessObjectForDeleteTest(factory);
				var creditorOverride = bO as AccChargeCreditorOverride;
				if (creditorOverride != null)
				{
					creditorOverride.ACC_TransportMode = "ALL";
					creditorOverride.ACC_DefaultingRule = "SCA";
					creditorOverride.ACC_Direction = "ALL";
					creditorOverride.ACC_JobType = "SHP";
					creditorOverride.ACC_PaymentTerm = "ALL";
					OrgHeader creditor = factory.NewWithValidTestData<OrgHeader>();
					creditorOverride.ACC_OH_Creditor = creditor.PK;
				}
				factory.Save();
				AssertEquals("BO.IsDeleted", false, bO.IsDeleted);
				BusinessObjectFactory separateFactory = NewFactory();

				AssertNotNull("The BizO is saved and should have been persisted", separateFactory.Load(bO.GetType(), bO.PK));

				if (CanPersistedObjectBeDeleted)
				{
					bO.Delete();

					if (bO.TableName != "StmALog")
					{
						try
						{
							bO.Factory.Save();
						}
						catch (ZSaveException ex)
						{
							if (ex.ToString().Contains("conflicted with the REFERENCE constraint"))
							{
								ErrorReporter.ReportOnce("Tables were last saved in the following order:\r\n" + string.Join(",", 1));
							}
							throw;
						}
						separateFactory = NewFactory();
						AssertNull("The BizO should have been deleted from DB", separateFactory.Load(bO.GetType(), bO.PK));
					}
				}
			}
		}

		#endregion

		AccChargeCreditorOverride ChargeCreditor => GetNewBusinessObject() as AccChargeCreditorOverride;

		protected override void SetUp()
		{
			base.SetUp();
			ChargeCreditor.ACC_Direction = "ALL";
		}
	}
}
