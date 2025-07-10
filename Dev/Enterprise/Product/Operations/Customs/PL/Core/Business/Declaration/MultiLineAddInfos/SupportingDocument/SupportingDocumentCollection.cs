
using CargoWise.EntityFramework;

namespace Enterprise.Customs.PL.Business.Declaration;

public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
{
	public SupportingDocumentCollection(BusinessObject parent)
		: base(parent)
	{ }

	public new SupportingDocument this[int i] => (SupportingDocument)base[i];

	public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();
}
