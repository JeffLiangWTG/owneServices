using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.TW.Business;

public class CustomsDocumentWrapperProvider : Integration.Customs.TW.ICustomsDocumentWrapperProvider
{
	IDocumentWrapper Integration.Customs.Shared.ICustomsDocumentWrapperProvider.GetDocumentWrapper(BusinessObject parent)
	{
		if (parent is CusPackingList packingList)
		{
			return DocCusPackingList.New(packingList, parent.Factory);
		}
		return null;
	}
}
