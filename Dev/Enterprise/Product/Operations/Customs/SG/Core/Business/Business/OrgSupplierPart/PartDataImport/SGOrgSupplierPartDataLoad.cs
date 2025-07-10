using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.SG.V4.Business
{
	public class SGOrgSupplierPartDataLoad : GlobalOrgSupplierPartDataLoad, Integration.Customs.SG.ISGOrgSupplierPartDataLoad
	{
		protected override ZString GetClassificationType(PartsDataToLoad dataToLoad)
		{
			return Classification.DefaultClassificationType;
		}

		protected override BaseCusClassification GetClassification(string partClassCode, string classType)
		{
			return base.GetClassification(partClassCode, Classification.DefaultClassificationType);
		}

		protected override bool ClassificationLookupExistsOrIsNotRequired(ZString impLookup, ZString expLookup)
		{
			//SG only has 1 lookup used for both import & export (Check if user has used either data field)
			bool classificationExistsOrNotRequired = true;
			if (!impLookup.IsEmpty)
			{
				if (GetClassificationPK(impLookup, Classification.DefaultClassificationType).IsEmpty)
				{
					classificationExistsOrNotRequired = false;
				}
			}
			else if (!expLookup.IsEmpty)
			{
				if (GetClassificationPK(expLookup, Classification.DefaultClassificationType).IsEmpty)
				{
					classificationExistsOrNotRequired = false;
				}
			}
			return classificationExistsOrNotRequired;
		}

		protected override IEnumerable<string> GetFieldNames()
		{
			return Enumerable.Empty<string>();
		}
	}
}
