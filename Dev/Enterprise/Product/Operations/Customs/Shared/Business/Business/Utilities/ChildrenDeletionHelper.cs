using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Extensions;

namespace Enterprise.Customs.Business
{
	public class ChildrenDeletionHelper : Integration.Customs.IChildrenDeletionHelper
	{
		public void DeleteChildren(BusinessObject bizObj)
		{
			bizObj.FetchForLoadChildEditableObjectsIfNeeded();
			bizObj.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
		}
	}
}
