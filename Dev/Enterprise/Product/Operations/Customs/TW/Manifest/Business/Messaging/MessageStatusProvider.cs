using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;
		public override bool AllowModificationMessage(IMessageParent parent) => false;
		public override bool AllowOriginalMessage(IMessageParent parent) => true;
		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;
		public override bool MessageStatusCanBeReset(IMessageParent parent) => false;
		public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode) => factory.GetCachedValue<TWMessageStatusCodeList>();
		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode) => RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, Codes.CustomsManifestStatus, ZDateTime.Today);
	}
}
