using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;
		public override bool AllowModificationMessage(IMessageParent parent) => false;
		public override bool AllowOriginalMessage(IMessageParent parent) => true;
		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;
		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;
		public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<CodeDescriptionPairList>();
	}
}
