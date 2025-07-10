using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public interface IVesselMovementsUrlGenerator
	{
		Task<VesselMovementsAuthTokenResult> GetTokenAsync(string correlationId, OrgContact contact = null, CancellationToken ct = default);
		(Uri url, string errorMessage) Generate(VesselMovementsAuthToken token, IVesselMovementsUrlSupporter supporter, VesselMovementsUrlGeneratorOptions options);
	}

	[Flags]
	public enum VesselMovementsUrlGeneratorOptions
	{
		None = 0,
		CollapseInfoPanel = 1 << 0,
		CollapsePortCallsPanel = 1 << 1,
	}

	public sealed class VesselMovementsAuthTokenResult
	{
		public VesselMovementsAuthToken Token { get; set; }
		public Uri RedirectUrl { get; set; }
		public string ErrorMessage { get; set; }
	}

	public sealed class VesselMovementsAuthToken
	{
		public VesselMovementsAuthToken(VesselMovementsAuthTokenType type, string value)
		{
			Type = type;
			Value = value;
		}

		public VesselMovementsAuthTokenType Type { get; }
		public string Value { get; }
	}

	public enum VesselMovementsAuthTokenType
	{
		Rating,
		MyAccount,
	}
}
