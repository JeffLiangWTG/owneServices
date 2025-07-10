using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.US.AIM.Messaging;
using IMessageParent = Enterprise.Customs.ASYCUDA.Business.IMessageParent;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ACEManifest.Business
{
	public class MessageStatusProvider : ASYCUDA.Business.MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent) => false;

		public override bool AllowModificationMessage(IMessageParent parent) => false;

		public override bool AllowOriginalMessage(IMessageParent parent) => true;

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent) => false;

		public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode)
		{
			return factory.GetCachedValue("ACEManifest_USMessageStatusCodeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(MessageStatusCodeList.Codes.Sent, MessageStatusCodeList.Descriptions.Sent);
				result.AddPair(MessageStatusCodeList.Codes.Registered, MessageStatusCodeList.Descriptions.Registered);
				result.AddPair(MessageStatusCodeList.Codes.Cancel, MessageStatusCodeList.Descriptions.Cancel);
				result.AddPair(MessageStatusCodeList.Codes.Error, MessageStatusCodeList.Descriptions.Error);
				result.AddPair(MessageStatusCodeList.Codes.Warning, MessageStatusCodeList.Descriptions.Warning);
				return result;
			});
		}

		public override CodeDescriptionPairList GetArrivalStatusList(BusinessObjectFactory factory, ZString countryCode)
		{
			return factory.GetCachedValue("ACEManifest_USArrivalStatusCodeList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(MessageStatusCodeList.Codes.Sent, MessageStatusCodeList.Descriptions.Sent);
				result.AddPair(MessageStatusCodeList.Codes.Registered, MessageStatusCodeList.Descriptions.Registered);
				result.AddPair(MessageStatusCodeList.Codes.Error, MessageStatusCodeList.Descriptions.Error);
				return result;
			});
		}

		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode)
		{
			return AIMDispositionCodesHelper.GetCustomsStatusList(factory);
		}
	}
}
