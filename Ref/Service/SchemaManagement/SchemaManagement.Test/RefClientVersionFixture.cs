using System;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.Service.SchemaManagement.Test
{
	public class RefClientVersionFixture : DataSetVersionFixture
	{
		protected override object[] PrepareData()
		{
			return new[]
			{
				new RefClient
				{
					RCT_PK = Guid.NewGuid(),
					RCT_ClientID = "AAA",
					RCT_Certificate = new byte[] { 0x20, 0x20, 0x20 },
					RCT_LegacyCertificate = new byte[] { 0x20, 0x20 },
					RCT_Signature = new byte[] { 0x20 }
				}
			};
		}

		protected override bool UpdateData(object data)
		{
			if (data is RefClient client)
			{
				client.RCT_Signature = new byte[] { 0x19 };
			}
			return true;
		}
	}
}
