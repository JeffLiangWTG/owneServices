using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusSeaManOBLHeaderSendersMessageReferenceTest : SendersMessageReferenceProviderTest
	{
		#region Implementation

		protected override ISendersMessageReferenceProvider GetSavableProvider()
		{
			return (Factory.New<CusSeaManTranHead>()).OceanBills.AddNew();
		}

		protected override ISendersMessageReferenceProvider GetProviderThatThrowsExceptionWhilstSaving()
		{
			return Factory.New<TestHelperCusSeaManOBLHeader>();
		}

		class TestHelperCusSeaManOBLHeader : CusSeaManOBLHeader
		{
			public TestHelperCusSeaManOBLHeader(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new ApplicationException("Test Helper Exception");
			}
		}
		#endregion
	}
}
