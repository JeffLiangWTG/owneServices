using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public partial class DocumentTypeCodeList
	{
		public static ZString GetDescriptionFromDocumentType(BusinessObjectFactory factory, ZString documentType)
		{
			return documentType.Length == 2
				? (ZString)factory.GetCachedValue<DocumentTypeCodeList>().GetDescriptionFromCode(documentType)
				: ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, documentType, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USDISFormList, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
		}
	}
}
