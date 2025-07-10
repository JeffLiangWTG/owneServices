using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginSendersMessageReferenceTest : SendersMessageReferenceProviderTest
	{
		#region Implementation

		protected override ISendersMessageReferenceProvider GetSavableProvider()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			return voyage.Origins.AddNew();
		}

		protected override ISendersMessageReferenceProvider GetProviderThatThrowsExceptionWhilstSaving()
		{
			return Factory.New<TestHelperVoyageOrigin>();
		}

		public class TestHelperVoyageOrigin : VoyageOrigin
		{
			public TestHelperVoyageOrigin(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new ApplicationException("Test Helper Exception");
			}

			public void InvokeOnFactorySavingBeforeTransactionCore()
			{
				base.HasChanges = true;
				base.OnFactorySavingBeforeTransactionCore();
			}
		}
		#endregion
	}
}
