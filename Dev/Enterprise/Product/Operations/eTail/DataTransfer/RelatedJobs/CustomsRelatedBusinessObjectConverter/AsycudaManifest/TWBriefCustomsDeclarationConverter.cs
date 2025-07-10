using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.MasterFiles.Business;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.eTail.DataTransfer
{
	public class TWBriefCustomsDeclarationConverter : AsycudaManifestConverter
	{
		public TWBriefCustomsDeclarationConverter(BaseHVLVRelatedJobCommand relatedJobCommand)
			: base(relatedJobCommand)
		{
		}

		public override CodeDescriptionPair ManifestType
		{
			get
			{
				var result = new CodeDescriptionPair();
				if (Shipment.IsImport())
				{
					result.Code = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
					result.Description = TWManifestTypes.Descriptions.ImportLowValueDutyFreeGoods;
				}
				else if (Shipment.IsExport())
				{
					result.Code = TWManifestTypes.Codes.ExportLowValueGoods;
					result.Description = TWManifestTypes.Descriptions.ExportLowValueGoods;
				}

				return result;
			}
		}

		public override CodeDescriptionPair ManifestApplicationTypeCode => new CodeDescriptionPair { Code = ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration, Description = ApplicationCodeTypeList.Descriptions.TWBriefCustomsDeclaration };
	}
}
