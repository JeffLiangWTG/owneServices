using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class DocManagerInfoExtensions
	{
		public static IEnumerable<BusinessObject> GetRelatedObjectsChain(this DocManagerInfo docManager, Type chidType, SchemaGuidColumn parentIDColumn)
		{
			var children = docManager.BusinessEntity.Factory.Load(chidType, new ZQuery(parentIDColumn, docManager.BusinessEntity.PK));
			var expectedCountryCode = GlbCompany.CurrentCompany.Country.Code;

			bool alreadyAdded = false;
			foreach (var child in children)
			{
				if (ShouldLoadBusinessObjectUnderCurrentCountry(child, expectedCountryCode))
				{
					var docSupport = child as IDocManagerSupport;
					if (docSupport != null)
					{
						foreach (var obj in docSupport.DocManagerInfo.RelatedObjects)
						{
							yield return obj;
						}
						if (!alreadyAdded)
						{
							yield return docSupport.DocManagerInfo.BusinessEntity;
							alreadyAdded = true;
						}
					}
				}
			}
		}

		public static bool ShouldLoadBusinessObjectUnderCurrentCountry(BusinessObject child, ZString expectedCountryCode)
		{
			var result = true;
			var countryCode = (child as Enterprise.Integration.Customs.Shared.ICountryCodeProvider)?.CountryCode ?? ZString.Empty;
			if (!countryCode.IsEmpty && expectedCountryCode != countryCode)
			{
				result = false;
			}
			return result;
		}
	}
}
