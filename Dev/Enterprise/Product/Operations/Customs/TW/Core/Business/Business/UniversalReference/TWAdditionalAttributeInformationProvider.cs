using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.TW.Business
{
	public class TWAdditionalAttributeInformationProvider : IAdditionalAttributeInformationProvider
	{
		public TWAdditionalAttributeInformationProvider(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public bool AdditionalDescriptionVisible => true;

		public ZString AdditionalDescription(ZString attributeName, ZString attributeValue)
		{
			var result = ZString.Empty;
			var codeType = ZString.Empty;
			switch (attributeName)
			{
				case Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements:
					codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWCustomsRequirements;
					break;
				case Constants.UniversalReferenceConstants.CusTariffAttributeName.ImportRegulations:
					codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWImportRegulations;
					break;
				case Constants.UniversalReferenceConstants.CusTariffAttributeName.ExportRegulations:
					codeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.TWExportRegulations;
					break;
			}
			if (!codeType.IsEmpty)
			{
				result = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Taiwan, codeType, ZDateTime.Now)?.GetDescriptionFromCode(attributeValue) ?? ZString.Empty;
			}
			return result;
		}
	}
}
