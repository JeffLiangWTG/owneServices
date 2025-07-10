using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business;

public abstract class AESBaseProviderWithPartiesBase : AESBaseProvider, IAESWithPartiesBase
{
	protected AESBaseProviderWithPartiesBase(BaseMessageSendingObject sendingObject)
		: base(sendingObject)
	{
	}

	public IExporter Exporter => CachedValueHelper.GetValue(ref exporter, () => GetExporterCore(AddressHelper.GetExporterAddressWithSupplierFallback(Declaration)));
	protected virtual IExporter GetExporterCore(OrgAddress orgAddress) => AESExporterProvider.NewOrNull(orgAddress);
	CachedValue<IExporter> exporter;

	public IAESDeclarantWithIdentificationNumbers Declarant => CachedValueHelper.GetValue(ref declarant, () => GetDeclarantCore(Declaration.DeclarantAddress));
	protected virtual IAESDeclarantWithIdentificationNumbers GetDeclarantCore(OrgAddress orgAddress) => AESDeclarantWithIdentificationNumbersProvider.NewOrNull(orgAddress, Declaration);
	CachedValue<IAESDeclarantWithIdentificationNumbers> declarant;

	public IAESRepresentative Representative => CachedValueHelper.GetValue(ref representative, () =>
		Declaration.JE_DeclarantType == PLRepresentationTypeList.Codes._4Direct
			? AESRepresentativeProvider.NewOrNull(Declaration.Representative?.Header)
			: null);
	CachedValue<IAESRepresentative> representative;
}
