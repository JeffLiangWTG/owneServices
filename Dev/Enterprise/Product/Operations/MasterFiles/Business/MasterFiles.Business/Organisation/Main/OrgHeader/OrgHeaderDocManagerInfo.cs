using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class OrgHeaderDocManagerInfo : DocManagerInfo
	{
		public OrgHeaderDocManagerInfo(OrgHeader parent, ZString docManagerCode) : base(parent, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			ArrayList list = new ArrayList();
			OrgHeader organisation = (OrgHeader)BusinessEntity;
			if (organisation != null && organisation.CompanyData != null)
			{
				list.Add(organisation.CompanyData);
				list.AddRange(organisation.SalesOpportunities);
				foreach (OrgOpportunity opp in organisation.SalesOpportunities)
				{
					list.AddRange(opp.WorkflowItems);
				}
			}

			return (BusinessObject[])list.ToArray(typeof(BusinessObject));
		}
	}
}
