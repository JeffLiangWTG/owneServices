using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Business;

public interface IPreviousDocumentsProvider
{
	ICusSupportingInfoCollection<PreviousDocument> PreviousDocuments { get; }
}
