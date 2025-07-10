using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Business
{
	public class DocumentTypeCommonDataParser : CommonDataParser
	{
		public DocumentTypeCommonDataParser()
			: base(Constants.DocumentTypeCommon.RDEntityAttributeValue, Constants.DocumentTypeCommon.DocumentTypeAttributeValue)
		{

		}

		protected override void SetCusCodeListAttributes(RefCusCodeList refCusCodeList, XElement entity)
		{
			var isTransportDocument = Utils.FindXElementValueByAttribute(
				entity,
				Constants.Common.DataItem,
				Constants.Common.RDEntityAttributeName,
				Constants.DocumentTypeCommon.TransportDocumentAttributeValue
			);

			if (string.Equals(isTransportDocument, "1", System.StringComparison.Ordinal))
			{
				refCusCodeList.RefCusCodeListAttributes = new[]
				{
					new RefCusCodeListAttribute()
					{
						ZZE_ZXE_NKName = Constants.DocumentTypeCommon.IsTransportDocument,
						ZZE_Value = Constants.DocumentTypeCommon.Yes
					}
				};
			}
		}
	}
}
