using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.TW.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent)
			: base(parent)
		{
		}

		public new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override CodeDescriptionPairList Natures => Universal.Helper.ShipmentTypeList.Export22AndImport23();

		public override CodeDescriptionPairList TransportModeList => Factory.GetCachedValue("TWTransportModeList", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea);
			result.AddPair(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air);
			return result;
		});

		public ICodeDescriptionPairList PaymentMethods => Factory.GetCachedValue<IMPPaymentMethod>();

		public CodeDescriptionPairList BoxNumbers => Parent.CusBrokerageBoxNumbers.PairList;

		public CodeDescriptionPairList MailboxList => Factory.GetValue(ref mailboxListCached, () =>
		{
			var result = new CodeDescriptionPairList();
			foreach (var password in Parent.Passwords)
			{
				result.AddPair(password.GP_MailBoxID);
			}
			return result;
		});
		CachedProperty<CodeDescriptionPairList> mailboxListCached;

		public GlbPersonCollection PersonsList => new GlbPersonCollection(Factory);
	}
}
