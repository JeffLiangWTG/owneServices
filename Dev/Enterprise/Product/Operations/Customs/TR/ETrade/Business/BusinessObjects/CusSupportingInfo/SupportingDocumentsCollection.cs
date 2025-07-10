using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class SupportingDocumentsCollection : CusSupportingInfoCollection<SupportingDocuments>
	{
		public SupportingDocumentsCollection(BusinessObject parent)
			: base(parent, SupportingDocuments.SupportingDocumentsType)
		{
		}
	}
}
