
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PreviousDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentCollection
{
	public PreviousDocumentCollection(BusinessObject parent)
		: base(parent)
	{ }

	public new PreviousDocument this[int i] => (PreviousDocument)base[i];

	public new PreviousDocument AddNew() => (PreviousDocument)base.AddNew();

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var isExport = (Master as ICanBeImportOrExport)?.IsExport ?? false;
		if (isExport)
		{
			var prevDoc = child as PreviousDocument;
			if (prevDoc != null)
			{
				prevDoc.CSI_SubType = ExportPreviousDocSubTypeList.Codes.STD;
			}
		}
	}
}
