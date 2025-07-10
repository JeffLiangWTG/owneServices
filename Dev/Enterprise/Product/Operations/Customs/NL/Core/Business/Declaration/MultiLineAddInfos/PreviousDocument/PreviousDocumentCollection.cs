using CargoWise.EntityFramework;

namespace Enterprise.Customs.NL.Business.Declaration;

public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
{
	public PreviousDocumentCollection(BusinessObject parent) : base(parent)
	{
	}
	public new PreviousDocument this[int i] => (PreviousDocument)base[i];

	public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();
}
