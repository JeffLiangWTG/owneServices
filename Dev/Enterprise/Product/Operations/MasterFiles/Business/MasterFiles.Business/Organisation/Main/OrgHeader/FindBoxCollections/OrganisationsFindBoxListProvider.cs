using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public class OrganisationsFindBoxListProvider : FindBoxListProvider
	{
		public OrganisationsFindBoxListProvider(OrganisationsFindBoxCollection collection)
			: base(collection)
		{
			OrganisationList = collection;
		}

		readonly IOrganisationDefaultProvider OrganisationList;

		protected override IEnumerable<BusinessObject> GetBusinessObjectsFromCodeCore(string code)
		{
			return GetBusinessObjectsFromCode(code, base.GetBusinessObjectsFromCodeCore);
		}

		protected override IEnumerable<BusinessObject> BizObjsFromCodeWithoutFilter(string code)
		{
			return GetBusinessObjectsFromCode(code, base.BizObjsFromCodeWithoutFilter);
		}

		#region implementation

		IEnumerable<BusinessObject> GetBusinessObjectsFromCode(string code, Func<string, IEnumerable<BusinessObject>> baseMethod)
		{
			if (code == OrgHeader.UnmatchedOrganisationCode)
			{
				OrganisationsFindBoxListHelper.SetFilterBusinessObjectFromUnmatchOrgRecord((OrganisationsFindBoxCollection)OrganisationList);
			}

			if ((!string.IsNullOrEmpty(code) && code != OrgHeader.UnmatchedOrganisationCode) ||
				(code == OrgHeader.UnmatchedOrganisationCode && OrganisationList.ConditionalDefaults.Count == 0))
			{
				return baseMethod(code);
			}
			else
			{
				OrganisationList.ShouldSetValuesFromConditionalDefaults = (code == OrgHeader.UnmatchedOrganisationCode);
				return Enumerable.Empty<BusinessObject>();
			}
		}

		#endregion
	}
}
