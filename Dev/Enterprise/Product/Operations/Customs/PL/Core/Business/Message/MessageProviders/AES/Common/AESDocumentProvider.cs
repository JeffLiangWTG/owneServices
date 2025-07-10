using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class AESDocumentProvider : IDocument
{
	public AESDocumentProvider(CusSupportingInfo cusSupportingInfo, bool referenceNumberAsDescription)
	{
		this.cusSupportingInfo = Argument.NotNull(cusSupportingInfo, nameof(cusSupportingInfo));
		this.referenceNumberAsDescription = referenceNumberAsDescription;
	}
	readonly CusSupportingInfo cusSupportingInfo;
	readonly bool referenceNumberAsDescription;

	public string Type => CachedValueHelper.GetValue(ref type, () => MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Code));
	CachedValue<string> type;

	public string Description => CachedValueHelper.GetValue(ref description, () => referenceNumberAsDescription ? GetReferenceNumberCore() : GetDescriptionCore());
	protected virtual string GetReferenceNumberCore() =>
		MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_ReferenceNumber);
	protected virtual string GetDescriptionCore() =>
		MessageProviderHelper.ReturnNullIfEmpty(cusSupportingInfo.CSI_Description);
	CachedValue<string> description;
}
