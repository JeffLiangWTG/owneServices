using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class EntityTypeSupportedToken
	{
		internal static EntityTypeSupportedToken Supported
		{
			get { return new EntityTypeSupportedToken(true, null); }
		}

		internal static EntityTypeSupportedToken NotSupported(MultilingualString notSupportedReason)
		{
			return new EntityTypeSupportedToken(false, notSupportedReason);
		}

		EntityTypeSupportedToken(bool isSupported, MultilingualString notSupportedReason)
		{
			IsSupported = isSupported;
			NotSupportedReason = notSupportedReason;
		}

		public bool IsSupported { get; }
		public MultilingualString NotSupportedReason { get; }
	}
}
