using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DataFixWI00514782TransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"SELECT COUNT(1) FROM RefShippingLineMessagingRequirement WHERE RSR_RSL_ShippingLine = 'E09BE845-EA2E-48B3-9209-D4652146658B'";
				Assert.AreEqual(1, cmd.ExecuteScalar());
				cmd.CommandText = @"SELECT COUNT(1) FROM RefShippingLineMessagingRequirement WHERE RSR_RSL_ShippingLine = 'E09BE845-EA2E-48B3-9209-D4652146658C'";
				Assert.AreEqual(0, cmd.ExecuteScalar());
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00514782Transformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT RefShippingLineMessagingRequirementType (RST_PK, RST_Code, RST_Description)
VALUES ('236F18A4-F61F-4ADB-A2B9-09F322E9EBBB', 'CON', 'Contract Number Mandatory'),
('236F18A4-F61F-4ADB-A2B9-09F322E9EBBC', 'TST', 'Test');

INSERT RefShippingLine (RSL_PK, RSL_IsActive, RSL_IsNVO, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_VerifiedGrossContainerWeightAvailable, RSL_ShippingOrderAvailable, RSL_EManifestAvailable, RSL_CarrierName, RSL_StandardCarrierAlphaCode, RSL_CargoWiseOneCode, RSL_OceanCarrierMessagingAvailable, RSL_GlobalSailingScheduleAvailable, RSL_ContainerAutomationAvailable, RSL_CargoSphereRatesAvailable, RSL_InvoiceAvailable, RSL_IsCW1User, RSL_EHubIds)
VALUES ('E09BE845-EA2E-48B3-9209-D4652146658B', 1, 0, 0, 0, 0, 0, 0, 'Test Carrier Name 1', '', 'CW01', 1, 0, 0, 0, 0, 0, ''),
('E09BE845-EA2E-48B3-9209-D4652146658C', 1, 0, 0, 0, 0, 0, 0, 'Test Carrier Name 2', '', 'CW02', 1, 0, 0, 0, 0, 0, '');

INSERT RefShippingLineMessagingRequirement (RSR_PK, RSR_RSL_ShippingLine, RSR_RST_NKType, RSR_IsBookingRequest, RSR_IsShippingInstruction, RSR_IsShippingOrder)
VALUES (NEWID(), 'E09BE845-EA2E-48B3-9209-D4652146658B', 'CON', 1, 0, 0),
(NEWID(), 'E09BE845-EA2E-48B3-9209-D4652146658B', 'TST', 0, 0, 0),
(NEWID(), 'E09BE845-EA2E-48B3-9209-D4652146658C', 'CON', 0, 0, 0),
(NEWID(), 'E09BE845-EA2E-48B3-9209-D4652146658C', 'TST', 0, 0, 0);
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
