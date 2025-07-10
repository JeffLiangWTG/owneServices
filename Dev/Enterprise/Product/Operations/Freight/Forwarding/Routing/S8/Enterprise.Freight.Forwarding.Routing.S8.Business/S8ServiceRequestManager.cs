using System;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class S8ServiceRequestManager : IServiceRequestManager
	{
		ZDateTime suppressUntilUtc;

		public bool IsSuppressed()
		{
			return ZDateTime.UtcNow <= SuppressUntilUtc;
		}

		public void OnSuccessfulRequest()
		{
			SuppressUntilUtc = DateTime.MinValue;
		}

		public void OnTimedOutRequest()
		{
			SuppressUntilUtc = ZDateTime.UtcNow.AddMinutes(SuppressionTimeoutInMinutes);
		}

		public void HandleException(Exception ex, Uri uri = null)
		{
			OnTimedOutRequest();
		}

		int SuppressionTimeoutInMinutes => FreightDataRegistry.Instance.S8LoginSuppressionTimeoutInMinutes.Value;

		public ZDateTime SuppressUntilUtc
		{
			get => suppressUntilUtc;
			private set
			{
				suppressUntilUtc = value.IsValid ? value : DateTime.MinValue;
			}
		}

		public TimeSpan RequestTimeout => throw new NotImplementedException();
	}
}
