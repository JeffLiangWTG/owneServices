using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class RelatedDeclarationForExportCollection : CusSupportingInfoCollection<RelatedDeclarationForExport>
	{
		public RelatedDeclarationForExportCollection(BusinessObject parent)
			: base(parent, RelatedDeclarationForExport.RelatedDeclarationForExportType)
		{
		}
	}
}
