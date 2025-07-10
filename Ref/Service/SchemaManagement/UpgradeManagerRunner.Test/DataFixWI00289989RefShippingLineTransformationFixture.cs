using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner.Test
{
	class DataFixWI00289989RefShippingLineTransformationFixture : TransformationFixture
	{
		protected override void AssertTransformationResults()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.Transaction = Transaction;
				cmd.CommandText = @"Select count(1) from RefShippingLine where RSL_PK = 'AFA7F600-492A-7D3B-06D6-08D784537A47' and RSL_IsCW1User = 1 and RSL_EHubIds = 'SSTJAX,SSTSEA'";
				Assert.AreEqual(0, Convert.ToInt32(cmd.ExecuteScalar(), CultureInfo.InvariantCulture));
			}
		}

		protected override IDataTransformationTask GetTask()
		{
			return new DataFixWI00271682MeursingDataTransformation(49);
		}

		protected override void PrepareTestData()
		{
			using (var cmd = Connection.CreateCommand())
			{
				cmd.CommandText = @"
INSERT INTO [dbo].[RefShippingLine]
([RSL_PK],[RSL_IsActive],[RSL_IsNVO],[RSL_CarrierName],[RSL_StandardCarrierAlphaCode],[RSL_CargoWiseOneCode],[RSL_OceanCarrierMessagingAvailable]
,[RSL_GlobalSailingScheduleAvailable],[RSL_ContainerAutomationAvailable],[RSL_CargoSphereRatesAvailable],[RSL_InvoiceAvailable],[RSL_IsCW1User],[RSL_EHubIds])
VALUES ('AFA7F600-492A-7D3B-06D6-08D784537A47',1,1,'Tote Maritime','TMGT','C1TM',0,0,1,0,0,0,'');
";
				cmd.Transaction = Transaction;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
