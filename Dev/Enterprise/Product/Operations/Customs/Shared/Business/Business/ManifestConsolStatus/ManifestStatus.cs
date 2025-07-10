using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.Business
{
	[Immutable]
	public class ManifestStatus
	{
		protected ManifestStatus(MultilingualString value)
		{
			this.Value = value;
		}

		public readonly MultilingualString Value;

		public string AsString { get { return Value; } }

		/// <summary>
		/// Please only use this when you are getting the status text from the message. Define it as a standard type below otherwise.
		/// </summary>
		public static ManifestStatus FromString(string status)
		{
			return new ManifestStatus((NoResString)status);
		}

		public override string ToString()
		{
			return Res.GetString("f9ce305d-2023-4bc8-81c5-b22cfbd61e14", "Manifest Status of {0}", Value);
		}

		public static readonly ManifestStatus NotSent = new ManifestStatus(ResString.GetMultilingualString("ae5b1a3a-84f0-42ab-84c1-642007ed307e", "Not Sent"));
		public static readonly ManifestStatus AwaitingResponse = new ManifestStatus(ResString.GetMultilingualString("77f16c7c-ac95-42d2-bea7-8c0864b2f9e0", "Awaiting Response"));
		public static readonly ManifestStatus Rejected = new ManifestStatus(ResString.GetMultilingualString("91daedee-eb0b-4792-b89f-9c26bad9f053", "Manifest Rejected - Correct and Resend"));
		public static readonly ManifestStatus Cleared = new ManifestStatus(ResString.GetMultilingualString("64840ffc-c1b3-40e2-bc7d-7546ff618c0c", "Clear"));
		public static readonly ManifestStatus PartiallyCleared = new ManifestStatus(ResString.GetMultilingualString("f6da9ecc-54d1-4d08-859e-936500c73a40", "Partially Cleared"));
		public static readonly ManifestStatus AcknowledgedByCustoms = new ManifestStatus(ResString.GetMultilingualString("0d3ff1ef-209c-492c-b0a1-a6f55c349575", "Acknowledged by Customs"));
		public static readonly ManifestStatus Unknown = new ManifestStatus(ResString.GetMultilingualString("077aec87-d88c-4f17-9c24-51c8bc8c4ec2", "unknown"));
	}
}
