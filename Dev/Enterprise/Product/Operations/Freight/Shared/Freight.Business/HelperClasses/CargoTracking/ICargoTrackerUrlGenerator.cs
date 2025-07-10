using System;
using System.Threading;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface ICargoTrackerUrlGenerator
	{
		CargoTrackerAuthTokenResult GetToken(string correlationId, OrgContact contact = null, CancellationToken ct = default);
		CargoTrackerAuthTokenResult GetSelfSignedSystemToSystemToken(string licenseCode);
		(Uri url, string errorMessage) Generate(CargoTrackerAuthToken token, string licenseCode, string consignmentNumber);
	}

	public sealed class CargoTrackerAuthTokenResult
	{
		public CargoTrackerAuthToken Token { get; set; }
		public Uri RedirectUrl { get; set; }
		public string ErrorMessage { get; set; }
	}

	public sealed class CargoTrackerAuthToken
	{
		public CargoTrackerAuthToken(CargoTrackerAuthTokenType type, string value)
		{
			Type = type;
			Value = value;
		}

		public CargoTrackerAuthTokenType Type { get; }
		public string Value { get; }
	}

	public enum CargoTrackerAuthTokenType
	{
		Rating,
		SelfSignedSystemToSystem
	}
}
