using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.ZA;
using Enterprise.ZArchitecture.Core;
using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent)
		{
			return HasManifestBeenAcceptedByCustoms(parent);
		}

		public override bool AllowModificationMessage(IMessageParent parent)
		{
			return HasManifestBeenAcceptedByCustoms(parent);
		}

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			return !HasManifestBeenAcceptedByCustoms(parent);
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => parent != null && (parent.HasCustomsNumbers || parent.IsCustomsCleared);

		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;

		public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<ZAMessageStatusList>();
	}
}
