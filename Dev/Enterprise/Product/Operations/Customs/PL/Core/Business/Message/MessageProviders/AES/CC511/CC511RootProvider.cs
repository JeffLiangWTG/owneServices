using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business;

public class CC511RootProvider : AESBaseProvider, ICC511CRoot
{
	public CC511RootProvider(BaseMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public ICC511CExportOperation ExportOperation => exportOperation ??= new CC511CExportOperationProvider(EntryHeader);
	ICC511CExportOperation exportOperation;

	public string CustomsOfficeOfPresentationReferenceNumber => CachedValueHelper.GetValue(ref customsOfficeOfPresentationReferenceNumber, () => Declaration.CustomsOfficeOfPresentationReferenceNumber());
	CachedValue<string> customsOfficeOfPresentationReferenceNumber;

	public string CustomsOfficeOfExportReferenceNumber => Declaration.JE_CustomsOffice;

	public ICC511GoodsShipment GoodsShipment => goodsShipment ??= new CC511GoodsShipmentProvider(EntryHeader);
	ICC511GoodsShipment goodsShipment;

	public override string MessageType => Constants.MessageType.AES.CC511C;

	public IAESDeclarant Declarant => CachedValueHelper.GetValue(ref declarant, () => AESDeclarantProvider.NewOrNull(Declaration.DeclarantAddress, Declaration));
	CachedValue<IAESDeclarant> declarant;

	public IRepresentative Representative => CachedValueHelper.GetValue(ref representative, () => RepresentativeProvider.NewOrNull(Declaration.Representative?.Header));
	CachedValue<IRepresentative> representative;
}
