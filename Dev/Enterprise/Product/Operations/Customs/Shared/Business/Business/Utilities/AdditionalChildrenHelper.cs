using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.Business
{
	public static class AdditionalChildrenHelper
	{
		public static void AddAdditionalChildrenFetchHints(BusinessObject bizObj)
		{
			var entBizObj = bizObj as EnterpriseBusinessObject;
			if (entBizObj != null && entBizObj.IsInDatabase && entBizObj.SupportsNotes)
			{
				entBizObj.Factory.AddFetchHint(StmNoteSchema.ST_ParentID, entBizObj.PK);
			}
		}

		public static BusinessObject[] GetAdditionalChildrenIfSupported(BusinessObject bizObj)
		{
			var result = new List<BusinessObject>();

			if (bizObj is ICusAddInfoTypeSupporter supportCusAddInfo)
			{
				result.AddRange(supportCusAddInfo.GetCusAddInfoChildren());
			}

			if (bizObj is ICusCodeDataTypeSupporter supportCusCodeData)
			{
				result.AddRange(supportCusCodeData.GetCusCodeDataChildren());
			}

			if (bizObj is IDocAddresses docAddresses)
			{
				result.AddRange(docAddresses.DocAddresses);
			}

			return result.ToArray();
		}
	}
}
