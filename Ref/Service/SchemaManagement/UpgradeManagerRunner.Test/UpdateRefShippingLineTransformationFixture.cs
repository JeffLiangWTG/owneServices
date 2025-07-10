using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class UpdateRefShippingLineTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM RefShippingLine WHERE RSL_PK = 'E09BE845-EA2E-48B3-9209-D4652146658B' AND RSL_BookingRequestAvailable = 1";
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT COUNT(1) FROM RefShippingLine WHERE RSL_PK = '53D56270-8013-11EC-92E6-011864910190' AND RSL_BookingRequestAvailable = 1 AND RSL_ShippingInstructionAvailable = 1";
				Assert.AreEqual(1, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new UpdateRefShippingLineTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT RefShippingLineMessagingRequirementType (RST_PK, RST_Code, RST_Description)
VALUES ('236F18A4-F61F-4ADB-A2B9-09F322E9EBBB', 'CON', 'Contract Number Mandatory');

INSERT RefShippingLine (RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds)
VALUES ('E09BE845-EA2E-48B3-9209-D4652146658B', 1, 0, 0, 0, 0, 0, 0, 'Test Carrier Name 1', '', 'CW01', 1, 0, 0, 0, 0, 0, '');
INSERT RefShippingLineMessagingRequirement (RSR_PK, RSR_RSL_ShippingLine, RSR_RST_NKType, RSR_IsBookingRequest, RSR_IsShippingInstruction, RSR_IsShippingOrder)
VALUES (NEWID(), 'E09BE845-EA2E-48B3-9209-D4652146658B', 'CON', 1, 0, 0);

INSERT RefShippingLine (RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds)
VALUES ('53D56270-8013-11EC-92E6-011864910190', 1, 0, 0, 0, 0, 0, 0, 'Test Carrier Name 2', '', 'CW02', 1, 0, 0, 0, 0, 0, '');
INSERT RefShippingLineMessagingRequirement (RSR_PK, RSR_RSL_ShippingLine, RSR_RST_NKType, RSR_IsBookingRequest, RSR_IsShippingInstruction, RSR_IsShippingOrder)
VALUES (NEWID(), '53D56270-8013-11EC-92E6-011864910190', 'CON', 1, 1, 0);
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
