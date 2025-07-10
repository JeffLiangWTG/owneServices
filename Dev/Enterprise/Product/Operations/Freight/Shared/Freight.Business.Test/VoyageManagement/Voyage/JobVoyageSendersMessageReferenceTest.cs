using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobVoyageSendersMessageReferenceTest : SendersMessageReferenceProviderTest
	{
		#region Implementation

		protected override ISendersMessageReferenceProvider GetSavableProvider()
		{
			return Factory.New<JobVoyage>();
		}

		protected override ISendersMessageReferenceProvider GetProviderThatThrowsExceptionWhilstSaving()
		{
			return Factory.New<TestHelperJobVoyage>();
		}

		class TestHelperJobVoyage : JobVoyage
		{
			public TestHelperJobVoyage(BusinessObjectFactory factory, DataRow row)
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
