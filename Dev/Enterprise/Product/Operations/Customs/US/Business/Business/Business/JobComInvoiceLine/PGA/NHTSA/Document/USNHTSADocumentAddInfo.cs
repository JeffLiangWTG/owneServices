using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.US.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.USNHTSADocument)]
	public class USNHTSADocumentAddInfo : AutoUSNHTSADocumentAddInfo
	{
		public USNHTSADocumentAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);

			US_NHTDocumentTypeInfo.HumanReadableName = "Document Type";
			US_NHTDocumentOwnerInfo.HumanReadableName = "Who has a copy?";
		}

		public NHTSADocument Document => Parent as NHTSADocument;

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;
				if (HasChanges)
				{
					var document = Parent as NHTSADocument;
					if (document != null && !document.IsMarkingAsNeedingValidationSuspended)
					{
						document.Header?.MarkAsNeedingValidation();
					}
				}
			}
		}
	}
}
