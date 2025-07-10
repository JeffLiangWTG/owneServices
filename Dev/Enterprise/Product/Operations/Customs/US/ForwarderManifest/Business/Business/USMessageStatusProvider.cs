using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class USMessageStatusProvider : MessageStatusProvider
	{
		public override bool AllowCancellationMessage(IMessageParent parent)
		{
			return false;
		}

		public override bool AllowModificationMessage(IMessageParent parent)
		{
			return false;
		}

		public override bool AllowOriginalMessage(IMessageParent parent)
		{
			return false;
		}

		public override bool HasManifestBeenAcceptedByCustoms(IMessageParent parent)
		{
			return false;
		}

		public override CodeDescriptionPairList GetMessageStatusList(BusinessObjectFactory factory, ZString countryCode)
		{
			return new CodeDescriptionPairList();
		}

		public override CodeDescriptionPairList GetArrivalStatusList(BusinessObjectFactory factory, ZString countryCode)
		{
			return new CodeDescriptionPairList();
		}

		public override CodeDescriptionPairList GetRegistrationStatusListCore(BusinessObjectFactory factory, ZString countryCode)
		{
			return new CodeDescriptionPairList();
		}
	}
}
