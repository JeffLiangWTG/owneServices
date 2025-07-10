using System.Globalization;
using System;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	internal class DataFixShippingLineIsShippingLineTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;

				cmd.CommandText = @"SELECT COUNT(*) FROM RefShippingLine WHERE RSL_IsNVO = 0 and RSL_IsShippingLine <> 1";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixShippingLineIsShippingLineTransformation(0);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO RefShippingLine (RSL_PK,RSL_IsActive,RSL_IsNVO,RSL_BookingRequestAvailable,RSL_ShippingInstructionAvailable,RSL_VerifiedGrossContainerWeightAvailable,RSL_ShippingOrderAvailable,RSL_EManifestAvailable,RSL_CarrierName,RSL_StandardCarrierAlphaCode,RSL_CargoWiseOneCode,RSL_OceanCarrierMessagingAvailable,RSL_GlobalSailingScheduleAvailable,RSL_ContainerAutomationAvailable,RSL_CargoSphereRatesAvailable,RSL_IsShippingLine,RSL_InvoiceAvailable,RSL_IsCW1User,RSL_EHubIds)
VALUES
('A4713629-FEA4-4EEC-A818-A49D74636513','1', '0', '0', '0', '0', '0', '0', 'Carrier Name','','CW1','0', '0', '0', '0', '0', '0', '0', '');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
